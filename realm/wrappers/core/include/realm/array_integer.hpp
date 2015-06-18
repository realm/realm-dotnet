/*************************************************************************
 *
 * REALM CONFIDENTIAL
 * __________________
 *
 *  [2011] - [2014] Realm Inc
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
***************************************************************************/

#ifndef REALM_ARRAY_INTEGER_HPP
#define REALM_ARRAY_INTEGER_HPP

#include <realm/array.hpp>
#include <realm/util/safe_int_ops.hpp>

namespace realm {

class ArrayInteger: public Array {
public:
    typedef int64_t value_type;

    explicit ArrayInteger(no_prealloc_tag) REALM_NOEXCEPT;
    explicit ArrayInteger(Allocator&) REALM_NOEXCEPT;
    ~ArrayInteger() REALM_NOEXCEPT override {}

    /// Construct an array of the specified type and size, and return just the
    /// reference to the underlying memory. All elements will be initialized to
    /// the specified value.
    static MemRef create_array(Type, bool context_flag, std::size_t size, int_fast64_t value,
                               Allocator&);

    void add(int64_t value);
    void set(std::size_t ndx, int64_t value);
    void set_uint(std::size_t ndx, uint64_t value);
    int64_t get(std::size_t ndx) const REALM_NOEXCEPT;
    uint64_t get_uint(std::size_t ndx) const REALM_NOEXCEPT;
    static int64_t get(const char* header, std::size_t ndx) REALM_NOEXCEPT;

    /// Add \a diff to the element at the specified index.
    void adjust(std::size_t ndx, int_fast64_t diff);

    /// Add \a diff to all the elements in the specified index range.
    void adjust(std::size_t begin, std::size_t end, int_fast64_t diff);

    /// Add signed \a diff to all elements that are greater than, or equal to \a
    /// limit.
    void adjust_ge(int_fast64_t limit, int_fast64_t diff);

    int64_t operator[](std::size_t ndx) const REALM_NOEXCEPT { return get(ndx); }
    int64_t front() const REALM_NOEXCEPT;
    int64_t back() const REALM_NOEXCEPT;

    std::size_t lower_bound(int64_t value) const REALM_NOEXCEPT;
    std::size_t upper_bound(int64_t value) const REALM_NOEXCEPT;

    std::vector<int64_t> ToVector() const;

private:
    template<size_t w> bool minmax(size_t from, size_t to, uint64_t maxdiff,
                                   int64_t* min, int64_t* max) const;
};


// Implementation:

inline ArrayInteger::ArrayInteger(Array::no_prealloc_tag) REALM_NOEXCEPT:
    Array(Array::no_prealloc_tag())
{
}

inline ArrayInteger::ArrayInteger(Allocator& alloc) REALM_NOEXCEPT:
    Array(alloc)
{
}

inline MemRef ArrayInteger::create_array(Type type, bool context_flag, std::size_t size,
                                  int_fast64_t value, Allocator& alloc)
{
    return Array::create(type, context_flag, wtype_Bits, size, value, alloc); // Throws
}

inline void ArrayInteger::add(int64_t value)
{
    Array::add(value);
}

inline int64_t ArrayInteger::get(size_t ndx) const REALM_NOEXCEPT
{
    return Array::get(ndx);
}

inline uint64_t ArrayInteger::get_uint(std::size_t ndx) const REALM_NOEXCEPT
{
    return get(ndx);
}

inline int64_t ArrayInteger::get(const char* header, size_t ndx) REALM_NOEXCEPT
{
    return Array::get(header, ndx);
}

inline void ArrayInteger::set(size_t ndx, int64_t value)
{
    Array::set(ndx, value);
}

inline void ArrayInteger::set_uint(std::size_t ndx, uint_fast64_t value)
{
    // When a value of a signed type is converted to an unsigned type, the C++
    // standard guarantees that negative values are converted from the native
    // representation to 2's complement, but the effect of conversions in the
    // opposite direction is left unspecified by the
    // standard. `realm::util::from_twos_compl()` is used here to perform the
    // correct opposite unsigned-to-signed conversion, which reduces to a no-op
    // when 2's complement is the native representation of negative values.
    set(ndx, util::from_twos_compl<int_fast64_t>(value));
}


inline int64_t ArrayInteger::front() const REALM_NOEXCEPT
{
    return Array::front();
}

inline int64_t ArrayInteger::back() const REALM_NOEXCEPT
{
    return Array::back();
}

inline void ArrayInteger::adjust(std::size_t ndx, int_fast64_t diff)
{
    Array::adjust(ndx, diff);
}

inline void ArrayInteger::adjust(std::size_t begin, std::size_t end, int_fast64_t diff)
{
    Array::adjust(begin, end, diff);
}

inline void ArrayInteger::adjust_ge(int_fast64_t limit, int_fast64_t diff)
{
    Array::adjust_ge(limit, diff);
}

inline std::size_t ArrayInteger::lower_bound(int64_t value) const REALM_NOEXCEPT
{
    return lower_bound_int(value);
}

inline std::size_t ArrayInteger::upper_bound(int64_t value) const REALM_NOEXCEPT
{
    return upper_bound_int(value);
}


}

#endif // REALM_ARRAY_INTEGER_HPP
