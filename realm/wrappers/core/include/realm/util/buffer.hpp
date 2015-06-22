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
#ifndef REALM_UTIL_BUFFER_HPP
#define REALM_UTIL_BUFFER_HPP

#include <cstddef>
#include <algorithm>
#include <exception>
#include <limits>
#include <utility>

#include <realm/util/features.h>
#include <realm/util/safe_int_ops.hpp>
#include <memory>

namespace realm {
namespace util {


/// A simple buffer concept that owns a region of memory and knows its
/// size.
template<class T> class Buffer {
public:
    Buffer() REALM_NOEXCEPT: m_data(nullptr), m_size(0) {}
    Buffer(std::size_t size);
    ~Buffer() REALM_NOEXCEPT {}

    T& operator[](std::size_t i) REALM_NOEXCEPT { return m_data[i]; }
    const T& operator[](std::size_t i) const REALM_NOEXCEPT { return m_data[i]; }

    T* data() REALM_NOEXCEPT { return m_data.get(); }
    const T* data() const REALM_NOEXCEPT { return m_data.get(); }
    std::size_t size() const REALM_NOEXCEPT { return m_size; }

    /// Discards the original contents.
    void set_size(std::size_t new_size);

    /// \param copy_begin, copy_end Specifies a range of element
    /// values to be retained. \a copy_end must be less than, or equal
    /// to size().
    ///
    /// \param copy_to Specifies where the retained range should be
    /// copied to. `\a copy_to + \a copy_end - \a copy_begin` must be
    /// less than, or equal to \a new_size.
    void resize(std::size_t new_size, std::size_t copy_begin, std::size_t copy_end,
                std::size_t copy_to);

    void reserve(std::size_t used_size, std::size_t min_capacity);

    void reserve_extra(std::size_t used_size, std::size_t min_extra_capacity);

    T* release() REALM_NOEXCEPT;

    friend void swap(Buffer&a, Buffer&b) REALM_NOEXCEPT
    {
        using std::swap;
        swap(a.m_data, b.m_data);
        swap(a.m_size, b.m_size);
    }

private:
    std::unique_ptr<T[]> m_data;
    std::size_t m_size;
};


/// A buffer that can be efficiently resized. It acheives this by
/// using an underlying buffer that may be larger than the logical
/// size, and is automatically expanded in progressively larger steps.
template<class T> class AppendBuffer {
public:
    AppendBuffer() REALM_NOEXCEPT;
    ~AppendBuffer() REALM_NOEXCEPT {}

    /// Returns the current size of the buffer.
    std::size_t size() const REALM_NOEXCEPT;

    /// Gives read and write access to the elements.
    T* data() REALM_NOEXCEPT;

    /// Gives read access the elements.
    const T* data() const REALM_NOEXCEPT;

    /// Append the specified elements. This increases the size of this
    /// buffer by \a size. If the caller has previously requested a
    /// minimum capacity that is greater than, or equal to the
    /// resulting size, this function is guaranteed to not throw.
    void append(const T* data, std::size_t size);

    /// If the specified size is less than the current size, then the
    /// buffer contents is truncated accordingly. If the specified
    /// size is greater than the current size, then the extra elements
    /// will have undefined values. If the caller has previously
    /// requested a minimum capacity that is greater than, or equal to
    /// the specified size, this function is guaranteed to not throw.
    void resize(std::size_t size);

    /// This operation does not change the size of the buffer as
    /// returned by size(). If the specified capacity is less than the
    /// current capacity, this operation has no effect.
    void reserve(std::size_t min_capacity);

    /// Set the size to zero. The capacity remains unchanged.
    void clear() REALM_NOEXCEPT;

private:
    util::Buffer<char> m_buffer;
    std::size_t m_size;
};




// Implementation:

class BufferSizeOverflow: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override
    {
        return "Buffer size overflow";
    }
};

template<class T> inline Buffer<T>::Buffer(std::size_t size):
    m_data(new T[size]), // Throws
    m_size(size)
{
}

template<class T> inline void Buffer<T>::set_size(std::size_t new_size)
{
    m_data.reset(new T[new_size]); // Throws
    m_size = new_size;
}

template<class T> inline void Buffer<T>::resize(std::size_t new_size, std::size_t copy_begin,
                                                std::size_t copy_end, std::size_t copy_to)
{
    std::unique_ptr<T[]> new_data(new T[new_size]); // Throws
    std::copy(m_data.get() + copy_begin, m_data.get() + copy_end, new_data.get() + copy_to);
    m_data.reset(new_data.release());
    m_size = new_size;
}

template<class T> inline void Buffer<T>::reserve(std::size_t used_size,
                                                 std::size_t min_capacity)
{
    std::size_t current_capacity = m_size;
    if (REALM_LIKELY(current_capacity >= min_capacity))
        return;
    std::size_t new_capacity = current_capacity;
    if (REALM_UNLIKELY(int_multiply_with_overflow_detect(new_capacity, 2)))
        new_capacity = std::numeric_limits<std::size_t>::max();
    if (REALM_UNLIKELY(new_capacity < min_capacity))
        new_capacity = min_capacity;
    resize(new_capacity, 0, used_size, 0); // Throws
}

template<class T> inline void Buffer<T>::reserve_extra(std::size_t used_size,
                                                       std::size_t min_extra_capacity)
{
    std::size_t min_capacity = used_size;
    if (REALM_UNLIKELY(int_add_with_overflow_detect(min_capacity, min_extra_capacity)))
        throw BufferSizeOverflow();
    reserve(used_size, min_capacity); // Throws
}

template<class T> inline T* Buffer<T>::release() REALM_NOEXCEPT
{
    m_size = 0;
    return m_data.release();
}


template<class T> inline AppendBuffer<T>::AppendBuffer() REALM_NOEXCEPT: m_size(0)
{
}

template<class T> inline std::size_t AppendBuffer<T>::size() const REALM_NOEXCEPT
{
    return m_size;
}

template<class T> inline T* AppendBuffer<T>::data() REALM_NOEXCEPT
{
    return m_buffer.data();
}

template<class T> inline const T* AppendBuffer<T>::data() const REALM_NOEXCEPT
{
    return m_buffer.data();
}

template<class T> inline void AppendBuffer<T>::append(const T* data, std::size_t size)
{
    m_buffer.reserve_extra(m_size, size); // Throws
    std::copy(data, data+size, m_buffer.data()+m_size);
    m_size += size;
}

template<class T> inline void AppendBuffer<T>::reserve(std::size_t min_capacity)
{
    m_buffer.reserve(m_size, min_capacity);
}

template<class T> inline void AppendBuffer<T>::resize(std::size_t size)
{
    reserve(size);
    m_size = size;
}

template<class T> inline void AppendBuffer<T>::clear() REALM_NOEXCEPT
{
    m_size = 0;
}


} // namespace util
} // namespace realm

#endif // REALM_UTIL_BUFFER_HPP
