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
#ifndef REALM_TABLE_REF_HPP
#define REALM_TABLE_REF_HPP

#include <cstddef>
#include <algorithm>

#include <realm/util/bind_ptr.hpp>

namespace realm {


class Table;
template<class> class BasicTable;


/// A reference-counting "smart pointer" for referring to table
/// accessors.
///
/// The purpose of this smart pointer is to keep the referenced table
/// accessor alive for as long as anybody is referring to it, however,
/// for stack allocated table accessors, the lifetime is necessarily
/// determined by scope (see below).
///
/// Please take note of the distinction between a "table" and a "table
/// accessor" here. A table accessor is an instance of `Table` or
/// `BasicTable<Spec>`, and it may, or may not be attached to an
/// actual table at any specific point in time, but this state of
/// attachment of the accessor has nothing to do with the function of
/// the smart pointer. Also, in the rest of the documentation of this
/// class, whenever you see `Table::foo`, you are supposed to read it
/// as, `Table::foo` or `BasicTable<Spec>::foo`.
///
///
/// Table accessors are either created directly by an application via
/// a call to one of the public table constructors, or they are
/// created internally by the Realm library, such as when the
/// application calls Group::get_table(), Table::get_subtable(), or
/// Table::create().
///
/// Applications can safely assume that all table accessors, created
/// internally by the Realm library, have a lifetime that is managed
/// by reference counting. This means that the application can prolong
/// the lifetime of *such* table accessors indefinitely by holding on
/// to at least one smart pointer, but note that the guarantee of the
/// continued existence of the accessor, does not imply that the
/// accessor remains attached to the underlying table (see
/// Table::is_attached() for details). Accessors whose lifetime are
/// controlled by reference counting are destroyed exactly when the
/// reference count drops to zero.
///
/// When an application creates a new table accessor by a direct call
/// to one of the public constructors, the lifetime of that table
/// accessor is *not*, and cannot be managed by reference
/// counting. This is true regardless of the way the accessor is
/// created (i.e., regardless of whether it is an automatic variable
/// on the stack, or created on the heap using `new`). However, for
/// convenience, but with one important caveat, it is still possible
/// to use smart pointers to refer to such accessors. The caveat is
/// that no smart pointers are allowed to refer to the accessor at the
/// point in time when its destructor is called. It is entirely the
/// responsibility of the application to ensure that this requirement
/// is met. Failing to do so, will result in undefined
/// behavior. Finally, please note that an application is always free
/// to use Table::create() as an alternative to creating free-standing
/// top-level tables on the stack, and that this is indeed neccessary
/// when fully reference counted lifetimes are required.
///
/// So, at any time, and for any table accessor, an application can
/// call Table::get_table_ref() to obtain a smart pointer that refers
/// to that table, however, while that is always possible and safe, it
/// is not always possible to extend the lifetime of an accessor by
/// holding on to a smart pointer. The question of whether that is
/// possible, depends directly on the way the accessor was created.
///
///
/// Apart from keeping track of the number of references, these smart
/// pointers behaves almost exactly like regular pointers. In
/// particular, it is possible to dereference a TableRef and get a
/// `Table&` out of it, however, if you are not careful, this can
/// easily lead to dangling references:
///
/// \code{.cpp}
///
///   Table& sub_1 = *(table.get_subtable(0,0));
///   sub_1.add_empty_row(); // Oops, sub_1 may be dangling!
///
/// \endcode
///
/// Whether `sub_1` is actually dangling in the example above will
/// depend on whether other references to the same subtable accessor
/// already exist, but it is never wise to rely in this. Here is a
/// safe and proper alternative:
///
/// \code{.cpp}
///
///   TableRef sub_2 = table.get_subtable(0,0);
///   sub_2.add_empty_row(); // Safe!
///
///   void do_something(Table&);
///   do_something(*(table.get_subtable(0,0))); // Also safe!
///
/// \endcode
///
///
/// This class provides a form of move semantics that is compatible
/// with C++03. It is similar to, but not as powerful as what is
/// provided natively by C++11. Instead of using `std::move()` (in
/// C++11), one must use `move()` without qualification. This will
/// call a special function that is a friend of this class. The
/// effectiveness of this form of move semantics relies on 'return
/// value optimization' being enabled in the compiler.
///
/// \sa Table
/// \sa TableRef
template<class T> class BasicTableRef: util::bind_ptr<T> {
public:
    REALM_CONSTEXPR BasicTableRef() REALM_NOEXCEPT {}
    ~BasicTableRef() REALM_NOEXCEPT {}

#ifdef REALM_HAVE_CXX11_RVALUE_REFERENCE

    // Copy construct
    BasicTableRef(const BasicTableRef& r) REALM_NOEXCEPT: util::bind_ptr<T>(r) {}
    template<class U> BasicTableRef(const BasicTableRef<U>& r) REALM_NOEXCEPT:
        util::bind_ptr<T>(r) {}

    // Copy assign
    BasicTableRef& operator=(const BasicTableRef&) REALM_NOEXCEPT;
    template<class U> BasicTableRef& operator=(const BasicTableRef<U>&) REALM_NOEXCEPT;

    // Move construct
    BasicTableRef(BasicTableRef&& r) REALM_NOEXCEPT: util::bind_ptr<T>(std::move(r)) {}
    template<class U> BasicTableRef(BasicTableRef<U>&& r) REALM_NOEXCEPT:
        util::bind_ptr<T>(std::move(r)) {}

    // Move assign
    BasicTableRef& operator=(BasicTableRef&&) REALM_NOEXCEPT;
    template<class U> BasicTableRef& operator=(BasicTableRef<U>&&) REALM_NOEXCEPT;

#else // !REALM_HAVE_CXX11_RVALUE_REFERENCE

    // Copy construct
    BasicTableRef(const BasicTableRef& r) REALM_NOEXCEPT: util::bind_ptr<T>(r) {}
    template<class U> BasicTableRef(BasicTableRef<U> r) REALM_NOEXCEPT:
        util::bind_ptr<T>(move(r)) {}

    // Copy assign
    BasicTableRef& operator=(BasicTableRef) REALM_NOEXCEPT;
    template<class U> BasicTableRef& operator=(BasicTableRef<U>) REALM_NOEXCEPT;

#endif // !REALM_HAVE_CXX11_RVALUE_REFERENCE

    // Replacement for std::move() in C++03
    friend BasicTableRef move(BasicTableRef& r) REALM_NOEXCEPT
    {
        return BasicTableRef(&r, move_tag());
    }

    //@{
    /// Comparison
    template<class U> bool operator==(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator==(U*) const REALM_NOEXCEPT;
    template<class U> bool operator!=(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator!=(U*) const REALM_NOEXCEPT;
    template<class U> bool operator<(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator<(U*) const REALM_NOEXCEPT;
    template<class U> bool operator>(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator>(U*) const REALM_NOEXCEPT;
    template<class U> bool operator<=(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator<=(U*) const REALM_NOEXCEPT;
    template<class U> bool operator>=(const BasicTableRef<U>&) const REALM_NOEXCEPT;
    template<class U> bool operator>=(U*) const REALM_NOEXCEPT;
    //@}

    // Dereference
#ifdef __clang__
    // Clang has a bug that causes it to effectively ignore the 'using' declaration.
    T& operator*() const REALM_NOEXCEPT { return util::bind_ptr<T>::operator*(); }
#else
    using util::bind_ptr<T>::operator*;
#endif
    using util::bind_ptr<T>::operator->;

#ifdef REALM_HAVE_CXX11_EXPLICIT_CONV_OPERATORS
    using util::bind_ptr<T>::operator bool;
#else
#  ifdef __clang__
    // Clang 3.0 and 3.1 has a bug that causes it to effectively
    // ignore the 'using' declaration.
    typedef typename util::bind_ptr<T>::unspecified_bool_type unspecified_bool_type;
    operator unspecified_bool_type() const REALM_NOEXCEPT
    {
        return util::bind_ptr<T>::operator unspecified_bool_type();
    }
#  else
    using util::bind_ptr<T>::operator typename util::bind_ptr<T>::unspecified_bool_type;
#  endif
#endif

    T* get() const REALM_NOEXCEPT { return util::bind_ptr<T>::get(); }
    void reset() REALM_NOEXCEPT { util::bind_ptr<T>::reset(); }
    void reset(T* t) REALM_NOEXCEPT { util::bind_ptr<T>::reset(t); }

    void swap(BasicTableRef& r) REALM_NOEXCEPT { this->util::bind_ptr<T>::swap(r); }
    friend void swap(BasicTableRef& a, BasicTableRef& b) REALM_NOEXCEPT { a.swap(b); }

    template<class U>
    friend BasicTableRef<U> unchecked_cast(BasicTableRef<Table>) REALM_NOEXCEPT;
    template<class U>
    friend BasicTableRef<const U> unchecked_cast(BasicTableRef<const Table>) REALM_NOEXCEPT;

private:
    template<class> struct GetRowAccType { typedef void type; };
    template<class Spec> struct GetRowAccType<BasicTable<Spec>> {
        typedef typename BasicTable<Spec>::RowAccessor type;
    };
    template<class Spec> struct GetRowAccType<const BasicTable<Spec>> {
        typedef typename BasicTable<Spec>::ConstRowAccessor type;
    };
    typedef typename GetRowAccType<T>::type RowAccessor;

public:
    /// Same as 'table[i]' where 'table' is the referenced table.
    RowAccessor operator[](std::size_t i) const REALM_NOEXCEPT { return (*this->get())[i]; }

private:
    friend class ColumnSubtableParent;
    friend class Table;
    friend class Group;
    template<class> friend class BasicTable;
    template<class> friend class BasicTableRef;

    explicit BasicTableRef(T* t) REALM_NOEXCEPT: util::bind_ptr<T>(t) {}

    typedef typename util::bind_ptr<T>::move_tag move_tag;
    BasicTableRef(BasicTableRef* r, move_tag) REALM_NOEXCEPT:
        util::bind_ptr<T>(r, move_tag()) {}

    typedef typename util::bind_ptr<T>::casting_move_tag casting_move_tag;
    template<class U> BasicTableRef(BasicTableRef<U>* r, casting_move_tag) REALM_NOEXCEPT:
        util::bind_ptr<T>(r, casting_move_tag()) {}
};


typedef BasicTableRef<Table> TableRef;
typedef BasicTableRef<const Table> ConstTableRef;


template<class C, class T, class U>
inline std::basic_ostream<C,T>& operator<<(std::basic_ostream<C,T>& out, const BasicTableRef<U>& p)
{
    out << static_cast<const void*>(&*p);
    return out;
}

template<class T> inline BasicTableRef<T> unchecked_cast(TableRef t) REALM_NOEXCEPT
{
    return BasicTableRef<T>(&t, typename BasicTableRef<T>::casting_move_tag());
}

template<class T> inline BasicTableRef<const T> unchecked_cast(ConstTableRef t) REALM_NOEXCEPT
{
    return BasicTableRef<const T>(&t, typename BasicTableRef<T>::casting_move_tag());
}


//@{
/// Comparison
template<class T, class U> bool operator==(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
template<class T, class U> bool operator!=(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
template<class T, class U> bool operator<(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
template<class T, class U> bool operator>(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
template<class T, class U> bool operator<=(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
template<class T, class U> bool operator>=(T*, const BasicTableRef<U>&) REALM_NOEXCEPT;
//@}





// Implementation:

#ifdef REALM_HAVE_CXX11_RVALUE_REFERENCE

template<class T>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(const BasicTableRef& r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(r);
    return *this;
}

template<class T> template<class U>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(const BasicTableRef<U>& r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(r);
    return *this;
}

template<class T>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(BasicTableRef&& r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(std::move(r));
    return *this;
}

template<class T> template<class U>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(BasicTableRef<U>&& r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(std::move(r));
    return *this;
}

#else // !REALM_HAVE_CXX11_RVALUE_REFERENCE

template<class T>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(BasicTableRef r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(move(static_cast<util::bind_ptr<T>&>(r)));
    return *this;
}

template<class T> template<class U>
inline BasicTableRef<T>& BasicTableRef<T>::operator=(BasicTableRef<U> r) REALM_NOEXCEPT
{
    this->util::bind_ptr<T>::operator=(move(static_cast<util::bind_ptr<U>&>(r)));
    return *this;
}

#endif // !REALM_HAVE_CXX11_RVALUE_REFERENCE

template<class T> template<class U>
bool BasicTableRef<T>::operator==(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() == p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator==(U* p) const REALM_NOEXCEPT
{
    return get() == p;
}

template<class T> template<class U>
bool BasicTableRef<T>::operator!=(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() != p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator!=(U* p) const REALM_NOEXCEPT
{
    return get() != p;
}

template<class T> template<class U>
bool BasicTableRef<T>::operator<(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() < p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator<(U* p) const REALM_NOEXCEPT
{
    return get() < p;
}

template<class T> template<class U>
bool BasicTableRef<T>::operator>(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() > p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator>(U* p) const REALM_NOEXCEPT
{
    return get() > p;
}

template<class T> template<class U>
bool BasicTableRef<T>::operator<=(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() <= p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator<=(U* p) const REALM_NOEXCEPT
{
    return get() <= p;
}

template<class T> template<class U>
bool BasicTableRef<T>::operator>=(const BasicTableRef<U>& p) const REALM_NOEXCEPT
{
    return get() >= p.get();
}

template<class T> template<class U> bool BasicTableRef<T>::operator>=(U* p) const REALM_NOEXCEPT
{
    return get() >= p;
}

template<class T, class U> bool operator==(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b == a;
}

template<class T, class U> bool operator!=(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b != a;
}

template<class T, class U> bool operator<(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b > a;
}

template<class T, class U> bool operator>(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b < a;
}

template<class T, class U> bool operator<=(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b >= a;
}

template<class T, class U> bool operator>=(T* a, const BasicTableRef<U>& b) REALM_NOEXCEPT
{
    return b <= a;
}


} // namespace realm

#endif // REALM_TABLE_REF_HPP
