using System;
using System.Threading.Tasks;
using System.Windows.Input;
using QuickJournal.Models;
using QuickJournal.Services;
using Realms;
using Xamarin.CommunityToolkit.ObjectModel;

namespace QuickJournal.ViewModels
{
    public class JournalEntryDetailsViewModel : BaseViewModel
    {
        private Transaction transaction;

        public JournalEntry Entry { get; }

        public ICommand SaveCommand { get; }

        public JournalEntryDetailsViewModel(JournalEntry entry, Transaction transaction)
        {
            this.transaction = transaction;
            Entry = entry;

            SaveCommand = new AsyncCommand(Save);
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

