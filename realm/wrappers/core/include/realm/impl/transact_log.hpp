/*************************************************************************
 *
 * REALM CONFIDENTIAL
 * __________________
 *
 *  [2011] - [2015] Realm Inc
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

#ifndef REALM_IMPL_TRANSACT_LOG_HPP
#define REALM_IMPL_TRANSACT_LOG_HPP

#include <stdexcept>

#include <realm/string_data.hpp>
#include <realm/data_type.hpp>
#include <realm/binary_data.hpp>
#include <realm/datetime.hpp>
#include <realm/mixed.hpp>
#include <realm/util/buffer.hpp>
#include <realm/util/string_buffer.hpp>
#include <realm/util/tuple.hpp>
#include <realm/impl/input_stream.hpp>

#include <realm/group.hpp>
#include <realm/descriptor.hpp>

namespace realm {
namespace _impl {

/// Transaction log instruction encoding
enum Instruction {
    instr_InsertGroupLevelTable =  1,
    instr_EraseGroupLevelTable  =  2, // Remove columnless table from group
    instr_RenameGroupLevelTable =  3,
    instr_SelectTable           =  4,
    instr_SetInt                =  5,
    instr_SetBool               =  6,
    instr_SetFloat              =  7,
    instr_SetDouble             =  8,
    instr_SetString             =  9,
    instr_SetBinary             = 10,
    instr_SetDateTime           = 11,
    instr_SetTable              = 12,
    instr_SetMixed              = 13,
    instr_SetLink               = 14,
    instr_InsertInt             = 15,
    instr_InsertBool            = 16,
    instr_InsertFloat           = 17,
    instr_InsertDouble          = 18,
    instr_InsertString          = 19,
    instr_InsertBinary          = 20,
    instr_InsertDateTime        = 21,
    instr_InsertTable           = 22,
    instr_InsertMixed           = 23,
    instr_InsertLink            = 24,
    instr_InsertLinkList        = 25,
    instr_RowInsertComplete     = 26,
    instr_InsertEmptyRows       = 27,
    instr_EraseRows             = 28, // Remove (multiple) rows
    instr_AddIntToColumn        = 29, // Add an integer value to all cells in a column
    instr_ClearTable            = 30, // Remove all rows in selected table
    instr_OptimizeTable         = 31,
    instr_SelectDescriptor      = 32, // Select descriptor from currently selected root table
    instr_InsertColumn          = 33, // Insert new column into to selected descriptor
    instr_InsertLinkColumn      = 34, // do, but for a link-type column
    instr_EraseColumn           = 35, // Remove column from selected descriptor
    instr_EraseLinkColumn       = 36, // Remove link-type column from selected descriptor
    instr_RenameColumn          = 37, // Rename column in selected descriptor
    instr_AddSearchIndex        = 38, // Add a search index to a column
    instr_RemoveSearchIndex     = 39, // Remove a search index from a column
    instr_AddPrimaryKey         = 40, // Add a primary key to a table
    instr_RemovePrimaryKey      = 41, // Remove primary key from a table
    instr_SetLinkType           = 42, // Strong/weak
    instr_SelectLinkList        = 43,
    instr_LinkListSet           = 44, // Assign to link list entry
    instr_LinkListInsert        = 45, // Insert entry into link list
    instr_LinkListMove          = 46, // Move an entry within a link list
    instr_LinkListErase         = 47, // Remove an entry from a link list
    instr_LinkListClear         = 48, // Ramove all entries from a link list
    instr_LinkListSetAll        = 49  // Assign to link list entry
};


class TransactLogStream {
public:
    /// Ensure contiguous free space in the transaction log
    /// buffer. This method must update `out_free_begin`
    /// and `out_free_end` such that they refer to a chunk
    /// of free space whose size is at least \a n.
    ///
    /// \param n The required amount of contiguous free space. Must be
    /// small (probably not greater than 1024)
    /// \param n Must be small (probably not greater than 1024)
    virtual void transact_log_reserve(std::size_t size, char** out_free_begin, char** out_free_end) = 0;

    /// Copy the specified data into the transaction log buffer. This
    /// function should be called only when the specified data does
    /// not fit inside the chunk of free space currently referred to
    /// by `out_free_begin` and `out_free_end`.
    ///
    /// This method must update `out_begin` and
    /// `out_end` such that, upon return, they still
    /// refer to a (possibly empty) chunk of free space.
    virtual void transact_log_append(const char* data, std::size_t size, char** out_free_begin, char** out_free_end) = 0;
};

class TransactLogBufferStream: public TransactLogStream {
public:
    void transact_log_reserve(std::size_t size, char** out_free_begin, char** out_free_end) override;
    void transact_log_append(const char* data, std::size_t size, char** out_free_begin, char** out_free_end) override;

    const char* transact_log_data() const;
private:
    util::Buffer<char> m_buffer;
};


class TransactLogEncoder {
public:
    /// The following methods are also those that TransactLogParser expects
    /// to find on the `InstructionHandler`.

    // No selection needed:
    bool select_table(std::size_t group_level_ndx, std::size_t levels, const std::size_t* path);
    bool select_descriptor(std::size_t levels, const std::size_t* path);
    bool select_link_list(std::size_t col_ndx, std::size_t row_ndx);
    bool insert_group_level_table(std::size_t table_ndx, std::size_t num_tables, StringData name);
    bool erase_group_level_table(std::size_t table_ndx, std::size_t num_tables);
    bool rename_group_level_table(std::size_t table_ndx, StringData new_name);

    // Must have table selected:
    bool insert_empty_rows(std::size_t row_ndx, std::size_t num_rows, std::size_t tbl_sz, bool unordered);
    bool erase_rows(std::size_t row_ndx, std::size_t num_rows, std::size_t tbl_sz, bool unordered);
    bool clear_table();
    bool insert_int(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, int_fast64_t);
    bool insert_bool(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, bool);
    bool insert_float(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, float);
    bool insert_double(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, double);
    bool insert_string(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, StringData);
    bool insert_binary(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, BinaryData);
    bool insert_date_time(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, DateTime);
    bool insert_table(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz);
    bool insert_mixed(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, const Mixed&);
    bool insert_link(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz, std::size_t);
    bool insert_link_list(std::size_t col_ndx, std::size_t row_ndx, std::size_t tbl_sz);
    bool row_insert_complete();
    bool set_int(std::size_t col_ndx, std::size_t row_ndx, int_fast64_t);
    bool set_bool(std::size_t col_ndx, std::size_t row_ndx, bool);
    bool set_float(std::size_t col_ndx, std::size_t row_ndx, float);
    bool set_double(std::size_t col_ndx, std::size_t row_ndx, double);
    bool set_string(std::size_t col_ndx, std::size_t row_ndx, StringData);
    bool set_binary(std::size_t col_ndx, std::size_t row_ndx, BinaryData);
    bool set_date_time(std::size_t col_ndx, std::size_t row_ndx, DateTime);
    bool set_table(std::size_t col_ndx, std::size_t row_ndx);
    bool set_mixed(std::size_t col_ndx, std::size_t row_ndx, const Mixed&);
    bool set_link(std::size_t col_ndx, std::size_t row_ndx, std::size_t);
    bool add_int_to_column(std::size_t col_ndx, int_fast64_t value);
    bool optimize_table();

    // Must have descriptor selected:
    bool insert_link_column(std::size_t col_ndx, DataType, StringData name, std::size_t link_target_table_ndx, std::size_t backlink_col_ndx);
    bool insert_column(std::size_t col_ndx, DataType, StringData name);
    bool erase_link_column(std::size_t col_ndx, std::size_t link_target_table_ndx, std::size_t backlink_col_ndx);
    bool erase_column(std::size_t col_ndx);
    bool rename_column(std::size_t col_ndx, StringData new_name);
    bool add_search_index(std::size_t col_ndx);
    bool remove_search_index(std::size_t col_ndx);
    bool add_primary_key(std::size_t col_ndx);
    bool remove_primary_key();
    bool set_link_type(std::size_t col_ndx, LinkType);

    // Must have linklist selected:
    bool link_list_set(std::size_t link_ndx, std::size_t value);
    bool link_list_set_all(const Column& values);
    bool link_list_insert(std::size_t link_ndx, std::size_t value);
    bool link_list_move(std::size_t old_link_ndx, std::size_t new_link_ndx);
    bool link_list_erase(std::size_t link_ndx);
    bool link_list_clear();

    /// End of methods expected by parser.


    TransactLogEncoder(TransactLogStream& out_stream);
    void set_buffer(char* new_free_begin, char* new_free_end);
    char* write_position() const { return m_transact_log_free_begin; }

private:
    std::size_t max_required_bytes_for_string_cmd(std::size_t string_size) const;
    std::size_t max_required_bytes_for_string_value(std::size_t string_size) const;
    std::size_t max_required_bytes_for_simple_cmd(std::size_t num_numbers) const;
    std::size_t max_required_bytes_for_mixed_cmd(const Mixed& value) const;
    std::size_t max_required_bytes_for_mixed_value(const Mixed& value) const;
    std::size_t max_required_bytes_for_select_table(const Table*) const;
    std::size_t max_required_bytes_for_select_desc(const Descriptor&) const;
    std::size_t max_required_bytes_for_select_link_list(const LinkView&) const;

    // Make sure this is in agreement with the actual integer encoding
    // scheme (see encode_int()).
    static const int max_enc_bytes_per_int = 10;
    static const int max_enc_bytes_per_double = sizeof (double);
    static const int max_enc_bytes_per_num = max_enc_bytes_per_int <
        max_enc_bytes_per_double ? max_enc_bytes_per_double : max_enc_bytes_per_int;

    TransactLogStream& m_stream;

    // These two delimit a contiguous region of free space in a
    // transaction log buffer following the last written data. It may
    // be empty.
    char* m_transact_log_free_begin;
    char* m_transact_log_free_end;

    char* reserve(std::size_t size);
    /// \param ptr Must be in the range [m_transact_log_free_begin, m_transact_log_free_end]
    void advance(char* ptr) REALM_NOEXCEPT;
    void append(const char* data, std::size_t size);

    void string_cmd(Instruction, std::size_t col_ndx, std::size_t ndx, const char* data, std::size_t size);
    void string_value(const char* data, std::size_t size);
    void mixed_cmd(Instruction, std::size_t col_ndx, std::size_t ndx, const Mixed& value);
    void mixed_value(const Mixed& value);

    template <class L>
    void simple_cmd(Instruction, const util::Tuple<L>& numbers);

    template <class T>
    void append_num(T value);

    template <class T>
    static char* encode_int(char*, T value);
    static char* encode_float(char*, float value);
    static char* encode_double(char*, double value);
    template <class> struct EncodeNumber;
};

class TransactLogConvenientEncoder {
public:
    void insert_group_level_table(std::size_t table_ndx, std::size_t num_tables, StringData name);
    void erase_group_level_table(std::size_t table_ndx, std::size_t num_tables);
    void rename_group_level_table(std::size_t table_ndx, StringData new_name);
    void insert_column(const Descriptor&, std::size_t col_ndx, DataType type, StringData name,
                       const Table* link_target_table);
    void erase_column(const Descriptor&, std::size_t col_ndx);
    void rename_column(const Descriptor&, std::size_t col_ndx, StringData name);

    void set_int(const Table*, std::size_t col_ndx, std::size_t ndx, int_fast64_t value);
    void set_bool(const Table*, std::size_t col_ndx, std::size_t ndx, bool value);
    void set_float(const Table*, std::size_t col_ndx, std::size_t ndx, float value);
    void set_double(const Table*, std::size_t col_ndx, std::size_t ndx, double value);
    void set_string(const Table*, std::size_t col_ndx, std::size_t ndx, StringData value);
    void set_binary(const Table*, std::size_t col_ndx, std::size_t ndx, BinaryData value);
    void set_date_time(const Table*, std::size_t col_ndx, std::size_t ndx, DateTime value);
    void set_table(const Table*, std::size_t col_ndx, std::size_t ndx);
    void set_mixed(const Table*, std::size_t col_ndx, std::size_t ndx, const Mixed& value);
    void set_link(const Table*, std::size_t col_ndx, std::size_t ndx, std::size_t value);
    void set_link_list(const LinkView&, const Column& values);

    void insert_int(const Table*, std::size_t col_ndx, std::size_t ndx, int_fast64_t value);
    void insert_bool(const Table*, std::size_t col_ndx, std::size_t ndx, bool value);
    void insert_float(const Table*, std::size_t col_ndx, std::size_t ndx, float value);
    void insert_double(const Table*, std::size_t col_ndx, std::size_t ndx, double value);
    void insert_string(const Table*, std::size_t col_ndx, std::size_t ndx, StringData value);
    void insert_binary(const Table*, std::size_t col_ndx, std::size_t ndx, BinaryData value);
    void insert_date_time(const Table*, std::size_t col_ndx, std::size_t ndx, DateTime value);
    void insert_table(const Table*, std::size_t col_ndx, std::size_t ndx);
    void insert_mixed(const Table*, std::size_t col_ndx, std::size_t ndx, const Mixed& value);
    void insert_link(const Table*, std::size_t col_ndx, std::size_t ndx, std::size_t value);
    void insert_link_list(const Table*, std::size_t col_ndx, std::size_t ndx);

    void row_insert_complete(const Table*);
    void insert_empty_rows(const Table*, std::size_t row_ndx, std::size_t num_rows);
    void erase_row(const Table*, std::size_t row_ndx, bool move_last_over);
    void add_int_to_column(const Table*, std::size_t col_ndx, int_fast64_t value);
    void add_search_index(const Table*, std::size_t col_ndx);
    void remove_search_index(const Table*, std::size_t col_ndx);
    void add_primary_key(const Table*, std::size_t col_ndx);
    void remove_primary_key(const Table*);
    void set_link_type(const Table*, std::size_t col_ndx, LinkType);
    void clear_table(const Table*);
    void optimize_table(const Table*);

    void link_list_set(const LinkView&, std::size_t link_ndx, std::size_t value);
    void link_list_insert(const LinkView&, std::size_t link_ndx, std::size_t value);
    void link_list_move(const LinkView&, std::size_t old_link_ndx, std::size_t new_link_ndx);
    void link_list_erase(const LinkView&, std::size_t link_ndx);
    void link_list_clear(const LinkView&);

    void on_table_destroyed(const Table*) REALM_NOEXCEPT;
    void on_spec_destroyed(const Spec*) REALM_NOEXCEPT;
    void on_link_list_destroyed(const LinkView&) REALM_NOEXCEPT;

protected:
    TransactLogConvenientEncoder(TransactLogStream& encoder);

    void reset_selection_caches();
    void set_buffer(char* new_free_begin, char* new_free_end) { m_encoder.set_buffer(new_free_begin, new_free_end); }
    char* write_position() const { return m_encoder.write_position(); }

private:
    TransactLogEncoder m_encoder;
    // These are mutable because they are caches.
    mutable util::Buffer<std::size_t> m_subtab_path_buf;
    mutable const Table*    m_selected_table;
    mutable const Spec*     m_selected_spec;
    mutable const LinkView* m_selected_link_list;

    void select_table(const Table*);
    void select_desc(const Descriptor&);
    void select_link_list(const LinkView&);
    // These reset the above caches and modify them as necessary
    void do_select_table(const Table*);
    void do_select_desc(const Descriptor&);
    void do_select_link_list(const LinkView&);

    friend class Group::TransactReverser;
};


class TransactLogParser {
public:
    class BadTransactLog; // Exception

    TransactLogParser(InputStream& transact_log);

    ~TransactLogParser() REALM_NOEXCEPT;

    /// See `TransactLogEncoder` for a list of methods that the `InstructionHandler` must define.
    /// parse() promises that the path passed by reference to
    /// InstructionHandler::select_descriptor() will remain valid
    /// during subsequent calls to all descriptor modifying functions.
    template<class InstructionHandler> void parse(InstructionHandler&);

private:
    // The input stream is assumed to consist of chunks of memory organised such that
    // every instruction resides in a single chunk only.
    InputStream& m_input;
    // pointer into transaction log, each instruction is parsed from m_input_begin and onwards.
    // Each instruction are assumed to be contiguous in memory.
    const char* m_input_begin;
    // pointer to one past current instruction log chunk. If m_input_begin reaches m_input_end,
    // a call to next_input_buffer will move m_input_begin and m_input_end to a new chunk of
    // memory. Setting m_input_end to 0 disables this check, and is used if it is already known
    // that all of the instructions are in memory.
    const char* m_input_end;
    util::StringBuffer m_string_buffer;
    static const int m_max_levels = 1024;
    util::Buffer<std::size_t> m_path;

    REALM_NORETURN void parser_error() const;
    template<class InstructionHandler> void do_parse(InstructionHandler&);

    template<class T> T read_int();

    void read_bytes(char* data, std::size_t size);

    float read_float();
    double read_double();

    StringData read_string(util::StringBuffer&);
    BinaryData read_binary(util::StringBuffer&);
    void read_mixed(Mixed*);

    // Advance m_input_begin and m_input_end to reflect the next block of instructions
    // Returns false if no more input was available
    bool next_input_buffer();

    // return true if input was available
    bool read_char(char&); // throws

    bool is_valid_data_type(int type);
    bool is_valid_link_type(int type);
};


class TransactLogParser::BadTransactLog: public std::exception {
public:
    const char* what() const REALM_NOEXCEPT_OR_NOTHROW override
    {
        return "Bad transaction log";
    }
};



/// Implementation:

inline void TransactLogBufferStream::transact_log_reserve(std::size_t n, char** inout_new_begin, char** out_new_end)
{
    char* data = m_buffer.data();
    REALM_ASSERT(*inout_new_begin >= data);
    REALM_ASSERT(*inout_new_begin <= (data + m_buffer.size()));
    std::size_t size = *inout_new_begin - data;
    m_buffer.reserve_extra(size, n);
    data = m_buffer.data(); // May have changed
    *inout_new_begin = data + size;
    *out_new_end = data + m_buffer.size();
}

inline void TransactLogBufferStream::transact_log_append(const char* data, std::size_t size, char** out_new_begin, char** out_new_end)
{
    transact_log_reserve(size, out_new_begin, out_new_end);
    *out_new_begin = std::copy(data, data + size, *out_new_begin);
}

inline const char* TransactLogBufferStream::transact_log_data() const
{
    return m_buffer.data();
}

inline TransactLogEncoder::TransactLogEncoder(TransactLogStream& stream):
    m_stream(stream),
    m_transact_log_free_begin(nullptr),
    m_transact_log_free_end(nullptr)
{
}

inline void TransactLogEncoder::set_buffer(char* free_begin, char* free_end)
{
    REALM_ASSERT(free_begin <= free_end);
    m_transact_log_free_begin = free_begin;
    m_transact_log_free_end   = free_end;
}

inline void TransactLogConvenientEncoder::reset_selection_caches()
{
    m_selected_table = nullptr;
    m_selected_spec  = nullptr;
    m_selected_link_list  = nullptr;
}

inline char* TransactLogEncoder::reserve(std::size_t n)
{
    if (std::size_t(m_transact_log_free_end - m_transact_log_free_begin) < n) {
        m_stream.transact_log_reserve(n, &m_transact_log_free_begin, &m_transact_log_free_end);
    }
    return m_transact_log_free_begin;
}

inline void TransactLogEncoder::advance(char* ptr) REALM_NOEXCEPT
{
    REALM_ASSERT_DEBUG(m_transact_log_free_begin <= ptr);
    REALM_ASSERT_DEBUG(ptr <= m_transact_log_free_end);
    m_transact_log_free_begin = ptr;
}

inline void TransactLogEncoder::append(const char* data, std::size_t size)
{
    if (std::size_t(m_transact_log_free_end - m_transact_log_free_begin) < size) {
        m_stream.transact_log_append(data, size, &m_transact_log_free_begin, &m_transact_log_free_end);
    }
    else {
        advance(std::copy(data, data + size, m_transact_log_free_begin));
    }
}


// The integer encoding is platform independent. Also, it does not
// depend on the type of the specified integer. Integers of any type
// can be encoded as long as the specified buffer is large enough (see
// below). The decoding does not have to use the same type. Decoding
// will fail if, and only if the encoded value falls outside the range
// of the requested destination type.
//
// The encoding uses one or more bytes. It never uses more than 8 bits
// per byte. The last byte in the sequence is the first one that has
// its 8th bit set to zero.
//
// Consider a particular non-negative value V. Let W be the number of
// bits needed to encode V using the trivial binary encoding of
// integers. The total number of bytes produced is then
// ceil((W+1)/7). The first byte holds the 7 least significant bits of
// V. The last byte holds at most 6 bits of V including the most
// significant one. The value of the first bit of the last byte is
// always 2**((N-1)*7) where N is the total number of bytes.
//
// A negative value W is encoded by setting the sign bit to one and
// then encoding the positive result of -(W+1) as described above. The
// advantage of this representation is that it converts small negative
// values to small positive values which require a small number of
// bytes. This would not have been true for 2's complements
// representation, for example. The sign bit is always stored as the
// 7th bit of the last byte.
//
//               value bits    value + sign    max bytes
//     --------------------------------------------------
//     int8_t         7              8              2
//     uint8_t        8              9              2
//     int16_t       15             16              3
//     uint16_t      16             17              3
//     int32_t       31             32              5
//     uint32_t      32             33              5
//     int64_t       63             64             10
//     uint64_t      64             65             10
//
template <class T>
char* TransactLogEncoder::encode_int(char* ptr, T value)
{
    REALM_STATIC_ASSERT(std::numeric_limits<T>::is_integer, "Integer required");
    bool negative = util::is_negative(value);
    if (negative) {
        // The following conversion is guaranteed by C++11 to never
        // overflow (contrast this with "-value" which indeed could
        // overflow). See C99+TC3 section 6.2.6.2 paragraph 2.
        value = -(value + 1);
    }
    // At this point 'value' is always a positive number. Also, small
    // negative numbers have been converted to small positive numbers.
    REALM_ASSERT(!util::is_negative(value));
    // One sign bit plus number of value bits
    const int num_bits = 1 + std::numeric_limits<T>::digits;
    // Only the first 7 bits are available per byte. Had it not been
    // for the fact that maximum guaranteed bit width of a char is 8,
    // this value could have been increased to 15 (one less than the
    // number of value bits in 'unsigned').
    const int bits_per_byte = 7;
    const int max_bytes = (num_bits + (bits_per_byte-1)) / bits_per_byte;
    REALM_STATIC_ASSERT(max_bytes <= max_enc_bytes_per_int, "Bad max_enc_bytes_per_int");
    // An explicit constant maximum number of iterations is specified
    // in the hope that it will help the optimizer (to do loop
    // unrolling, for example).
    typedef unsigned char uchar;
    for (int i=0; i<max_bytes; ++i) {
        if (value >> (bits_per_byte-1) == 0)
            break;
        *reinterpret_cast<uchar*>(ptr) = uchar((1U<<bits_per_byte) | unsigned(value & ((1U<<bits_per_byte)-1)));
        ++ptr;
        value >>= bits_per_byte;
    }
    *reinterpret_cast<uchar*>(ptr) = uchar(negative ? (1U<<(bits_per_byte-1)) | unsigned(value) : value);
    return ++ptr;
}

inline char* TransactLogEncoder::encode_float(char* ptr, float value)
{
    REALM_STATIC_ASSERT(std::numeric_limits<float>::is_iec559 &&
                          sizeof (float) * std::numeric_limits<unsigned char>::digits == 32,
                          "Unsupported 'float' representation");
    const char* val_ptr = reinterpret_cast<char*>(&value);
    return std::copy(val_ptr, val_ptr + sizeof value, ptr);
}

inline char* TransactLogEncoder::encode_double(char* ptr, double value)
{
    REALM_STATIC_ASSERT(std::numeric_limits<double>::is_iec559 &&
                          sizeof (double) * std::numeric_limits<unsigned char>::digits == 64,
                          "Unsupported 'double' representation");
    const char* val_ptr = reinterpret_cast<char*>(&value);
    return std::copy(val_ptr, val_ptr + sizeof value, ptr);
}

template <class T>
struct TransactLogEncoder::EncodeNumber {
    void operator()(T value, char** ptr)
    {
        *ptr = encode_int(*ptr, value);
    }
};
template <>
struct TransactLogEncoder::EncodeNumber<float> {
    void operator()(float value, char** ptr)
    {
        *ptr = encode_float(*ptr, value);
    }
};
template <>
struct TransactLogEncoder::EncodeNumber<double> {
    void operator()(double value, char** ptr)
    {
        *ptr = encode_double(*ptr, value);
    }
};


template <class L>
void TransactLogEncoder::simple_cmd(Instruction instr, const util::Tuple<L>& numbers)
{
    char* ptr = reserve(max_required_bytes_for_simple_cmd(util::TypeCount<L>::value));
    *ptr++ = char(instr);
    util::for_each<EncodeNumber>(numbers, &ptr);
    advance(ptr);
}


template <class T>
void TransactLogEncoder::append_num(T value)
{
    char* ptr = reserve(sizeof(value));
    EncodeNumber<T>()(value, &ptr);
    advance(ptr);
}

inline
void TransactLogConvenientEncoder::select_table(const Table* table)
{
    if (table != m_selected_table) {
        do_select_table(table);
    }
}

inline
void TransactLogConvenientEncoder::select_desc(const Descriptor& desc)
{
    typedef _impl::DescriptorFriend df;
    if (&df::get_spec(desc) != m_selected_spec)
        do_select_desc(desc); // Throws
}

inline
void TransactLogConvenientEncoder::select_link_list(const LinkView& list)
{
    if (&list != m_selected_link_list)
        do_select_link_list(list); // Throws
}


inline
void TransactLogEncoder::string_cmd(Instruction instr, std::size_t col_ndx,
    std::size_t ndx, const char* data, std::size_t size)
{
    simple_cmd(instr, util::tuple(col_ndx, ndx)); // Throws
    string_value(data, size); // Throws
}

inline
void TransactLogEncoder::string_value(const char* data, std::size_t size)
{
    char* buf = reserve(max_required_bytes_for_string_value(size));
    buf = encode_int(buf, uint64_t(size));
    buf = std::copy(data, data + size, buf);
    advance(buf);
}

inline
void TransactLogEncoder::mixed_cmd(Instruction instr, std::size_t col_ndx,
    std::size_t ndx, const Mixed& value)
{
    simple_cmd(instr, util::tuple(col_ndx, ndx));
    mixed_value(value);
}

inline
void TransactLogEncoder::mixed_value(const Mixed& value)
{
    DataType type = value.get_type();
    append_num(int(type));
    switch (type) {
        case type_Int:
            append_num(value.get_int());
            return;
        case type_Bool:
            append_num(int32_t(value.get_bool()));
            return;
        case type_Float:
            append_num(value.get_float());
            return;
        case type_Double:
            append_num(value.get_double());
            return;
        case type_DateTime:
            append_num(value.get_datetime().get_datetime());
            return;
        case type_String: {
            StringData data = value.get_string();
            string_value(data.data(), data.size());
            return;
        }
        case type_Binary: {
            BinaryData data = value.get_binary();
            append_num(data.size());
            append(data.data(), data.size());
            return;
        }
        case type_Table:
            return;
        case type_Mixed:
            REALM_ASSERT_RELEASE(false); // Mixed in mixed?
        case type_Link:
        case type_LinkList:
            // FIXME: Need to handle new link types here.
            REALM_ASSERT_RELEASE(false);
    }
    REALM_ASSERT_RELEASE(false);
}


inline bool TransactLogEncoder::insert_group_level_table(std::size_t table_ndx, std::size_t num_tables,
                                                  StringData name)
{
    simple_cmd(instr_InsertGroupLevelTable, util::tuple(table_ndx, num_tables,
                                                        name.size())); // Throws
    append(name.data(), name.size()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_group_level_table(std::size_t table_ndx, std::size_t num_tables,
                                                  StringData name)
{
    m_encoder.insert_group_level_table(table_ndx, num_tables, name);
}

inline bool TransactLogEncoder::erase_group_level_table(std::size_t table_ndx, std::size_t num_tables)
{
    simple_cmd(instr_EraseGroupLevelTable, util::tuple(table_ndx, num_tables)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::erase_group_level_table(std::size_t table_ndx, std::size_t num_tables)
{
    m_encoder.erase_group_level_table(table_ndx, num_tables);
}

inline bool TransactLogEncoder::rename_group_level_table(std::size_t table_ndx, StringData new_name)
{
    simple_cmd(instr_RenameGroupLevelTable, util::tuple(table_ndx, new_name.size())); // Throws
    append(new_name.data(), new_name.size()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::rename_group_level_table(std::size_t table_ndx, StringData new_name)
{
    m_encoder.rename_group_level_table(table_ndx, new_name);
}

inline bool TransactLogEncoder::insert_column(std::size_t col_ndx, DataType type, StringData name)
{
    simple_cmd(instr_InsertColumn, util::tuple(col_ndx, int(type), name.size()));
    append(name.data(), name.size());
    return true;
}

inline bool TransactLogEncoder::insert_link_column(std::size_t col_ndx, DataType type, StringData name,
    std::size_t link_target_table_ndx, std::size_t backlink_col_ndx)
{
    REALM_ASSERT(_impl::TableFriend::is_link_type(ColumnType(type)));
    simple_cmd(instr_InsertLinkColumn, util::tuple(col_ndx, int(type), name.size())); // Throws
    append(name.data(), name.size()); // Throws
    append_num(link_target_table_ndx); // Throws
    append_num(backlink_col_ndx);
    return true;
}


inline void TransactLogConvenientEncoder::insert_column(const Descriptor& desc, std::size_t col_ndx, DataType type,
                                       StringData name, const Table* link_target_table)
{
    select_desc(desc); // Throws
    if (link_target_table) {
        typedef _impl::TableFriend tf;
        typedef _impl::DescriptorFriend df;
        std::size_t target_table_ndx = link_target_table->get_index_in_group();
        const Table& origin_table = df::get_root_table(desc);
        REALM_ASSERT(origin_table.is_group_level());
        const Spec& target_spec = tf::get_spec(*link_target_table);
        std::size_t origin_table_ndx = origin_table.get_index_in_group();
        std::size_t backlink_col_ndx = target_spec.find_backlink_column(origin_table_ndx, col_ndx);
        m_encoder.insert_link_column(col_ndx, type, name, target_table_ndx, backlink_col_ndx); // Throws
    }
    else {
        m_encoder.insert_column(col_ndx, type, name);
    }
}

inline bool TransactLogEncoder::erase_column(std::size_t col_ndx)
{
    simple_cmd(instr_EraseColumn, util::tuple(col_ndx)); // Throws
    return true;
}

inline bool TransactLogEncoder::erase_link_column(std::size_t col_ndx, std::size_t link_target_table_ndx, std::size_t backlink_col_ndx)
{
    simple_cmd(instr_EraseLinkColumn, util::tuple(col_ndx, link_target_table_ndx, backlink_col_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::erase_column(const Descriptor& desc, std::size_t col_ndx)
{
    select_desc(desc); // Throws

    DataType type = desc.get_column_type(col_ndx);
    typedef _impl::TableFriend tf;
    if (!tf::is_link_type(ColumnType(type))) {
        m_encoder.erase_column(col_ndx); // Throws
    }
    else { // it's a link column:
        REALM_ASSERT(desc.is_root());
        typedef _impl::DescriptorFriend df;
        const Table& origin_table = df::get_root_table(desc);
        REALM_ASSERT(origin_table.is_group_level());
        const Table& target_table = *tf::get_link_target_table_accessor(origin_table, col_ndx);
        std::size_t target_table_ndx = target_table.get_index_in_group();
        const Spec& target_spec = tf::get_spec(target_table);
        std::size_t origin_table_ndx = origin_table.get_index_in_group();
        std::size_t backlink_col_ndx = target_spec.find_backlink_column(origin_table_ndx, col_ndx);
        m_encoder.erase_link_column(col_ndx, target_table_ndx, backlink_col_ndx); // Throws
    }
}

inline bool TransactLogEncoder::rename_column(std::size_t col_ndx, StringData new_name)
{
    simple_cmd(instr_RenameColumn, util::tuple(col_ndx, new_name.size())); // Throws
    append(new_name.data(), new_name.size());
    return true;
}

inline void TransactLogConvenientEncoder::rename_column(const Descriptor& desc, std::size_t col_ndx,
                                       StringData name)
{
    select_desc(desc); // Throws
    m_encoder.rename_column(col_ndx, name); // Throws
}


inline bool TransactLogEncoder::set_int(std::size_t col_ndx, std::size_t ndx, int_fast64_t value)
{
    simple_cmd(instr_SetInt, util::tuple(col_ndx, ndx, value));
    return true;
}

inline void TransactLogConvenientEncoder::set_int(const Table* t, std::size_t col_ndx,
                                 std::size_t ndx, int_fast64_t value)
{
    select_table(t); // Throws
    m_encoder.set_int(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_bool(std::size_t col_ndx, std::size_t ndx, bool value)
{
    simple_cmd(instr_SetBool, util::tuple(col_ndx, ndx, value));
    return true;
}

inline void TransactLogConvenientEncoder::set_bool(const Table* t, std::size_t col_ndx,
                                  std::size_t ndx, bool value)
{
    select_table(t); // Throws
    m_encoder.set_bool(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_float(std::size_t col_ndx, std::size_t ndx, float value)
{
    simple_cmd(instr_SetFloat, util::tuple(col_ndx, ndx, value));
    return true;
}

inline void TransactLogConvenientEncoder::set_float(const Table* t, std::size_t col_ndx,
                                   std::size_t ndx, float value)
{
    select_table(t); // Throws
    m_encoder.set_float(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_double(std::size_t col_ndx, std::size_t ndx, double value)
{
    simple_cmd(instr_SetDouble, util::tuple(col_ndx, ndx, value));
    return true;
}

inline void TransactLogConvenientEncoder::set_double(const Table* t, std::size_t col_ndx,
                                    std::size_t ndx, double value)
{
    select_table(t); // Throws
    m_encoder.set_double(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_string(std::size_t col_ndx, std::size_t ndx, StringData value)
{
    string_cmd(instr_SetString, col_ndx, ndx, value.data(), value.size()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_string(const Table* t, std::size_t col_ndx,
                                    std::size_t ndx, StringData value)
{
    select_table(t); // Throws
    m_encoder.set_string(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_binary(std::size_t col_ndx, std::size_t ndx, BinaryData value)
{
    string_cmd(instr_SetBinary, col_ndx, ndx, value.data(), value.size()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_binary(const Table* t, std::size_t col_ndx,
                                    std::size_t ndx, BinaryData value)
{
    select_table(t); // Throws
    m_encoder.set_binary(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_date_time(std::size_t col_ndx, std::size_t ndx, DateTime value)
{
    simple_cmd(instr_SetDateTime, util::tuple(col_ndx, ndx, value.get_datetime())); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_date_time(const Table* t, std::size_t col_ndx,
                                       std::size_t ndx, DateTime value)
{
    select_table(t); // Throws
    m_encoder.set_date_time(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_table(std::size_t col_ndx, std::size_t ndx)
{
    simple_cmd(instr_SetTable, util::tuple(col_ndx, ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_table(const Table* t, std::size_t col_ndx,
                                   std::size_t ndx)
{
    select_table(t); // Throws
    m_encoder.set_table(col_ndx, ndx); // Throws
}

inline bool TransactLogEncoder::set_mixed(std::size_t col_ndx, std::size_t ndx, const Mixed& value)
{
    mixed_cmd(instr_SetMixed, col_ndx, ndx, value); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_mixed(const Table* t, std::size_t col_ndx,
                                   std::size_t ndx, const Mixed& value)
{
    select_table(t); // Throws
    m_encoder.set_mixed(col_ndx, ndx, value); // Throws
}

inline bool TransactLogEncoder::set_link(std::size_t col_ndx, std::size_t ndx, std::size_t value)
{
    // Map `realm::npos` to zero, and `n` to `n+1`, where `n` is a target row
    // index.
    std::size_t value_2 = std::size_t(1) + value;
    simple_cmd(instr_SetLink, util::tuple(col_ndx, ndx, value_2));
    return true;
}

inline void TransactLogConvenientEncoder::set_link(const Table* t, std::size_t col_ndx,
                                  std::size_t ndx, std::size_t value)
{
    select_table(t); // Throws
    m_encoder.set_link(col_ndx, ndx, value); // Throws
}


inline bool TransactLogEncoder::insert_int(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, int_fast64_t value)
{
    simple_cmd(instr_InsertInt, util::tuple(col_ndx, ndx, tbl_sz, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_int(const Table* t, std::size_t col_ndx,
                                    std::size_t ndx, int_fast64_t value)
{
    select_table(t); // Throws
    m_encoder.insert_int(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_bool(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, bool value)
{
    simple_cmd(instr_InsertBool, util::tuple(col_ndx, ndx, tbl_sz, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_bool(const Table* t, std::size_t col_ndx,
                                     std::size_t ndx, bool value)
{
    select_table(t); // Throws
    m_encoder.insert_bool(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_float(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, float value)
{
    simple_cmd(instr_InsertFloat, util::tuple(col_ndx, ndx, tbl_sz, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_float(const Table* t, std::size_t col_ndx,
                                      std::size_t ndx, float value)
{
    select_table(t); // Throws
    m_encoder.insert_float(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_double(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, double value)
{
    simple_cmd(instr_InsertDouble, util::tuple(col_ndx, ndx, tbl_sz, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_double(const Table* t, std::size_t col_ndx,
                                       std::size_t ndx, double value)
{
    select_table(t); // Throws
    m_encoder.insert_double(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_string(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, StringData value)
{
    simple_cmd(instr_InsertString, util::tuple(col_ndx, ndx, tbl_sz));
    string_value(value.data(), value.size());
    return true;
}

inline void TransactLogConvenientEncoder::insert_string(const Table* t, std::size_t col_ndx,
                                       std::size_t ndx, StringData value)
{
    select_table(t); // Throws
    m_encoder.insert_string(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_binary(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, BinaryData value)
{
    simple_cmd(instr_InsertBinary, util::tuple(col_ndx, ndx, tbl_sz));
    string_value(value.data(), value.size());
    return true;
}

inline void TransactLogConvenientEncoder::insert_binary(const Table* t, std::size_t col_ndx,
                                       std::size_t ndx, BinaryData value)
{
    select_table(t); // Throws
    m_encoder.insert_binary(col_ndx, ndx, t->size(), value);
}

inline bool TransactLogEncoder::insert_date_time(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, DateTime value)
{
    simple_cmd(instr_InsertDateTime, util::tuple(col_ndx, ndx, tbl_sz, value.get_datetime())); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_date_time(const Table* t, std::size_t col_ndx,
                                          std::size_t ndx, DateTime value)
{
    select_table(t); // Throws
    m_encoder.insert_date_time(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_table(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz)
{
    simple_cmd(instr_InsertTable, util::tuple(col_ndx, ndx, tbl_sz)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_table(const Table* t, std::size_t col_ndx,
                                      std::size_t ndx)
{
    select_table(t); // Throws
    m_encoder.insert_table(col_ndx, ndx, t->size()); // Throws
}

inline bool TransactLogEncoder::insert_mixed(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, const Mixed& value)
{
    simple_cmd(instr_InsertMixed, util::tuple(col_ndx, ndx, tbl_sz));
    mixed_value(value); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_mixed(const Table* t, std::size_t col_ndx,
                                      std::size_t ndx, const Mixed& value)
{
    select_table(t); // Throws
    m_encoder.insert_mixed(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_link(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz, std::size_t value)
{
    // FIXME: Should the value be transformed in the same was as in `set_link`?
    simple_cmd(instr_InsertLink, util::tuple(col_ndx, ndx, tbl_sz, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_link(const Table* t, std::size_t col_ndx,
                                     std::size_t ndx, std::size_t value)
{
    select_table(t); // Throws
    m_encoder.insert_link(col_ndx, ndx, t->size(), value); // Throws
}

inline bool TransactLogEncoder::insert_link_list(std::size_t col_ndx, std::size_t ndx, std::size_t tbl_sz)
{
    simple_cmd(instr_InsertLinkList, util::tuple(col_ndx, ndx, tbl_sz)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::insert_link_list(const Table* t, std::size_t col_ndx, std::size_t ndx)
{
    select_table(t); // Throws
    m_encoder.insert_link_list(col_ndx, ndx, t->size()); // Throws
}

inline bool TransactLogEncoder::row_insert_complete()
{
    simple_cmd(instr_RowInsertComplete, util::tuple()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::row_insert_complete(const Table* t)
{
    select_table(t); // Throws
    m_encoder.row_insert_complete(); // Throws
}

inline bool TransactLogEncoder::insert_empty_rows(std::size_t row_ndx, std::size_t num_rows, std::size_t tbl_sz, bool unordered)
{
    simple_cmd(instr_InsertEmptyRows, util::tuple(row_ndx, num_rows, tbl_sz, unordered)); // Throws
    return true;
}


inline void TransactLogConvenientEncoder::insert_empty_rows(const Table* t, std::size_t row_ndx,
                                           std::size_t num_rows)
{
    select_table(t); // Throws
    // default to unordered, if we are inserting at the end:
    bool unordered = row_ndx == t->size()-num_rows;
    m_encoder.insert_empty_rows(row_ndx, num_rows, t->size(), unordered); // Throws
}

inline bool TransactLogEncoder::erase_rows(std::size_t row_ndx, std::size_t num_rows, std::size_t tbl_sz, bool unordered)
{
    std::size_t last_row_ndx = tbl_sz - 1;
    simple_cmd(instr_EraseRows, util::tuple(row_ndx, num_rows, last_row_ndx, unordered));
    return true;
}


inline void TransactLogConvenientEncoder::erase_row(const Table* t, std::size_t row_ndx, bool move_last_over)
{
    select_table(t); // Throws
    std::size_t num_rows = 1; // FIXME: might want to make this parameter externally visible?
    m_encoder.erase_rows(row_ndx, num_rows, t->size(), move_last_over); // Throws
}

inline bool TransactLogEncoder::add_int_to_column(std::size_t col_ndx, int_fast64_t value)
{
    simple_cmd(instr_AddIntToColumn, util::tuple(col_ndx, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::add_int_to_column(const Table* t, std::size_t col_ndx, int_fast64_t value)
{
    select_table(t); // Throws
    m_encoder.add_int_to_column(col_ndx, value); // Throws
}


inline bool TransactLogEncoder::add_search_index(std::size_t col_ndx)
{
    simple_cmd(instr_AddSearchIndex, util::tuple(col_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::add_search_index(const Table* t, std::size_t col_ndx)
{
    select_table(t); // Throws
    m_encoder.add_search_index(col_ndx); // Throws
}


inline bool TransactLogEncoder::remove_search_index(std::size_t col_ndx)
{
    simple_cmd(instr_RemoveSearchIndex, util::tuple(col_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::remove_search_index(const Table* t, std::size_t col_ndx)
{
    select_table(t); // Throws
    m_encoder.remove_search_index(col_ndx); // Throws
}


inline bool TransactLogEncoder::add_primary_key(std::size_t col_ndx)
{
    simple_cmd(instr_AddPrimaryKey, util::tuple(col_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::add_primary_key(const Table* t, std::size_t col_ndx)
{
    select_table(t); // Throws
    m_encoder.add_primary_key(col_ndx); // Throws
}


inline bool TransactLogEncoder::remove_primary_key()
{
    simple_cmd(instr_RemovePrimaryKey, util::tuple()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::remove_primary_key(const Table* t)
{
    select_table(t); // Throws
    m_encoder.remove_primary_key(); // Throws
}


inline bool TransactLogEncoder::set_link_type(std::size_t col_ndx, LinkType link_type)
{
    simple_cmd(instr_SetLinkType, util::tuple(col_ndx, int(link_type))); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::set_link_type(const Table* t, std::size_t col_ndx, LinkType link_type)
{
    select_table(t); // Throws
    m_encoder.set_link_type(col_ndx, link_type); // Throws
}


inline bool TransactLogEncoder::clear_table()
{
    simple_cmd(instr_ClearTable, util::tuple()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::clear_table(const Table* t)
{
    select_table(t); // Throws
    m_encoder.clear_table(); // Throws
}

inline bool TransactLogEncoder::optimize_table()
{
    simple_cmd(instr_OptimizeTable, util::tuple()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::optimize_table(const Table* t)
{
    select_table(t); // Throws
    m_encoder.optimize_table(); // Throws
}

inline bool TransactLogEncoder::link_list_set(std::size_t link_ndx, std::size_t value)
{
    simple_cmd(instr_LinkListSet, util::tuple(link_ndx, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::link_list_set(const LinkView& list, std::size_t link_ndx,
                                       std::size_t value)
{
    select_link_list(list); // Throws
    m_encoder.link_list_set(link_ndx, value); // Throws
}

inline bool TransactLogEncoder::link_list_set_all(const Column& values)
{
    simple_cmd(instr_LinkListSetAll, util::tuple(values.size())); // Throws
    for (std::size_t i = 0; i < values.size(); i++)
        append_num(values.get(i));
    return true;
}

inline void TransactLogConvenientEncoder::set_link_list(const LinkView& list, const Column& values)
{
    select_link_list(list); // Throws
    m_encoder.link_list_set_all(values); // Throws
}

inline bool TransactLogEncoder::link_list_insert(std::size_t link_ndx, std::size_t value)
{
    simple_cmd(instr_LinkListInsert, util::tuple(link_ndx, value)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::link_list_insert(const LinkView& list, std::size_t link_ndx,
                                          std::size_t value)
{
    select_link_list(list); // Throws
    m_encoder.link_list_insert(link_ndx, value); // Throws
}

inline bool TransactLogEncoder::link_list_move(std::size_t old_link_ndx, std::size_t new_link_ndx)
{
    simple_cmd(instr_LinkListMove, util::tuple(old_link_ndx, new_link_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::link_list_move(const LinkView& list, std::size_t old_link_ndx,
                                        std::size_t new_link_ndx)
{
    select_link_list(list); // Throws
    m_encoder.link_list_move(old_link_ndx, new_link_ndx); // Throws
}

inline bool TransactLogEncoder::link_list_erase(std::size_t link_ndx)
{
    simple_cmd(instr_LinkListErase, util::tuple(link_ndx)); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::link_list_erase(const LinkView& list, std::size_t link_ndx)
{
    select_link_list(list); // Throws
    m_encoder.link_list_erase(link_ndx); // Throws
}

inline bool TransactLogEncoder::link_list_clear()
{
    simple_cmd(instr_LinkListClear, util::tuple()); // Throws
    return true;
}

inline void TransactLogConvenientEncoder::link_list_clear(const LinkView& list)
{
    select_link_list(list); // Throws
    m_encoder.link_list_clear(); // Throws
}

inline void TransactLogConvenientEncoder::on_table_destroyed(const Table* t) REALM_NOEXCEPT
{
    if (m_selected_table == t)
        m_selected_table = 0;
}

inline void TransactLogConvenientEncoder::on_spec_destroyed(const Spec* s) REALM_NOEXCEPT
{
    if (m_selected_spec == s)
        m_selected_spec = 0;
}


inline void TransactLogConvenientEncoder::on_link_list_destroyed(const LinkView& list) REALM_NOEXCEPT
{
    if (m_selected_link_list == &list)
        m_selected_link_list = 0;
}

inline std::size_t TransactLogEncoder::max_required_bytes_for_string_value(std::size_t size) const
{
    return max_enc_bytes_per_int + size;
}

inline std::size_t TransactLogEncoder::max_required_bytes_for_string_cmd(std::size_t size) const
{
    return 1 + max_required_bytes_for_string_value(size);
}

inline std::size_t TransactLogEncoder::max_required_bytes_for_simple_cmd(std::size_t num_numbers) const
{
    return 1 + max_enc_bytes_per_num * num_numbers;
}

inline TransactLogParser::TransactLogParser(InputStream& transact_log):
    m_input(transact_log)
{
    m_input_begin = m_input_end = 0;
}


inline TransactLogParser::~TransactLogParser() REALM_NOEXCEPT
{
}


template<class InstructionHandler>
void TransactLogParser::parse(InstructionHandler& handler)
{
    do_parse(handler);
}


template<class InstructionHandler>
void TransactLogParser::do_parse(InstructionHandler& handler)
{
    while (m_input_begin != m_input_end || next_input_buffer()) {

        char instr;
        if (!read_char(instr))
            parser_error();

        // std::cout << "parsing " << (int) instr << " @ " << std::hex << (long) m_input_begin << std::endl;
        switch (Instruction(instr)) {
            case instr_SetInt: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                // FIXME: Don't depend on the existence of int64_t,
                // but don't allow values to use more than 64 bits
                // either.
                int_fast64_t value = read_int<int64_t>(); // Throws
                if (!handler.set_int(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetBool: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                bool value = read_int<bool>(); // Throws
                if (!handler.set_bool(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetFloat: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                float value = read_float(); // Throws
                if (!handler.set_float(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetDouble: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                double value = read_double(); // Throws
                if (!handler.set_double(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetString: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                StringData value = read_string(m_string_buffer); // Throws
                if (!handler.set_string(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetBinary: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                BinaryData value = read_binary(m_string_buffer); // Throws
                if (!handler.set_binary(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetDateTime: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::time_t value = read_int<std::time_t>(); // Throws
                if (!handler.set_date_time(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetTable: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                if (!handler.set_table(col_ndx, row_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetMixed: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                Mixed value;
                read_mixed(&value); // Throws
                if (!handler.set_mixed(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SetLink: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t value = read_int<std::size_t>(); // Throws
                if (!handler.set_link(col_ndx, row_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertInt: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                // FIXME: Don't depend on the existence of int64_t,
                // but don't allow values to use more than 64 bits
                // either.
                int_fast64_t value = read_int<int64_t>(); // Throws
                if (!handler.insert_int(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertBool: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                bool value = read_int<bool>(); // Throws
                if (!handler.insert_bool(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertFloat: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                float value = read_float(); // Throws
                if (!handler.insert_float(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertDouble: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                double value = read_double(); // Throws
                if (!handler.insert_double(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertString: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                StringData value = read_string(m_string_buffer); // Throws
                if (!handler.insert_string(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertBinary: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                BinaryData value = read_binary(m_string_buffer); // Throws
                if (!handler.insert_binary(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertDateTime: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                std::time_t value = read_int<std::time_t>(); // Throws
                if (!handler.insert_date_time(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertTable: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                if (!handler.insert_table(col_ndx, row_ndx, tbl_sz)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertMixed: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                Mixed value;
                read_mixed(&value); // Throws
                if (!handler.insert_mixed(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertLink: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                std::size_t value = read_int<std::size_t>(); // Throws
                if (!handler.insert_link(col_ndx, row_ndx, tbl_sz, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertLinkList: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                if (!handler.insert_link_list(col_ndx, row_ndx, tbl_sz)) // Throws
                    parser_error();
                continue;
            }
            case instr_RowInsertComplete: {
                if (!handler.row_insert_complete()) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertEmptyRows: {
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t num_rows = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                bool unordered = read_int<bool>(); // Throws
                if (!handler.insert_empty_rows(row_ndx, num_rows, tbl_sz, unordered)) // Throws
                    parser_error();
                continue;
            }
            case instr_EraseRows: {
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                std::size_t num_rows = read_int<std::size_t>(); // Throws
                std::size_t tbl_sz = read_int<std::size_t>(); // Throws
                bool unordered = read_int<bool>(); // Throws
                if (!handler.erase_rows(row_ndx, num_rows, tbl_sz, unordered)) // Throws
                    parser_error();
                continue;
            }
            case instr_AddIntToColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                // FIXME: Don't depend on the existence of int64_t,
                // but don't allow values to use more than 64 bits
                // either.
                int_fast64_t value = read_int<int64_t>(); // Throws
                if (!handler.add_int_to_column(col_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_SelectTable: {
                int levels = read_int<int>(); // Throws
                if (levels < 0 || levels > m_max_levels)
                    parser_error();
                m_path.reserve(0, 2*levels); // Throws
                std::size_t* path = m_path.data();
                std::size_t group_level_ndx = read_int<std::size_t>(); // Throws
                for (int i = 0; i != levels; ++i) {
                    std::size_t col_ndx = read_int<std::size_t>(); // Throws
                    std::size_t row_ndx = read_int<std::size_t>(); // Throws
                    path[2*i + 0] = col_ndx;
                    path[2*i + 1] = row_ndx;
                }
                if (!handler.select_table(group_level_ndx, levels, path)) // Throws
                    parser_error();
                continue;
            }
            case instr_ClearTable: {
                if (!handler.clear_table()) // Throws
                    parser_error();
                continue;
            }
            case instr_LinkListSet: {
                std::size_t link_ndx = read_int<std::size_t>(); // Throws
                std::size_t value = read_int<std::size_t>(); // Throws
                if (!handler.link_list_set(link_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_LinkListSetAll: {
                // todo, log that it's a SetAll we're doing
                std::size_t size = read_int<std::size_t>(); // Throws
                for (std::size_t i = 0; i < size; i++) {
                    std::size_t link = read_int<std::size_t>(); // Throws
                    if (!handler.link_list_set(i, link)) // Throws
                        parser_error();
                }
                continue;
            }
            case instr_LinkListInsert: {
                std::size_t link_ndx = read_int<std::size_t>(); // Throws
                std::size_t value = read_int<std::size_t>(); // Throws
                if (!handler.link_list_insert(link_ndx, value)) // Throws
                    parser_error();
                continue;
            }
            case instr_LinkListMove: {
                std::size_t old_link_ndx = read_int<std::size_t>(); // Throws
                std::size_t new_link_ndx = read_int<std::size_t>(); // Throws
                if (!handler.link_list_move(old_link_ndx, new_link_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_LinkListErase: {
                std::size_t link_ndx = read_int<std::size_t>(); // Throws
                if (!handler.link_list_erase(link_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_LinkListClear: {
                if (!handler.link_list_clear()) // Throws
                    parser_error();
                continue;
            }
            case instr_SelectLinkList: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t row_ndx = read_int<std::size_t>(); // Throws
                if (!handler.select_link_list(col_ndx, row_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_AddSearchIndex: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                if (!handler.add_search_index(col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_RemoveSearchIndex: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                if (!handler.remove_search_index(col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_AddPrimaryKey: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                if (!handler.add_primary_key(col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_RemovePrimaryKey: {
                if (!handler.remove_primary_key()) // Throws
                    parser_error();
                continue;
            }
            case instr_SetLinkType: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                int link_type = read_int<int>(); // Throws
                if (!is_valid_link_type(link_type))
                    parser_error();
                if (!handler.set_link_type(col_ndx, LinkType(link_type))) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                int type = read_int<int>(); // Throws
                if (!is_valid_data_type(type))
                    parser_error();
                StringData name = read_string(m_string_buffer); // Throws
                if (!handler.insert_column(col_ndx, DataType(type), name)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertLinkColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                int type = read_int<int>(); // Throws
                if (!is_valid_data_type(type))
                    parser_error();
                StringData name = read_string(m_string_buffer); // Throws
                std::size_t link_target_table_ndx = read_int<std::size_t>(); // Throws
                std::size_t backlink_col_ndx = read_int<std::size_t>(); // Throws
                if (!handler.insert_link_column(col_ndx, DataType(type), name,
                                                link_target_table_ndx, backlink_col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_EraseColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                if (!handler.erase_column(col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_EraseLinkColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                std::size_t link_target_table_ndx = read_int<std::size_t>(); // Throws
                std::size_t backlink_col_ndx      = read_int<std::size_t>(); // Throws
                if (!handler.erase_link_column(col_ndx, link_target_table_ndx,
                                               backlink_col_ndx)) // Throws
                    parser_error();
                continue;
            }
            case instr_RenameColumn: {
                std::size_t col_ndx = read_int<std::size_t>(); // Throws
                StringData name = read_string(m_string_buffer); // Throws
                if (!handler.rename_column(col_ndx, name)) // Throws
                    parser_error();
                continue;
            }
            case instr_SelectDescriptor: {
                int levels = read_int<int>(); // Throws
                if (levels < 0 || levels > m_max_levels)
                    parser_error();
                m_path.reserve(0, levels); // Throws
                std::size_t* path = m_path.data();
                for (int i = 0; i != levels; ++i) {
                    std::size_t col_ndx = read_int<std::size_t>(); // Throws
                    path[i] = col_ndx;
                }
                if (!handler.select_descriptor(levels, path)) // Throws
                    parser_error();
                continue;
            }
            case instr_InsertGroupLevelTable: {
                std::size_t table_ndx  = read_int<std::size_t>(); // Throws
                std::size_t num_tables = read_int<std::size_t>(); // Throws
                StringData name = read_string(m_string_buffer); // Throws
                if (!handler.insert_group_level_table(table_ndx, num_tables, name)) // Throws
                    parser_error();
                continue;
            }
            case instr_EraseGroupLevelTable: {
                std::size_t table_ndx  = read_int<std::size_t>(); // Throws
                std::size_t num_tables = read_int<std::size_t>(); // Throws
                if (!handler.erase_group_level_table(table_ndx, num_tables)) // Throws
                    parser_error();
                continue;
            }
            case instr_RenameGroupLevelTable: {
                std::size_t table_ndx = read_int<std::size_t>(); // Throws
                StringData new_name = read_string(m_string_buffer); // Throws
                if (!handler.rename_group_level_table(table_ndx, new_name)) // Throws
                    parser_error();
                continue;
            }
            case instr_OptimizeTable: {
                if (!handler.optimize_table()) // Throws
                    parser_error();
                continue;
            }
        }
        // coming here is not possible
        REALM_ASSERT(false);
        return;
    }
}


template<class T> T TransactLogParser::read_int()
{
    T value = 0;
    int part = 0;
    const int max_bytes = (std::numeric_limits<T>::digits+1+6)/7;
    for (int i = 0; i != max_bytes; ++i) {
        char c;
        if (!read_char(c))
            goto bad_transact_log;
        part = static_cast<unsigned char>(c);
        if (0xFF < part)
            goto bad_transact_log; // Only the first 8 bits may be used in each byte
        if ((part & 0x80) == 0) {
            T p = part & 0x3F;
            if (util::int_shift_left_with_overflow_detect(p, i*7))
                goto bad_transact_log;
            value |= p;
            break;
        }
        if (i == max_bytes-1)
            goto bad_transact_log; // Too many bytes
        value |= T(part & 0x7F) << (i*7);
    }
    if (part & 0x40) {
        // The real value is negative. Because 'value' is positive at
        // this point, the following negation is guaranteed by C++11
        // to never overflow. See C99+TC3 section 6.2.6.2 paragraph 2.
        value = -value;
        if (util::int_subtract_with_overflow_detect(value, 1))
            goto bad_transact_log;
    }
    return value;

  bad_transact_log:
    throw BadTransactLog();
}


inline void TransactLogParser::read_bytes(char* data, std::size_t size)
{
    for (;;) {
        const std::size_t avail = m_input_end - m_input_begin;
        if (size <= avail)
            break;
        const char* to = m_input_begin + avail;
        std::copy(m_input_begin, to, data);
        if (!next_input_buffer())
            throw BadTransactLog();
        data += avail;
        size -= avail;
    }
    const char* to = m_input_begin + size;
    std::copy(m_input_begin, to, data);
    m_input_begin = to;
}


inline float TransactLogParser::read_float()
{
    REALM_STATIC_ASSERT(std::numeric_limits<float>::is_iec559 &&
                          sizeof (float) * std::numeric_limits<unsigned char>::digits == 32,
                          "Unsupported 'float' representation");
    float value;
    read_bytes(reinterpret_cast<char*>(&value), sizeof value); // Throws
    return value;
}


inline double TransactLogParser::read_double()
{
    REALM_STATIC_ASSERT(std::numeric_limits<double>::is_iec559 &&
                          sizeof (double) * std::numeric_limits<unsigned char>::digits == 64,
                          "Unsupported 'double' representation");
    double value;
    read_bytes(reinterpret_cast<char*>(&value), sizeof value); // Throws
    return value;
}


inline StringData TransactLogParser::read_string(util::StringBuffer& buf)
{
    std::size_t size = read_int<std::size_t>(); // Throws

    const std::size_t avail = m_input_end - m_input_begin;
    if (avail >= size) {
        m_input_begin += size;
        return StringData(m_input_begin - size, size);
    }

    buf.clear();
    buf.resize(size); // Throws
    read_bytes(buf.data(), size);
    return StringData(buf.data(), size);
}


inline BinaryData TransactLogParser::read_binary(util::StringBuffer& buf)
{
    StringData str = read_string(buf); // Throws;
    return BinaryData(str.data(), str.size());
}


inline void TransactLogParser::read_mixed(Mixed* mixed)
{
    DataType type = DataType(read_int<int>()); // Throws
    switch (type) {
        case type_Int: {
            // FIXME: Don't depend on the existence of
            // int64_t, but don't allow values to use more
            // than 64 bits either.
            int_fast64_t value = read_int<int64_t>(); // Throws
            mixed->set_int(value);
            return;
        }
        case type_Bool: {
            bool value = read_int<bool>(); // Throws
            mixed->set_bool(value);
            return;
        }
        case type_Float: {
            float value = read_float(); // Throws
            mixed->set_float(value);
            return;
        }
        case type_Double: {
            double value = read_double(); // Throws
            mixed->set_double(value);
            return;
        }
        case type_DateTime: {
            std::time_t value = read_int<std::time_t>(); // Throws
            mixed->set_datetime(value);
            return;
        }
        case type_String: {
            StringData value = read_string(m_string_buffer); // Throws
            mixed->set_string(value);
            return;
        }
        case type_Binary: {
            BinaryData value = read_binary(m_string_buffer); // Throws
            mixed->set_binary(value);
            return;
        }
        case type_Table: {
            *mixed = Mixed::subtable_tag();
            return;
        }
        case type_Mixed:
            break;
        case type_Link:
        case type_LinkList:
            // FIXME: Need to handle new link types here
            REALM_ASSERT(false);
            break;
    }
    REALM_ASSERT(false);
}


inline bool TransactLogParser::next_input_buffer()
{
    std::size_t sz = m_input.next_block(m_input_begin, m_input_end);
    if (sz == 0)
        return false;
    else
        return true;
}


inline bool TransactLogParser::read_char(char& c)
{
    if (m_input_begin == m_input_end && !next_input_buffer())
        return false;
    c = *m_input_begin++;
    return true;
}


inline bool TransactLogParser::is_valid_data_type(int type)
{
    switch (DataType(type)) {
        case type_Int:
        case type_Bool:
        case type_Float:
        case type_Double:
        case type_String:
        case type_Binary:
        case type_DateTime:
        case type_Table:
        case type_Mixed:
        case type_Link:
        case type_LinkList:
            return true;
    }
    return false;
}


inline bool TransactLogParser::is_valid_link_type(int type)
{
    switch (LinkType(type)) {
        case link_Strong:
        case link_Weak:
            return true;
    }
    return false;
}



}
}

#endif // REALM_IMPL_TRANSACT_LOG_HPP
