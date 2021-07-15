using System;
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
            this.transaction = transaction;
            Entry = entry;

            InitCommands();
        }

        private void InitCommands()
        {
            SaveCommand = new Command(async () => await Save());
        }

        private async Task Save()
        {
            if (string.IsNullOrEmpty(Entry.Title))
            {
                await DialogService.ShowAlert("Error", "You cannot save a note with an empty title");
                return;
            }

            Entry.Metadata.LastModifiedDate = DateTimeOffset.Now;
            transaction.Commit();
            transaction = null;
            await NavigationService.GoBack();
        }

        override internal void OnDisappearing()
        {
            transaction?.Dispose();
        }
    }
}

