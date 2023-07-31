using QuickJournalSync.Views;

namespace QuickJournalSync;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("entryDetail", typeof(EntryDetailPage));
    }
}