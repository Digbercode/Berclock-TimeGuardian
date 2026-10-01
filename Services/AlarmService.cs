using System.Collections.Concurrent;
using Win7AlarmClassic.Models;

namespace Win7AlarmClassic.Services;

public sealed class AlarmTriggeredEventArgs : EventArgs
{
    public required Alarm Alarm { get; init; }
}

public sealed class AlarmService : IDisposable
{
    private readonly JsonAlarmRepository _repository;
    private readonly System.Threading.Timer _timer;
    private readonly object _sync = new();
    private readonly List<Alarm> _alarms;
    private readonly ConcurrentDictionary<Guid, DateTime> _lastTriggered = new();
    private bool _disposed;

    public event EventHandler<AlarmTriggeredEventArgs>? AlarmTriggered;

    public IReadOnlyList<Alarm> Alarms
    {
        get
        {
            lock (_sync)
                return _alarms.Select(Clone).ToList();
        }
    }

    public AlarmService(JsonAlarmRepository repository)
    {
        _repository = repository;
        _alarms = repository.Load();
        _timer = new System.Threading.Timer(_ => Tick(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
    }

    public void Add(Alarm alarm)
    {
        lock (_sync)
        {
            alarm.Id = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(alarm.Group))
                alarm.Group = "Geral";
            _alarms.Add(alarm);
            SaveUnsafe();
        }
    }

    public void Update(Alarm alarm)
    {
        lock (_sync)
        {
            var existing = _alarms.FindIndex(a => a.Id == alarm.Id);
            if (existing < 0) return;

            if (string.IsNullOrWhiteSpace(alarm.Group))
                alarm.Group = "Geral";

            _alarms[existing] = alarm;
            SaveUnsafe();
        }
    }

    public void Remove(Guid id)
    {
        lock (_sync)
        {
            _alarms.RemoveAll(a => a.Id == id);
            _lastTriggered.TryRemove(id, out _);
            SaveUnsafe();
        }
    }

    public void Toggle(Guid id, bool enabled)
    {
        lock (_sync)
        {
            var alarm = _alarms.FirstOrDefault(a => a.Id == id);
            if (alarm is null) return;

            alarm.Enabled = enabled;
            SaveUnsafe();
        }
    }

    private void Tick()
    {
        if (_disposed) return;

        var now = DateTime.Now;
        var dateKey = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        List<Alarm> matches;

        lock (_sync)
        {
            matches = _alarms
                .Where(a =>
                    a.Enabled &&
                    a.ParsedTime.Hour == now.Hour &&
                    a.ParsedTime.Minute == now.Minute &&
                    a.IsScheduledFor(now.DayOfWeek) &&
                    (!_lastTriggered.TryGetValue(a.Id, out var last) || last != dateKey))
                .Select(Clone)
                .ToList();

            foreach (var match in matches)
                _lastTriggered[match.Id] = dateKey;
        }

        foreach (var alarm in matches)
            AlarmTriggered?.Invoke(this, new AlarmTriggeredEventArgs { Alarm = alarm });
    }

    private void SaveUnsafe() => _repository.Save(_alarms);

    private static Alarm Clone(Alarm a) => new()
    {
        Id = a.Id,
        Label = a.Label,
        Group = a.Group,
        Time = a.Time,
        Days = a.Days,
        Enabled = a.Enabled,
        CustomSoundPath = a.CustomSoundPath,
        BuiltInSound = a.BuiltInSound,
        DurationSeconds = a.DurationSeconds,
        FadeInEnabled = a.FadeInEnabled,
        FadeInSeconds = a.FadeInSeconds,
        FadeOutEnabled = a.FadeOutEnabled,
        FadeOutSeconds = a.FadeOutSeconds
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Dispose();
    }
}
