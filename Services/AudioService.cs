using System.Media;
using NAudio.Wave;
using Win7AlarmClassic.Models;

namespace Win7AlarmClassic.Services;

public sealed class AudioService : IDisposable
{
    private readonly object _sync = new();
    private WaveOutEvent? _output;
    private AudioFileReader? _fileReader;
    private LoopStream? _loopStream;
    private CancellationTokenSource? _fallbackCts;
    private CancellationTokenSource? _fadeCts;
    private bool _disposed;

    public bool IsPlaying { get; private set; }

    public void PlayLoop(Alarm alarm)
    {
        lock (_sync)
        {
            StopInternal();
            if (_disposed) return;

            if (!string.IsNullOrWhiteSpace(alarm.CustomSoundPath) &&
                File.Exists(alarm.CustomSoundPath))
            {
                try
                {
                    _fileReader = new AudioFileReader(alarm.CustomSoundPath);
                    _loopStream = new LoopStream(_fileReader);
                    _output = new WaveOutEvent();
                    _output.Init(_loopStream);
                    _output.Volume = alarm.FadeInEnabled ? 0f : 1f;
                    _output.Play();
                    IsPlaying = true;

                    if (alarm.FadeInEnabled && alarm.FadeInSeconds > 0)
                        _ = FadeInAsync(_output, alarm.FadeInSeconds);

                    return;
                }
                catch
                {
                    StopInternal();
                }
            }

            _fallbackCts = new CancellationTokenSource();
            _ = PlaySystemSoundLoopAsync(alarm.BuiltInSound, _fallbackCts.Token);
            IsPlaying = true;
        }
    }

    public async Task StopWithFadeAsync(int seconds)
    {
        WaveOutEvent? output;

        lock (_sync)
        {
            if (!IsPlaying)
            {
                StopInternal();
                return;
            }

            output = _output;
        }

        if (output is null || seconds <= 0)
        {
            Stop();
            return;
        }

        try
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new CancellationTokenSource();
            var token = _fadeCts.Token;

            const int steps = 20;
            var delay = Math.Max(20, seconds * 1000 / steps);

            for (int i = steps - 1; i >= 0; i--)
            {
                token.ThrowIfCancellationRequested();

                lock (_sync)
                {
                    if (!IsPlaying || !ReferenceEquals(_output, output))
                        return;

                    output.Volume = i / (float)steps;
                }

                await Task.Delay(delay, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_output, output))
                    StopInternal();
            }
        }
    }

    public void Stop()
    {
        lock (_sync)
            StopInternal();
    }

    private async Task FadeInAsync(WaveOutEvent output, int seconds)
    {
        try
        {
            const int steps = 20;
            var delay = Math.Max(20, seconds * 1000 / steps);

            for (int i = 1; i <= steps; i++)
            {
                await Task.Delay(delay);

                lock (_sync)
                {
                    if (!IsPlaying || !ReferenceEquals(_output, output))
                        return;

                    output.Volume = i / (float)steps;
                }
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task PlaySystemSoundLoopAsync(BuiltInSound sound, CancellationToken token)
    {
        var systemSound = sound switch
        {
            BuiltInSound.Exclamation => SystemSounds.Exclamation,
            BuiltInSound.Beep => SystemSounds.Beep,
            BuiltInSound.Hand => SystemSounds.Hand,
            _ => SystemSounds.Asterisk
        };

        try
        {
            while (!token.IsCancellationRequested)
            {
                systemSound.Play();
                await Task.Delay(900, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StopInternal()
    {
        IsPlaying = false;

        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;

        _fallbackCts?.Cancel();
        _fallbackCts?.Dispose();
        _fallbackCts = null;

        try { _output?.Stop(); } catch { }

        _output?.Dispose();
        _output = null;

        _loopStream?.Dispose();
        _loopStream = null;

        _fileReader?.Dispose();
        _fileReader = null;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            StopInternal();
        }
    }

    private sealed class LoopStream : WaveStream
    {
        private readonly WaveStream _source;

        public LoopStream(WaveStream source) => _source = source;

        public override WaveFormat WaveFormat => _source.WaveFormat;
        public override long Length => _source.Length;

        public override long Position
        {
            get => _source.Position;
            set => _source.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int total = 0;

            while (total < count)
            {
                int read = _source.Read(buffer, offset + total, count - total);

                if (read == 0)
                {
                    _source.Position = 0;
                    read = _source.Read(buffer, offset + total, count - total);
                    if (read == 0) break;
                }

                total += read;
            }

            return total;
        }
    }
}
