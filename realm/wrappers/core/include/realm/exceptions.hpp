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

#ifndef REALM_EXCEPTIONS_HPP
#define REALM_EXCEPTIONS_HPP

#include <stdexcept>

#include <realm/util/features.h>

namespace realm {

/// Thrown by various functions to indicate that a specified table does not
/// exist.
class NoSuchTable: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override;
};


/// Thrown by various functions to indicate that a specified table name is
/// already in use.
class TableNameInUse: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override;
};


// Thrown by functions that require a table to **not** be the target of link
// columns, unless those link columns are part of the table itself.
class CrossTableLinkTarget: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override;
};


/// Thrown by various functions to indicate that the dynamic type of a table
/// does not match a particular other table type (dynamic or static).
class DescriptorMismatch: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override;
};


/// Reports errors that are a consequence of faulty logic within the program,
/// such as violating logical preconditions or class invariants, and can be
/// easily predicted.
class LogicError: public std::exception {
public:
    enum ErrorKind {
        string_too_big,
        binary_too_big,
        table_name_too_long,
        column_name_too_long,
        table_index_out_of_range,
        row_index_out_of_range,
        column_index_out_of_range,

        /// Indicates that an argument has a value that is illegal in combination
        /// with another argument, or with the state of an involved object.
        illegal_combination,

        /// Indicates a data type mismatch, such as when `Table::find_pkey_int()` is
        /// called and the type of the primary key is not `type_Int`.
        type_mismatch,

        /// Indicates that an involved table is of the wrong kind, i.e., if it is a
        /// subtable, and the function requires a root table.
        wrong_kind_of_table,

        /// Indicates that an involved accessor is was detached, i.e., was not
        /// attached to an underlying object.
        detached_accessor,

        // Indicates that an involved column lacks a search index.
        no_search_index,

        // Indicates that an involved table lacks a primary key.
        no_primary_key,

        // Indicates that an attempt was made to add a primary key to a table that
        // already had a primary key.
        has_primary_key,

        /// Indicates that a modification to a column was attempted that cannot
        /// be done because the column is the primary key of the table.
        is_primary_key,

        /// Indicates that a modification was attempted that would have produced a
        /// duplicate primary value.
        unique_constraint_violation
    };

    LogicError(ErrorKind message);

    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override;
    ErrorKind kind() const REALM_NOEXCEPT_OR_NOTHROW;
private:
    ErrorKind m_kind;
};


// Implementation:

inline const char* NoSuchTable::what() const REALM_NOEXCEPT_OR_NOTHROW
{
    return "No such table exists";
}

inline const char* TableNameInUse::what() const REALM_NOEXCEPT_OR_NOTHROW
{
    return "The specified table name is already in use";
}

inline const char* CrossTableLinkTarget::what() const REALM_NOEXCEPT_OR_NOTHROW
{
    return "Table is target of cross-table link columns";
}

inline const char* DescriptorMismatch::what() const REALM_NOEXCEPT_OR_NOTHROW
{
    return "Table descriptor mismatch";
}

inline LogicError::LogicError(LogicError::ErrorKind kind):
    m_kind(kind)
{
}

inline LogicError::ErrorKind LogicError::kind() const REALM_NOEXCEPT_OR_NOTHROW
{
    return m_kind;
}


} // namespace realm

#endif // REALM_EXCEPTIONS_HPP
