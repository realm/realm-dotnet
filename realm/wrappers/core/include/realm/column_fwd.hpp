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
#ifndef REALM_COLUMN_FWD_HPP
#define REALM_COLUMN_FWD_HPP

namespace realm {


class ColumnBase;
class Column;
template<class T> class BasicColumn;
typedef BasicColumn<double> ColumnDouble;
typedef BasicColumn<float> ColumnFloat;
class AdaptiveStringColumn;
class ColumnStringEnum;
class ColumnBinary;
class ColumnTable;
class ColumnMixed;
class ColumnLink;
class ColumnLinkList;

} // namespace realm

#endif // REALM_COLUMN_FWD_HPP
