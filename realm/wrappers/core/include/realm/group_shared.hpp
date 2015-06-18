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
#ifndef REALM_GROUP_SHARED_HPP
#define REALM_GROUP_SHARED_HPP

#include <limits>

#include <realm/util/features.h>
#include <realm/util/thread.hpp>
#include <realm/util/platform_specific_condvar.hpp>
#include <realm/group.hpp>
//#include <realm/commit_log.hpp>

namespace realm {

namespace _impl {
class WriteLogCollector;
}

// Thrown by SharedGroup::open if the lock file is already open in another
// process which can't share mutexes with this process
struct IncompatibleLockFile : std::runtime_error {
    IncompatibleLockFile() : runtime_error("Incompatible lock file") { }
};

/// A SharedGroup facilitates transactions.
///
/// When multiple threads or processes need to access a database
/// concurrently, they must do so using transactions. By design,
/// Realm does not allow for multiple threads (or processes) to
/// share a single instance of SharedGroup. Instead, each concurrently
/// executing thread or process must use a separate instance of
/// SharedGroup.
///
/// Each instance of SharedGroup manages a single transaction at a
/// time. That transaction can be either a read transaction, or a
/// write transaction.
///
/// Utility classes ReadTransaction and WriteTransaction are provided
/// to make it safe and easy to work with transactions in a scoped
/// manner (by means of the RAII idiom). However, transactions can
/// also be explicitly started (begin_read(), begin_write()) and
/// stopped (end_read(), commit(), rollback()).
///
/// If a transaction is active when the SharedGroup is destroyed, that
/// transaction is implicitely terminated, either by a call to
/// end_read() or rollback().
///
/// Two processes that want to share a database file must reside on
/// the same host.
///
///
/// Desired exception behavior (not yet fully implemented)
/// ------------------------------------------------------
///
///  - If any data access API function throws an unexpcted exception during a
///    read transaction, the shared group accessor is left in state "error
///    during read".
///
///  - If any data access API function throws an unexpcted exception during a
///    write transaction, the shared group accessor is left in state "error
///    during write".
///
///  - If GroupShared::begin_write() or GroupShared::begin_read() throws an
///    unexpcted exception, the shared group accessor is left in state "no
///    transaction in progress".
///
///  - GroupShared::end_read() and GroupShared::rollback() do not throw.
///
///  - If GroupShared::commit() throws an unexpcted exception, the shared group
///    accessor is left in state "error during write" and the transaction was
///    not comitted.
///
///  - If GroupShared::advance_read() or GroupShared::promote_to_write() throws
///    an unexpcted exception, the shared group accessor is left in state "error
///    during read".
///
///  - If GroupShared::commit_and_continue_as_read() or
///    GroupShared::rollback_and_continue_as_read() throws an unexpcted
///    exception, the shared group accessor is left in state "error during
///    write".
///
/// It has not yet been decided exactly what an "unexpected exception" is, but
/// `std::bad_alloc` is surely one example. On the other hand, an expected
/// exception is one that is mentioned in the function specific documentation,
/// and is used to abort an operation due to a special, but expected condition.
///
/// States
/// ------
///
///  - A newly created shared group accessor is in state "no transaction in
///    progress".
///
///  - In state "error during read", almost all Realm API functions are
///    illegal on the connected group of accessors. The only valid operations
///    are destruction of the shared group, and GroupShared::end_read(). If
///    GroupShared::end_read() is called, the new state becomes "no transaction
///    in progress".
///
///  - In state "error during write", almost all Realm API functions are
///    illegal on the connected group of accessors. The only valid operations
///    are destruction of the shared group, and GroupShared::rollback(). If
///    GroupShared::end_write() is called, the new state becomes "no transaction
///    in progress"
class SharedGroup {
public:
    enum DurabilityLevel {
        durability_Full,
        durability_MemOnly
#ifndef _WIN32
        // Async commits are not yet supported on windows
        , durability_Async
#endif
    };

    /// Equivalent to calling open(const std::string&, bool,
    /// DurabilityLevel) on a default constructed instance.
    explicit SharedGroup(const std::string& file, bool no_create = false,
                         DurabilityLevel dlevel = durability_Full,
                         const char *encryption_key = 0);

    struct unattached_tag {};

    /// Create a SharedGroup instance in its unattached state. It may
    /// then be attached to a database file later by calling
    /// open(). You may test whether this instance is currently in its
    /// attached state by calling is_attached(). Calling any other
    /// function (except the destructor) while in the unattached state
    /// has undefined behavior.
    SharedGroup(unattached_tag) REALM_NOEXCEPT;

    // close any open database, returning to the unattached state.
    void close() REALM_NOEXCEPT;

    ~SharedGroup() REALM_NOEXCEPT;

    /// Attach this SharedGroup instance to the specified database
    /// file.
    ///
    /// If the database file does not already exist, it will be
    /// created (unless \a no_create is set to true.) When multiple
    /// threads are involved, it is safe to let the first thread, that
    /// gets to it, create the file.
    ///
    /// While at least one instance of SharedGroup exists for a
    /// specific database file, a "lock" file will be present too. The
    /// lock file will be placed in the same directory as the database
    /// file, and its name will be derived by appending ".lock" to the
    /// name of the database file.
    ///
    /// When multiple SharedGroup instances refer to the same file,
    /// they must specify the same durability level, otherwise an
    /// exception will be thrown.
    ///
    /// Calling open() on a SharedGroup instance that is already in
    /// the attached state has undefined behavior.
    ///
    /// \param file Filesystem path to a Realm database file.
    ///
    /// \throw util::File::AccessError If the file could not be
    /// opened. If the reason corresponds to one of the exception
    /// types that are derived from util::File::AccessError, the
    /// derived exception type is thrown. Note that InvalidDatabase is
    /// among these derived exception types.
    void open(const std::string& file, bool no_create = false,
              DurabilityLevel dlevel = durability_Full,
              bool is_backend = false, const char *encryption_key = 0);

#ifdef REALM_ENABLE_REPLICATION

    /// Equivalent to calling open(Replication&) on a
    /// default constructed instance.
    explicit SharedGroup(Replication& repl,
                         DurabilityLevel dlevel = durability_Full,
                         const char* encryption_key = 0);

    /// Open this group in replication mode. The specified Replication
    /// instance must remain in exixtence for as long as the
    /// SharedGroup.
    void open(Replication&, DurabilityLevel dlevel = durability_Full,
              const char* encryption_key = 0);

    friend class Replication;

#endif

    /// A SharedGroup may be created in the unattached state, and then
    /// later attached to a file with a call to open(). Calling any
    /// function other than open(), is_attached(), and ~SharedGroup()
    /// on an unattached instance results in undefined behavior.
    bool is_attached() const REALM_NOEXCEPT;

    /// Reserve disk space now to avoid allocation errors at a later
    /// point in time, and to minimize on-disk fragmentation. In some
    /// cases, less fragmentation translates into improved
    /// performance.
    ///
    /// When supported by the system, a call to this function will
    /// make the database file at least as big as the specified size,
    /// and cause space on the target device to be allocated (note
    /// that on many systems on-disk allocation is done lazily by
    /// default). If the file is already bigger than the specified
    /// size, the size will be unchanged, and on-disk allocation will
    /// occur only for the initial section that corresponds to the
    /// specified size. On systems that do not support preallocation,
    /// this function has no effect. To know whether preallocation is
    /// supported by Realm on your platform, call
    /// util::File::is_prealloc_supported().
    ///
    /// It is an error to call this function on an unattached shared
    /// group. Doing so will result in undefined behavior.
    void reserve(std::size_t size_in_bytes);

    /// Querying for changes:
    ///
    /// NOTE:
    /// "changed" means that one or more commits has been made to the database
    /// since the SharedGroup (on which wait_for_change() is called) last
    /// started, committed, promoted or advanced a transaction.
    ///
    /// No distinction is made between changes done by another process
    /// and changes done by another thread in the same process as the caller.
    ///
    /// Has db been changed ?
    bool has_changed();

#ifndef __APPLE__
    /// The calling thread goes to sleep until the database is changed, or
    /// until wait_for_change_release() is called. After a call to wait_for_change_release()
    /// further calls to wait_for_change() will return immediately. To restore
    /// the ability to wait for a change, a call to enable_wait_for_change()
    /// is required. Return true if the database has changed, false if it might have. 
    bool wait_for_change();

    /// release any thread waiting in wait_for_change() on *this* SharedGroup.
    void wait_for_change_release();

    /// re-enable waiting for change
    void enable_wait_for_change();
#endif
    // Transactions:

    struct VersionID {
        uint_fast64_t version;
        uint_fast32_t index;
        VersionID(const VersionID& v)
        {
            version = v.version;
            index = v.index;
        }
        VersionID(uint_fast64_t version = 0, uint_fast32_t index = 0)
        {
            this->version = version;
            this->index = index;
        }
        bool operator==(const VersionID& other) { return version == other.version; }
        bool operator!=(const VersionID& other) { return version != other.version; }
        bool operator<(const VersionID& other) { return version < other.version; }
        bool operator<=(const VersionID& other) { return version <= other.version; }
        bool operator>(const VersionID& other) { return version > other.version; }
        bool operator>=(const VersionID& other) { return version >= other.version; }
    };

    /// Exception thrown if an attempt to lock on to a specific version fails.
    class UnreachableVersion : public std::exception {
    public:
        const char* what() const REALM_NOEXCEPT_OR_NOTHROW override
        {
            return "Failed to lock on to specific version";
        }
    };

    /// Begin a new read transaction. Accessors obtained prior to this point
    /// are invalid (if they weren't already) and new accessors must be
    /// obtained from the group returned.
    /// If a \a specific_version is given as parameter, an attempt will be made
    /// to start the read transaction at that specific version. This is only
    /// guaranteed to succeed if at least one other SharedGroup has a transaction
    /// open pointing at that specific version. If the attempt fails, an exception
    /// of type UnreachableVersion is thrown
    const Group& begin_read(VersionID specific_version = VersionID());

    /// End a read transaction. Accessors are detached.
    void end_read() REALM_NOEXCEPT;

    /// Get a version id which may be used to request a different SharedGroup
    /// to start transaction at a specific version.
    VersionID get_version_of_current_transaction();

    /// Begin a new write transaction. Accessors obtained prior to this point
    /// are invalid (if they weren't already) and new accessors must be
    /// obtained from the group returned. It is illegal to call begin_write
    /// inside an active transaction.
    Group& begin_write();

    /// End the current write transaction. All accessors are detached.
    void commit();

    /// End the current write transaction. All accessors are detached.
    void rollback() REALM_NOEXCEPT;

    /// Report the number of distinct versions currently stored in the database.
    /// Note: the database only cleans up versions as part of commit, so ending
    /// a read transaction will not immediately release any versions.
    uint_fast64_t get_number_of_versions();

    /// Compact the database file.
    /// - The method will throw if called inside a transaction.
    /// - The method will return false if other SharedGroups are accessing the database
    ///   in which case compaction is not done.
    /// It will return true following succesful compaction.
    /// While compaction is in progress, attempts by other
    /// threads or processes to open the database will wait.
    /// Be warned that resource requirements for compaction is proportional to the amount
    /// of live data in the database.
    /// Compaction works by writing the database contents to a temporary databasefile and
    /// then replacing the database with the temporary one. The name of the temporary
    /// file is formed by appending ".tmp_compaction_space" to the name of the databse
    /// 
    bool compact();

#ifdef REALM_DEBUG
    void test_ringbuf();
#endif

private:
    struct SharedInfo;
    struct ReadCount;
    struct ReadLockInfo {
        uint_fast64_t   m_version;
        uint_fast32_t   m_reader_idx;
        ref_type        m_top_ref;
        size_t          m_file_size;
        ReadLockInfo() : m_version(std::numeric_limits<std::size_t>::max()), 
                         m_reader_idx(0), m_top_ref(0), m_file_size(0) {};
    };

    // Member variables
    Group      m_group;
    ReadLockInfo m_readlock;
    uint_fast32_t   m_local_max_entry;
    util::File m_file;
    util::File::Map<SharedInfo> m_file_map; // Never remapped
    util::File::Map<SharedInfo> m_reader_map;
    bool m_wait_for_change_enabled;
    std::string m_lockfile_path;
    std::string m_db_path;
    const char* m_key;
    enum TransactStage {
        transact_Ready,
        transact_Reading,
        transact_Writing
    };
    TransactStage m_transact_stage;
#ifndef _WIN32
    util::PlatformSpecificCondVar m_room_to_write;
    util::PlatformSpecificCondVar m_work_to_do;
    util::PlatformSpecificCondVar m_daemon_becomes_ready;
    util::PlatformSpecificCondVar m_new_commit_available;
#endif

    // Ring buffer managment
    bool        ringbuf_is_empty() const REALM_NOEXCEPT;
    std::size_t ringbuf_size() const REALM_NOEXCEPT;
    std::size_t ringbuf_capacity() const REALM_NOEXCEPT;
    bool        ringbuf_is_first(std::size_t ndx) const REALM_NOEXCEPT;
    void        ringbuf_remove_first() REALM_NOEXCEPT;
    std::size_t ringbuf_find(uint64_t version) const REALM_NOEXCEPT;
    ReadCount&  ringbuf_get(std::size_t ndx) REALM_NOEXCEPT;
    ReadCount&  ringbuf_get_first() REALM_NOEXCEPT;
    ReadCount&  ringbuf_get_last() REALM_NOEXCEPT;
    void        ringbuf_put(const ReadCount& v);
    void        ringbuf_expand();

    // Grab the latest readlock and update readlock info. Compare latest against
    // current (before updating) and determine if the version is the same as before.
    // As a side effect update memory mapping to ensure that the ringbuffer entries
    // referenced in the readlock info is accessible.
    // The caller may provide an uninitialized readlock in which case same_as_before
    // is given an undefined value.
    void grab_latest_readlock(ReadLockInfo& readlock, bool& same_as_before);

    // Try to grab a readlock for a specific version. Fails if the version is no longer
    // accessible.
    bool grab_specific_readlock(ReadLockInfo& readlock, bool& same_as_before, 
                                VersionID specific_version);

    // Release a specific readlock. The readlock info MUST have been obtained by a
    // call to grab_latest_readlock() or grab_specific_readlock().
    void release_readlock(ReadLockInfo& readlock) REALM_NOEXCEPT;

    void do_begin_write();
    void do_commit();

    // return the current version of the database - note, this is not necessarily
    // the version seen by any currently open transactions.
    uint_fast64_t get_current_version();

    // make sure the given index is within the currently mapped area.
    // if not, expand the mapped area. Returns true if the area is expanded.
    bool grow_reader_mapping(uint_fast32_t index);

    // Must be called only by someone that has a lock on the write
    // mutex.
    void low_level_commit(uint_fast64_t new_version);

    void do_async_commits();

#ifdef REALM_ENABLE_REPLICATION

    /// Advance the current read transaction to include latest state.
    /// All accessors are retained and synchronized to the new state
    /// according to the (to be) defined operational transform.
    /// If a \a specific_version is given as parameter, an attempt will be made
    /// to start the read transaction at that specific version. This is only
    /// guaranteed to succeed if at least one other SharedGroup has a transaction
    /// open pointing at that specific version, and if the version requested
    /// is the same or later than the one currently accessed.
    /// Fails with exception UnreachableVersion.
    void advance_read(VersionID specific_version = VersionID());

    /// Promote the current read transaction to a write transaction.
    /// CAUTION: This also synchronizes with latest state of the database,
    /// including synchronization of all accessors.
    /// FIXME: A version of this which does NOT synchronize with latest
    /// state will be made available later, once we are able to merge commits.
    void promote_to_write();

    /// End the current write transaction and transition atomically into
    /// a read transaction, WITHOUT synchronizing to external changes
    /// to data. All accessors are retained and continue to reflect the
    /// state at commit.
    void commit_and_continue_as_read();

    /// Abort the current write transaction, discarding all changes within it,
    /// and thus restoring state to when promote_to_write() was last called.
    /// Any accessors referring to the aborted state will be detached. Accessors
    /// which was detached during the write transaction (for whatever reason)
    /// are not restored but will remain detached.
    void rollback_and_continue_as_read();

    /// called by WriteLogCollector to transfer the actual commit log for
    /// accessor retention/update as part of rollback.
    void do_rollback_and_continue_as_read(const char* begin, const char* end);
#endif
    friend class ReadTransaction;
    friend class WriteTransaction;
    friend class LangBindHelper;
    friend class _impl::WriteLogCollector;
};



class ReadTransaction {
public:
    ReadTransaction(SharedGroup& sg):
        m_shared_group(sg)
    {
        m_shared_group.begin_read(); // Throws
    }

    ~ReadTransaction() REALM_NOEXCEPT
    {
        m_shared_group.end_read();
    }

    bool has_table(StringData name) const REALM_NOEXCEPT
    {
        return get_group().has_table(name);
    }

    ConstTableRef get_table(std::size_t table_ndx) const
    {
        return get_group().get_table(table_ndx); // Throws
    }

    ConstTableRef get_table(StringData name) const
    {
        return get_group().get_table(name); // Throws
    }

    template<class T> BasicTableRef<const T> get_table(StringData name) const
    {
        return get_group().get_table<T>(name); // Throws
    }

    const Group& get_group() const REALM_NOEXCEPT
    {
        return m_shared_group.m_group;
    }

private:
    SharedGroup& m_shared_group;
};


class WriteTransaction {
public:
    WriteTransaction(SharedGroup& sg):
        m_shared_group(&sg)
    {
        m_shared_group->begin_write(); // Throws
    }

    ~WriteTransaction() REALM_NOEXCEPT
    {
        if (m_shared_group)
            m_shared_group->rollback();
    }

    bool has_table(StringData name) const REALM_NOEXCEPT
    {
        return get_group().has_table(name);
    }

    TableRef get_table(std::size_t table_ndx) const
    {
        return get_group().get_table(table_ndx); // Throws
    }

    TableRef get_table(StringData name) const
    {
        return get_group().get_table(name); // Throws
    }

    TableRef add_table(StringData name, bool require_unique_name = true) const
    {
        return get_group().add_table(name, require_unique_name); // Throws
    }

    TableRef get_or_add_table(StringData name, bool* was_added = 0) const
    {
        return get_group().get_or_add_table(name, was_added); // Throws
    }

    template<class T> BasicTableRef<T> get_table(StringData name) const
    {
        return get_group().get_table<T>(name); // Throws
    }

    template<class T>
    BasicTableRef<T> add_table(StringData name, bool require_unique_name = true) const
    {
        return get_group().add_table<T>(name, require_unique_name); // Throws
    }

    template<class T> BasicTableRef<T> get_or_add_table(StringData name, bool* was_added = 0) const
    {
        return get_group().get_or_add_table<T>(name, was_added); // Throws
    }

    Group& get_group() const REALM_NOEXCEPT
    {
        REALM_ASSERT(m_shared_group);
        return m_shared_group->m_group;
    }

    void commit()
    {
        REALM_ASSERT(m_shared_group);
        m_shared_group->commit();
        m_shared_group = 0;
    }

    void rollback() REALM_NOEXCEPT
    {
        REALM_ASSERT(m_shared_group);
        m_shared_group->rollback();
        m_shared_group = 0;
    }

private:
    SharedGroup* m_shared_group;
};





// Implementation:

inline SharedGroup::SharedGroup(const std::string& file, bool no_create, DurabilityLevel dlevel, const char* key):
    m_group(Group::shared_tag())
{
    open(file, no_create, dlevel, false, key);
}

inline SharedGroup::SharedGroup(unattached_tag) REALM_NOEXCEPT:
    m_group(Group::shared_tag())
{
}

inline bool SharedGroup::is_attached() const REALM_NOEXCEPT
{
    return m_file_map.is_attached();
}

#ifdef REALM_ENABLE_REPLICATION
inline SharedGroup::SharedGroup(Replication& repl, DurabilityLevel dlevel, const char* key):
    m_group(Group::shared_tag())
{
    open(repl, dlevel, key);
}
#endif

} // namespace realm

#endif // REALM_GROUP_SHARED_HPP
