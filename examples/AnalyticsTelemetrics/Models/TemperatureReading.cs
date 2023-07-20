using MongoDB.Bson;
using Realms;

namespace AnalyticsTelemetrics.Models
{
    public partial class TemperatureReading : IAsymmetricObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public float Temperature { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public SensorInfo Sensor { get; set; }
    }

    public partial class SensorInfo : IEmbeddedObject
    {
        public int Id { get; set; }

        public string Location { get; set; }
    }
}