////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

#include "catch.hpp"

#include "util/test_file.hpp"

#include "thread_confined.hpp"
#include "list.hpp"
#include "object_accessor.hpp"
#include "object_schema.hpp"
#include "property.hpp"
#include "schema.hpp"

#include <realm/commit_log.hpp>
#include <realm/util/optional.hpp>

#include <thread>
#include <future>

using namespace realm;

static TableRef get_table(Realm& realm, const ObjectSchema &object_schema) {
    return ObjectStore::table_for_object_type(realm.read_group(), object_schema.name);
}

static Object create_object(SharedRealm realm, const ObjectSchema &object_schema) {
    TableRef table = get_table(*realm, object_schema);
    return Object(std::move(realm), object_schema, (*table)[table->add_empty_row()]);
}

static List get_list(const Object& object, size_t column_ndx) {
    return List(object.realm(), object.row().get_linklist(column_ndx));
}

static Property nullable(Property p) {
    p.is_nullable = true;
    return p;
}

TEST_CASE("handover") {
    InMemoryTestFile config;
    config.cache = false;
    config.automatic_change_notifications = false;

    SharedRealm r = Realm::get_shared_realm(config);

    static const ObjectSchema string_object({"string_object", {
        nullable({"value", PropertyType::String}),
    }});
    static const ObjectSchema int_object({"int_object", {
        {"value", PropertyType::Int},
    }});
    static const ObjectSchema int_array_object({"int_array_object", {
        {"value", PropertyType::Array, "int_object"}
    }});
    r->update_schema({string_object, int_object, int_array_object});

    SECTION("disallowed during write transactions") {
        SECTION("export") {
            r->begin_transaction();
            REQUIRE_THROWS(r->package_for_handover({}));
        }
        SECTION("import") {
            auto h = r->package_for_handover({});
            r->begin_transaction();
            REQUIRE_THROWS(r->accept_handover(std::move(h)));
        }
    }

    SECTION("cleanup properly unpins version") {
        auto history = realm::make_client_history(config.path, config.encryption_key.data());
        SharedGroup shared_group(*history, SharedGroup::durability_MemOnly);

        auto get_current_version = [&]() -> SharedGroup::VersionID {
            shared_group.begin_read();
            auto version = shared_group.get_version_of_current_transaction();
            shared_group.end_read();
            return version;
        };

        auto handover_version = get_current_version();
        auto h = util::make_optional(r->package_for_handover({}));
        r->begin_transaction(); r->commit_transaction(); // Advance version

        REQUIRE(get_current_version() != handover_version); // Ensure advanced
        REQUIRE_NOTHROW(shared_group.begin_read(handover_version)); shared_group.end_read(); // Ensure pinned

        SECTION("destroyed without being imported") {
            h = {}; // Destroy handover, unpinning version
        }
        SECTION("exception thrown on import") {
            r->begin_transaction(); // Get into invalid state for accepting handover
            REQUIRE_THROWS(r->accept_handover(std::move(*h)));
            r->commit_transaction();
        }
        r->begin_transaction(); r->commit_transaction(); // Clean up old versions
        REQUIRE_THROWS(shared_group.begin_read(handover_version)); // Ensure unpinned
    }

    SECTION("version mismatch") {
        SECTION("import into older version") {
            r->begin_transaction();
            Object num = create_object(r, int_object);
            num.row().set_int(0, 7);
            r->commit_transaction();

            REQUIRE(num.row().get_int(0) == 7);
            auto h = std::async([config]() -> auto {
                SharedRealm r = Realm::get_shared_realm(config);
                auto results = Results(r, get_table(*r, int_object)->where());
                REQUIRE(results.size() == 1);
                Object num = Object(r, int_object, results.get(0));
                REQUIRE(num.row().get_int(0) == 7);

                r->begin_transaction();
                num.row().set_int(0, 9);
                r->commit_transaction();
                REQUIRE(num.row().get_int(0) == 9);

                return r->package_for_handover({{{num}}});
            }).get();
            REQUIRE(num.row().get_int(0) == 7);
            auto h_import = r->accept_handover(std::move(h));
            Object num_import = h_import[0].get_object();
            REQUIRE(num_import.row().get_int(0) == 9);
            REQUIRE(num.row().get_int(0) == 9);
            r->begin_transaction();
            num.row().set_int(0, 11);
            r->commit_transaction();
            REQUIRE(num_import.row().get_int(0) == 11);
            REQUIRE(num.row().get_int(0) == 11);
        }
        SECTION("import into newer version") {
            r->begin_transaction();
            Object num = create_object(r, int_object);
            num.row().set_int(0, 7);
            r->commit_transaction();

            REQUIRE(num.row().get_int(0) == 7);
            auto h = r->package_for_handover({{{num}}});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto results = Results(r, get_table(*r, int_object)->where());
                REQUIRE(results.size() == 1);
                Object num = Object(r, int_object, results.get(0));
                REQUIRE(num.row().get_int(0) == 7);

                r->begin_transaction();
                num.row().set_int(0, 9);
                r->commit_transaction();
                REQUIRE(num.row().get_int(0) == 9);

                auto h_import = r->accept_handover(std::move(h));
                Object num_import = h_import[0].get_object();
                REQUIRE(num_import.row().get_int(0) == 9);
                r->begin_transaction();
                num_import.row().set_int(0, 11);
                r->commit_transaction();
                REQUIRE(num.row().get_int(0) == 11);
                REQUIRE(num_import.row().get_int(0) == 11);
            }).join();
            REQUIRE(num.row().get_int(0) == 7);
            r->refresh();
            REQUIRE(num.row().get_int(0) == 11);
        }
        SECTION("import multiple versions") {
            auto commit_new_num = [&](int value) -> Object {
                r->begin_transaction();
                Object num = create_object(r, int_object);
                num.row().set_int(0, value);
                r->commit_transaction();
                return num;
            };

            auto h1 = r->package_for_handover({{{commit_new_num(1)}}});
            auto h2 = r->package_for_handover({{{commit_new_num(2)}}});
            std::thread([h1{std::move(h1)}, h2{std::move(h2)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h2_import = r->accept_handover(std::move(h2));
                auto h1_import = r->accept_handover(std::move(h1));

                auto num1 = h1_import[0].get_object();
                auto num2 = h2_import[0].get_object();

                REQUIRE(num1.row().get_int(0) == 1);
                REQUIRE(num2.row().get_int(0) == 2);
            }).join();
        }
    }

    SECTION("same thread") {
        r->begin_transaction();
        Object num = create_object(r, int_object);
        num.row().set_int(0, 7);
        r->commit_transaction();

        REQUIRE(num.row().get_int(0) == 7);
        auto h = r->package_for_handover({{{num}}});
        SECTION("same realm") {
            {
                auto h_import = r->accept_handover(std::move(h));
                Object num = h_import[0].get_object();
                REQUIRE(num.row().get_int(0) == 7);
                r->begin_transaction();
                num.row().set_int(0, 9);
                r->commit_transaction();
                REQUIRE(num.row().get_int(0) == 9);
            }
            REQUIRE(num.row().get_int(0) == 9);
        }
        SECTION("different realm") {
            {
                config.cache = false;
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));
                Object num = h_import[0].get_object();
                REQUIRE(num.row().get_int(0) == 7);
                r->begin_transaction();
                num.row().set_int(0, 9);
                r->commit_transaction();
                REQUIRE(num.row().get_int(0) == 9);
            }
            REQUIRE(num.row().get_int(0) == 7);
        }
        r->refresh();
        REQUIRE(num.row().get_int(0) == 9);
    }

    SECTION("passing over") {
        SECTION("nothing") {
            r->begin_transaction();
            Object num = create_object(r, int_object);
            r->commit_transaction();

            auto results = Results(r, get_table(*r, int_object)->where());
            REQUIRE(results.size() == 1);
            auto h = r->package_for_handover({});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));

                auto results = Results(r, get_table(*r, int_object)->where());
                REQUIRE(results.size() == 1);
                r->begin_transaction();
                Object num = create_object(r, int_object);
                r->commit_transaction();
                REQUIRE(results.size() == 2);
            }).join();
            REQUIRE(results.size() == 1);
            r->refresh();
            REQUIRE(results.size() == 2);
        }

        SECTION("objects") {
            r->begin_transaction();
            Object str = create_object(r, string_object);
            Object num = create_object(r, int_object);
            r->commit_transaction();

            REQUIRE(str.row().get_string(0).is_null());
            REQUIRE(num.row().get_int(0) == 0);
            auto h = r->package_for_handover({{str}, {num}});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));
                Object str = h_import[0].get_object();
                Object num = h_import[1].get_object();

                REQUIRE(str.row().get_string(0).is_null());
                REQUIRE(num.row().get_int(0) == 0);
                r->begin_transaction();
                str.row().set_string(0, "the meaning of life");
                num.row().set_int(0, 42);
                r->commit_transaction();
                REQUIRE(str.row().get_string(0) == "the meaning of life");
                REQUIRE(num.row().get_int(0) == 42);
            }).join();

            REQUIRE(str.row().get_string(0).is_null());
            REQUIRE(num.row().get_int(0) == 0);
            r->refresh();
            REQUIRE(str.row().get_string(0) == "the meaning of life");
            REQUIRE(num.row().get_int(0) == 42);
        }

        SECTION("array") {
            r->begin_transaction();
            Object zero = create_object(r, int_object);
            zero.row().set_int(0, 0);
            List lst = get_list(create_object(r, int_array_object), 0);
            lst.add(zero.row().get_index());
            r->commit_transaction();

            REQUIRE(lst.size() == 1);
            REQUIRE(lst.get(0).get_int(0) == 0);
            auto h = r->package_for_handover({{lst}});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));
                List lst = h_import[0].get_list();

                REQUIRE(lst.size() == 1);
                REQUIRE(lst.get(0).get_int(0) == 0);
                r->begin_transaction();
                lst.remove_all();
                Object one = create_object(r, int_object);
                one.row().set_int(0, 1);
                lst.add(one.row().get_index());
                Object two = create_object(r, int_object);
                two.row().set_int(0, 2);
                lst.add(two.row().get_index());
                r->commit_transaction();
                REQUIRE(lst.size() == 2);
                REQUIRE(lst.get(0).get_int(0) == 1);
                REQUIRE(lst.get(1).get_int(0) == 2);
            }).join();

            REQUIRE(lst.size() == 1);
            REQUIRE(lst.get(0).get_int(0) == 0);
            r->refresh();
            REQUIRE(lst.size() == 2);
            REQUIRE(lst.get(0).get_int(0) == 1);
            REQUIRE(lst.get(1).get_int(0) == 2);
        }

        SECTION("results") {
            auto& table = *get_table(*r, string_object);
            auto results = Results(r, table.where().not_equal(0, "C")).sort({table, {{0}}, {false}});

            r->begin_transaction();
            Object strA = create_object(r, string_object);
            strA.row().set_string(0, "A");
            Object strB = create_object(r, string_object);
            strB.row().set_string(0, "B");
            Object strC = create_object(r, string_object);
            strC.row().set_string(0, "C");
            Object strD = create_object(r, string_object);
            strD.row().set_string(0, "D");
            r->commit_transaction();

            REQUIRE(results.size() == 3);
            REQUIRE(results.get(0).get_string(0) == "D");
            REQUIRE(results.get(1).get_string(0) == "B");
            REQUIRE(results.get(2).get_string(0) == "A");
            auto h = r->package_for_handover({{results}});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));
                Results results = h_import[0].get_results();

                REQUIRE(results.size() == 3);
                REQUIRE(results.get(0).get_string(0) == "D");
                REQUIRE(results.get(1).get_string(0) == "B");
                REQUIRE(results.get(2).get_string(0) == "A");
                r->begin_transaction();
                results.get(2).move_last_over();
                results.get(0).move_last_over();
                Object strE = create_object(r, string_object);
                strE.row().set_string(0, "E");
                r->commit_transaction();
                REQUIRE(results.size() == 2);
                REQUIRE(results.get(0).get_string(0) == "E");
                REQUIRE(results.get(1).get_string(0) == "B");
            }).join();

            REQUIRE(results.size() == 3);
            REQUIRE(results.get(0).get_string(0) == "D");
            REQUIRE(results.get(1).get_string(0) == "B");
            REQUIRE(results.get(2).get_string(0) == "A");
            r->refresh();
            REQUIRE(results.size() == 2);
            REQUIRE(results.get(0).get_string(0) == "E");
            REQUIRE(results.get(1).get_string(0) == "B");
        }

        SECTION("multiple types") {
            auto results = Results(r, get_table(*r, int_object)->where().equal(0, 5));

            r->begin_transaction();
            Object num = create_object(r, int_object);
            num.row().set_int(0, 5);
            List lst = get_list(create_object(r, int_array_object), 0);
            r->commit_transaction();

            REQUIRE(lst.size() == 0);
            REQUIRE(results.size() == 1);
            REQUIRE(results.get(0).get_int(0) == 5);
            auto h = r->package_for_handover({{num}, {lst}, {results}});
            std::thread([h{std::move(h)}, config]() mutable {
                SharedRealm r = Realm::get_shared_realm(config);
                auto h_import = r->accept_handover(std::move(h));
                Object num = h_import[0].get_object();
                List lst = h_import[1].get_list();
                Results results = h_import[2].get_results();

                REQUIRE(lst.size() == 0);
                REQUIRE(results.size() == 1);
                REQUIRE(results.get(0).get_int(0) == 5);
                r->begin_transaction();
                num.row().set_int(0, 6);
                lst.add(num.row().get_index());
                r->commit_transaction();
                REQUIRE(lst.size() == 1);
                REQUIRE(lst.get(0).get_int(0) == 6);
                REQUIRE(results.size() == 0);
            }).join();

            REQUIRE(lst.size() == 0);
            REQUIRE(results.size() == 1);
            REQUIRE(results.get(0).get_int(0) == 5);
            r->refresh();
            REQUIRE(lst.size() == 1);
            REQUIRE(lst.get(0).get_int(0) == 6);
            REQUIRE(results.size() == 0);
        }
    }

    SECTION("lifetime") {
        SECTION("retains source realm") { // else version will become unpinned
            auto h = r->package_for_handover({});
            r = nullptr;
            r = Realm::get_shared_realm(config);
            REQUIRE_NOTHROW(r->accept_handover(std::move(h)));
        }
    }

    SECTION("metadata") {
        r->begin_transaction();
        Object num = create_object(r, int_object);
        r->commit_transaction();
        REQUIRE(num.get_object_schema().name == "int_object");
        auto h = r->package_for_handover({{num}});
        std::thread([h{std::move(h)}, config]() mutable {
            SharedRealm r = Realm::get_shared_realm(config);
            auto h_import = r->accept_handover(std::move(h));
            Object num = h_import[0].get_object();
            REQUIRE(num.get_object_schema().name == "int_object");
        }).join();
    }

    SECTION("disallow multiple imports") {
        auto h = r->package_for_handover({});
        r->accept_handover(std::move(h));
        REQUIRE_THROWS(r->accept_handover(std::move(h)));
    }
}
