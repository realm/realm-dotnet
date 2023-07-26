using AnalyticsTelemetrics.Models;
using AnalyticsTelemetrics.Services;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalyticsTelemetrics.ViewModels
{
    public partial class AnalyticsViewModel : ObservableObject
    {
        // Configuration values for the analytics generation
        private const int _numberOfUsers = 100;
        private const int _numberOfUsersToTake = 10;
        private const int _collectionDelayMilliseconds = 1200;

        // Events and platforms to use for analytics generation
        private static readonly string[] _events = new string[] { "login", "logout", "signUp", "purchase", "refund", "watch" };
        private static readonly string[] _platforms = new string[] { "iOS", "Android", "macOS", "windows" };

        private readonly List<FakeUser> _fakeUsers;

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _analyticsGenerationTask;

        [ObservableProperty]
        private string _logText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopAnalyticsGenerationCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartAnalyticsGenerationCommand))]
        private bool _isGenerationRunning = false;

        public AnalyticsViewModel()
        {
            // Generate fake users
            _fakeUsers = new Faker<FakeUser>()
                .RuleFor(f => f.Country, f => f.Address.Country())
                .RuleFor(f => f.DeviceId, f => f.Random.Guid())
                .RuleFor(f => f.Age, f => f.Random.Int(18, 99))
                .RuleFor(f => f.Platform, f => f.Random.ArrayElement(_platforms))
                .RuleFor(f => f.AppVersion, f => f.Random.Int(1, 10))
                .UseSeed(200)
                .Generate(_numberOfUsers);
        }

        [RelayCommand]
        public async Task OnDisappearing()
        {
            await StopAnalyticsGeneration();
        }

        [RelayCommand(CanExecute = nameof(CanStartGeneration))]
        public void StartAnalyticsGeneration()
        {
            if (IsGenerationRunning)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _analyticsGenerationTask = Task.Run(() => AnalyticsGeneration(_cancellationTokenSource.Token));
            IsGenerationRunning = true;
        }

        [RelayCommand(CanExecute = nameof(CanStopGeneration))]
        public async Task StopAnalyticsGeneration()
        {
            if (!IsGenerationRunning || _analyticsGenerationTask is null)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            await _analyticsGenerationTask;

            _cancellationTokenSource = null;
            _analyticsGenerationTask = null;
            IsGenerationRunning = false;
        }

        private bool CanStopGeneration() => IsGenerationRunning;

        private bool CanStartGeneration() => !IsGenerationRunning;

        private async Task AnalyticsGeneration(CancellationToken cancellationToken)
        {
            AddToLog("Generation started");

            var random = new Random();
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // A new realm instance is obtained on each iteration, because after Task.Delay the execution
                    // could continue on a different thread. As realm instances are thread-confined,
                    // this will raise an exception if the realm is opened outside of the loop.
                    // If you prefer to keep a realm open for the lifetime of the task, then
                    // you need to ensure that the realm is always accessed on the thread it was created,
                    // for example waiting synchronously with Task.Delay(...).Wait().
                    using var realm = RealmService.GetRealm();

                    // Selecting only a subset of the fake users at each step to generate more realistic data
                    foreach (var fakeUser in _fakeUsers.OrderBy(t => random.Next()).Take(_numberOfUsersToTake))
                    {
                        var analyticEvent = fakeUser.GetRandomAnalyticsEvent();

                        AddToLog($"{fakeUser.DeviceId.ToString()[..5]} - {analyticEvent.EventType}");

                        realm.Write(() =>
                        {
                            realm.Add(analyticEvent);
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

            AddToLog(Environment.NewLine + "Generation stopped");
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

        private class FakeUser
        {
            private readonly Random _random;

            public Guid DeviceId { get; set; }

            public string Platform { get; set; } = null!;

            public int AppVersion { get; set; }

            public string Country { get; set; } = null!;

            public int Age { get; set; }

            public FakeUser()
            {
                _random = new Random(Guid.NewGuid().GetHashCode());
            }

            public AnalyticsData GetRandomAnalyticsEvent()
            {
                return new AnalyticsData(timestamp: DateTimeOffset.Now,
                     eventType: _events[_random.Next(_events.Length)],
                     metadata: new Metadata(DeviceId, Platform, AppVersion, Country, Age));
            }
        }
    }
}