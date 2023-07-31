using MongoDB.Bson;
using QuickJournalSync.Services;
using Realms;

namespace QuickJournalSync.Models
{
    public partial class JournalEntry : IRealmObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        [MapTo("userId")]
        public string UserId { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public EntryMetadata? Metadata { get; set; }

        public JournalEntry()
        {
            if (RealmService.CurrentUser == null)
            {
                throw new Exception("Cannot create a Journal Entry before login!");
            }

            UserId = RealmService.CurrentUser.Id;
        }
    }
}
