using System.Text.Json;
using System.Text.Json.Serialization;
using Win7AlarmClassic.Models;

namespace Win7AlarmClassic.Services;

public sealed class JsonAlarmRepository : IDisposable
{
    private readonly string _folder;
    private readonly string _file;
    private readonly JsonSerializerOptions _options;
    private readonly object _sync = new();
    private bool _disposed;

    public JsonAlarmRepository()
    {
        _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Win7AlarmClassic");
        _file = Path.Combine(_folder, "settings.json");
        Directory.CreateDirectory(_folder);
        _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public List<Alarm> Load()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            if (!File.Exists(_file)) return new List<Alarm>();
            try
            {
                var json = File.ReadAllText(_file);
                return JsonSerializer.Deserialize<List<Alarm>>(json, _options) ?? new List<Alarm>();
            }
            catch
            {
                return new List<Alarm>();
            }
        }
    }

    public void Save(IEnumerable<Alarm> alarms)
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            Directory.CreateDirectory(_folder);
            var temp = _file + ".tmp";
            var json = JsonSerializer.Serialize(alarms.ToList(), _options);
            File.WriteAllText(temp, json);
            File.Move(temp, _file, true);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(JsonAlarmRepository));
    }

    public void Dispose() => _disposed = true;
}
