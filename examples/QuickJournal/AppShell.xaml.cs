using QuickJournal.Views;

namespace QuickJournal;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("entryDetail", typeof(JournalEntryDetailPage));
    }
}