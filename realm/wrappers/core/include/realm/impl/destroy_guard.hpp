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

#ifndef REALM_IMPL_DESTROY_GUARD_HPP
#define REALM_IMPL_DESTROY_GUARD_HPP

#include <realm/util/features.h>
#include <realm/array.hpp>

namespace realm {
namespace _impl {


/// Calls `ptr->destroy()` if the guarded pointer (`ptr`) is not null
/// when the guard is destroyed. For arrays (`T` = `Array`) this means
/// that the array is destroyed in a shallow fashion. See
/// `DeepArrayDestroyGuard` for an alternative.
template<class T> class DestroyGuard {
public:
    DestroyGuard() REALM_NOEXCEPT;

    DestroyGuard(T*) REALM_NOEXCEPT;

    ~DestroyGuard() REALM_NOEXCEPT;

    void reset(T*) REALM_NOEXCEPT;

    T* get() const REALM_NOEXCEPT;

    T* release() REALM_NOEXCEPT;

private:
    T* m_ptr;
};

typedef DestroyGuard<Array> ShallowArrayDestroyGuard;


/// Calls `ptr->destroy_deep()` if the guarded Array pointer (`ptr`)
/// is not null when the guard is destroyed.
class DeepArrayDestroyGuard {
public:
    DeepArrayDestroyGuard() REALM_NOEXCEPT;

    DeepArrayDestroyGuard(Array*) REALM_NOEXCEPT;

    ~DeepArrayDestroyGuard() REALM_NOEXCEPT;

    void reset(Array*) REALM_NOEXCEPT;

    Array* get() const REALM_NOEXCEPT;

    Array* release() REALM_NOEXCEPT;

private:
    Array* m_ptr;
};


/// Calls `Array::destroy_deep(ref, alloc)` if the guarded 'ref'
/// (`ref`) is not zero when the guard is destroyed.
class DeepArrayRefDestroyGuard {
public:
    DeepArrayRefDestroyGuard(Allocator&) REALM_NOEXCEPT;

    DeepArrayRefDestroyGuard(ref_type, Allocator&) REALM_NOEXCEPT;

    ~DeepArrayRefDestroyGuard() REALM_NOEXCEPT;

    void reset(ref_type) REALM_NOEXCEPT;

    ref_type get() const REALM_NOEXCEPT;

    ref_type release() REALM_NOEXCEPT;

private:
    ref_type m_ref;
    Allocator& m_alloc;
};





// Implementation:

// DestroyGuard<T>

template<class T> inline DestroyGuard<T>::DestroyGuard() REALM_NOEXCEPT:
    m_ptr(0)
{
}

template<class T> inline DestroyGuard<T>::DestroyGuard(T* ptr) REALM_NOEXCEPT:
    m_ptr(ptr)
{
}

template<class T> inline DestroyGuard<T>::~DestroyGuard() REALM_NOEXCEPT
{
    if (m_ptr)
        m_ptr->destroy();
}

template<class T> inline void DestroyGuard<T>::reset(T* ptr) REALM_NOEXCEPT
{
    if (m_ptr)
        m_ptr->destroy();
    m_ptr = ptr;
}

template<class T> inline T* DestroyGuard<T>::get() const REALM_NOEXCEPT
{
    return m_ptr;
}

template<class T> inline T* DestroyGuard<T>::release() REALM_NOEXCEPT
{
    T* ptr = m_ptr;
    m_ptr = 0;
    return ptr;
}


// DeepArrayDestroyGuard

inline DeepArrayDestroyGuard::DeepArrayDestroyGuard() REALM_NOEXCEPT:
    m_ptr(0)
{
}

inline DeepArrayDestroyGuard::DeepArrayDestroyGuard(Array* ptr) REALM_NOEXCEPT:
    m_ptr(ptr)
{
}

inline DeepArrayDestroyGuard::~DeepArrayDestroyGuard() REALM_NOEXCEPT
{
    if (m_ptr)
        m_ptr->destroy_deep();
}

inline void DeepArrayDestroyGuard::reset(Array* ptr) REALM_NOEXCEPT
{
    if (m_ptr)
        m_ptr->destroy_deep();
    m_ptr = ptr;
}

inline Array* DeepArrayDestroyGuard::get() const REALM_NOEXCEPT
{
    return m_ptr;
}

inline Array* DeepArrayDestroyGuard::release() REALM_NOEXCEPT
{
    Array* ptr = m_ptr;
    m_ptr = 0;
    return ptr;
}


// DeepArrayRefDestroyGuard

inline DeepArrayRefDestroyGuard::DeepArrayRefDestroyGuard(Allocator& alloc) REALM_NOEXCEPT:
    m_ref(0),
    m_alloc(alloc)
{
}

inline DeepArrayRefDestroyGuard::DeepArrayRefDestroyGuard(ref_type ref,
                                                          Allocator& alloc) REALM_NOEXCEPT:
    m_ref(ref),
    m_alloc(alloc)
{
}

inline DeepArrayRefDestroyGuard::~DeepArrayRefDestroyGuard() REALM_NOEXCEPT
{
    if (m_ref)
        Array::destroy_deep(m_ref, m_alloc);
}

inline void DeepArrayRefDestroyGuard::reset(ref_type ref) REALM_NOEXCEPT
{
    if (m_ref)
        Array::destroy_deep(m_ref, m_alloc);
    m_ref = ref;
}

inline ref_type DeepArrayRefDestroyGuard::get() const REALM_NOEXCEPT
{
    return m_ref;
}

inline ref_type DeepArrayRefDestroyGuard::release() REALM_NOEXCEPT
{
    ref_type ref = m_ref;
    m_ref = 0;
    return ref;
}


} // namespace _impl
} // namespace realm

#endif // REALM_IMPL_DESTROY_GUARD_HPP
