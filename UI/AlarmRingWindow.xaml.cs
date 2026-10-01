using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Win7AlarmClassic.Models;
using Win7AlarmClassic.Services;

namespace Win7AlarmClassic.UI;

public partial class AlarmRingWindow : Window
{
    private readonly Alarm _alarm;
    private readonly AudioService _audio;

    private readonly DispatcherTimer _timer;
    private DispatcherTimer? _gifTimer;

    private DateTime _startedAt;

    private bool _closing;

    private int _gifFrameIndex;

    private BitmapFrame[] _gifFrames =
        Array.Empty<BitmapFrame>();

    private TextBlock _alarmNameText = null!;
    private TextBlock _clockText = null!;
    private TextBlock _remainingText = null!;
    private Border _progressBar = null!;
    private System.Windows.Controls.Image _wizardImage = null!;

    private static readonly Random _random =
        new();

    public AlarmRingWindow(
        Alarm alarm,
        AudioService audio)
    {
        InitializeComponent();

        _alarm = alarm;
        _audio = audio;

        _alarmNameText =
            (TextBlock)FindName(
                "AlarmNameText");

        _clockText =
            (TextBlock)FindName(
                "ClockText");

        _remainingText =
            (TextBlock)FindName(
                "RemainingText");

        _progressBar =
            (Border)FindName(
                "ProgressBar");

        _wizardImage =
            (System.Windows.Controls.Image)
            FindName(
                "WizardImage");

        _alarmNameText.Text =
            string.IsNullOrWhiteSpace(
                _alarm.Label)
                ? "ALARME"
                : _alarm.Label;

        _clockText.Text =
            DateTime.Now.ToString(
                "HH:mm:ss");

        if (_alarm.DurationSeconds > 0)
        {
            _remainingText.Text =
                $"◈  TEMPO RESTANTE  •  {_alarm.DurationSeconds}s";
        }
        else
        {
            _remainingText.Text =
                "◈  TOQUE CONTÍNUO  ◈";
        }

        // =====================================================
        // GIF
        // =====================================================

        LoadWizardGif();

        // =====================================================
        // TIMER DO ALARME
        // =====================================================

        _timer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMilliseconds(
                        250)
            };

        _timer.Tick +=
            Timer_Tick;

        // =====================================================
        // EVENTOS
        // =====================================================

        Loaded +=
            (_, _) =>
            {
                Activate();
                Focus();

                _startedAt =
                    DateTime.Now;

                _audio.PlayLoop(
                    _alarm);

                if (_gifFrames.Length > 1 &&
                    _gifTimer != null)
                {
                    _gifTimer.Start();
                }

                _timer.Start();
            };

        Closed +=
            (_, _) =>
            {
                _timer.Stop();

                _gifTimer?.Stop();

                _audio.Stop();
            };
    }

    // =========================================================
    // TIMER
    // =========================================================

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_closing)
            return;

        _clockText.Text =
            DateTime.Now.ToString(
                "HH:mm:ss");

        if (_alarm.DurationSeconds <= 0)
            return;

        var elapsed =
            DateTime.Now -
            _startedAt;

        var remaining =
            TimeSpan.FromSeconds(
                _alarm.DurationSeconds) -
            elapsed;

        if (remaining <= TimeSpan.Zero)
        {
            _remainingText.Text =
                "◈  TEMPO ENCERRADO  ◈";

            _progressBar.Width =
                0;

            _ = FinishByTimerAsync();

            return;
        }

        _remainingText.Text =
            $"◈  TEMPO RESTANTE  •  {remaining:hh\\:mm\\:ss}";

        double percentage =
            Math.Clamp(
                remaining.TotalSeconds /
                _alarm.DurationSeconds,
                0,
                1);

        _progressBar.Width =
            380 * percentage;
    }

    // =========================================================
    // GIF
    // =========================================================

    private void LoadWizardGif()
    {
        string assetsPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets");

        if (!Directory.Exists(assetsPath))
            return;

        string[] gifs =
            Directory.GetFiles(
                assetsPath,
                "*.gif",
                SearchOption.TopDirectoryOnly);

        if (gifs.Length == 0)
            return;

        string selected =
            gifs[_random.Next(gifs.Length)];

        try
        {
            var decoder =
                new GifBitmapDecoder(
                    new Uri(
                        selected,
                        UriKind.Absolute),
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

            _gifFrames =
                decoder.Frames.ToArray();

            if (_gifFrames.Length == 0)
                return;

            _gifFrameIndex =
                0;

            _wizardImage.Source =
                _gifFrames[0];

            // -------------------------------------------------
            // GIF ANIMADO
            // -------------------------------------------------

            if (_gifFrames.Length > 1)
            {
                _gifTimer =
                    new DispatcherTimer
                    {
                        Interval =
                            TimeSpan.FromMilliseconds(100)
                    };

                _gifTimer.Tick +=
                    GifTimer_Tick;
            }
        }
        catch
        {
            _wizardImage.Source =
                null;
        }
    }

    private void GifTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_closing)
            return;

        if (_wizardImage == null ||
            _gifFrames.Length == 0)
        {
            return;
        }

        _gifFrameIndex =
            (_gifFrameIndex + 1) %
            _gifFrames.Length;

        _wizardImage.Source =
            _gifFrames[_gifFrameIndex];
    }

    // =========================================================
    // DISPENSAR
    // =========================================================

    private void DismissButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Dismiss();
    }

    private void Window_KeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key ==
            Key.Escape)
        {
            Dismiss();

            e.Handled =
                true;
        }
    }

    private void Dismiss()
    {
        if (_closing)
            return;

        _closing =
            true;

        _timer.Stop();

        _gifTimer?.Stop();

        if (_alarm.FadeOutEnabled &&
            _alarm.FadeOutSeconds > 0)
        {
            _ = CloseWithFadeAsync();
        }
        else
        {
            _audio.Stop();

            Close();
        }
    }

    // =========================================================
    // FINALIZAÇÃO
    // =========================================================

    private async Task FinishByTimerAsync()
    {
        if (_closing)
            return;

        _closing =
            true;

        _timer.Stop();

        _gifTimer?.Stop();

        if (_alarm.FadeOutEnabled &&
            _alarm.FadeOutSeconds > 0)
        {
            await _audio.StopWithFadeAsync(
                _alarm.FadeOutSeconds);
        }
        else
        {
            _audio.Stop();
        }

        if (!Dispatcher.HasShutdownStarted)
            Close();
    }

    private async Task CloseWithFadeAsync()
    {
        await _audio.StopWithFadeAsync(
            _alarm.FadeOutSeconds);

        if (!Dispatcher.HasShutdownStarted)
            Close();
    }

    // =========================================================
    // FECHAMENTO
    // =========================================================

    protected override void OnClosing(
        System.ComponentModel.CancelEventArgs e)
    {
        if (!_closing)
        {
            _closing =
                true;

            _timer.Stop();

            _gifTimer?.Stop();

            _audio.Stop();
        }

        base.OnClosing(e);
    }
}
