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
#ifndef REALM_UTIL_BIND_HPP
#define REALM_UTIL_BIND_HPP

namespace realm {


namespace _impl {


template<class A> class FunOneArgBinder0 {
public:
    FunOneArgBinder0(void (*fun)(A), const A& a):
        m_fun(fun),
        m_a(a)
    {
    }
    void operator()() const
    {
        (*m_fun)(m_a);
    }
private:
    void (*const m_fun)(A);
    const A m_a;
};

template<class A, class B> class FunOneArgBinder1 {
public:
    FunOneArgBinder1(void (*fun)(A,B), const A& a):
        m_fun(fun),
        m_a(a)
    {
    }
    void operator()(B b) const
    {
        (*m_fun)(m_a, b);
    }
private:
    void (*const m_fun)(A,B);
    const A m_a;
};

template<class A, class B, class C> class FunOneArgBinder2 {
public:
    FunOneArgBinder2(void (*fun)(A,B,C), const A& a):
        m_fun(fun),
        m_a(a)
    {
    }
    void operator()(B b, C c) const
    {
        (*m_fun)(m_a, b, c);
    }
private:
    void (*const m_fun)(A,B,C);
    const A m_a;
};



template<class A, class B> class FunTwoArgBinder0 {
public:
    FunTwoArgBinder0(void (*fun)(A,B), const A& a, const B& b):
        m_fun(fun),
        m_a(a),
        m_b(b)
    {
    }
    void operator()() const
    {
        (*m_fun)(m_a, m_b);
    }
private:
    void (*const m_fun)(A,B);
    const A m_a;
    const B m_b;
};

template<class A, class B, class C> class FunTwoArgBinder1 {
public:
    FunTwoArgBinder1(void (*fun)(A,B,C), const A& a, const B& b):
        m_fun(fun),
        m_a(a),
        m_b(b)
    {
    }
    void operator()(C c) const
    {
        (*m_fun)(m_a, m_b, c);
    }
private:
    void (*const m_fun)(A,B,C);
    const A m_a;
    const B m_b;
};

template<class A, class B, class C, class D> class FunTwoArgBinder2 {
public:
    FunTwoArgBinder2(void (*fun)(A,B,C,D), const A& a, const B& b):
        m_fun(fun),
        m_a(a),
        m_b(b)
    {
    }
    void operator()(C c, D d) const
    {
        (*m_fun)(m_a, m_b, c, d);
    }
private:
    void (*const m_fun)(A,B,C,D);
    const A m_a;
    const B m_b;
};



template<class A, class B, class C> class FunThreeArgBinder0 {
public:
    FunThreeArgBinder0(void (*fun)(A,B,C), const A& a, const B& b, const C& c):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()() const
    {
        (*m_fun)(m_a, m_b, m_c);
    }
private:
    void (*const m_fun)(A,B,C);
    const A m_a;
    const B m_b;
    const C m_c;
};

template<class A, class B, class C, class D> class FunThreeArgBinder1 {
public:
    FunThreeArgBinder1(void (*fun)(A,B,C,D), const A& a, const B& b, const C& c):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()(D d) const
    {
        (*m_fun)(m_a, m_b, m_c, d);
    }
private:
    void (*const m_fun)(A,B,C,D);
    const A m_a;
    const B m_b;
    const C m_c;
};

template<class A, class B, class C, class D, class E> class FunThreeArgBinder2 {
public:
    FunThreeArgBinder2(void (*fun)(A,B,C,D,E), const A& a, const B& b, const C& c):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()(D d, E e) const
    {
        (*m_fun)(m_a, m_b, m_c, d, e);
    }
private:
    void (*const m_fun)(A,B,C,D,E);
    const A m_a;
    const B m_b;
    const C m_c;
};



template<class A, class B, class C, class D> class FunFourArgBinder0 {
public:
    FunFourArgBinder0(void (*fun)(A,B,C,D), const A& a, const B& b, const C& c, const D& d):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()() const
    {
        (*m_fun)(m_a, m_b, m_c, m_d);
    }
private:
    void (*const m_fun)(A,B,C,D);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};

template<class A, class B, class C, class D, class E> class FunFourArgBinder1 {
public:
    FunFourArgBinder1(void (*fun)(A,B,C,D,E), const A& a, const B& b, const C& c, const D& d):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()(E e) const
    {
        (*m_fun)(m_a, m_b, m_c, m_d, e);
    }
private:
    void (*const m_fun)(A,B,C,D,E);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};

template<class A, class B, class C, class D, class E, class F> class FunFourArgBinder2 {
public:
    FunFourArgBinder2(void (*fun)(A,B,C,D,E,F), const A& a, const B& b, const C& c, const D& d):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()(E e, F f) const
    {
        (*m_fun)(m_a, m_b, m_c, m_d, e, f);
    }
private:
    void (*const m_fun)(A,B,C,D,E,F);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};



template<class A, class B, class C, class D, class E> class FunFiveArgBinder0 {
public:
    FunFiveArgBinder0(void (*fun)(A,B,C,D,E), const A& a, const B& b, const C& c, const D& d,
                      const E& e):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d),
        m_e(e)
    {
    }
    void operator()() const
    {
        (*m_fun)(m_a, m_b, m_c, m_d, m_e);
    }
private:
    void (*const m_fun)(A,B,C,D,E);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
    const E m_e;
};

template<class A, class B, class C, class D, class E, class F> class FunFiveArgBinder1 {
public:
    FunFiveArgBinder1(void (*fun)(A,B,C,D,E,F), const A& a, const B& b, const C& c, const D& d,
                      const E& e):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d),
        m_e(e)
    {
    }
    void operator()(F f) const
    {
        (*m_fun)(m_a, m_b, m_c, m_d, m_e, f);
    }
private:
    void (*const m_fun)(A,B,C,D,E,F);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
    const E m_e;
};

template<class A, class B, class C, class D, class E, class F, class G> class FunFiveArgBinder2 {
public:
    FunFiveArgBinder2(void (*fun)(A,B,C,D,E,F,G), const A& a, const B& b, const C& c, const D& d,
                      const E& e):
        m_fun(fun),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d),
        m_e(e)
    {
    }
    void operator()(F f, G g) const
    {
        (*m_fun)(m_a, m_b, m_c, m_d, m_e, f, g);
    }
private:
    void (*const m_fun)(A,B,C,D,E,F,G);
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
    const E m_e;
};



template<class O> class MemFunObjZeroArgBinder0 {
public:
    MemFunObjZeroArgBinder0(void (O::*mem_fun)(), O* obj):
        m_mem_fun(mem_fun),
        m_obj(obj)
    {
    }
    void operator()() const
    {
        (m_obj->*m_mem_fun)();
    }
private:
    void (O::*const m_mem_fun)();
    O* const m_obj;
};

template<class O, class A> class MemFunObjZeroArgBinder1 {
public:
    MemFunObjZeroArgBinder1(void (O::*mem_fun)(A), O* obj):
        m_mem_fun(mem_fun),
        m_obj(obj)
    {
    }
    void operator()(A a) const
    {
        (m_obj->*m_mem_fun)(a);
    }
private:
    void (O::*const m_mem_fun)(A);
    O* const m_obj;
};

template<class O, class A, class B> class MemFunObjZeroArgBinder2 {
public:
    MemFunObjZeroArgBinder2(void (O::*mem_fun)(A,B), O* obj):
        m_mem_fun(mem_fun),
        m_obj(obj)
    {
    }
    void operator()(A a, B b) const
    {
        (m_obj->*m_mem_fun)(a,b);
    }
private:
    void (O::*const m_mem_fun)(A,B);
    O* const m_obj;
};



template<class O, class A> class MemFunObjOneArgBinder0 {
public:
    MemFunObjOneArgBinder0(void (O::*mem_fun)(A), O* obj, const A& a):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a)
    {
    }
    void operator()() const
    {
        (m_obj->*m_mem_fun)(m_a);
    }
private:
    void (O::*const m_mem_fun)(A);
    O* const m_obj;
    const A m_a;
};

template<class O, class A, class B> class MemFunObjOneArgBinder1 {
public:
    MemFunObjOneArgBinder1(void (O::*mem_fun)(A,B), O* obj, const A& a):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a)
    {
    }
    void operator()(B b) const
    {
        (m_obj->*m_mem_fun)(m_a, b);
    }
private:
    void (O::*const m_mem_fun)(A,B);
    O* const m_obj;
    const A m_a;
};

template<class O, class A, class B, class C> class MemFunObjOneArgBinder2 {
public:
    MemFunObjOneArgBinder2(void (O::*mem_fun)(A,B,C), O* obj, const A& a):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a)
    {
    }
    void operator()(B b, C c) const
    {
        (m_obj->*m_mem_fun)(m_a, b, c);
    }
private:
    void (O::*const m_mem_fun)(A,B,C);
    O* const m_obj;
    const A m_a;
};



template<class O, class A, class B> class MemFunObjTwoArgBinder0 {
public:
    MemFunObjTwoArgBinder0(void (O::*mem_fun)(A,B), O* obj, const A& a, const B& b):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b)
    {
    }
    void operator()() const
    {
        (m_obj->*m_mem_fun)(m_a, m_b);
    }
private:
    void (O::*const m_mem_fun)(A,B);
    O* const m_obj;
    const A m_a;
    const B m_b;
};

template<class O, class A, class B, class C> class MemFunObjTwoArgBinder1 {
public:
    MemFunObjTwoArgBinder1(void (O::*mem_fun)(A,B,C), O* obj, const A& a, const B& b):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b)
    {
    }
    void operator()(C c) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, c);
    }
private:
    void (O::*const m_mem_fun)(A,B,C);
    O* const m_obj;
    const A m_a;
    const B m_b;
};

template<class O, class A, class B, class C, class D> class MemFunObjTwoArgBinder2 {
public:
    MemFunObjTwoArgBinder2(void (O::*mem_fun)(A,B,C,D), O* obj, const A& a, const B& b):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b)
    {
    }
    void operator()(C c, D d) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, c, d);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D);
    O* const m_obj;
    const A m_a;
    const B m_b;
};



template<class O, class A, class B, class C> class MemFunObjThreeArgBinder0 {
public:
    MemFunObjThreeArgBinder0(void (O::*mem_fun)(A,B,C), O* obj, const A& a, const B& b,
                             const C& c):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()() const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c);
    }
private:
    void (O::*const m_mem_fun)(A,B,C);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
};

template<class O, class A, class B, class C, class D> class MemFunObjThreeArgBinder1 {
public:
    MemFunObjThreeArgBinder1(void (O::*mem_fun)(A,B,C,D), O* obj, const A& a, const B& b,
                             const C& c):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()(D d) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c, d);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
};

template<class O, class A, class B, class C, class D, class E> class MemFunObjThreeArgBinder2 {
public:
    MemFunObjThreeArgBinder2(void (O::*mem_fun)(A,B,C,D,E), O* obj, const A& a, const B& b,
                             const C& c):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c)
    {
    }
    void operator()(D d, E e) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c, d, e);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D,E);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
};



template<class O, class A, class B, class C, class D> class MemFunObjFourArgBinder0 {
public:
    MemFunObjFourArgBinder0(void (O::*mem_fun)(A,B,C,D), O* obj, const A& a, const B& b,
                            const C& c, const D& d):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()() const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c, m_d);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};

template<class O, class A, class B, class C, class D, class E> class MemFunObjFourArgBinder1 {
public:
    MemFunObjFourArgBinder1(void (O::*mem_fun)(A,B,C,D,E), O* obj, const A& a, const B& b,
                            const C& c, const D& d):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()(E e) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c, m_d, e);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D,E);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};

template<class O, class A, class B, class C, class D, class E, class F>
class MemFunObjFourArgBinder2 {
public:
    MemFunObjFourArgBinder2(void (O::*mem_fun)(A,B,C,D,E,F), O* obj, const A& a, const B& b,
                            const C& c, const D& d):
        m_mem_fun(mem_fun),
        m_obj(obj),
        m_a(a),
        m_b(b),
        m_c(c),
        m_d(d)
    {
    }
    void operator()(E e, F f) const
    {
        (m_obj->*m_mem_fun)(m_a, m_b, m_c, m_d, e, f);
    }
private:
    void (O::*const m_mem_fun)(A,B,C,D,E,F);
    O* const m_obj;
    const A m_a;
    const B m_b;
    const C m_c;
    const D m_d;
};


} // namespace _impl



namespace util {


/// Produce a nullary function by binding the argument of a unary function.
template<class A>
inline _impl::FunOneArgBinder0<A> bind(void (*fun)(A), const A& a)
{
    return _impl::FunOneArgBinder0<A>(fun, a);
}

/// Produce a unary function by binding the first argument of a binary function.
template<class A, class B>
inline _impl::FunOneArgBinder1<A,B> bind(void (*fun)(A,B), const A& a)
{
    return _impl::FunOneArgBinder1<A,B>(fun, a);
}

/// Produce a binary function by binding the first argument of a ternary
/// function.
template<class A, class B, class C>
inline _impl::FunOneArgBinder2<A,B,C> bind(void (*fun)(A,B,C), const A& a)
{
    return _impl::FunOneArgBinder2<A,B,C>(fun, a);
}



/// Produce a nullary function by binding both arguments of a binary function.
template<class A, class B>
inline _impl::FunTwoArgBinder0<A,B> bind(void (*fun)(A,B), const A& a, const B& b)
{
    return _impl::FunTwoArgBinder0<A,B>(fun, a, b);
}

/// Produce a unary function by binding the first two arguments of a ternary
/// function.
template<class A, class B, class C>
inline _impl::FunTwoArgBinder1<A,B,C> bind(void (*fun)(A,B,C), const A& a, const B& b)
{
    return _impl::FunTwoArgBinder1<A,B,C>(fun, a, b);
}

/// Produce a binary function by binding the first two arguments of a function
/// taking 4 arguments.
template<class A, class B, class C, class D>
inline _impl::FunTwoArgBinder2<A,B,C,D> bind(void (*fun)(A,B,C,D), const A& a, const B& b)
{
    return _impl::FunTwoArgBinder2<A,B,C,D>(fun, a, b);
}



/// Produce a nullary function by binding all three arguments of a ternary
/// function.
template<class A, class B, class C>
inline _impl::FunThreeArgBinder0<A,B,C> bind(void (*fun)(A,B,C), const A& a,
                                             const B& b, const C& c)
{
    return _impl::FunThreeArgBinder0<A,B,C>(fun, a, b, c);
}

/// Produce a unary function by binding the first three arguments of a function
/// taking 4 arguments.
template<class A, class B, class C, class D>
inline _impl::FunThreeArgBinder1<A,B,C,D> bind(void (*fun)(A,B,C,D), const A& a,
                                               const B& b, const C& c)
{
    return _impl::FunThreeArgBinder1<A,B,C,D>(fun, a, b, c);
}

/// Produce a binary function by binding the first three arguments of a function
/// taking 5 arguments.
template<class A, class B, class C, class D, class E>
inline _impl::FunThreeArgBinder2<A,B,C,D,E> bind(void (*fun)(A,B,C,D,E), const A& a,
                                                 const B& b, const C& c)
{
    return _impl::FunThreeArgBinder2<A,B,C,D,E>(fun, a, b, c);
}



/// Produce a nullary function by binding all 4 arguments of a function taking 4
/// arguments.
template<class A, class B, class C, class D>
inline _impl::FunFourArgBinder0<A,B,C,D> bind(void (*fun)(A,B,C,D), const A& a,
                                              const B& b, const C& c, const D& d)
{
    return _impl::FunFourArgBinder0<A,B,C,D>(fun, a, b, c, d);
}

/// Produce a unary function by binding the first 4 arguments of a function
/// taking 5 arguments.
template<class A, class B, class C, class D, class E>
inline _impl::FunFourArgBinder1<A,B,C,D,E> bind(void (*fun)(A,B,C,D,E), const A& a,
                                                const B& b, const C& c, const D& d)
{
    return _impl::FunFourArgBinder1<A,B,C,D,E>(fun, a, b, c, d);
}

/// Produce a binary function by binding the first 4 arguments of a function
/// taking 6 arguments.
template<class A, class B, class C, class D, class E, class F>
inline _impl::FunFourArgBinder2<A,B,C,D,E,F> bind(void (*fun)(A,B,C,D,E,F), const A& a,
                                                  const B& b, const C& c, const D& d)
{
    return _impl::FunFourArgBinder2<A,B,C,D,E,F>(fun, a, b, c, d);
}



/// Produce a nullary function by binding all 5 arguments of a function taking 5
/// arguments.
template<class A, class B, class C, class D, class E>
inline _impl::FunFiveArgBinder0<A,B,C,D,E> bind(void (*fun)(A,B,C,D,E), const A& a,
                                                const B& b, const C& c, const D& d,
                                                const E& e)
{
    return _impl::FunFiveArgBinder0<A,B,C,D,E>(fun, a, b, c, d, e);
}

/// Produce a unary function by binding the first 5 arguments of a function
/// taking 6 arguments.
template<class A, class B, class C, class D, class E, class F>
inline _impl::FunFiveArgBinder1<A,B,C,D,E,F> bind(void (*fun)(A,B,C,D,E,F), const A& a,
                                                  const B& b, const C& c, const D& d,
                                                  const E& e)
{
    return _impl::FunFiveArgBinder1<A,B,C,D,E,F>(fun, a, b, c, d, e);
}

/// Produce a binary function by binding the first 5 arguments of a function
/// taking 7 arguments.
template<class A, class B, class C, class D, class E, class F, class G>
inline _impl::FunFiveArgBinder2<A,B,C,D,E,F,G> bind(void (*fun)(A,B,C,D,E,F,G), const A& a,
                                                    const B& b, const C& c, const D& d,
                                                    const E& e)
{
    return _impl::FunFiveArgBinder2<A,B,C,D,E,F,G>(fun, a, b, c, d, e);
}



/// Produce a nullary function by binding the target object of a non-static
/// member function taking no arguments.
template<class O>
inline _impl::MemFunObjZeroArgBinder0<O> bind(void (O::*mem_fun)(), O* obj)
{
    return _impl::MemFunObjZeroArgBinder0<O>(mem_fun, obj);
}

/// Produce a unary function by binding the target object of a non-static member
/// function taking one argument.
template<class O, class A>
inline _impl::MemFunObjZeroArgBinder1<O,A> bind(void (O::*mem_fun)(A), O* obj)
{
    return _impl::MemFunObjZeroArgBinder1<O,A>(mem_fun, obj);
}

/// Produce a binary function by binding the target object of a non-static
/// member function taking two arguments.
template<class O, class A, class B>
inline _impl::MemFunObjZeroArgBinder2<O,A,B> bind(void (O::*mem_fun)(A,B), O* obj)
{
    return _impl::MemFunObjZeroArgBinder2<O,A,B>(mem_fun, obj);
}



/// Produce a nullary function by binding the target object and the only
/// argument of a non-static member function taking one argument.
template<class O, class A>
inline _impl::MemFunObjOneArgBinder0<O,A> bind(void (O::*mem_fun)(A), O* obj, const A& a)
{
    return _impl::MemFunObjOneArgBinder0<O,A>(mem_fun, obj, a);
}

/// Produce a unary function by binding the target object and first argument of
/// a non-static member function taking two arguments.
template<class O, class A, class B>
inline _impl::MemFunObjOneArgBinder1<O,A,B> bind(void (O::*mem_fun)(A,B), O* obj, const A& a)
{
    return _impl::MemFunObjOneArgBinder1<O,A,B>(mem_fun, obj, a);
}

/// Produce a binary function by binding the target object and first argument of
/// a non-static member function taking three arguments.
template<class O, class A, class B, class C>
inline _impl::MemFunObjOneArgBinder2<O,A,B,C> bind(void (O::*mem_fun)(A,B,C), O* obj, const A& a)
{
    return _impl::MemFunObjOneArgBinder2<O,A,B,C>(mem_fun, obj, a);
}



/// Produce a nullary function by binding the target object and both arguments
/// of a non-static member function taking two arguments.
template<class O, class A, class B>
inline _impl::MemFunObjTwoArgBinder0<O,A,B> bind(void (O::*mem_fun)(A,B), O* obj,
                                                 const A& a, const B& b)
{
    return _impl::MemFunObjTwoArgBinder0<O,A,B>(mem_fun, obj, a, b);
}

/// Produce a unary function by binding the target object and the first two
/// arguments of a non-static member function taking three arguments.
template<class O, class A, class B, class C>
inline _impl::MemFunObjTwoArgBinder1<O,A,B,C> bind(void (O::*mem_fun)(A,B,C), O* obj,
                                                   const A& a, const B& b)
{
    return _impl::MemFunObjTwoArgBinder1<O,A,B,C>(mem_fun, obj, a, b);
}

/// Produce a binary function by binding the target object and the first two
/// arguments of a non-static member function taking 4 arguments.
template<class O, class A, class B, class C, class D>
inline _impl::MemFunObjTwoArgBinder2<O,A,B,C,D> bind(void (O::*mem_fun)(A,B,C,D), O* obj,
                                                     const A& a, const B& b)
{
    return _impl::MemFunObjTwoArgBinder2<O,A,B,C,D>(mem_fun, obj, a, b);
}



/// Produce a nullary function by binding the target object and all three
/// arguments of a non-static member function taking three arguments.
template<class O, class A, class B, class C>
inline _impl::MemFunObjThreeArgBinder0<O,A,B,C> bind(void (O::*mem_fun)(A,B,C), O* obj,
                                                     const A& a, const B& b, const C& c)
{
    return _impl::MemFunObjThreeArgBinder0<O,A,B,C>(mem_fun, obj, a, b, c);
}

/// Produce a unary function by binding the target object and the first three
/// arguments of a non-static member function taking 4 arguments.
template<class O, class A, class B, class C, class D>
inline _impl::MemFunObjThreeArgBinder1<O,A,B,C,D> bind(void (O::*mem_fun)(A,B,C,D), O* obj,
                                                       const A& a, const B& b, const C& c)
{
    return _impl::MemFunObjThreeArgBinder1<O,A,B,C,D>(mem_fun, obj, a, b, c);
}

/// Produce a binary function by binding the target object and the first three
/// arguments of a non-static member function taking 5 arguments.
template<class O, class A, class B, class C, class D, class E>
inline _impl::MemFunObjThreeArgBinder2<O,A,B,C,D,E> bind(void (O::*mem_fun)(A,B,C,D,E), O* obj,
                                                         const A& a, const B& b, const C& c)
{
    return _impl::MemFunObjThreeArgBinder2<O,A,B,C,D,E>(mem_fun, obj, a, b, c);
}



/// Produce a nullary function by binding the target object and all 4 arguments
/// of a non-static member function taking 4 arguments.
template<class O, class A, class B, class C, class D>
inline _impl::MemFunObjFourArgBinder0<O,A,B,C,D> bind(void (O::*mem_fun)(A,B,C,D), O* obj,
                                                      const A& a, const B& b, const C& c,
                                                      const D& d)
{
    return _impl::MemFunObjFourArgBinder0<O,A,B,C,D>(mem_fun, obj, a, b, c, d);
}

/// Produce a unary function by binding the target object and the first 4
/// arguments of a non-static member function taking 5 arguments.
template<class O, class A, class B, class C, class D, class E>
inline _impl::MemFunObjFourArgBinder1<O,A,B,C,D,E> bind(void (O::*mem_fun)(A,B,C,D,E), O* obj,
                                                        const A& a, const B& b, const C& c,
                                                        const D& d)
{
    return _impl::MemFunObjFourArgBinder1<O,A,B,C,D,E>(mem_fun, obj, a, b, c, d);
}

/// Produce a binary function by binding the target object and the first 4
/// arguments of a non-static member function taking 6 arguments.
template<class O, class A, class B, class C, class D, class E, class F>
inline _impl::MemFunObjFourArgBinder2<O,A,B,C,D,E,F> bind(void (O::*mem_fun)(A,B,C,D,E,F), O* obj,
                                                          const A& a, const B& b, const C& c,
                                                          const D& d)
{
    return _impl::MemFunObjFourArgBinder2<O,A,B,C,D,E,F>(mem_fun, obj, a, b, c, d);
}


} // namespace util
} // namespace realm

#endif // REALM_UTIL_BIND_HPP
