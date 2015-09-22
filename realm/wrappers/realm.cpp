#include <iostream>
#include <realm.hpp>
#include <realm/table.hpp>
#include <realm/commit_log.hpp>
#include <realm/lang_bind_helper.hpp>
#include "object-store/shared_realm.hpp"


enum class RealmErrorType {
    unknown = 0,
    system = 1
};

struct RealmError {
    RealmErrorType type;
    std::string message;
};

static
void convert_exception_to_error(RealmError* error)
{
    try {
        throw;
    }
    catch (const std::exception& ex) {
        error->type = RealmErrorType::system;
        error->message = ex.what();
    }
    catch (...) {
        error->type = RealmErrorType::unknown;
        error->message = "Unknown exception thrown";
    }
}

template <class T>
struct Default {
    static T default_value() {
        return T{};
    }
};
template <>
struct Default<void> {
    static void default_value() {}
};

template <class F>
auto handle_errors(RealmError** out_error, F&& func) -> decltype(func())
{
    using RetVal = decltype(func());
    try {
        return func();
    }
    catch (...) {
        *out_error = new RealmError;
        convert_exception_to_error(*out_error);
        return Default<RetVal>::default_value();
    }
}

extern "C" {

using namespace realm;

void realm_error_destroy(RealmError* error)
{
    delete error;
}

void realm_error_get_message(const RealmError* error, char const** out_message, size_t* out_message_size)
{
    *out_message = error->message.c_str();
    *out_message_size = error->message.size();
}

RealmErrorType realm_error_get_type(const RealmError* error)
{
    return error->type;
}

Schema* realm_schema_new()
{
    return new Schema;
}

ObjectSchema* realm_object_schema_new(const char* name)
{
    auto p = new ObjectSchema;
    p->name = name;
    return p;
}

void realm_object_schema_add_property(ObjectSchema* cls, const char* name, DataType type, const char* object_type,
                                      bool is_primary, bool is_indexed, bool is_nullable)
{
    Property p;
    p.name = name;
    p.type = static_cast<PropertyType>(type);
    p.object_type = object_type ? std::string(object_type) : std::string();
    p.is_primary = is_primary;
    p.is_indexed = is_indexed;
    p.is_nullable = is_nullable;
    cls->properties.push_back(std::move(p));
}


void realm_schema_add_class(Schema* schema, const char* name, ObjectSchema* cls)
{
    (*schema)[name] = *cls;
}

SharedRealm* realm_open(RealmError** err, Schema* schema, const char* path, bool read_only, SharedGroup::DurabilityLevel durability,
                        const char* encryption_key)
{
    Realm::Config config;
    config.path = path;
    config.read_only = read_only;
    config.in_memory = durability != SharedGroup::durability_Full;
    config.encryption_key = encryption_key;
    config.schema.reset(schema);
    return new SharedRealm{Realm::get_shared_realm(config)};
}

void realm_destroy(SharedRealm* realm)
{
    delete realm;
}

bool realm_has_table(SharedRealm* realm, const char* name)
{
    Group* g = (*realm)->read_group();
    return g->has_table(name);
}

void realm_begin_transaction(RealmError** err, SharedRealm* realm)
{
    handle_errors(err, [&]() {
        (*realm)->begin_transaction();
    });
}

void realm_commit_transaction(RealmError** err, SharedRealm* realm)
{
    handle_errors(err, [&]() {
        (*realm)->commit_transaction();
    });
}

void realm_cancel_transaction(RealmError** err, SharedRealm* realm)
{
    handle_errors(err, [&]() {
        (*realm)->cancel_transaction();
    });
}

bool realm_is_in_transaction(SharedRealm* realm)
{
    return (*realm)->is_in_transaction();
}

bool realm_refresh(SharedRealm* realm)
{
    return (*realm)->refresh();
}

void realm_shared_group_promote_to_write(RealmError** err, SharedGroup* sg, ClientHistory* hist)
{
    handle_errors(err, [&]() {
	LangBindHelper::promote_to_write(*sg, *hist);
    });
}

SharedGroup::version_type realm_shared_group_commit(RealmError** err, SharedGroup* sg)
{
    return handle_errors(err, [&]() {
        return sg->commit();
    });
}

void realm_shared_group_commit_and_continue_as_read(RealmError** err,
									 SharedGroup* sg)
{
    LangBindHelper::commit_and_continue_as_read(*sg);
}

void realm_shared_group_rollback(SharedGroup* sg)
{
    sg->rollback();
}

void realm_shared_group_rollback_and_continue_as_read(RealmError** err, SharedGroup* sg, ClientHistory* hist)
{
    handle_errors(err, [&]() {
	LangBindHelper::rollback_and_continue_as_read(*sg, *hist);
    });
}

void realm_table_retain(const Table* table)
{
    LangBindHelper::bind_table_ptr(table);
}

void realm_table_release(const Table* table)
{
    LangBindHelper::unbind_table_ptr(table);
}

const Table* realm_get_table(RealmError** err, SharedGroup* sg, const char* name)
{
    using sgf = _impl::SharedGroupFriend;
    return handle_errors(err, [&]() {
        auto& group = sgf::get_group(*sg);
        return LangBindHelper::get_table(group, name);
    });
}

Table* realm_get_table_mut(RealmError** err, SharedGroup* sg, const char* name)
{
    using sgf = _impl::SharedGroupFriend;
    return handle_errors(err, [&]() {
        auto& group = sgf::get_group(*sg);
        return LangBindHelper::get_table(group, name);
    });
}

Table* realm_add_table(RealmError** err, SharedGroup* sg, const char* name)
{
    using sgf = _impl::SharedGroupFriend;
    return handle_errors(err, [&]() {
        auto& group = sgf::get_group(*sg);
        return LangBindHelper::add_table(group, name);
    });
}

size_t realm_table_add_column(RealmError** err, Table* table, DataType type, const char* name, bool nullable)
{
    return handle_errors(err, [&]() {
        return table->add_column(type, name, nullable);
    });
}

size_t realm_table_add_empty_row(RealmError** err, Table* table)
{
    return handle_errors(err, [&]() {
        return table->add_empty_row();
    });
}

int64_t realm_table_get_int(const Table* table, size_t col_ndx, size_t row_ndx)
{
    return table->get_int(col_ndx, row_ndx);
}

void realm_table_set_int(RealmError** err, Table* table, size_t col_ndx, size_t row_ndx, int64_t value)
{
    return handle_errors(err, [&]() {
        table->set_int(col_ndx, row_ndx, value);
    });
}

void realm_row_destroy(Table::RowExpr* expr) {
    delete expr;
}

int64_t realm_row_get_int(const Table::RowExpr* expr, size_t col_ndx) {
    return expr->get_int(col_ndx);
}

}
