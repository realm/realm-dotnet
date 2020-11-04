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

#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_user.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include <realm/object-store/sync/app.hpp>
#include <realm/object-store/sync/mongo_client.hpp>
#include <realm/object-store/sync/mongo_database.hpp>
#include <realm/object-store/sync/mongo_collection.hpp>
#include "app_cs.hpp"

using namespace realm;
using namespace realm::binding;
using namespace app;

using SharedSyncUser = std::shared_ptr<SyncUser>;

struct FindAndModifyOptions
{
    uint16_t* projection_buf;
    size_t projection_len;
    uint16_t* sort_buf;
    size_t sort_len;

    bool upsert;
    bool return_new_document;
    int64_t limit;

    MongoCollection::FindOneAndModifyOptions to_find_and_modify_options() {
        MongoCollection::FindOneAndModifyOptions options;
        
        if (projection_buf != nullptr) {
            options.projection_bson = to_document(projection_buf, projection_len);
        }

        if (sort_buf != nullptr) {
            options.sort_bson = to_document(sort_buf, sort_len);
        }

        options.upsert = upsert;
        options.return_new_document = return_new_document;

        return options;
    }

    MongoCollection::FindOptions to_find_options() {
        MongoCollection::FindOptions options;

        if (projection_buf != nullptr) {
            options.projection_bson = to_document(projection_buf, projection_len);
        }

        if (sort_buf != nullptr) {
            options.sort_bson = to_document(sort_buf, sort_len);
        }

        if (limit != 0) {
            options.limit = limit;
        }

        return options;
    }
};

extern "C" {
    REALM_EXPORT MongoCollection* realm_mongo_collection_get(SharedSyncUser& user,
            uint16_t* service_buf, size_t service_len,
            uint16_t* database_buf, size_t database_len,
            uint16_t* collection_buf, size_t collection_len,
            NativeException::Marshallable& ex) {
        return handle_errors(ex, [&]() {
            Utf16StringAccessor service(service_buf, service_len);
            Utf16StringAccessor database(database_buf, database_len);
            Utf16StringAccessor collection(collection_buf, collection_len);
            
            auto col = user->mongo_client(service).db(database).collection(collection);
            return new MongoCollection(col);
        });
    }

    REALM_EXPORT void realm_mongo_collection_destroy(MongoCollection* collection)
    {
        delete collection;
    }

    REALM_EXPORT void realm_mongo_collection_find(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, FindAndModifyOptions options, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.find_bson(filter, options.to_find_options(), get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_find_one(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, FindAndModifyOptions options, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.find_one_bson(filter, options.to_find_options(), get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_aggregate(MongoCollection& collection, uint16_t* pipeline_buf, size_t pipeline_len, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto pipeline = to_array(pipeline_buf, pipeline_len);
            collection.aggregate_bson(pipeline, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_count(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, int64_t limit, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.count_bson(filter, limit, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_insert_one(MongoCollection& collection, uint16_t* doc_buf, size_t doc_len, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto doc = to_document(doc_buf, doc_len);
            collection.insert_one_bson(doc, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_insert_many(MongoCollection& collection, uint16_t* docs_buf, size_t docs_len, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto docs = to_array(docs_buf, docs_len);
            collection.insert_many_bson(docs, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_delete_one(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.delete_one_bson(filter, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_delete_many(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.delete_many_bson(filter, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_update_one(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, uint16_t* doc_buf, size_t doc_len, bool upsert, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            auto doc = to_document(doc_buf, doc_len);
            collection.update_one_bson(filter, doc, upsert, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_update_many(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, uint16_t* doc_buf, size_t doc_len, bool upsert, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            auto doc = to_document(doc_buf, doc_len);
            collection.update_many_bson(filter, doc, upsert, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_find_one_and_update(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, uint16_t* doc_buf, size_t doc_len, FindAndModifyOptions options, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            auto doc = to_document(doc_buf, doc_len);
            collection.find_one_and_update_bson(filter, doc, options.to_find_and_modify_options(), get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_find_one_and_replace(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, uint16_t* doc_buf, size_t doc_len, FindAndModifyOptions options, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            auto doc = to_document(doc_buf, doc_len);
            collection.find_one_and_replace_bson(filter, doc, options.to_find_and_modify_options(), get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_mongo_collection_find_one_and_delete(MongoCollection& collection, uint16_t* filter_buf, size_t filter_len, FindAndModifyOptions options, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto filter = to_document(filter_buf, filter_len);
            collection.find_one_and_delete_bson(filter, options.to_find_and_modify_options(), get_bson_callback_handler(tcs_ptr));
        });
    }
}
