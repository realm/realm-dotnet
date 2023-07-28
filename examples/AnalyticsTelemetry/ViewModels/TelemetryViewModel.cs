using AnalyticsTelemetry.Models;
using AnalyticsTelemetry.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nito.AsyncEx;

namespace AnalyticsTelemetry.ViewModels
{
    public partial class TelemetryViewModel : ObservableObject
    {
        // Configuration values for the sensor collection
        private const int _numberOfSensors = 5;
        private const int _collectionDelayMilliseconds = 500;

        private readonly List<Sensor> _sensors;

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _sensorCollectionTask;

        [ObservableProperty]
        private string _logText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopSensorCollectionCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartSensorCollectionCommand))]
        private bool _isCollectionRunning = false;

        public TelemetryViewModel()
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
            if (!IsCollectionRunning || _sensorCollectionTask is null)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            await _sensorCollectionTask;

            _cancellationTokenSource = null;
            _sensorCollectionTask = null;
            IsCollectionRunning = false;
        }

        private bool CanStopCollection() => IsCollectionRunning;

        private bool CanStartCollection() => !IsCollectionRunning;

        private void SensorCollection(CancellationToken cancellationToken)
        {
            // AsyncContext.Run runs the code on a single-threaded synchronization context.
            // Without it, it is not possible to guarantee that the continuation of async methods,
            // such as Task.Delay, will be executed on the same thread. As realm instances are thread confined,
            // an exception will be raised if a realm is used on a different thread than the one it was opened on.
            // The use of AsyncContext.Run is not mandatory, but simplifies the use of realm for this example.
            // Without it, you need to ensure that the realm is always accessed on the thread it was created,
            // for example waiting synchronously with Task.Delay(...).Wait().
            AsyncContext.Run(async () =>
            {
                AddToLog("Collection started");

                using var realm = RealmService.GetRealm();

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        foreach (var sensor in _sensors)
                        {
                            var tempReading = sensor.GetTemperatureReading();

                            AddToLog($"Sensor_{tempReading.Sensor?.Id} - {tempReading.Temperature}");

                            realm.Write(() =>
                            {
                                realm.Add(tempReading);
                            });
                        }

                        AddToLog("- - - - - -");
                        await Task.Delay(_collectionDelayMilliseconds, cancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    // This exception is raised if the task gets canceled during Task.Delay
                }

                AddToLog(Environment.NewLine + "Collection stopped");
            });
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
                return new TemperatureReading(timestamp: DateTimeOffset.Now,
                    temperature: _random.Next(22, 26),
                    sensor: new SensorInfo(_id, _location));
            }
        }
    }
}