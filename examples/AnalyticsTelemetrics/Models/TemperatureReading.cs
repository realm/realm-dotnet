using MongoDB.Bson;
using Realms;

namespace AnalyticsTelemetry.Models
{
    public partial class TemperatureReading : IAsymmetricObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public DateTimeOffset Timestamp { get; private set; }

        public float Temperature { get; private set; }

        public SensorInfo? Sensor { get; private set; }

        public TemperatureReading(DateTimeOffset timestamp, float temperature, SensorInfo? sensor)
        {
            Temperature = temperature;
            Timestamp = timestamp;
            Sensor = sensor;
        }
    }

    public partial class SensorInfo : IEmbeddedObject
    {
        public int Id { get; private set; }

        public string Location { get; private set; }

        public SensorInfo(int id, string location)
        {
            Id = id;
            Location = location;
        }
    }
}