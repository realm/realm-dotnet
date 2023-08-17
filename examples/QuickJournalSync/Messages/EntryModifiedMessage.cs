using CommunityToolkit.Mvvm.Messaging.Messages;
using QuickJournalSync.Models;

namespace QuickJournalSync.Messages;

public class EntryModifiedMessage : ValueChangedMessage<JournalEntry>
{
    public EntryModifiedMessage(JournalEntry entry) : base(entry)
    {
    }
}