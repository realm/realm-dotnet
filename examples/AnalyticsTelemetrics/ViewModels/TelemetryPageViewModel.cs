using AnalyticsTelemetrics.Models;
using AnalyticsTelemetrics.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalyticsTelemetrics.ViewModels
{
    public partial class TelemetryPageViewModel : ObservableObject
    {
        // Configuration values for the sensor collection
        private const int _numberOfSensors = 5;
        private const int _collectionDelayMilliseconds = 500;

        private readonly List<Sensor> _sensors;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _sensorCollectionTask;

        [ObservableProperty]
        private string _logText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopSensorCollectionCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartSensorCollectionCommand))]
        private bool _isCollectionRunning = false;

        public TelemetryPageViewModel()
        {
            _sensors = new List<Sensor>();
            for (int i = 1; i <= _numberOfSensors; i++)
            {
                _sensors.Add(new Sensor(i));
            }
        }

        [RelayCommand]
        public async Task OnDisappearing()
        {
            await StopSensorCollection();
        }

        [RelayCommand(CanExecute = nameof(CanStartCollection))]
        public void StartSensorCollection()
        {
            if (IsCollectionRunning)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _sensorCollectionTask = Task.Run(() => SensorCollection(_cancellationTokenSource.Token));
            IsCollectionRunning = true;
        }

        [RelayCommand(CanExecute = nameof(CanStopCollection))]
        public async Task StopSensorCollection()
        {
            if (!IsCollectionRunning)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            await _sensorCollectionTask;

            _cancellationTokenSource = null;
            _sensorCollectionTask = null;
            IsCollectionRunning = false;
        }

        private bool CanStopCollection() => IsCollectionRunning;

        private bool CanStartCollection() => !IsCollectionRunning;

        private async Task SensorCollection(CancellationToken cancellationToken)
        {
            AddToLog("Collection started");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var sensor in _sensors)
                    {
                        var realm = RealmService.GetRealm();

                        var tempReading = sensor.GetTemperatureReading();

                        AddToLog($"Sensor_{tempReading.Sensor.Id} - {tempReading.Temperature}");

                        realm.Write(() =>
                        {
                            realm.Add(tempReading);
                        });
                    }

                    AddToLog("- - - - - -");
                    Console.WriteLine(Environment.CurrentManagedThreadId);
                    await Task.Delay(_collectionDelayMilliseconds, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // This exception is raised if the taks gets canceled during Task.Delay
            }

            AddToLog(Environment.NewLine + "Collection stopped");
        }

        private void AddToLog(string text)
        {
            var newText = text + Environment.NewLine + LogText;

            // This is used to avoid making the log grow indefinitely
            if (newText.Length > 3000)
            {
                LogText = newText[..2000];
            }
            else
            {
                LogText = newText;
            }
        }

        private class Sensor
        {
            private readonly int _id;
            private readonly string _location;
            private readonly Random _random;

            public Sensor(int id)
            {
                _id = id;
                _location = $"Location_{_id}";
                _random = new Random(Guid.NewGuid().GetHashCode());
            }

            public TemperatureReading GetTemperatureReading()
            {
                return new TemperatureReading()
                {
                    Timestamp = DateTimeOffset.Now,
                    Temperature = _random.Next(22, 26),
                    Sensor = new SensorInfo { Id = _id, Location = _location },
                };
            }
        }
    }
}