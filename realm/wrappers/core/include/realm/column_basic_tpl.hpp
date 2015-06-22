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
#ifndef REALM_COLUMN_BASIC_TPL_HPP
#define REALM_COLUMN_BASIC_TPL_HPP

// Todo: It's bad design (headers are entangled) that a Column uses query_engine.hpp which again uses Column.
// It's the aggregate() method that calls query_engine, and a quick fix (still not optimal) could be to create
// the call and include inside float and double's .cpp files.
#include <realm/query_engine.hpp>

namespace realm {

// Predeclarations from query_engine.hpp
class ParentNode;
template<class T, class F> class FloatDoubleNode;
template<class T> class SequentialGetter;


template<class T>
BasicColumn<T>::BasicColumn(Allocator& alloc, ref_type ref)
{
    char* header = alloc.translate(ref);
    bool root_is_leaf = !Array::get_is_inner_bptree_node_from_header(header);
    if (root_is_leaf) {
        BasicArray<T>* root = new BasicArray<T>(alloc); // Throws
        root->init_from_mem(MemRef(header, ref));
        m_array.reset(root);
    }
    else {
        Array* root = new Array(alloc); // Throws
        root->init_from_mem(MemRef(header, ref));
        m_array.reset(root);
    }
}

template<class T>
inline std::size_t BasicColumn<T>::size() const REALM_NOEXCEPT
{
    if (root_is_leaf())
        return m_array->size();
    return m_array->get_bptree_size();
}


template<class T>
T BasicColumn<T>::get(std::size_t ndx) const REALM_NOEXCEPT
{
    REALM_ASSERT_DEBUG(ndx < size());
    if (root_is_leaf())
        return static_cast<const BasicArray<T>*>(m_array.get())->get(ndx);

    std::pair<MemRef, std::size_t> p = m_array->get_bptree_leaf(ndx);
    const char* leaf_header = p.first.m_addr;
    std::size_t ndx_in_leaf = p.second;
    return BasicArray<T>::get(leaf_header, ndx_in_leaf);
}


template<class T>
class BasicColumn<T>::SetLeafElem: public Array::UpdateHandler {
public:
    Allocator& m_alloc;
    const T m_value;
    SetLeafElem(Allocator& alloc, T value) REALM_NOEXCEPT: m_alloc(alloc), m_value(value) {}
    void update(MemRef mem, ArrayParent* parent, std::size_t ndx_in_parent,
                std::size_t elem_ndx_in_leaf) override
    {
        BasicArray<T> leaf(m_alloc);
        leaf.init_from_mem(mem);
        leaf.set_parent(parent, ndx_in_parent);
        leaf.set(elem_ndx_in_leaf, m_value); // Throws
    }
};

template<class T>
void BasicColumn<T>::set(std::size_t ndx, T value)
{
    if (!m_array->is_inner_bptree_node()) {
        static_cast<BasicArray<T>*>(m_array.get())->set(ndx, value); // Throws
        return;
    }

    SetLeafElem set_leaf_elem(m_array->get_alloc(), value);
    m_array->update_bptree_elem(ndx, set_leaf_elem); // Throws
}

template<class T> inline void BasicColumn<T>::add(T value)
{
    std::size_t row_ndx = realm::npos;
    std::size_t num_rows = 1;
    do_insert(row_ndx, value, num_rows); // Throws
}

template<class T> inline void BasicColumn<T>::insert(std::size_t row_ndx, T value)
{
    std::size_t size = this->size(); // Slow
    REALM_ASSERT_3(row_ndx, <=, size);
    std::size_t row_ndx_2 = row_ndx == size ? realm::npos : row_ndx;
    std::size_t num_rows = 1;
    do_insert(row_ndx_2, value, num_rows); // Throws
}

template<class T> inline void BasicColumn<T>::erase(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    bool is_last = row_ndx == last_row_ndx;
    do_erase(row_ndx, is_last); // Throws
}

template<class T> inline void BasicColumn<T>::move_last_over(std::size_t row_ndx)
{
    std::size_t last_row_ndx = size() - 1; // Note that size() is slow
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

template<class T> inline void BasicColumn<T>::clear()
{
    do_clear(); // Throws
}

template<class T>
bool BasicColumn<T>::compare(const BasicColumn& c) const
{
    std::size_t n = size();
    if (c.size() != n)
        return false;
    for (std::size_t i = 0; i != n; ++i) {
        T v_1 = get(i);
        T v_2 = c.get(i);
        if (v_1 != v_2)
            return false;
    }
    return true;
}


template<class T>
class BasicColumn<T>::EraseLeafElem: public ColumnBase::EraseHandlerBase {
public:
    EraseLeafElem(BasicColumn<T>& column) REALM_NOEXCEPT:
        EraseHandlerBase(column) {}
    bool erase_leaf_elem(MemRef leaf_mem, ArrayParent* parent,
                         std::size_t leaf_ndx_in_parent,
                         std::size_t elem_ndx_in_leaf) override
    {
        BasicArray<T> leaf(get_alloc());
        leaf.init_from_mem(leaf_mem);
        leaf.set_parent(parent, leaf_ndx_in_parent);
        REALM_ASSERT_3(leaf.size(), >=, 1);
        std::size_t last_ndx = leaf.size() - 1;
        if (last_ndx == 0)
            return true;
        std::size_t ndx = elem_ndx_in_leaf;
        if (ndx == npos)
            ndx = last_ndx;
        leaf.erase(ndx); // Throws
        return false;
    }
    void destroy_leaf(MemRef leaf_mem) REALM_NOEXCEPT override
    {
        Array::destroy(leaf_mem, get_alloc()); // Shallow
    }
    void replace_root_by_leaf(MemRef leaf_mem) override
    {
        BasicArray<T>* leaf = new BasicArray<T>(get_alloc()); // Throws
        leaf->init_from_mem(leaf_mem);
        replace_root(leaf); // Throws, but accessor ownership is passed to callee
    }
    void replace_root_by_empty_leaf() override
    {
        std::unique_ptr<BasicArray<T>> leaf;
        leaf.reset(new BasicArray<T>(get_alloc())); // Throws
        leaf->create(); // Throws
        replace_root(leaf.release()); // Throws, but accessor ownership is passed to callee
    }
};

template<class T>
void BasicColumn<T>::do_erase(std::size_t ndx, bool is_last)
{
    REALM_ASSERT_3(ndx, <, size());
    REALM_ASSERT_3(is_last, ==, (ndx == size() - 1));

    if (!m_array->is_inner_bptree_node()) {
        static_cast<BasicArray<T>*>(m_array.get())->erase(ndx); // Throws
        return;
    }

    std::size_t ndx_2 = is_last ? npos : ndx;
    EraseLeafElem erase_leaf_elem(*this);
    Array::erase_bptree_elem(m_array.get(), ndx_2, erase_leaf_elem); // Throws
}


template<class T>
void BasicColumn<T>::do_move_last_over(std::size_t row_ndx, std::size_t last_row_ndx)
{
    REALM_ASSERT_3(row_ndx, <=, last_row_ndx);
    REALM_ASSERT_3(last_row_ndx + 1, ==, size());

    T value = get(last_row_ndx);
    set(row_ndx, value); // Throws

    bool is_last = true;
    erase(last_row_ndx, is_last); // Throws
}

template<class T> void BasicColumn<T>::do_clear()
{
    if (!m_array->is_inner_bptree_node()) {
        static_cast<BasicArray<T>*>(m_array.get())->clear(); // Throws
        return;
    }

    // Revert to generic array
    std::unique_ptr<BasicArray<T>> array;
    array.reset(new BasicArray<T>(m_array->get_alloc())); // Throws
    array->create(); // Throws
    array->set_parent(m_array->get_parent(), m_array->get_ndx_in_parent());
    array->update_parent(); // Throws

    // Remove original node
    m_array->destroy_deep();

    m_array = std::move(array);
}


template<class T> class BasicColumn<T>::CreateHandler: public ColumnBase::CreateHandler {
public:
    CreateHandler(Allocator& alloc): m_alloc(alloc) {}
    ref_type create_leaf(std::size_t size) override
    {
        MemRef mem = BasicArray<T>::create_array(size, m_alloc); // Throws
        T* tp = reinterpret_cast<T*>(Array::get_data_from_header(mem.m_addr));
        std::fill(tp, tp + size, T());
        return mem.m_ref;
    }
private:
    Allocator& m_alloc;
};

template<class T> ref_type BasicColumn<T>::create(Allocator& alloc, std::size_t size)
{
    CreateHandler handler(alloc);
    return ColumnBase::create(alloc, size, handler);
}


template<class T> class BasicColumn<T>::SliceHandler: public ColumnBase::SliceHandler {
public:
    SliceHandler(Allocator& alloc): m_leaf(alloc) {}
    MemRef slice_leaf(MemRef leaf_mem, size_t offset, size_t size,
                      Allocator& target_alloc) override
    {
        m_leaf.init_from_mem(leaf_mem);
        return m_leaf.slice(offset, size, target_alloc); // Throws
    }
private:
    BasicArray<T> m_leaf;
};

template<class T> ref_type BasicColumn<T>::write(size_t slice_offset, size_t slice_size,
                                                 size_t table_size, _impl::OutputStream& out) const
{
    ref_type ref;
    if (root_is_leaf()) {
        Allocator& alloc = Allocator::get_default();
        BasicArray<T>* leaf = static_cast<BasicArray<T>*>(m_array.get());
        MemRef mem = leaf->slice(slice_offset, slice_size, alloc); // Throws
        Array slice(alloc);
        _impl::DeepArrayDestroyGuard dg(&slice);
        slice.init_from_mem(mem);
        size_t pos = slice.write(out); // Throws
        ref = pos;
    }
    else {
        SliceHandler handler(get_alloc());
        ref = ColumnBase::write(m_array.get(), slice_offset, slice_size,
                                table_size, handler, out); // Throws
    }
    return ref;
}


// Implementing pure virtual method of ColumnBase.
template<class T>
inline void BasicColumn<T>::insert(std::size_t row_ndx, std::size_t num_rows, bool is_append)
{
    std::size_t row_ndx_2 = is_append ? realm::npos : row_ndx;
    T value = T();
    do_insert(row_ndx_2, value, num_rows); // Throws
}

// Implementing pure virtual method of ColumnBase.
template<class T> inline void BasicColumn<T>::erase(std::size_t row_ndx, bool is_last)
{
    do_erase(row_ndx, is_last); // Throws
}

// Implementing pure virtual method of ColumnBase.
template<class T>
void BasicColumn<T>::move_last_over(std::size_t row_ndx, std::size_t last_row_ndx, bool)
{
    do_move_last_over(row_ndx, last_row_ndx); // Throws
}

// Implementing pure virtual method of ColumnBase.
template<class T> void BasicColumn<T>::clear(std::size_t, bool)
{
    do_clear(); // Throws
}


template<class T> void BasicColumn<T>::refresh_accessor_tree(std::size_t, const Spec&)
{
    // The type of the cached root array accessor may no longer match the
    // underlying root node. In that case we need to replace it. Note that when
    // the root node is an inner B+-tree node, then only the top array accessor
    // of that node is cached. The top array accessor of an inner B+-tree node
    // is of type Array.

    ref_type root_ref = m_array->get_ref_from_parent();
    MemRef root_mem(root_ref, m_array->get_alloc());
    bool new_root_is_leaf = !Array::get_is_inner_bptree_node_from_header(root_mem.m_addr);
    bool old_root_is_leaf = !m_array->is_inner_bptree_node();

    bool root_type_changed = old_root_is_leaf != new_root_is_leaf;
    if (!root_type_changed) {
        // Keep, but refresh old root accessor
        if (old_root_is_leaf) {
            // Root is leaf
            BasicArray<T>* root = static_cast<BasicArray<T>*>(m_array.get());
            root->init_from_parent();
            return;
        }
        // Root is inner node
        Array* root = m_array.get();
        root->init_from_parent();
        return;
    }

    // Create new root accessor
    Array* new_root;
    Allocator& alloc = m_array->get_alloc();
    if (new_root_is_leaf) {
        // New root is leaf
        BasicArray<T>* root = new BasicArray<T>(alloc); // Throws
        root->init_from_mem(root_mem);
        new_root = root;
    }
    else {
        // New root is inner node
        Array* root = new Array(alloc); // Throws
        root->init_from_mem(root_mem);
        new_root = root;
    }
    new_root->set_parent(m_array->get_parent(), m_array->get_ndx_in_parent());

    // Instate new root
    m_array.reset(new_root);
}


#ifdef REALM_DEBUG

template<class T>
std::size_t BasicColumn<T>::verify_leaf(MemRef mem, Allocator& alloc)
{
    BasicArray<T> leaf(alloc);
    leaf.init_from_mem(mem);
    leaf.Verify();
    return leaf.size();
}

template<class T>
void BasicColumn<T>::Verify() const
{
    if (root_is_leaf()) {
        static_cast<BasicArray<T>*>(m_array.get())->Verify();
        return;
    }

    m_array->verify_bptree(&BasicColumn<T>::verify_leaf);
}


template<class T>
void BasicColumn<T>::to_dot(std::ostream& out, StringData title) const
{
    ref_type ref = m_array->get_ref();
    out << "subgraph cluster_basic_column" << ref << " {\n";
    out << " label = \"Basic column";
    if (title.size() != 0)
        out << "\\n'" << title << "'";
    out << "\";\n";
    tree_to_dot(out);
    out << "}\n";
}

template<class T>
void BasicColumn<T>::leaf_to_dot(MemRef leaf_mem, ArrayParent* parent, std::size_t ndx_in_parent,
                                 std::ostream& out) const
{
    BasicArray<T> leaf(m_array->get_alloc());
    leaf.init_from_mem(leaf_mem);
    leaf.set_parent(parent, ndx_in_parent);
    leaf.to_dot(out);
}

template<class T>
inline void BasicColumn<T>::leaf_dumper(MemRef mem, Allocator& alloc, std::ostream& out, int level)
{
    BasicArray<T> leaf(alloc);
    leaf.init_from_mem(mem);
    int indent = level * 2;
    out << std::setw(indent) << "" << "Basic leaf (size: "<<leaf.size()<<")\n";
}

template<class T>
inline void BasicColumn<T>::do_dump_node_structure(std::ostream& out, int level) const
{
    m_array->dump_bptree_structure(out, level, &leaf_dumper);
}

#endif // REALM_DEBUG


template<class T>
std::size_t BasicColumn<T>::find_first(T value, std::size_t begin, std::size_t end) const
{
    REALM_ASSERT_3(begin, <=, size());
    REALM_ASSERT(end == npos || (begin <= end && end <= size()));

    if (root_is_leaf())
        return static_cast<BasicArray<T>*>(m_array.get())->
            find_first(value, begin, end); // Throws (maybe)

    // FIXME: It would be better to always require that 'end' is
    // specified explicitely, since Table has the size readily
    // available, and Array::get_bptree_size() is deprecated.
    if (end == npos)
        end = m_array->get_bptree_size();

    std::size_t ndx_in_tree = begin;
    while (ndx_in_tree < end) {
        std::pair<MemRef, std::size_t> p = m_array->get_bptree_leaf(ndx_in_tree);
        BasicArray<T> leaf(m_array->get_alloc());
        leaf.init_from_mem(p.first);
        std::size_t ndx_in_leaf = p.second;
        std::size_t leaf_offset = ndx_in_tree - ndx_in_leaf;
        std::size_t end_in_leaf = std::min(leaf.size(), end - leaf_offset);
        std::size_t ndx = leaf.find_first(value, ndx_in_leaf, end_in_leaf); // Throws (maybe)
        if (ndx != not_found)
            return leaf_offset + ndx;
        ndx_in_tree = leaf_offset + end_in_leaf;
    }

    return not_found;
}

template<class T>
void BasicColumn<T>::find_all(Column &result, T value, std::size_t begin, std::size_t end) const
{
    REALM_ASSERT_3(begin, <=, size());
    REALM_ASSERT(end == npos || (begin <= end && end <= size()));

    if (root_is_leaf()) {
        std::size_t leaf_offset = 0;
        static_cast<BasicArray<T>*>(m_array)->find_all(&result, value, leaf_offset, begin, end); // Throws
        return;
    }

    // FIXME: It would be better to always require that 'end' is
    // specified explicitely, since Table has the size readily
    // available, and Array::get_bptree_size() is deprecated.
    if (end == npos)
        end = m_array->get_bptree_size();

    std::size_t ndx_in_tree = begin;
    while (ndx_in_tree < end) {
        std::pair<MemRef, std::size_t> p = m_array->get_bptree_leaf(ndx_in_tree);
        BasicArray<T> leaf(m_array->get_alloc());
        leaf.init_from_mem(p.first);
        std::size_t ndx_in_leaf = p.second;
        std::size_t leaf_offset = ndx_in_tree - ndx_in_leaf;
        std::size_t end_in_leaf = std::min(leaf.size(), end - leaf_offset);
        leaf.find_all(&result, value, leaf_offset, ndx_in_leaf, end_in_leaf); // Throws
        ndx_in_tree = leaf_offset + end_in_leaf;
    }
}

template<class T> std::size_t BasicColumn<T>::count(T target) const
{
    return std::size_t(ColumnBase::aggregate<T, int64_t, act_Count, Equal>(target, 0, size()));
}

template<class T>
typename BasicColumn<T>::SumType BasicColumn<T>::sum(std::size_t begin, std::size_t end,
    std::size_t limit, std::size_t* return_ndx) const
{
    return ColumnBase::aggregate<T, SumType, act_Sum, None>(0, begin, end, limit, return_ndx);
}
template<class T>
T BasicColumn<T>::minimum(std::size_t begin, std::size_t end, std::size_t limit, size_t* return_ndx) const
{
    return ColumnBase::aggregate<T, T, act_Min, None>(0, begin, end, limit, return_ndx);
}

template<class T>
T BasicColumn<T>::maximum(std::size_t begin, std::size_t end, std::size_t limit, size_t* return_ndx) const
{
    return ColumnBase::aggregate<T, T, act_Max, None>(0, begin, end, limit, return_ndx);
}

template<class T>
double BasicColumn<T>::average(std::size_t begin, std::size_t end, std::size_t limit, size_t* /*return_ndx*/) const
{
    if (end == npos)
        end = size();

    if(limit != npos && begin + limit < end)
        end = begin + limit;

    std::size_t size = end - begin;
    double sum1 = sum(begin, end);
    double avg = sum1 / ( size == 0 ? 1 : size );
    return avg;
}

template<class T> void BasicColumn<T>::do_insert(std::size_t row_ndx, T value, std::size_t num_rows)
{
    REALM_ASSERT(row_ndx == realm::npos || row_ndx < size());
    ref_type new_sibling_ref;
    Array::TreeInsert<BasicColumn<T>> state;
    for (std::size_t i = 0; i != num_rows; ++i) {
        std::size_t row_ndx_2 = row_ndx == realm::npos ? realm::npos : row_ndx + i;
        if (root_is_leaf()) {
            REALM_ASSERT(row_ndx_2 == realm::npos || row_ndx_2 < REALM_MAX_BPNODE_SIZE);
            BasicArray<T>* leaf = static_cast<BasicArray<T>*>(m_array.get());
            new_sibling_ref = leaf->bptree_leaf_insert(row_ndx_2, value, state);
        }
        else {
            state.m_value = value;
            if (row_ndx_2 == realm::npos) {
                new_sibling_ref = m_array->bptree_append(state); // Throws
            }
            else {
                new_sibling_ref = m_array->bptree_insert(row_ndx_2, state); // Throws
            }
        }
        if (REALM_UNLIKELY(new_sibling_ref)) {
            bool is_append = row_ndx_2 == realm::npos;
            introduce_new_root(new_sibling_ref, state, is_append); // Throws
        }
    }
}

template<class T> REALM_FORCEINLINE
ref_type BasicColumn<T>::leaf_insert(MemRef leaf_mem, ArrayParent& parent,
                                     std::size_t ndx_in_parent,
                                     Allocator& alloc, std::size_t insert_ndx,
                                     Array::TreeInsert<BasicColumn<T>>& state)
{
    BasicArray<T> leaf(alloc);
    leaf.init_from_mem(leaf_mem);
    leaf.set_parent(&parent, ndx_in_parent);
    return leaf.bptree_leaf_insert(insert_ndx, state.m_value, state);
}


template<class T> inline std::size_t BasicColumn<T>::lower_bound(T value) const REALM_NOEXCEPT
{
    if (root_is_leaf()) {
        return static_cast<const BasicArray<T>*>(m_array.get())->lower_bound(value);
    }
    return ColumnBase::lower_bound(*this, value);
}

template<class T> inline std::size_t BasicColumn<T>::upper_bound(T value) const REALM_NOEXCEPT
{
    if (root_is_leaf()) {
        return static_cast<const BasicArray<T>*>(m_array.get())->upper_bound(value);
    }
    return ColumnBase::upper_bound(*this, value);
}


} // namespace realm

#endif // REALM_COLUMN_BASIC_TPL_HPP
