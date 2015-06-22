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
#ifndef REALM_COLUMN_STRING_HPP
#define REALM_COLUMN_STRING_HPP

#include <memory>
#include <realm/array_string.hpp>
#include <realm/array_string_long.hpp>
#include <realm/array_blobs_big.hpp>
#include <realm/column.hpp>

namespace realm {

// Pre-declarations
class StringIndex;


/// A string column (AdaptiveStringColumn) is a single B+-tree, and
/// the root of the column is the root of the B+-tree. Leaf nodes are
/// either of type ArrayString (array of small strings),
/// ArrayStringLong (array of medium strings), or ArrayBigBlobs (array
/// of big strings).
///
/// A string column can optionally be equipped with a search index. If
/// it is, then the root ref of the index is stored in
/// Table::m_columns immediately after the root ref of the string
/// column.
///
/// FIXME: Rename AdaptiveStringColumn to StringColumn
class AdaptiveStringColumn: public ColumnBase, public ColumnTemplate<StringData> {
public:
    typedef StringData value_type;

    AdaptiveStringColumn(Allocator&, ref_type);
    ~AdaptiveStringColumn() REALM_NOEXCEPT override;

    void destroy() REALM_NOEXCEPT override;

    std::size_t size() const REALM_NOEXCEPT;
    bool is_empty() const REALM_NOEXCEPT { return size() == 0; }

    StringData get(std::size_t ndx) const REALM_NOEXCEPT;
    void set(std::size_t ndx, StringData);
    void add(StringData value = StringData());
    void insert(std::size_t ndx, StringData value = StringData());
    void erase(std::size_t row_ndx);
    void move_last_over(std::size_t row_ndx);
    void clear();

    std::size_t count(StringData value) const;
    std::size_t find_first(StringData value, std::size_t begin = 0,
                           std::size_t end = npos) const;
    void find_all(Column& result, StringData value, std::size_t begin = 0,
                  std::size_t end = npos) const;

    int compare_values(std::size_t, std::size_t) const override;

    //@{
    /// Find the lower/upper bound for the specified value assuming
    /// that the elements are already sorted in ascending order
    /// according to StringData::operator<().
    std::size_t lower_bound_string(StringData value) const REALM_NOEXCEPT;
    std::size_t upper_bound_string(StringData value) const REALM_NOEXCEPT;
    //@}

    void set_string(std::size_t, StringData) override;

    FindRes find_all_indexref(StringData value, std::size_t& dst) const;

    // Search index
    bool has_search_index() const REALM_NOEXCEPT override;
    void set_search_index_ref(ref_type, ArrayParent*, std::size_t, bool) override;
    void set_search_index_allow_duplicate_values(bool) REALM_NOEXCEPT override;
    StringIndex* get_search_index() REALM_NOEXCEPT;
    const StringIndex* get_search_index() const REALM_NOEXCEPT;
    std::unique_ptr<StringIndex> release_search_index() REALM_NOEXCEPT;
    StringIndex* create_search_index();
    void destroy_search_index() REALM_NOEXCEPT override;

    // Optimizing data layout
    bool auto_enumerate(ref_type& keys, ref_type& values) const;

    /// Compare two string columns for equality.
    bool compare_string(const AdaptiveStringColumn&) const;

    enum LeafType {
        leaf_type_Small,  ///< ArrayString
        leaf_type_Medium, ///< ArrayStringLong
        leaf_type_Big     ///< ArrayBigBlobs
    };

    LeafType GetBlock(std::size_t ndx, ArrayParent**, std::size_t& off,
                      bool use_retval = false) const;

    static ref_type create(Allocator&, std::size_t size = 0);

    static std::size_t get_size_from_ref(ref_type root_ref, Allocator&) REALM_NOEXCEPT;

    // Overrriding method in ColumnBase
    ref_type write(std::size_t, std::size_t, std::size_t,
                   _impl::OutputStream&) const override;

    bool is_string_col() const REALM_NOEXCEPT override;

    void insert(std::size_t, std::size_t, bool) override;
    void erase(std::size_t, bool) override;
    void move_last_over(std::size_t, std::size_t, bool) override;
    void clear(std::size_t, bool) override;
    void update_from_parent(std::size_t old_baseline) REALM_NOEXCEPT override;
    void refresh_accessor_tree(std::size_t, const Spec&) override;

#ifdef REALM_DEBUG
    void Verify() const override;
    void Verify(const Table&, std::size_t) const override;
    void to_dot(std::ostream&, StringData title) const override;
    void do_dump_node_structure(std::ostream&, int) const override;
#endif

protected:
    StringData get_val(std::size_t row) const { return get(row); }

private:
    std::unique_ptr<StringIndex> m_search_index;

    std::size_t do_get_size() const REALM_NOEXCEPT override { return size(); }

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

    /// \param row_ndx Must be `realm::npos` if appending.
    void bptree_insert(std::size_t row_ndx, StringData value, std::size_t num_rows);

    // Called by Array::bptree_insert().
    static ref_type leaf_insert(MemRef leaf_mem, ArrayParent&, std::size_t ndx_in_parent,
                                Allocator&, std::size_t insert_ndx,
                                Array::TreeInsert<AdaptiveStringColumn>& state);

    class EraseLeafElem;
    class CreateHandler;
    class SliceHandler;

    void do_erase(std::size_t row_ndx, bool is_last);
    void do_move_last_over(std::size_t row_ndx, std::size_t last_row_ndx);
    void do_clear();

    /// Root must be a leaf. Upgrades the root leaf as
    /// necessary. Returns the type of the root leaf as it is upon
    /// return.
    LeafType upgrade_root_leaf(std::size_t value_size);

    void refresh_root_accessor();

#ifdef REALM_DEBUG
    void leaf_to_dot(MemRef, ArrayParent*, std::size_t ndx_in_parent,
                     std::ostream&) const override;
#endif

    friend class Array;
    friend class ColumnBase;
};





// Implementation:

inline std::size_t AdaptiveStringColumn::size() const REALM_NOEXCEPT
{
    if (root_is_leaf()) {
        bool long_strings = m_array->has_refs();
        if (!long_strings) {
            // Small strings root leaf
            ArrayString* leaf = static_cast<ArrayString*>(m_array.get());
            return leaf->size();
        }
        bool is_big = m_array->get_context_flag();
        if (!is_big) {
            // Medium strings root leaf
            ArrayStringLong* leaf = static_cast<ArrayStringLong*>(m_array.get());
            return leaf->size();
        }
        // Big strings root leaf
        ArrayBigBlobs* leaf = static_cast<ArrayBigBlobs*>(m_array.get());
        return leaf->size();
    }
    // Non-leaf root
    return m_array->get_bptree_size();
}

inline void AdaptiveStringColumn::add(StringData value)
{
    std::size_t row_ndx = realm::npos;
    std::size_t num_rows = 1;
    do_insert(row_ndx, value, num_rows); // Throws
}

inline void AdaptiveStringColumn::insert(std::size_t row_ndx, StringData value)
{
    std::size_t size = this->size();
    REALM_ASSERT_3(row_ndx, <=, size);
    std::size_t num_rows = 1;
    bool is_append = row_ndx == size;
    do_insert(row_ndx, value, num_rows, is_append); // Throws
}

inline void AdaptiveStringColumn::erase(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    bool is_last = row_ndx == last_row_ndx;
    do_erase(row_ndx, is_last); // Throws
}

inline void AdaptiveStringColumn::move_last_over(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

inline void AdaptiveStringColumn::clear()
{
    do_clear(); // Throws
}

inline int AdaptiveStringColumn::compare_values(std::size_t row1, std::size_t row2) const
{
    StringData a = get(row1);
    StringData b = get(row2);
    if (a == b)
        return 0;
    return utf8_compare(a, b) ? 1 : -1;
}

inline void AdaptiveStringColumn::set_string(std::size_t row_ndx, StringData value)
{
    set(row_ndx, value); // Throws
}

inline bool AdaptiveStringColumn::has_search_index() const REALM_NOEXCEPT
{
    return m_search_index != 0;
}

inline StringIndex* AdaptiveStringColumn::get_search_index() REALM_NOEXCEPT
{
    return m_search_index.get();
}

inline const StringIndex* AdaptiveStringColumn::get_search_index() const REALM_NOEXCEPT
{
    return m_search_index.get();
}

inline std::size_t AdaptiveStringColumn::get_size_from_ref(ref_type root_ref,
                                                           Allocator& alloc) REALM_NOEXCEPT
{
    const char* root_header = alloc.translate(root_ref);
    bool root_is_leaf = !Array::get_is_inner_bptree_node_from_header(root_header);
    if (root_is_leaf) {
        bool long_strings = Array::get_hasrefs_from_header(root_header);
        if (!long_strings) {
            // Small strings leaf
            return ArrayString::get_size_from_header(root_header);
        }
        bool is_big = Array::get_context_flag_from_header(root_header);
        if (!is_big) {
            // Medium strings leaf
            return ArrayStringLong::get_size_from_header(root_header, alloc);
        }
        // Big strings leaf
        return ArrayBigBlobs::get_size_from_header(root_header);
    }
    return Array::get_bptree_size_from_header(root_header);
}

inline bool AdaptiveStringColumn::is_string_col() const REALM_NOEXCEPT
{
    return true;
}

// Implementing pure virtual method of ColumnBase.
inline void AdaptiveStringColumn::insert(std::size_t row_ndx, std::size_t num_rows, bool is_append)
{
    StringData value = StringData();
    do_insert(row_ndx, value, num_rows, is_append); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void AdaptiveStringColumn::erase(std::size_t row_ndx, bool is_last)
{
    do_erase(row_ndx, is_last); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void AdaptiveStringColumn::move_last_over(std::size_t row_ndx, std::size_t last_row_ndx,
                                                 bool)
{
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void AdaptiveStringColumn::clear(std::size_t, bool)
{
    do_clear(); // Throws
}

} // namespace realm

#endif // REALM_COLUMN_STRING_HPP
