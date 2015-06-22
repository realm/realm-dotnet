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
#ifndef REALM_COLUMN_BINARY_HPP
#define REALM_COLUMN_BINARY_HPP

#include <realm/column.hpp>
#include <realm/array_binary.hpp>
#include <realm/array_blobs_big.hpp>

namespace realm {


/// A binary column (ColumnBinary) is a single B+-tree, and the root
/// of the column is the root of the B+-tree. Leaf nodes are either of
/// type ArrayBinary (array of small blobs) or ArrayBigBlobs (array of
/// big blobs).
class ColumnBinary: public ColumnBase {
public:
    typedef BinaryData value_type;

    ColumnBinary(Allocator&, ref_type);

    std::size_t size() const REALM_NOEXCEPT;
    bool is_empty() const REALM_NOEXCEPT { return size() == 0; }

    BinaryData get(std::size_t ndx) const REALM_NOEXCEPT;

    void add(BinaryData value = BinaryData());
    void set(std::size_t ndx, BinaryData value, bool add_zero_term = false);
    void insert(std::size_t ndx, BinaryData value = BinaryData());
    void erase(std::size_t row_ndx);
    void move_last_over(std::size_t row_ndx);
    void clear();

    // Requires that the specified entry was inserted as StringData.
    StringData get_string(std::size_t ndx) const REALM_NOEXCEPT;

    void add_string(StringData value);
    void set_string(std::size_t ndx, StringData value);
    void insert_string(std::size_t ndx, StringData value);

    /// Compare two binary columns for equality.
    bool compare_binary(const ColumnBinary&) const;

    static ref_type create(Allocator&, std::size_t size = 0);

    static std::size_t get_size_from_ref(ref_type root_ref, Allocator&) REALM_NOEXCEPT;

    // Overrriding method in ColumnBase
    ref_type write(std::size_t, std::size_t, std::size_t,
                   _impl::OutputStream&) const override;

    void insert(std::size_t, std::size_t, bool) override;
    void erase(std::size_t, bool) override;
    void move_last_over(std::size_t, std::size_t, bool) override;
    void clear(std::size_t, bool) override;
    void update_from_parent(std::size_t) REALM_NOEXCEPT override;
    void refresh_accessor_tree(std::size_t, const Spec&) override;

#ifdef REALM_DEBUG
    void Verify() const override;
    void to_dot(std::ostream&, StringData title) const override;
    void do_dump_node_structure(std::ostream&, int) const override;
#endif

private:
    std::size_t do_get_size() const REALM_NOEXCEPT override { return size(); }

    /// \param row_ndx Must be `realm::npos` if appending.
    void do_insert(std::size_t row_ndx, BinaryData value, bool add_zero_term,
                   std::size_t num_rows);

    // Called by Array::bptree_insert().
    static ref_type leaf_insert(MemRef leaf_mem, ArrayParent&, std::size_t ndx_in_parent,
                                Allocator&, std::size_t insert_ndx,
                                Array::TreeInsert<ColumnBinary>& state);

    struct InsertState: Array::TreeInsert<ColumnBinary> {
        bool m_add_zero_term;
    };

    class EraseLeafElem;
    class CreateHandler;
    class SliceHandler;

    void do_erase(std::size_t row_ndx, bool is_last);
    void do_move_last_over(std::size_t row_ndx, std::size_t last_row_ndx);
    void do_clear();

    /// Root must be a leaf. Upgrades the root leaf if
    /// necessary. Returns true if, and only if the root is a 'big
    /// blobs' leaf upon return.
    bool upgrade_root_leaf(std::size_t value_size);

#ifdef REALM_DEBUG
    void leaf_to_dot(MemRef, ArrayParent*, std::size_t ndx_in_parent,
                     std::ostream&) const override;
#endif

    friend class Array;
    friend class ColumnBase;
};




// Implementation

inline std::size_t ColumnBinary::size() const  REALM_NOEXCEPT
{
    if (root_is_leaf()) {
        bool is_big = m_array->get_context_flag();
        if (!is_big) {
            // Small blobs root leaf
            ArrayBinary* leaf = static_cast<ArrayBinary*>(m_array.get());
            return leaf->size();
        }
        // Big blobs root leaf
        ArrayBigBlobs* leaf = static_cast<ArrayBigBlobs*>(m_array.get());
        return leaf->size();
    }
    // Non-leaf root
    return m_array->get_bptree_size();
}

inline void ColumnBinary::update_from_parent(std::size_t old_baseline) REALM_NOEXCEPT
{
    if (root_is_leaf()) {
        bool is_big = m_array->get_context_flag();
        if (!is_big) {
            // Small blobs root leaf
            ArrayBinary* leaf = static_cast<ArrayBinary*>(m_array.get());
            leaf->update_from_parent(old_baseline);
            return;
        }
        // Big blobs root leaf
        ArrayBigBlobs* leaf = static_cast<ArrayBigBlobs*>(m_array.get());
        leaf->update_from_parent(old_baseline);
        return;
    }
    // Non-leaf root
    m_array->update_from_parent(old_baseline);
}

inline BinaryData ColumnBinary::get(std::size_t ndx) const REALM_NOEXCEPT
{
    REALM_ASSERT_DEBUG(ndx < size());
    if (root_is_leaf()) {
        bool is_big = m_array->get_context_flag();
        if (!is_big) {
            // Small blobs root leaf
            ArrayBinary* leaf = static_cast<ArrayBinary*>(m_array.get());
            return leaf->get(ndx);
        }
        // Big blobs root leaf
        ArrayBigBlobs* leaf = static_cast<ArrayBigBlobs*>(m_array.get());
        return leaf->get(ndx);
    }

    // Non-leaf root
    std::pair<MemRef, std::size_t> p = m_array->get_bptree_leaf(ndx);
    const char* leaf_header = p.first.m_addr;
    std::size_t ndx_in_leaf = p.second;
    Allocator& alloc = m_array->get_alloc();
    bool is_big = Array::get_context_flag_from_header(leaf_header);
    if (!is_big) {
        // Small blobs
        return ArrayBinary::get(leaf_header, ndx_in_leaf, alloc);
    }
    // Big blobs
    return ArrayBigBlobs::get(leaf_header, ndx_in_leaf, alloc);
}

inline StringData ColumnBinary::get_string(std::size_t ndx) const REALM_NOEXCEPT
{
    BinaryData bin = get(ndx);
    REALM_ASSERT_3(0, <, bin.size());
    return StringData(bin.data(), bin.size()-1);
}

inline void ColumnBinary::set_string(std::size_t ndx, StringData value)
{
    BinaryData bin(value.data(), value.size());
    bool add_zero_term = true;
    set(ndx, bin, add_zero_term);
}

inline void ColumnBinary::add(BinaryData value)
{
    std::size_t row_ndx = realm::npos;
    bool add_zero_term = false;
    std::size_t num_rows = 1;
    do_insert(row_ndx, value, add_zero_term, num_rows); // Throws
}

inline void ColumnBinary::insert(std::size_t row_ndx, BinaryData value)
{
    std::size_t size = this->size(); // Slow
    REALM_ASSERT_3(row_ndx, <=, size);
    std::size_t row_ndx_2 = row_ndx == size ? realm::npos : row_ndx;
    bool add_zero_term = false;
    std::size_t num_rows = 1;
    do_insert(row_ndx_2, value, add_zero_term, num_rows); // Throws
}

inline void ColumnBinary::erase(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    bool is_last = row_ndx == last_row_ndx;
    do_erase(row_ndx, is_last); // Throws
}

inline void ColumnBinary::move_last_over(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

inline void ColumnBinary::clear()
{
    do_clear(); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void ColumnBinary::insert(std::size_t row_ndx, std::size_t num_rows, bool is_append)
{
    std::size_t row_ndx_2 = is_append ? realm::npos : row_ndx;
    BinaryData value = BinaryData();
    bool add_zero_term = false;
    do_insert(row_ndx_2, value, add_zero_term, num_rows); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void ColumnBinary::erase(std::size_t row_ndx, bool is_last)
{
    do_erase(row_ndx, is_last); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void ColumnBinary::move_last_over(std::size_t row_ndx, std::size_t last_row_ndx, bool)
{
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

// Implementing pure virtual method of ColumnBase.
inline void ColumnBinary::clear(std::size_t, bool)
{
    do_clear(); // Throws
}

inline void ColumnBinary::add_string(StringData value)
{
    std::size_t row_ndx = realm::npos;
    BinaryData value_2(value.data(), value.size());
    bool add_zero_term = true;
    std::size_t num_rows = 1;
    do_insert(row_ndx, value_2, add_zero_term, num_rows); // Throws
}

inline void ColumnBinary::insert_string(std::size_t row_ndx, StringData value)
{
    std::size_t size = this->size(); // Slow
    REALM_ASSERT_3(row_ndx, <=, size);
    std::size_t row_ndx_2 = row_ndx == size ? realm::npos : row_ndx;
    BinaryData value_2(value.data(), value.size());
    bool add_zero_term = false;
    std::size_t num_rows = 1;
    do_insert(row_ndx_2, value_2, add_zero_term, num_rows); // Throws
}

inline std::size_t ColumnBinary::get_size_from_ref(ref_type root_ref,
                                                   Allocator& alloc) REALM_NOEXCEPT
{
    const char* root_header = alloc.translate(root_ref);
    bool root_is_leaf = !Array::get_is_inner_bptree_node_from_header(root_header);
    if (root_is_leaf) {
        bool is_big = Array::get_context_flag_from_header(root_header);
        if (!is_big) {
            // Small blobs leaf
            return ArrayBinary::get_size_from_header(root_header, alloc);
        }
        // Big blobs leaf
        return ArrayBigBlobs::get_size_from_header(root_header);
    }
    return Array::get_bptree_size_from_header(root_header);
}


} // namespace realm

#endif // REALM_COLUMN_BINARY_HPP
