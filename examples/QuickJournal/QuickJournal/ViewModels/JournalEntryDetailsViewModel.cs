using System.Windows.Input;
using Realms;
using Xamarin.Forms;

namespace QuickJournal
{
    public class JournalEntryDetailsViewModel
    {
        private Transaction _transaction;

        public JournalEntry Entry { get; private set; }

        internal INavigation Navigation { get; set; }

        public ICommand SaveCommand { get; private set; }

        public JournalEntryDetailsViewModel(JournalEntry entry, Transaction transaction)
        {
            Entry = entry;
            _transaction = transaction;
            SaveCommand = new Command(Save);
        }

        private void Save()
        {
            _transaction.Commit();
            Navigation.PopAsync(true);
        }

        internal void OnDisappearing()
        {
            _transaction.Dispose();
        }
   }
}

