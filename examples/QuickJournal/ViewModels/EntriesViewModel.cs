using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

            realm.Write(() =>
            {
                realm.RemoveAll();
                for (int i = 0; i < 200; i++)
                {
                    realm.Add(new JournalEntry
                    {
                        Title = $"Title-{i}",
                        Body = $"Body-{i}",
                        Metadata = new EntryMetadata
                        {
                            CreatedDate = DateTimeOffset.Now,
                        }
                    });
                }
            });

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

            await GoToEntry(new JournalEntryViewModel(entry));
        }

        [RelayCommand]
        public async Task EditEntry(JournalEntryViewModel entry)
        {
            await GoToEntry(entry);
        }

        [RelayCommand]
        public async Task DeleteEntry(JournalEntryViewModel entry)
        {
            await realm.WriteAsync(() =>
            {
                realm.Remove(entry.Entry);
            });
        }

        private async Task GoToEntry(JournalEntryViewModel entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Entry", entry.Entry },
            };
            await Shell.Current.GoToAsync($"entryDetail", navigationParameter);
        }
    }

    public class WrapperCollection<T, TViewModel> : INotifyCollectionChanged, IReadOnlyList<TViewModel>
        where T : IRealmObject
        where TViewModel : class
    {
        private IRealmCollection<T> _results;
        private Func<T, TViewModel> _viewModelFactory;

        public int Count => _results.Count;

        public TViewModel this[int index]
        {
            get
            {
                var item = _viewModelFactory(_results[index]);
                Console.WriteLine($"Indexer: {item}");
                return item;
            }
        }

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
                Console.WriteLine($"Enumerator: {item}");
                yield return _viewModelFactory(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class JournalEntryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public JournalEntry Entry { get; private set; }

        public string Summary => Entry.Title + Entry.Body;

        public JournalEntryViewModel(JournalEntry entry)
        {
            Entry = entry;
            Entry.PropertyChanged += Inner_PropertyChanged;
        }

        public override string ToString()
        {
            return Entry.ToString();
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