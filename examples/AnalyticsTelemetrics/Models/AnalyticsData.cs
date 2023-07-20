using MongoDB.Bson;
using Realms;

namespace AnalyticsTelemetrics.Models
{
    public partial class AnalyticsData : IAsymmetricObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public DateTimeOffset Timestamp { get; set; }

        public string EventType { get; set; }

        public Metadata Metadata { get; set; }
    }

    public partial class Metadata : IEmbeddedObject
    {
        public Guid DeviceId { get; set; }

        public string Platform { get; set; }

        public int AppVersion { get; set; }

        public string Country { get; set; }

        public int Age { get; set; }
    }
}