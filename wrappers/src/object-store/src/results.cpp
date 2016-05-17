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

#include "results.hpp"

#include "impl/realm_coordinator.hpp"
#include "impl/results_notifier.hpp"
#include "object_store.hpp"

#include <stdexcept>

using namespace realm;

#ifdef __has_cpp_attribute
#define REALM_HAS_CCP_ATTRIBUTE(attr) __has_cpp_attribute(attr)
#else
#define REALM_HAS_CCP_ATTRIBUTE(attr) 0
#endif

#if REALM_HAS_CCP_ATTRIBUTE(clang::fallthrough)
#define REALM_FALLTHROUGH [[clang::fallthrough]]
#else
#define REALM_FALLTHROUGH
#endif

Results::Results(SharedRealm r, const ObjectSchema &o, Query q, SortOrder s)
: m_realm(std::move(r))
, m_object_schema(&o)
, m_query(std::move(q))
, m_table(m_query.get_table().get())
, m_sort(std::move(s))
, m_mode(Mode::Query)
{
    REALM_ASSERT(m_sort.column_indices.size() == m_sort.ascending.size());
}

Results::Results(SharedRealm r, const ObjectSchema &o, Table& table)
: m_realm(std::move(r))
, m_object_schema(&o)
, m_table(&table)
, m_mode(Mode::Table)
{
}

Results::Results(SharedRealm r, const ObjectSchema& o, LinkViewRef lv, util::Optional<Query> q, SortOrder s)
: m_realm(std::move(r))
, m_object_schema(&o)
, m_link_view(lv)
, m_table(&lv->get_target_table())
, m_sort(std::move(s))
, m_mode(Mode::LinkView)
{
    REALM_ASSERT(m_sort.column_indices.size() == m_sort.ascending.size());
    if (q) {
        m_query = std::move(*q);
        m_mode = Mode::Query;
    }
}

Results::~Results()
{
    if (m_notifier) {
        m_notifier->unregister();
    }
}

void Results::validate_read() const
{
    if (m_realm)
        m_realm->verify_thread();
    if (m_table && !m_table->is_attached())
        throw InvalidatedException();
    if (m_mode == Mode::TableView && !m_table_view.is_attached())
        throw InvalidatedException();
    if (m_mode == Mode::LinkView && !m_link_view->is_attached())
        throw InvalidatedException();
}

void Results::validate_write() const
{
    validate_read();
    if (!m_realm || !m_realm->is_in_transaction())
        throw InvalidTransactionException("Must be in a write transaction");
}

void Results::set_live(bool live)
{
    if (!live && m_mode == Mode::Table) {
        m_query = m_table->where();
        m_mode = Mode::Query;
    }

    update_tableview();
    m_live = live;
}

size_t Results::size()
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:    return 0;
        case Mode::Table:    return m_table->size();
        case Mode::Query:    return m_query.count();
        case Mode::LinkView: return m_link_view->size();
        case Mode::TableView:
            update_tableview();
            return m_table_view.size();
    }
    REALM_UNREACHABLE();
}

StringData Results::get_object_type() const noexcept
{
    return get_object_schema().name;
}

RowExpr Results::get(size_t row_ndx)
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty: break;
        case Mode::Table:
            if (row_ndx < m_table->size())
                return m_table->get(row_ndx);
            break;
        case Mode::LinkView:
            if (update_linkview()) {
               if (row_ndx < m_link_view->size())
                   return m_link_view->get(row_ndx);
                break;
            }
            REALM_FALLTHROUGH;
        case Mode::Query:
        case Mode::TableView:
            update_tableview();
            if (row_ndx >= m_table_view.size())
                break;
            if (!m_live && !m_table_view.is_row_attached(row_ndx))
                return {};
            return m_table_view.get(row_ndx);
    }

    throw OutOfBoundsIndexException{row_ndx, size()};
}

util::Optional<RowExpr> Results::first()
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
            return none;
        case Mode::Table:
            return m_table->size() == 0 ? util::none : util::make_optional(m_table->front());
        case Mode::LinkView:
            if (update_linkview())
                return m_link_view->size() == 0 ? util::none : util::make_optional(m_link_view->get(0));
            REALM_FALLTHROUGH;
        case Mode::Query:
        case Mode::TableView:
            update_tableview();
            return m_table_view.size() == 0 ? util::none : util::make_optional(m_table_view.front());
    }
    REALM_UNREACHABLE();
}

util::Optional<RowExpr> Results::last()
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
            return none;
        case Mode::Table:
            return m_table->size() == 0 ? util::none : util::make_optional(m_table->back());
        case Mode::LinkView:
            if (update_linkview())
                return m_link_view->size() == 0 ? util::none : util::make_optional(m_link_view->get(m_link_view->size() - 1));
            REALM_FALLTHROUGH;
        case Mode::Query:
        case Mode::TableView:
            update_tableview();
            return m_table_view.size() == 0 ? util::none : util::make_optional(m_table_view.back());
    }
    REALM_UNREACHABLE();
}

bool Results::update_linkview()
{
    if (m_sort) {
        m_query = get_query();
        m_mode = Mode::Query;
        update_tableview();
        return false;
    }
    return true;
}

void Results::update_tableview()
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
        case Mode::Table:
        case Mode::LinkView:
            return;
        case Mode::Query:
            m_table_view = m_query.find_all();
            if (m_sort) {
                m_table_view.sort(m_sort.column_indices, m_sort.ascending);
            }
            m_mode = Mode::TableView;
            break;
        case Mode::TableView:
            if (!m_live) {
                return;
            }
            if (!m_notifier && !m_realm->is_in_transaction() && m_realm->can_deliver_notifications()) {
                m_notifier = std::make_shared<_impl::ResultsNotifier>(*this);
                _impl::RealmCoordinator::register_notifier(m_notifier);
            }
            m_has_used_table_view = true;
            m_table_view.sync_if_needed();
            break;
    }
}

size_t Results::index_of(Row const& row)
{
    validate_read();
    if (!row) {
        throw DetatchedAccessorException{};
    }
    if (m_table && row.get_table() != m_table) {
        throw IncorrectTableException(m_object_schema->name,
            ObjectStore::object_type_for_table_name(row.get_table()->get_name()),
            "Attempting to get the index of a Row of the wrong type"
        );
    }
    return index_of(row.get_index());
}

size_t Results::index_of(size_t row_ndx)
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
            return not_found;
        case Mode::Table:
            return row_ndx;
        case Mode::LinkView:
            if (update_linkview())
                return m_link_view->find(row_ndx);
            REALM_FALLTHROUGH;
        case Mode::Query:
        case Mode::TableView:
            update_tableview();
            return m_table_view.find_by_source_ndx(row_ndx);
    }
    REALM_UNREACHABLE();
}

template<typename Int, typename Float, typename Double, typename DateTime>
util::Optional<Mixed> Results::aggregate(size_t column, bool return_none_for_empty,
                                         Int agg_int, Float agg_float,
                                         Double agg_double, DateTime agg_datetime)
{
    validate_read();
    if (!m_table)
        return none;
    if (column > m_table->get_column_count())
        throw OutOfBoundsIndexException{column, m_table->get_column_count()};

    auto do_agg = [&](auto const& getter) -> util::Optional<Mixed> {
        switch (m_mode) {
            case Mode::Empty:
                return none;
            case Mode::Table:
                if (return_none_for_empty && m_table->size() == 0)
                    return none;
                return util::Optional<Mixed>(getter(*m_table));
            case Mode::LinkView:
                m_query = this->get_query();
                m_mode = Mode::Query;
                REALM_FALLTHROUGH;
            case Mode::Query:
            case Mode::TableView:
                this->update_tableview();
                if (return_none_for_empty && m_table_view.size() == 0)
                    return none;
                return util::Optional<Mixed>(getter(m_table_view));
        }
        REALM_UNREACHABLE();
    };

    switch (m_table->get_column_type(column))
    {
        case type_DateTime: return do_agg(agg_datetime);
        case type_Double: return do_agg(agg_double);
        case type_Float: return do_agg(agg_float);
        case type_Int: return do_agg(agg_int);
        default:
            throw UnsupportedColumnTypeException{column, m_table};
    }
}

util::Optional<Mixed> Results::max(size_t column)
{
    return aggregate(column, true,
                     [=](auto const& table) { return table.maximum_int(column); },
                     [=](auto const& table) { return table.maximum_float(column); },
                     [=](auto const& table) { return table.maximum_double(column); },
                     [=](auto const& table) { return table.maximum_datetime(column); });
}

util::Optional<Mixed> Results::min(size_t column)
{
    return aggregate(column, true,
                     [=](auto const& table) { return table.minimum_int(column); },
                     [=](auto const& table) { return table.minimum_float(column); },
                     [=](auto const& table) { return table.minimum_double(column); },
                     [=](auto const& table) { return table.minimum_datetime(column); });
}

util::Optional<Mixed> Results::sum(size_t column)
{
    return aggregate(column, false,
                     [=](auto const& table) { return table.sum_int(column); },
                     [=](auto const& table) { return table.sum_float(column); },
                     [=](auto const& table) { return table.sum_double(column); },
                     [=](auto const&) -> util::None { throw UnsupportedColumnTypeException{column, m_table}; });
}

util::Optional<Mixed> Results::average(size_t column)
{
    return aggregate(column, true,
                     [=](auto const& table) { return table.average_int(column); },
                     [=](auto const& table) { return table.average_float(column); },
                     [=](auto const& table) { return table.average_double(column); },
                     [=](auto const&) -> util::None { throw UnsupportedColumnTypeException{column, m_table}; });
}

void Results::clear()
{
    switch (m_mode) {
        case Mode::Empty:
            return;
        case Mode::Table:
            validate_write();
            m_table->clear();
            break;
        case Mode::Query:
            // Not using Query:remove() because building the tableview and
            // clearing it is actually significantly faster
        case Mode::TableView:
            validate_write();
            update_tableview();
            m_table_view.clear(RemoveMode::unordered);
            break;
        case Mode::LinkView:
            validate_write();
            m_link_view->remove_all_target_rows();
            break;
    }
}

Query Results::get_query() const
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
        case Mode::Query:
            return m_query;
        case Mode::TableView:
            return m_table_view.get_query();
        case Mode::LinkView:
            return m_table->where(m_link_view);
        case Mode::Table:
            return m_table->where();
    }
    REALM_UNREACHABLE();
}

TableView Results::get_tableview()
{
    validate_read();
    switch (m_mode) {
        case Mode::Empty:
            return {};
        case Mode::LinkView:
            if (update_linkview())
                return m_table->where(m_link_view).find_all();
            REALM_FALLTHROUGH;
        case Mode::Query:
        case Mode::TableView:
            update_tableview();
            return m_table_view;
        case Mode::Table:
            return m_table->where().find_all();
    }
    REALM_UNREACHABLE();
}

Results Results::sort(realm::SortOrder&& sort) const
{
    REALM_ASSERT(sort.column_indices.size() == sort.ascending.size());
    if (m_link_view)
        return Results(m_realm, *m_object_schema, m_link_view, m_query, std::move(sort));
    return Results(m_realm, *m_object_schema, get_query(), std::move(sort));
}

Results Results::filter(Query&& q) const
{
    if (m_link_view)
        return Results(m_realm, *m_object_schema, m_link_view, get_query().and_query(std::move(q)), m_sort);
    return Results(m_realm, *m_object_schema, get_query().and_query(std::move(q)), m_sort);
}

void Results::prepare_async()
{
    if (m_realm->config().read_only) {
        throw InvalidTransactionException("Cannot create asynchronous query for read-only Realms");
    }
    if (m_realm->is_in_transaction()) {
        throw InvalidTransactionException("Cannot create asynchronous query while in a write transaction");
    }

    if (!m_notifier) {
        m_notifier = std::make_shared<_impl::ResultsNotifier>(*this);
        _impl::RealmCoordinator::register_notifier(m_notifier);
    }
}

NotificationToken Results::async(std::function<void (std::exception_ptr)> target)
{
    prepare_async();
    auto wrap = [=](CollectionChangeSet, std::exception_ptr e) { target(e); };
    return {m_notifier, m_notifier->add_callback(wrap)};
}

NotificationToken Results::add_notification_callback(CollectionChangeCallback cb)
{
    prepare_async();
    return {m_notifier, m_notifier->add_callback(std::move(cb))};
}

void Results::Internal::set_table_view(Results& results, realm::TableView &&tv)
{
    // If the previous TableView was never actually used, then stop generating
    // new ones until the user actually uses the Results object again
    if (results.m_mode == Mode::TableView) {
        results.m_wants_background_updates = results.m_has_used_table_view;
    }

    results.m_table_view = std::move(tv);
    results.m_mode = Mode::TableView;
    results.m_has_used_table_view = false;
    REALM_ASSERT(results.m_table_view.is_in_sync());
    REALM_ASSERT(results.m_table_view.is_attached());
}

Results::UnsupportedColumnTypeException::UnsupportedColumnTypeException(size_t column, const Table* table)
: std::runtime_error((std::string)"Operation not supported on '" + table->get_column_name(column).data() + "' columns")
, column_index(column)
, column_name(table->get_column_name(column))
, column_type(table->get_column_type(column))
{
}
