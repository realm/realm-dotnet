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
#ifndef REALM_COLUMN_STRING_ENUM_HPP
#define REALM_COLUMN_STRING_ENUM_HPP

#include <realm/column_string.hpp>

namespace realm {

// Pre-declarations
class StringIndex;


/// From the point of view of the application, an enumerated strings column
/// (ColumnStringEnum) is like a string column (AdaptiveStringColumn), yet it
/// manages its stings in such a way that each unique string is stored only
/// once. In fact, an enumerated strings column is a combination of two
/// subcolumns; a regular string column (AdaptiveStringColumn) that stores the
/// unique strings, and an integer column that stores one unique string index
/// for each entry in the enumerated strings column.
///
/// In terms of the underlying node structure, the subcolumn containing the
/// unique strings is not a true part of the enumerated strings column. Instead
/// it is a part of the spec structure that describes the table of which the
/// enumerated strings column is a part. This way, the unique strings can be
/// shared across enumerated strings columns of multiple subtables. This also
/// means that the root of an enumerated strings column coincides with the root
/// of the integer subcolumn, and in some sense, an enumerated strings column is
/// just the integer subcolumn.
///
/// An enumerated strings column can optionally be equipped with a
/// search index. If it is, then the root ref of the index is stored
/// in Table::m_columns immediately after the root ref of the
/// enumerated strings column.
class ColumnStringEnum: public Column {
public:
    typedef StringData value_type;

    ColumnStringEnum(Allocator&, ref_type ref, ref_type keys_ref);
    ~ColumnStringEnum() REALM_NOEXCEPT override;
    void destroy() REALM_NOEXCEPT override;

    int compare_values(size_t row1, size_t row2) const override
    {
        StringData a = get(row1);
        StringData b = get(row2);
        if (a == b)
            return 0;
        return utf8_compare(a, b) ? 1 : -1;
    }

    StringData get(std::size_t ndx) const REALM_NOEXCEPT;
    void set(std::size_t ndx, StringData value);
    void add(StringData value = StringData());
    void insert(std::size_t ndx, StringData value = StringData());
    void erase(std::size_t row_ndx);
    void move_last_over(std::size_t row_ndx);
    void clear();

    std::size_t count(StringData value) const;
    std::size_t find_first(StringData value, std::size_t begin = 0, std::size_t end = npos) const;
    void find_all(Column& res, StringData value,
                  std::size_t begin = 0, std::size_t end = npos) const;
    FindRes find_all_indexref(StringData value, std::size_t& dst) const;

    std::size_t count(std::size_t key_index) const;
    std::size_t find_first(std::size_t key_index, std::size_t begin=0, std::size_t end=-1) const;
    void find_all(Column& res, std::size_t key_index, std::size_t begin = 0, std::size_t end = -1) const;

    //@{
    /// Find the lower/upper bound for the specified value assuming
    /// that the elements are already sorted in ascending order
    /// according to StringData::operator<().
    std::size_t lower_bound_string(StringData value) const REALM_NOEXCEPT;
    std::size_t upper_bound_string(StringData value) const REALM_NOEXCEPT;
    //@}

    void set_string(std::size_t, StringData) override;

    void adjust_keys_ndx_in_parent(int diff) REALM_NOEXCEPT;

    // Search index
    bool has_search_index() const REALM_NOEXCEPT override;
    void set_search_index_ref(ref_type, ArrayParent*, std::size_t, bool) override;
    void set_search_index_allow_duplicate_values(bool) REALM_NOEXCEPT override;
    StringIndex* get_search_index() REALM_NOEXCEPT;
    const StringIndex* get_search_index() const REALM_NOEXCEPT;
    StringIndex* create_search_index();
    void install_search_index(std::unique_ptr<StringIndex>) REALM_NOEXCEPT;
    void destroy_search_index() REALM_NOEXCEPT override;

    // Compare two string columns for equality
    bool compare_string(const AdaptiveStringColumn&) const;
    bool compare_string(const ColumnStringEnum&) const;

    void insert(std::size_t, std::size_t, bool) override;
    void erase(std::size_t, bool) override;
    void move_last_over(std::size_t, std::size_t, bool) override;
    void clear(std::size_t, bool) override;
    void update_from_parent(std::size_t) REALM_NOEXCEPT override;
    void refresh_accessor_tree(std::size_t, const Spec&) override;

    std::size_t GetKeyNdx(StringData value) const;
    std::size_t GetKeyNdxOrAdd(StringData value);

    AdaptiveStringColumn& get_keys();
    const AdaptiveStringColumn& get_keys() const;

#ifdef REALM_DEBUG
    void Verify() const override;
    void Verify(const Table&, std::size_t) const override;
    void do_dump_node_structure(std::ostream&, int) const override;
    void to_dot(std::ostream&, StringData title) const override;
#endif

private:
    // Member variables
    AdaptiveStringColumn m_keys;
    std::unique_ptr<StringIndex> m_search_index;

    /// If you are appending and have the size of the column readily available,
    /// call the 4 argument version instead. If you are not appending, either
    /// one is fine.
    ///
    /// \param row_ndx Must be `realm::npos` if appending.
    void do_insert(std::size_t row_ndx, StringData value, std::size_t num_rows);

    /// If you are appending and you do not have the size of the column readily
    /// available, call the 3 argument version instead. If you are not
    /// appending, either one is fine.
    ///
    /// \param is_append Must be true if, and only if `row_ndx` is equal to the
    /// size of the column (before insertion).
    void do_insert(std::size_t row_ndx, StringData value, std::size_t num_rows, bool is_append);

    void do_erase(std::size_t row_ndx, bool is_last);
    void do_move_last_over(std::size_t row_ndx, std::size_t last_row_ndx);
    void do_clear();
};





// Implementation:

inline StringData ColumnStringEnum::get(std::size_t ndx) const REALM_NOEXCEPT
{
    REALM_ASSERT_3(ndx, <, Column::size());
    std::size_t key_ndx = to_size_t(Column::get(ndx));
    return m_keys.get(key_ndx);
}

inline void ColumnStringEnum::add(StringData value)
{
    std::size_t row_ndx = realm::npos;
    std::size_t num_rows = 1;
    do_insert(row_ndx, value, num_rows); // Throws
}

inline void ColumnStringEnum::insert(std::size_t row_ndx, StringData value)
{
    std::size_t size = this->size();
    REALM_ASSERT_3(row_ndx, <=, size);
    std::size_t num_rows = 1;
    bool is_append = row_ndx == size;
    do_insert(row_ndx, value, num_rows, is_append); // Throws
}

inline void ColumnStringEnum::erase(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    bool is_last = row_ndx == last_row_ndx;
    do_erase(row_ndx, is_last); // Throws
}

inline void ColumnStringEnum::move_last_over(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

inline void ColumnStringEnum::clear()
{
    do_clear(); // Throws
}

// Overriding virtual method of Column.
inline void ColumnStringEnum::insert(std::size_t row_ndx, std::size_t num_rows, bool is_append)
{
    StringData value = StringData();
    do_insert(row_ndx, value, num_rows, is_append); // Throws
}

// Overriding virtual method of Column.
inline void ColumnStringEnum::erase(std::size_t row_ndx, bool is_last)
{
    do_erase(row_ndx, is_last); // Throws
}

// Overriding virtual method of Column.
inline void ColumnStringEnum::move_last_over(std::size_t row_ndx, std::size_t last_row_ndx, bool)
{
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

// Overriding virtual method of Column.
inline void ColumnStringEnum::clear(std::size_t, bool)
{
    do_clear(); // Throws
}

inline std::size_t ColumnStringEnum::lower_bound_string(StringData value) const REALM_NOEXCEPT
{
    return ColumnBase::lower_bound(*this, value);
}

inline std::size_t ColumnStringEnum::upper_bound_string(StringData value) const REALM_NOEXCEPT
{
    return ColumnBase::upper_bound(*this, value);
}

inline void ColumnStringEnum::set_string(std::size_t row_ndx, StringData value)
{
    set(row_ndx, value); // Throws
}

inline bool ColumnStringEnum::has_search_index() const REALM_NOEXCEPT
{
    return m_search_index != 0;
}

inline StringIndex* ColumnStringEnum::get_search_index() REALM_NOEXCEPT
{
    return m_search_index.get();
}

inline const StringIndex* ColumnStringEnum::get_search_index() const REALM_NOEXCEPT
{
    return m_search_index.get();
}

inline AdaptiveStringColumn& ColumnStringEnum::get_keys()
{
    return m_keys;
}

inline const AdaptiveStringColumn& ColumnStringEnum::get_keys() const
{
    return m_keys;
}


} // namespace realm

#endif // REALM_COLUMN_STRING_ENUM_HPP
