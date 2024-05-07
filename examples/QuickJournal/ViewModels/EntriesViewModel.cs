using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournal.Messages;
using QuickJournal.Models;
using Realms;

namespace QuickJournal.ViewModels
{
    public partial class EntriesViewModel : ObservableObject
    {
        private readonly Realm realm;

        [ObservableProperty]
        private IEnumerable<JournalEntryViewModel>? entries;

        public EntriesViewModel()
        {
            realm = Realm.GetInstance();
            Entries = new WrapperCollection<JournalEntry, JournalEntryViewModel>(realm.All<JournalEntry>(), i => new JournalEntryViewModel(i));
        }

        [RelayCommand]
        public async Task AddEntry()
        {
            var entry = await realm.WriteAsync(() =>
            {
                return realm.Add(new JournalEntry
                {
                    Metadata = new EntryMetadata
                    {
                        CreatedDate = DateTimeOffset.Now,
                    }
                });
            });

            await GoToEntry(entry);
        }

        [RelayCommand]
        public async Task EditEntry(JournalEntryViewModel entry)
        {
            await GoToEntry(entry.Entry);
        }

        [RelayCommand]
        public async Task DeleteEntry(JournalEntryViewModel entry)
        {
            await realm.WriteAsync(() =>
            {
                realm.Remove(entry.Entry);
            });
        }

        private async Task GoToEntry(JournalEntry entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Entry", entry },
            };
            await Shell.Current.GoToAsync($"entryDetail", navigationParameter);
        }
    }

    public class WrapperCollection<T, TViewModel> : INotifyCollectionChanged, IEnumerable<TViewModel>
        where T : IRealmObject
        where TViewModel : class
    {
        private IRealmCollection<T> _results;
        private Func<T, TViewModel> _viewModelFactory;

        public int Count => _results.Count;

        public TViewModel this[int index] => _viewModelFactory(_results[index]);

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { _results.CollectionChanged += value; }
            remove { _results.CollectionChanged -= value; }
        }

        public WrapperCollection(IQueryable<T> query, Func<T, TViewModel> viewModelFactory)
        {
            _results = query.AsRealmCollection();
            _viewModelFactory = viewModelFactory;
        }

        public IEnumerator<TViewModel> GetEnumerator()
        {
            foreach (var item in _results)
            {
                yield return _viewModelFactory(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class JournalEntryViewModel : INotifyPropertyChanged
    {
        private JournalEntry _entry;

        public event PropertyChangedEventHandler? PropertyChanged;

        public JournalEntry Entry => _entry;

        public string Summary => _entry.Title + _entry.Body;

        public JournalEntryViewModel(JournalEntry entry)
        {
            _entry = entry;
            _entry.PropertyChanged += Inner_PropertyChanged;
        }

        private void Inner_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(JournalEntry.Title) || e.PropertyName == nameof(JournalEntry.Body))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Summary)));
            }
        }
    }
}