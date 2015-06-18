/*************************************************************************
 *
 * REALM CONFIDENTIAL
 * __________________
 *
 *  [2011] - [2012] Realm Inc
 *  All Rights Reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Realm Incorporated and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to Realm Incorporated
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Realm Incorporated.
 *
 **************************************************************************/
#ifndef REALM_COLUMN_TABLE_HPP
#define REALM_COLUMN_TABLE_HPP

#include <vector>

#include <realm/util/features.h>
#include <memory>
#include <realm/column.hpp>
#include <realm/table.hpp>

namespace realm {


/// Base class for any type of column that can contain subtables.
class ColumnSubtableParent: public Column, public Table::Parent {
public:
    void discard_child_accessors() REALM_NOEXCEPT;

    ~ColumnSubtableParent() REALM_NOEXCEPT override;

    static ref_type create(Allocator&, std::size_t size = 0);

    Table* get_subtable_accessor(std::size_t) const REALM_NOEXCEPT override;

    void insert(std::size_t, std::size_t, bool) override;
    void erase(std::size_t, bool) override;
    void move_last_over(std::size_t, std::size_t, bool) override;
    void clear(std::size_t, bool) override;
    void discard_subtable_accessor(std::size_t) REALM_NOEXCEPT override;
    void update_from_parent(std::size_t) REALM_NOEXCEPT override;
    void adj_acc_insert_rows(std::size_t, std::size_t) REALM_NOEXCEPT override;
    void adj_acc_erase_row(std::size_t) REALM_NOEXCEPT override;
    void adj_acc_move_over(std::size_t, std::size_t) REALM_NOEXCEPT override;
    void adj_acc_clear_root_table() REALM_NOEXCEPT override;
    void mark(int) REALM_NOEXCEPT override;
    void refresh_accessor_tree(std::size_t, const Spec&) override;

#ifdef REALM_DEBUG
    void Verify() const override;
    void Verify(const Table&, std::size_t) const override;
#endif

protected:
    /// A pointer to the table that this column is part of. For a free-standing
    /// column, this pointer is null.
    Table* const m_table;

    /// The index of this column within m_table.m_cols. For a free-standing
    /// column, this index is zero.
    std::size_t m_column_ndx;

    struct SubtableMap {
        ~SubtableMap() REALM_NOEXCEPT {}
        bool empty() const REALM_NOEXCEPT { return m_entries.empty(); }
        Table* find(std::size_t subtable_ndx) const REALM_NOEXCEPT;
        void add(std::size_t subtable_ndx, Table*);
        // Returns true if, and only if at least one entry was detached and
        // removed from the map.
        bool detach_and_remove_all() REALM_NOEXCEPT;
        // Returns true if, and only if the entry was found and removed, and it
        // was the last entry in the map.
        bool detach_and_remove(std::size_t subtable_ndx) REALM_NOEXCEPT;
        // Returns true if, and only if the entry was found and removed, and it
        // was the last entry in the map.
        bool remove(Table*) REALM_NOEXCEPT;
        void update_from_parent(std::size_t old_baseline) const REALM_NOEXCEPT;
        template<bool fix_ndx_in_parent>
        void adj_insert_rows(std::size_t row_ndx, std::size_t num_rows) REALM_NOEXCEPT;
        // Returns true if, and only if an entry was found and removed, and it
        // was the last entry in the map.
        template<bool fix_ndx_in_parent> bool adj_erase_row(std::size_t row_ndx) REALM_NOEXCEPT;
        // Returns true if, and only if an entry was found and removed, and it
        // was the last entry in the map.
        template<bool fix_ndx_in_parent>
        bool adj_move_over(std::size_t from_row_ndx, std::size_t to_row_ndx)
            REALM_NOEXCEPT;
        void update_accessors(const std::size_t* col_path_begin, const std::size_t* col_path_end,
                              _impl::TableFriend::AccessorUpdater&);
        void recursive_mark() REALM_NOEXCEPT;
        void refresh_accessor_tree(std::size_t spec_ndx_in_parent);
    private:
        struct entry {
            std::size_t m_subtable_ndx;
            Table* m_table;
        };
        typedef std::vector<entry> entries;
        entries m_entries;
    };

    /// Contains all existing accessors that are attached to a subtable in this
    /// column. It can map a row index into a pointer to the corresponding
    /// accessor when it exists.
    ///
    /// There is an invariant in force: Either `m_table` is null, or there is an
    /// additional referece count on `*m_table` when, and only when the map is
    /// non-empty.
    mutable SubtableMap m_subtable_map;

//    ColumnSubtableParent(Allocator&, Table*, std::size_t column_ndx);

    ColumnSubtableParent(Allocator&, ref_type, Table*, std::size_t column_ndx);

    /// Get a pointer to the accessor of the specified subtable. The
    /// accessor will be created if it does not already exist.
    ///
    /// The returned table pointer must **always** end up being
    /// wrapped in some instantiation of BasicTableRef<>.
    ///
    /// NOTE: This method must be used only for subtables with
    /// independent specs, i.e. for elements of a ColumnMixed.
    Table* get_subtable_ptr(std::size_t subtable_ndx);

    // Overriding method in ArrayParent
    void update_child_ref(std::size_t, ref_type) override;

    // Overriding method in ArrayParent
    ref_type get_child_ref(std::size_t) const REALM_NOEXCEPT override;

    // Overriding method in Table::Parent
    Table* get_parent_table(std::size_t*) REALM_NOEXCEPT override;

    // Overriding method in Table::Parent
    void child_accessor_destroyed(Table*) REALM_NOEXCEPT override;

    /// Assumes that the two tables have the same spec.
    static bool compare_subtable_rows(const Table&, const Table&);

    /// Construct a copy of the columns array of the specified table
    /// and return just the ref to that array.
    ///
    /// In the clone, no string column will be of the enumeration
    /// type.
    ref_type clone_table_columns(const Table*);

    std::size_t* record_subtable_path(std::size_t* begin,
                                      std::size_t* end) REALM_NOEXCEPT override;

    void update_table_accessors(const std::size_t* col_path_begin, const std::size_t* col_path_end,
                                _impl::TableFriend::AccessorUpdater&);

    /// \param row_ndx Must be `realm::npos` if appending.
    void do_insert(std::size_t row_ndx, int_fast64_t value, std::size_t num_rows);

#ifdef REALM_DEBUG
    std::pair<ref_type, std::size_t>
    get_to_dot_parent(std::size_t ndx_in_parent) const override;
#endif

    friend class Table;
};



class ColumnTable: public ColumnSubtableParent {
public:
    /// Create a subtable column accessor and attach it to a
    /// preexisting underlying structure of arrays.
    ///
    /// \param table If this column is used as part of a table you must
    /// pass a pointer to that table. Otherwise you must pass null.
    ///
    /// \param column_ndx If this column is used as part of a table
    /// you must pass the logical index of the column within that
    /// table. Otherwise you should pass zero.
    ColumnTable(Allocator&, ref_type, Table* table, std::size_t column_ndx);

    ~ColumnTable() REALM_NOEXCEPT override {}

    std::size_t get_subtable_size(std::size_t ndx) const REALM_NOEXCEPT;

    /// Get a pointer to the accessor of the specified subtable. The
    /// accessor will be created if it does not already exist.
    ///
    /// The returned table pointer must **always** end up being
    /// wrapped in some instantiation of BasicTableRef<>.
    Table* get_subtable_ptr(std::size_t subtable_ndx);

    const Table* get_subtable_ptr(std::size_t subtable_ndx) const;

    // When passing a table to add() or insert() it is assumed that
    // the table spec is compatible with this column. The number of
    // columns must be the same, and the corresponding columns must
    // have the same data type (as returned by
    // Table::get_column_type()).

    void add(const Table* value = 0);
    void insert(std::size_t ndx, const Table* value = 0);
    void set(std::size_t ndx, const Table*);
    void clear_table(std::size_t ndx);

    using ColumnSubtableParent::insert;

    void erase(std::size_t, bool) override;
    void move_last_over(std::size_t, std::size_t, bool) override;

    /// Compare two subtable columns for equality.
    bool compare_table(const ColumnTable&) const;

    void refresh_accessor_tree(std::size_t, const Spec&) override;

#ifdef REALM_DEBUG
    void Verify(const Table&, std::size_t) const override;
    void do_dump_node_structure(std::ostream&, int) const override;
    void to_dot(std::ostream&, StringData title) const override;
#endif

private:
    mutable std::size_t m_subspec_ndx; // Unknown if equal to `npos`

    std::size_t get_subspec_ndx() const REALM_NOEXCEPT;

    void destroy_subtable(std::size_t ndx) REALM_NOEXCEPT;

    void do_discard_child_accessors() REALM_NOEXCEPT override;
};





// Implementation

// Overriding virtual method of Column.
inline void ColumnSubtableParent::insert(std::size_t row_ndx, std::size_t num_rows, bool is_append)
{
    std::size_t row_ndx_2 = is_append ? realm::npos : row_ndx;
    int_fast64_t value = 0;
    do_insert(row_ndx_2, value, num_rows); // Throws
}

inline void ColumnSubtableParent::erase(std::size_t row_ndx, bool is_last)
{
    do_erase(row_ndx, is_last); // Throws

    const bool fix_ndx_in_parent = true;
    bool last_entry_removed = m_subtable_map.adj_erase_row<fix_ndx_in_parent>(row_ndx);
    typedef _impl::TableFriend tf;
    if (last_entry_removed)
        tf::unbind_ref(*m_table);
}

inline void ColumnSubtableParent::move_last_over(std::size_t row_ndx, std::size_t last_row_ndx,
                                                 bool)
{
    do_move_last_over(row_ndx, last_row_ndx); // Throws

    const bool fix_ndx_in_parent = true;
    bool last_entry_removed =
        m_subtable_map.adj_move_over<fix_ndx_in_parent>(last_row_ndx, row_ndx);
    typedef _impl::TableFriend tf;
    if (last_entry_removed)
        tf::unbind_ref(*m_table);
}

inline void ColumnSubtableParent::clear(std::size_t, bool)
{
    discard_child_accessors();
    do_clear(); // Throws
    // FIXME: This one is needed because Column::do_clear() forgets about the
    // leaf type. A better solution should probably be sought after.
    m_array->set_type(Array::type_HasRefs);
}

inline void ColumnSubtableParent::mark(int type) REALM_NOEXCEPT
{
    if (type & mark_Recursive)
        m_subtable_map.recursive_mark();
}

inline void ColumnSubtableParent::refresh_accessor_tree(std::size_t col_ndx, const Spec& spec)
{
    Column::refresh_accessor_tree(col_ndx, spec); // Throws
    m_column_ndx = col_ndx;
}

inline void ColumnSubtableParent::adj_acc_insert_rows(std::size_t row_ndx,
                                                      std::size_t num_rows) REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    const bool fix_ndx_in_parent = false;
    m_subtable_map.adj_insert_rows<fix_ndx_in_parent>(row_ndx, num_rows);
}

inline void ColumnSubtableParent::adj_acc_erase_row(std::size_t row_ndx) REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    const bool fix_ndx_in_parent = false;
    bool last_entry_removed = m_subtable_map.adj_erase_row<fix_ndx_in_parent>(row_ndx);
    typedef _impl::TableFriend tf;
    if (last_entry_removed)
        tf::unbind_ref(*m_table);
}

inline void ColumnSubtableParent::adj_acc_move_over(std::size_t from_row_ndx,
                                                    std::size_t to_row_ndx) REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    const bool fix_ndx_in_parent = false;
    bool last_entry_removed =
        m_subtable_map.adj_move_over<fix_ndx_in_parent>(from_row_ndx, to_row_ndx);
    typedef _impl::TableFriend tf;
    if (last_entry_removed)
        tf::unbind_ref(*m_table);
}

inline void ColumnSubtableParent::adj_acc_clear_root_table() REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    Column::adj_acc_clear_root_table();
    discard_child_accessors();
}

inline Table* ColumnSubtableParent::get_subtable_accessor(std::size_t row_ndx) const
    REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    Table* subtable = m_subtable_map.find(row_ndx);
    return subtable;
}

inline void ColumnSubtableParent::discard_subtable_accessor(std::size_t row_ndx) REALM_NOEXCEPT
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    bool last_entry_removed = m_subtable_map.detach_and_remove(row_ndx);
    typedef _impl::TableFriend tf;
    if (last_entry_removed)
        tf::unbind_ref(*m_table);
}

inline void ColumnSubtableParent::SubtableMap::add(std::size_t subtable_ndx, Table* table)
{
    entry e;
    e.m_subtable_ndx = subtable_ndx;
    e.m_table        = table;
    m_entries.push_back(e);
}

template<bool fix_ndx_in_parent>
void ColumnSubtableParent::SubtableMap::adj_insert_rows(std::size_t row_ndx, std::size_t num_rows)
    REALM_NOEXCEPT
{
    typedef entries::iterator iter;
    iter end = m_entries.end();
    for (iter i = m_entries.begin(); i != end; ++i) {
        if (i->m_subtable_ndx >= row_ndx) {
            i->m_subtable_ndx += num_rows;
            typedef _impl::TableFriend tf;
            if (fix_ndx_in_parent)
                tf::set_ndx_in_parent(*(i->m_table), i->m_subtable_ndx);
        }
    }
}

template<bool fix_ndx_in_parent>
bool ColumnSubtableParent::SubtableMap::adj_erase_row(std::size_t row_ndx) REALM_NOEXCEPT
{
    typedef _impl::TableFriend tf;
    typedef entries::iterator iter;
    iter end = m_entries.end();
    iter erase = end;
    for (iter i = m_entries.begin(); i != end; ++i) {
        if (i->m_subtable_ndx > row_ndx) {
            --i->m_subtable_ndx;
            if (fix_ndx_in_parent)
                tf::set_ndx_in_parent(*(i->m_table), i->m_subtable_ndx);
        }
        else if (i->m_subtable_ndx == row_ndx) {
            REALM_ASSERT(erase == end); // Subtable accessors are unique
            erase = i;
        }
    }
    if (erase == end)
        return false; // Not found, so nothing changed

    // Must hold a counted reference while detaching
    TableRef table(erase->m_table);
    tf::detach(*table);

    *erase = *--end; // Move last over
    m_entries.pop_back();
    return m_entries.empty();
}


template<bool fix_ndx_in_parent>
bool ColumnSubtableParent::SubtableMap::adj_move_over(std::size_t from_row_ndx,
                                                      std::size_t to_row_ndx) REALM_NOEXCEPT
{
    typedef _impl::TableFriend tf;

    std::size_t i = 0, n = m_entries.size();
    // We return true if, and only if we remove the last entry in the map.  We
    // need special handling for the case, where the set of entries are already
    // empty, otherwise the final return statement would return true in this
    // case, even though we didn't actually remove an entry.
    if (i == n)
        return false;

    while (i < n) {
        entry& e = m_entries[i];
        if (REALM_UNLIKELY(e.m_subtable_ndx == to_row_ndx)) {
            // Must hold a counted reference while detaching
            TableRef table(e.m_table);
            tf::detach(*table);
            // Delete entry by moving last over (faster and avoids invalidating
            // iterators)
            e = m_entries[--n];
            m_entries.pop_back();
        }
        else {
            if (REALM_UNLIKELY(e.m_subtable_ndx == from_row_ndx)) {
                e.m_subtable_ndx = to_row_ndx;
                if (fix_ndx_in_parent)
                    tf::set_ndx_in_parent(*(e.m_table), e.m_subtable_ndx);
            }
            ++i;
        }
    }
    return m_entries.empty();
}

inline ColumnSubtableParent::ColumnSubtableParent(Allocator& alloc, ref_type ref,
                                                  Table* table, std::size_t column_ndx):
    Column(alloc, ref), // Throws
    m_table(table),
    m_column_ndx(column_ndx)
{
}

inline void ColumnSubtableParent::update_child_ref(std::size_t child_ndx, ref_type new_ref)
{
    set(child_ndx, new_ref);
}

inline ref_type ColumnSubtableParent::get_child_ref(std::size_t child_ndx) const REALM_NOEXCEPT
{
    return get_as_ref(child_ndx);
}

inline void ColumnSubtableParent::discard_child_accessors() REALM_NOEXCEPT
{
    bool last_entry_removed = m_subtable_map.detach_and_remove_all();
    if (last_entry_removed && m_table)
        _impl::TableFriend::unbind_ref(*m_table);
}

inline ColumnSubtableParent::~ColumnSubtableParent() REALM_NOEXCEPT
{
    discard_child_accessors();
}

inline bool ColumnSubtableParent::compare_subtable_rows(const Table& a, const Table& b)
{
    return _impl::TableFriend::compare_rows(a,b);
}

inline ref_type ColumnSubtableParent::clone_table_columns(const Table* t)
{
    return _impl::TableFriend::clone_columns(*t, m_array->get_alloc());
}

inline ref_type ColumnSubtableParent::create(Allocator& alloc, std::size_t size)
{
    return Column::create(alloc, Array::type_HasRefs, size); // Throws
}

inline std::size_t* ColumnSubtableParent::record_subtable_path(std::size_t* begin,
                                                               std::size_t* end) REALM_NOEXCEPT
{
    if (end == begin)
        return 0; // Error, not enough space in buffer
    *begin++ = m_column_ndx;
    if (end == begin)
        return 0; // Error, not enough space in buffer
    return _impl::TableFriend::record_subtable_path(*m_table, begin, end);
}

inline void ColumnSubtableParent::
update_table_accessors(const std::size_t* col_path_begin, const std::size_t* col_path_end,
                       _impl::TableFriend::AccessorUpdater& updater)
{
    // This function must assume no more than minimal consistency of the
    // accessor hierarchy. This means in particular that it cannot access the
    // underlying node structure. See AccessorConsistencyLevels.

    m_subtable_map.update_accessors(col_path_begin, col_path_end, updater); // Throws
}

inline void ColumnSubtableParent::do_insert(std::size_t row_ndx, int_fast64_t value,
                                            std::size_t num_rows)
{
    Column::do_insert(row_ndx, value, num_rows); // Throws
    bool is_append = row_ndx == realm::npos;
    if (!is_append) {
        const bool fix_ndx_in_parent = true;
        m_subtable_map.adj_insert_rows<fix_ndx_in_parent>(row_ndx, num_rows);
    }
}


inline ColumnTable::ColumnTable(Allocator& alloc, ref_type ref,
                                Table* table, std::size_t column_ndx):
    ColumnSubtableParent(alloc, ref, table, column_ndx),
    m_subspec_ndx(realm::npos)
{
}

inline const Table* ColumnTable::get_subtable_ptr(std::size_t subtable_ndx) const
{
    return const_cast<ColumnTable*>(this)->get_subtable_ptr(subtable_ndx);
}

inline void ColumnTable::refresh_accessor_tree(std::size_t col_ndx, const Spec& spec)
{
    ColumnSubtableParent::refresh_accessor_tree(col_ndx, spec); // Throws
    m_subspec_ndx = spec.get_subspec_ndx(col_ndx);
    m_subtable_map.refresh_accessor_tree(m_subspec_ndx); // Throws
}

inline std::size_t ColumnTable::get_subspec_ndx() const REALM_NOEXCEPT
{
    if (REALM_UNLIKELY(m_subspec_ndx == realm::npos)) {
        typedef _impl::TableFriend tf;
        const Spec& spec = tf::get_spec(*m_table);
        m_subspec_ndx = spec.get_subspec_ndx(m_column_ndx);
    }
    return m_subspec_ndx;
}


} // namespace realm

#endif // REALM_COLUMN_TABLE_HPP
