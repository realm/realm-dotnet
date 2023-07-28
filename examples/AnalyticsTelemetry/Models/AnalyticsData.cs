using MongoDB.Bson;
using Realms;

namespace AnalyticsTelemetry.Models
{
    public partial class AnalyticsData : IAsymmetricObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public DateTimeOffset Timestamp { get; private set; }

        public string EventType { get; private set; }

        public Metadata? Metadata { get; private set; }

        public AnalyticsData(DateTimeOffset timestamp, string eventType, Metadata? metadata)
        {
            Timestamp = timestamp;
            EventType = eventType;
            Metadata = metadata;
        }
    }

    public partial class Metadata : IEmbeddedObject
    {
        public Guid DeviceId { get; private set; }

        public string Platform { get; private set; }

        public int AppVersion { get; private set; }

        public string Country { get; private set; }

        public int Age { get; private set; }

        public Metadata(Guid deviceId, string platform, int appVersion, string country, int age)
        {
            DeviceId = deviceId;
            Platform = platform;
            AppVersion = appVersion;
            Country = country;
            Age = age;
        }
    }
}