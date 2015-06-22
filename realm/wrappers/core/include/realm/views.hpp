#ifndef REALM_VIEWS_HPP
#define REALM_VIEWS_HPP

#include <realm/column.hpp>
#include <realm/column_string_enum.hpp>

using namespace realm;

// This class is for common functionality of ListView and LinkView which inherit from it. Currently it only 
// supports sorting.
class RowIndexes
{
public:
    RowIndexes(Column::unattached_root_tag urt, realm::Allocator& alloc) : 
#ifdef REALM_COOKIE_CHECK
        cookie(cookie_expected), 
#endif
        m_row_indexes(urt, alloc), m_auto_sort(false) 
    {}

    RowIndexes(Column&& col) : 
#ifdef REALM_COOKIE_CHECK
        cookie(cookie_expected),
#endif      
        m_row_indexes(std::move(col)), m_auto_sort(false) 
    {}

    virtual ~RowIndexes() 
    {
#ifdef REALM_COOKIE_CHECK
        cookie = 0x7765697633333333; // 0x77656976 = 'view'; 0x33333333 = '3333' = destructed
#endif
    }

    // Return a column of the table that m_row_indexes are pointing at (which is the target table for LinkList and
    // parent table for TableView)
    virtual const ColumnBase& get_column_base(size_t index) const = 0;

    virtual size_t size() const = 0;

    // These two methods are overridden by TableView. They are no-ops for LinkView because sync'ed automatically
    virtual uint_fast64_t sync_if_needed() const { return 0; }
    virtual bool is_in_sync() const { return true; }

    void check_cookie() const
    {
#ifdef REALM_COOKIE_CHECK
        REALM_ASSERT_RELEASE(cookie == cookie_expected);
#endif
    }

    // Predicate for std::sort
    struct Sorter
    {
        Sorter(){}
        Sorter(std::vector<size_t> columns, std::vector<bool> ascending) : m_column_indexes(columns), m_ascending(ascending) {}
        bool operator()(size_t i, size_t j) const
        {
            for (size_t t = 0; t < m_columns.size(); t++) {
                // todo/fixme, special treatment of ColumnStringEnum by calling ColumnStringEnum::compare_values()
                // instead of the general ColumnTemplate::compare_values() becuse it cannot overload inherited 
                // `int64_t get_val()` of Column. Such column inheritance needs to be cleaned up 
                int c;
                if (const ColumnStringEnum* cse = m_string_enum_columns[t])
                    c = cse->compare_values(i, j);
                else
                    c = m_columns[t]->compare_values(i, j);

                if (c != 0)
                    return m_ascending[t] ? c > 0 : c < 0;
            }
            return false; // row i == row j
        }

        void init(RowIndexes* row_indexes)
        {
            m_columns.clear();
            m_string_enum_columns.clear();
            m_columns.resize(m_column_indexes.size(), 0);
            m_string_enum_columns.resize(m_column_indexes.size(), 0);

            for (size_t i = 0; i < m_column_indexes.size(); i++) {
                const ColumnBase& cb = row_indexes->get_column_base(m_column_indexes[i]);
                const ColumnTemplateBase* ctb = dynamic_cast<const ColumnTemplateBase*>(&cb);
                REALM_ASSERT(ctb);
                if (const ColumnStringEnum* cse = dynamic_cast<const ColumnStringEnum*>(&cb))
                    m_string_enum_columns[i] = cse;
                else
                    m_columns[i] = ctb;
            }
        }

        std::vector<size_t> m_column_indexes;
        std::vector<bool> m_ascending;
        std::vector<const ColumnTemplateBase*> m_columns;
        std::vector<const ColumnStringEnum*> m_string_enum_columns;
    };

    // Sort m_row_indexes according to one column
    void sort(size_t column, bool ascending = true);

    // Sort m_row_indexes according to multiple columns
    void sort(std::vector<size_t> columns, std::vector<bool> ascending);

    // Re-sort view according to last used criterias
    void re_sort();

#ifdef REALM_COOKIE_CHECK
    static const uint64_t cookie_expected = 0x7765697677777777ull; // 0x77656976 = 'view'; 0x77777777 = '7777' = alive
    uint64_t cookie;
#endif

    Column m_row_indexes;
    Sorter m_sorting_predicate; // Stores sorting criterias (columns + ascending)
    bool m_auto_sort;
};

#endif // REALM_VIEWS_HPP
