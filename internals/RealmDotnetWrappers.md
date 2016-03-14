Wrappers Layer in the Realm C# Product
======================================

Unlike earlier products, we make heavy use of ObjectStore and try to push responsibilities down into that C++ layer.

This means we end up maintaining data references from C# to both Core and ObjectStore.

Row Handles
-----------

Our usual RowHandle code in `row_cs.cpp` maps to a `typedef BasicRow<Table> Row` aka a _row accessor_.

ObjectStore `Results::get` returns a `using RowExpr = BasicRowExpr<Table>` as does `Table::get`.

The `BasicRowExpr` is, in its own words
_a special kind of row accessor. It differes from a real row
accessor (BasicRow) by having a trivial and fast copy constructor and
descructor. It is supposed to be used as the return type of functions such
as Table::operator[](), and then to be used as a basis for constructing a
real row accessor. Objects of this class are intended to only ever exist as
temporaries._