using CommunityToolkit.Mvvm.Messaging.Messages;
using QuickJournal.Models;

namespace QuickJournal.Messages
{
    public class EntryModifiedMessage : ValueChangedMessage<JournalEntry>
    {
        public EntryModifiedMessage(JournalEntry entry) : base(entry)
        {
        }
    }
}

