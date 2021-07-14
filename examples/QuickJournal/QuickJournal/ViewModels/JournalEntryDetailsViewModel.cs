using System.Threading.Tasks;
using System.Windows.Input;
using QuickJournal.Models;
using QuickJournal.Services;
using Realms;
using Xamarin.Forms;

namespace QuickJournal.ViewModels
{
    public class JournalEntryDetailsViewModel : BaseViewModel
    {
        private Transaction transaction;

        public JournalEntry Entry { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public JournalEntryDetailsViewModel(JournalEntry entry, Transaction transaction)
        {
            Entry = entry;
            this.transaction = transaction;

            InitCommands();
        }

        private void InitCommands()
        {
            SaveCommand = new Command(async () => await Save());
        }

        private async Task Save()
        {
            transaction.Commit();
            await NavigationService.GoBack();
        }

        override internal void OnDisappearing()
        {
            transaction.Dispose();
        }
    }
}

