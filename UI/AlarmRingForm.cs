using Win7AlarmClassic.Models;
using Win7AlarmClassic.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Win7AlarmClassic.UI;

public sealed class AlarmRingForm : Form
{
    private readonly Alarm _alarm;
    private readonly AudioService _audio;

    private System.Windows.Forms.Timer _timer = null!;

    private bool _closing;
    private DateTime _startedAt;

    private RelicAlarmCanvas _canvas = null!;

    private PictureBox? _wizardGif;

    private static readonly Random _random = new();

    // =============================================================
    // PALETA
    // =============================================================

    private static readonly Color BlackStone =
        Color.FromArgb(7, 5, 4);

    private static readonly Color Stone =
        Color.FromArgb(27, 18, 14);

    private static readonly Color StoneLight =
        Color.FromArgb(52, 34, 25);

    private static readonly Color StoneDark =
        Color.FromArgb(15, 10, 8);

    private static readonly Color GoldDark =
        Color.FromArgb(91, 58, 20);

    private static readonly Color Gold =
        Color.FromArgb(191, 140, 48);

    private static readonly Color GoldBright =
        Color.FromArgb(244, 203, 99);

    private static readonly Color Ivory =
        Color.FromArgb(232, 218, 182);

    private static readonly Color IvoryDim =
        Color.FromArgb(164, 148, 111);

    private static readonly Color RedGem =
        Color.FromArgb(155, 34, 30);

    // =============================================================
    // CONSTRUTOR
    // =============================================================

    public AlarmRingForm(
        Alarm alarm,
        AudioService audio)
    {
        _alarm = alarm;
        _audio = audio;

        Text = $"BerClock - {_alarm.Label}";

        StartPosition =
            FormStartPosition.CenterScreen;

        FormBorderStyle =
            FormBorderStyle.None;

        MaximizeBox = false;
        MinimizeBox = false;

        TopMost = true;
        ShowInTaskbar = true;

        ClientSize =
            new Size(1000, 620);

        MinimumSize =
            new Size(760, 500);

        BackColor =
            BlackStone;

        DoubleBuffered = true;

        KeyPreview = true;

        BuildInterface();

        _timer =
            new System.Windows.Forms.Timer
            {
                Interval = 100
            };

        _timer.Tick +=
            (_, _) => UpdateDisplay();

        Shown +=
            (_, _) =>
            {
                BringToFront();
                Activate();
                Focus();

                _startedAt =
    DateTime.Now;

_canvas.SetStartedAt(
    _startedAt);

_audio.PlayLoop(_alarm);

_timer.Start();

_canvas.StartAnimation();
            };

        FormClosed +=
            (_, _) =>
            {
                _timer.Stop();

                _canvas.StopAnimation();

                _audio.Stop();

                if (_wizardGif?.Image != null)
                {
                    _wizardGif.Image.Dispose();
                    _wizardGif.Image = null;
                }
            };
    }

    // =============================================================
    // INTERFACE
    // =============================================================

    private void BuildInterface()
    {
        _canvas =
            new RelicAlarmCanvas(
                _alarm,
                Dismiss)
            {
                Dock = DockStyle.Fill
            };

        Controls.Add(_canvas);

        LoadWizardGif(
            _canvas);
    }

    // =============================================================
    // GIF
    // =============================================================

    private void LoadWizardGif(
        RelicAlarmCanvas parent)
    {
        string? gifPath =
            GetRandomWizardGif();

        if (gifPath == null)
        {
            return;
        }

        try
        {
            using var stream =
                new FileStream(
                    gifPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

            using var temp =
                Image.FromStream(stream);

            _wizardGif =
                new PictureBox
                {
                    SizeMode =
                        PictureBoxSizeMode.Zoom,

                    BackColor =
                        Color.FromArgb(
                            6,
                            4,
                            3),

                    TabStop = false
                };

            _wizardGif.Image =
                new Bitmap(temp);

            parent.SetWizardControl(
                _wizardGif);

            parent.Controls.Add(
                _wizardGif);

            _wizardGif.BringToFront();
        }
        catch
        {
            _wizardGif = null;
        }
    }

    private string? GetRandomWizardGif()
    {
        string assetsPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets");

        if (!Directory.Exists(
            assetsPath))
        {
            return null;
        }

        var gifs =
            Directory.GetFiles(
                assetsPath,
                "*.gif",
                SearchOption.TopDirectoryOnly);

        if (gifs.Length == 0)
        {
            return null;
        }

        return gifs[
            _random.Next(
                gifs.Length)];
    }

    // =============================================================
    // ATUALIZAÇÃO
    // =============================================================

    private void UpdateDisplay()
    {
        if (IsDisposed ||
            _canvas.IsDisposed)
        {
            return;
        }

        _canvas.UpdateClock();

        if (_alarm.DurationSeconds <= 0)
        {
            return;
        }

        var elapsed =
            DateTime.Now -
            _startedAt;

        var remaining =
            TimeSpan.FromSeconds(
                _alarm.DurationSeconds) -
            elapsed;

        if (remaining <=
            TimeSpan.Zero)
        {
            _canvas.SetTimerFinished();

            _ = FinishByTimerAsync();

            return;
        }

        _canvas.SetRemaining(
            remaining);
    }

    // =============================================================
    // FINALIZAÇÃO PELO TIMER
    // =============================================================

    private async Task FinishByTimerAsync()
    {
        if (_closing)
        {
            return;
        }

        _closing = true;

        _timer.Stop();

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

        if (!IsDisposed &&
            IsHandleCreated)
        {
            BeginInvoke(
                new Action(Close));
        }
    }

    // =============================================================
    // DISPENSAR
    // =============================================================

    private void Dismiss()
    {
        if (_closing)
        {
            return;
        }

        _closing = true;

        _timer.Stop();

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

    private async Task CloseWithFadeAsync()
    {
        await _audio.StopWithFadeAsync(
            _alarm.FadeOutSeconds);

        if (!IsDisposed &&
            IsHandleCreated)
        {
            BeginInvoke(
                new Action(Close));
        }
    }

    // =============================================================
    // TECLADO
    // =============================================================

    protected override bool ProcessCmdKey(
        ref Message msg,
        Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Dismiss();
            return true;
        }

        if (keyData == Keys.Enter)
        {
            if (_canvas.IsDismissHovered)
            {
                Dismiss();
                return true;
            }
        }

        return base.ProcessCmdKey(
            ref msg,
            keyData);
    }

    // =============================================================
    // FECHAMENTO
    // =============================================================

    protected override void OnFormClosing(
        FormClosingEventArgs e)
    {
        if (e.CloseReason ==
            CloseReason.UserClosing &&
            !_closing)
        {
            _closing = true;

            _timer.Stop();

            _audio.Stop();
        }

        base.OnFormClosing(e);
    }

    // =============================================================
    // CANVAS PRINCIPAL
    // =============================================================

    private sealed class RelicAlarmCanvas :
        Panel
    {
        private readonly Alarm _alarm;
        private readonly Action _dismiss;

        private PictureBox? _wizard;

        private Rectangle _dismissButtonRect;

        private bool _dismissHover;
        private bool _dismissPressed;

      private DateTime _displayTime =
    DateTime.Now;

private DateTime _startedAt;

private TimeSpan _remaining;

private bool _timerFinished;

        private float _pulse;

        private bool _pulseDirection = true;

        private System.Windows.Forms.Timer? _animationTimer;

        // =========================================================
        // FONTES
        // =========================================================

        private readonly Font _logoFont =
            new(
                "Georgia",
                21F,
                FontStyle.Bold);

        private readonly Font _subtitleFont =
            new(
                "Segoe UI",
                8F,
                FontStyle.Bold);

        private readonly Font _statusFont =
            new(
                "Segoe UI",
                9F,
                FontStyle.Bold);

        private readonly Font _sectionFont =
            new(
                "Georgia",
                13F,
                FontStyle.Bold);

        private readonly Font _alarmNameFont =
            new(
                "Segoe UI",
                14F,
                FontStyle.Bold);

        private readonly Font _clockFont =
            new(
                "Consolas",
                40F,
                FontStyle.Bold);

        private readonly Font _remainingFont =
            new(
                "Segoe UI",
                9F,
                FontStyle.Bold);

        private readonly Font _buttonFont =
            new(
                "Georgia",
                10F,
                FontStyle.Bold);

        private readonly Font _footerFont =
            new(
                "Segoe UI",
                7F,
                FontStyle.Bold);

        private readonly Font _entityFont =
            new(
                "Georgia",
                10F,
                FontStyle.Bold);

        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public RelicAlarmCanvas(
    Alarm alarm,
    Action dismiss)
{
    _alarm = alarm;
    _dismiss = dismiss;

    DoubleBuffered = true;

    TabStop = true;

    BackColor =
        BlackStone;

    SetStyle(
        ControlStyles.UserPaint |
        ControlStyles.AllPaintingInWmPaint |
        ControlStyles.OptimizedDoubleBuffer |
        ControlStyles.ResizeRedraw,
        true);

    MouseMove +=
        CanvasMouseMove;

    MouseLeave +=
        CanvasMouseLeave;

    MouseDown +=
        CanvasMouseDown;

    MouseUp +=
        CanvasMouseUp;

    Resize +=
        (_, _) =>
        {
            UpdateWizardBounds();
            Invalidate();
        };
}
public void SetStartedAt(
    DateTime startedAt)
{
    _startedAt = startedAt;
}

        // =========================================================
        // GIF
        // =========================================================

        public void SetWizardControl(
            PictureBox wizard)
        {
            _wizard = wizard;

            UpdateWizardBounds();
        }

        private void UpdateWizardBounds()
        {
            if (_wizard == null)
            {
                return;
            }

            Rectangle entity =
                GetEntityImageRect();

            _wizard.Bounds =
                entity;

            _wizard.BringToFront();
        }

        // =========================================================
        // ANIMAÇÃO
        // =========================================================

        public void StartAnimation()
        {
            if (_animationTimer != null)
            {
                return;
            }

            _animationTimer =
                new System.Windows.Forms.Timer
                {
                    Interval = 45
                };

            _animationTimer.Tick +=
                (_, _) =>
                {
                    if (_pulseDirection)
                    {
                        _pulse += 0.035f;

                        if (_pulse >= 1f)
                        {
                            _pulse = 1f;
                            _pulseDirection = false;
                        }
                    }
                    else
                    {
                        _pulse -= 0.035f;

                        if (_pulse <= 0f)
                        {
                            _pulse = 0f;
                            _pulseDirection = true;
                        }
                    }

                    Invalidate();
                };

            _animationTimer.Start();
        }

        public void StopAnimation()
        {
            _animationTimer?.Stop();
            _animationTimer?.Dispose();
            _animationTimer = null;
        }

        // =========================================================
        // ESTADO
        // =========================================================

        public bool IsDismissHovered =>
            _dismissHover;

        public void UpdateClock()
        {
            _displayTime =
                DateTime.Now;

            Invalidate();
        }

        public void SetRemaining(
            TimeSpan remaining)
        {
            _remaining =
                remaining;

            _timerFinished = false;

            Invalidate();
        }

        public void SetTimerFinished()
        {
            _timerFinished = true;

            Invalidate();
        }

        // =========================================================
        // PAINT
        // =========================================================

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g =
                e.Graphics;

            g.SmoothingMode =
                SmoothingMode.AntiAlias;

            g.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            g.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            g.TextRenderingHint =
                TextRenderingHint.AntiAliasGridFit;

            DrawBackground(
                g);

            DrawOuterFrame(
                g);

            DrawHeader(
                g);

            DrawEntityFrame(
                g);

            DrawAlarmFrame(
                g);

            DrawBottomDetails(
                g);
        }

        // =========================================================
        // FUNDO
        // =========================================================

        private void DrawBackground(
            Graphics g)
        {
            Rectangle r =
                ClientRectangle;

            using var background =
                new LinearGradientBrush(
                    r,
                    Color.FromArgb(
                        5,
                        4,
                        3),
                    Color.FromArgb(
                        42,
                        27,
                        19),
                    LinearGradientMode.Vertical);

            g.FillRectangle(
                background,
                r);

            // Mancha central suave
            using var glow =
                new PathGradientBrush(
                    new[]
                    {
                        new Point(
                            r.Width / 2,
                            r.Height / 2 - 50),

                        new Point(
                            r.Width / 2 + 280,
                            r.Height / 2),

                        new Point(
                            r.Width / 2,
                            r.Height / 2 + 180),

                        new Point(
                            r.Width / 2 - 280,
                            r.Height / 2)
                    });

            glow.CenterColor =
                Color.FromArgb(
                    24,
                    151,
                    107,
                    48);

            glow.SurroundColors =
                new[]
                {
                    Color.FromArgb(
                        0,
                        0,
                        0,
                        0)
                };

            g.FillEllipse(
                glow,
                r.Width / 2 - 340,
                r.Height / 2 - 240,
                680,
                480);

            // Vinheta superior
            using var topDark =
                new LinearGradientBrush(
                    new Rectangle(
                        0,
                        0,
                        r.Width,
                        150),
                    Color.FromArgb(
                        150,
                        0,
                        0,
                        0),
                    Color.FromArgb(
                        0,
                        0,
                        0,
                        0),
                    LinearGradientMode.Vertical);

            g.FillRectangle(
                topDark,
                0,
                0,
                r.Width,
                150);

            // Pequenos riscos decorativos
            using var scratches =
                new Pen(
                    Color.FromArgb(
                        15,
                        221,
                        184,
                        111),
                    1);

            for (int i = 0; i < 25; i++)
            {
                int x =
                    (i * 137) %
                    Math.Max(1, r.Width);

                int y =
                    115 +
                    ((i * 71) %
                     Math.Max(
                         1,
                         r.Height - 130));

                g.DrawLine(
                    scratches,
                    x,
                    y,
                    x + 5,
                    y + 1);
            }
        }

        // =========================================================
        // MOLDURA EXTERNA
        // =========================================================

        private void DrawOuterFrame(
            Graphics g)
        {
            Rectangle r =
                ClientRectangle;

            if (r.Width < 80 ||
                r.Height < 80)
            {
                return;
            }

            using var shadow =
                new Pen(
                    Color.FromArgb(
                        180,
                        0,
                        0,
                        0),
                    8);

            g.DrawRectangle(
                shadow,
                15,
                17,
                r.Width - 30,
                r.Height - 34);

            using var outer =
                new Pen(
                    GoldDark,
                    7);

            g.DrawRectangle(
                outer,
                13,
                13,
                r.Width - 26,
                r.Height - 26);

            using var gold =
                new Pen(
                    Gold,
                    2);

            g.DrawRectangle(
                gold,
                24,
                24,
                r.Width - 48,
                r.Height - 48);

            using var inner =
                new Pen(
                    Color.FromArgb(
                        90,
                        62,
                        30),
                    1);

            g.DrawRectangle(
                inner,
                30,
                30,
                r.Width - 60,
                r.Height - 60);

            // Linha abaixo do cabeçalho
            g.DrawLine(
                inner,
                52,
                104,
                r.Width - 52,
                104);

            DrawCorner(
                g,
                28,
                28,
                1,
                1);

            DrawCorner(
                g,
                r.Width - 28,
                28,
                -1,
                1);

            DrawCorner(
                g,
                28,
                r.Height - 28,
                1,
                -1);

            DrawCorner(
                g,
                r.Width - 28,
                r.Height - 28,
                -1,
                -1);

            DrawSeal(
                g,
                r.Width / 2,
                25);
        }

        // =========================================================
        // CABEÇALHO
        // =========================================================

        private void DrawHeader(
            Graphics g)
        {
            Rectangle r =
                ClientRectangle;

            using var goldBrush =
                new SolidBrush(
                    GoldBright);

            g.DrawString(
                "◈  B E R C L O C K",
                _logoFont,
                goldBrush,
                55,
                36);

            using var subtitleBrush =
                new SolidBrush(
                    Color.FromArgb(
                        135,
                        96,
                        43));

            g.DrawString(
                "CHRONOMANCER RELIC SYSTEM",
                _subtitleFont,
                subtitleBrush,
                59,
                79);

            // Status
            string statusText =
                "●  ALARM ACTIVE";

            int alpha =
                170 +
                (int)(
                    70 *
                    _pulse);

            using var statusBrush =
                new SolidBrush(
                    Color.FromArgb(
                        Math.Clamp(
                            alpha,
                            0,
                            255),
                        RedGem));

            using var statusFormat =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Far,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                statusText,
                _statusFont,
                statusBrush,
                new RectangleF(
                    r.Width - 300,
                    43,
                    245,
                    32),
                statusFormat);
        }

        // =========================================================
        // PAINEL DA ENTIDADE
        // =========================================================

        private void DrawEntityFrame(
            Graphics g)
        {
            Rectangle frame =
                GetEntityFrameRect();

            DrawRelicFrame(
                g,
                frame);

            using var titleBrush =
                new SolidBrush(
                    Gold);

            using var titleFormat =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                "◈  SUMMONED ENTITY  ◈",
                _entityFont,
                titleBrush,
                new RectangleF(
                    frame.X + 15,
                    frame.Y + 14,
                    frame.Width - 30,
                    30),
                titleFormat);

            Rectangle image =
                GetEntityImageRect();

            if (_wizard == null)
            {
                using var fallbackBrush =
                    new SolidBrush(
                        Color.FromArgb(
                            100,
                            72,
                            40));

                using var fallbackFormat =
                    new StringFormat
                    {
                        Alignment =
                            StringAlignment.Center,

                        LineAlignment =
                            StringAlignment.Center
                    };

                g.DrawString(
                    "𓂀",
                    new Font(
                        "Georgia",
                        42F,
                        FontStyle.Bold),
                    fallbackBrush,
                    image,
                    fallbackFormat);

                g.DrawString(
                    "BERCLOCK\nENTITY",
                    new Font(
                        "Georgia",
                        14F,
                        FontStyle.Bold),
                    fallbackBrush,
                    new RectangleF(
                        image.X,
                        image.Y + 100,
                        image.Width,
                        80),
                    fallbackFormat);
            }

            using var footerBrush =
                new SolidBrush(
                    Color.FromArgb(
                        130,
                        92,
                        42));

            using var footerFormat =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                "◈  BERCLOCK ENTITY  ◈",
                _footerFont,
                footerBrush,
                new RectangleF(
                    frame.X + 20,
                    frame.Bottom - 43,
                    frame.Width - 40,
                    24),
                footerFormat);
        }

        // =========================================================
        // PAINEL DO ALARME
        // =========================================================

        private void DrawAlarmFrame(
            Graphics g)
        {
            Rectangle frame =
                GetAlarmFrameRect();

            DrawRelicFrame(
                g,
                frame);

            using var titleBrush =
                new SolidBrush(
                    GoldBright);

            using var titleFormat =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                "◈  ALARM SEAL  ◈",
                _sectionFont,
                titleBrush,
                new RectangleF(
                    frame.X + 20,
                    frame.Y + 14,
                    frame.Width - 40,
                    38),
                titleFormat);

            // Nome do alarme
            using var nameBrush =
                new SolidBrush(
                    Ivory);

            g.DrawString(
                _alarm.Label,
                _alarmNameFont,
                nameBrush,
                new RectangleF(
                    frame.X + 35,
                    frame.Y + 60,
                    frame.Width - 70,
                    38),
                titleFormat);

            // Display
            Rectangle display =
                new Rectangle(
                    frame.X + 48,
                    frame.Y + 110,
                    frame.Width - 96,
                    118);

            DrawClockDisplay(
                g,
                display);

            // Tempo restante
            DrawRemaining(
                g,
                frame);

            // Indicador
            DrawProgressBar(
                g,
                frame);

            // Botão
            DrawDismissButton(
                g,
                frame);

            // Rodapé
            using var footerBrush =
                new SolidBrush(
                    Color.FromArgb(
                        100,
                        71,
                        36));

            g.DrawString(
                "CHRONOMANCER DEVICE  •  SYSTEM ONLINE",
                _footerFont,
                footerBrush,
                new RectangleF(
                    frame.X + 30,
                    frame.Bottom - 43,
                    frame.Width - 60,
                    25),
                titleFormat);
        }

        // =========================================================
        // DISPLAY
        // =========================================================

        private void DrawClockDisplay(
            Graphics g,
            Rectangle rect)
        {
            using var bg =
                new LinearGradientBrush(
                    rect,
                    Color.FromArgb(
                        5,
                        3,
                        2),
                    Color.FromArgb(
                        25,
                        15,
                        10),
                    LinearGradientMode.Vertical);

            g.FillRectangle(
                bg,
                rect);

            using var outer =
                new Pen(
                    GoldDark,
                    4);

            g.DrawRectangle(
                outer,
                rect);

            Rectangle innerRect =
                Rectangle.Inflate(
                    rect,
                    -8,
                    -8);

            using var inner =
                new Pen(
                    Gold,
                    1);

            g.DrawRectangle(
                inner,
                innerRect);

            // Brilho pulsante
            int glowAlpha =
                15 +
                (int)(
                    28 *
                    _pulse);

            using var glow =
                new SolidBrush(
                    Color.FromArgb(
                        glowAlpha,
                        244,
                        203,
                        99));

            g.FillEllipse(
                glow,
                rect.X + rect.Width / 2 - 145,
                rect.Y + 20,
                290,
                75);

            string time =
                _displayTime.ToString(
                    "HH:mm:ss");

            using var format =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            // Sombra do relógio
            using var shadow =
                new SolidBrush(
                    Color.FromArgb(
                        180,
                        0,
                        0,
                        0));

            g.DrawString(
                time,
                _clockFont,
                shadow,
                new RectangleF(
                    rect.X + 3,
                    rect.Y + 4,
                    rect.Width,
                    rect.Height),
                format);

            // Texto principal
            using var clockBrush =
                new SolidBrush(
                    GoldBright);

            g.DrawString(
                time,
                _clockFont,
                clockBrush,
                rect,
                format);
        }

        // =========================================================
        // TEMPO
        // =========================================================

        private void DrawRemaining(
            Graphics g,
            Rectangle frame)
        {
            string text;

            if (_timerFinished)
            {
                text =
                    "◈  TEMPO ENCERRADO  ◈";
            }
            else if (_alarm.DurationSeconds <= 0)
            {
                text =
                    "◈  TOQUE CONTÍNUO  ◈";
            }
            else
            {
                text =
                    $"◈  TEMPO RESTANTE  •  {_remaining:hh\\:mm\\:ss}";
            }

            using var brush =
                new SolidBrush(
                    _timerFinished
                        ? RedGem
                        : Color.FromArgb(
                            171,
                            132,
                            72));

            using var format =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                text,
                _remainingFont,
                brush,
                new RectangleF(
                    frame.X + 35,
                    frame.Y + 238,
                    frame.Width - 70,
                    32),
                format);
        }

        // =========================================================
        // BARRA DE PROGRESSO
        // =========================================================

        private void DrawProgressBar(
            Graphics g,
            Rectangle frame)
        {
            Rectangle bar =
                new Rectangle(
                    frame.X + 48,
                    frame.Y + 280,
                    frame.Width - 96,
                    6);

            using var bg =
                new SolidBrush(
                    Color.FromArgb(
                        25,
                        15,
                        9));

            g.FillRectangle(
                bg,
                bar);

           float progress = 1f;

if (_alarm.DurationSeconds > 0)
{
    double elapsed =
        (DateTime.Now -
         _startedAt)
        .TotalSeconds;

    progress =
        (float)(
            1.0 -
            elapsed /
            _alarm.DurationSeconds);

    progress =
        Math.Clamp(
            progress,
            0f,
            1f);
}

            int width =
                (int)(
                    bar.Width *
                    progress);

            if (width > 0)
            {
                using var gold =
                    new LinearGradientBrush(
                        new Rectangle(
                            bar.X,
                            bar.Y,
                            Math.Max(
                                width,
                                1),
                            bar.Height),
                        GoldDark,
                        GoldBright,
                        LinearGradientMode.Horizontal);

                g.FillRectangle(
                    gold,
                    bar.X,
                    bar.Y,
                    width,
                    bar.Height);
            }

            using var outline =
                new Pen(
                    Color.FromArgb(
                        100,
                        73,
                        34),
                    1);

            g.DrawRectangle(
                outline,
                bar);
        }

        // =========================================================
        // BOTÃO DISPENSAR
        // =========================================================

        private void DrawDismissButton(
            Graphics g,
            Rectangle frame)
        {
            _dismissButtonRect =
                new Rectangle(
                    frame.X +
                    (frame.Width - 245) / 2,
                    frame.Y + 305,
                    245,
                    62);

            Rectangle r =
                _dismissButtonRect;

            r.Inflate(
                -4,
                -4);

            // Sombra
            using var shadow =
                new SolidBrush(
                    Color.FromArgb(
                        180,
                        0,
                        0,
                        0));

            g.FillRectangle(
                shadow,
                r.X + 6,
                r.Y + 7,
                r.Width - 2,
                r.Height - 2);

            Color top =
                _dismissPressed
                    ? Color.FromArgb(
                        42,
                        24,
                        14)
                    : _dismissHover
                        ? Color.FromArgb(
                            108,
                            70,
                            31)
                        : Color.FromArgb(
                            63,
                            39,
                            23);

            Color bottom =
                _dismissPressed
                    ? Color.FromArgb(
                        15,
                        8,
                        5)
                    : Color.FromArgb(
                        24,
                        14,
                        9);

            using var body =
                new LinearGradientBrush(
                    r,
                    top,
                    bottom,
                    LinearGradientMode.Vertical);

            g.FillRectangle(
                body,
                r);

            using var outer =
                new Pen(
                    _dismissHover
                        ? GoldBright
                        : GoldDark,
                    _dismissHover
                        ? 4
                        : 3);

            g.DrawRectangle(
                outer,
                r);

            Rectangle innerRect =
                Rectangle.Inflate(
                    r,
                    -7,
                    -7);

            using var inner =
                new Pen(
                    Gold,
                    1);

            g.DrawRectangle(
                inner,
                innerRect);

            using var format =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            using var textBrush =
                new SolidBrush(
                    _dismissHover
                        ? Color.White
                        : Ivory);

            g.DrawString(
                "◈  DISPENSAR  ◈",
                _buttonFont,
                textBrush,
                r,
                format);
        }

        // =========================================================
        // DETALHES INFERIORES
        // =========================================================

        private void DrawBottomDetails(
            Graphics g)
        {
            Rectangle r =
                ClientRectangle;

            using var brush =
                new SolidBrush(
                    Color.FromArgb(
                        80,
                        58,
                        34));

            using var format =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            g.DrawString(
                "ESC  •  DISPENSAR",
                new Font(
                    "Segoe UI",
                    7F,
                    FontStyle.Bold),
                brush,
                new RectangleF(
                    50,
                    r.Height - 55,
                    200,
                    22),
                format);

            g.DrawString(
                "BERCLOCK RELIC UI",
                new Font(
                    "Segoe UI",
                    7F,
                    FontStyle.Bold),
                brush,
                new RectangleF(
                    r.Width - 250,
                    r.Height - 55,
                    200,
                    22),
                format);
        }

        // =========================================================
        // MOLDURA
        // =========================================================

        private static void DrawRelicFrame(
            Graphics g,
            Rectangle r)
        {
            // Sombra
            using var shadow =
                new SolidBrush(
                    Color.FromArgb(
                        160,
                        0,
                        0,
                        0));

            g.FillRectangle(
                shadow,
                r.X + 7,
                r.Y + 9,
                r.Width - 5,
                r.Height - 5);

            // Pedra
            using var stone =
                new LinearGradientBrush(
                    r,
                    StoneLight,
                    Stone,
                    LinearGradientMode.Vertical);

            g.FillRectangle(
                stone,
                r);

            // Borda externa
            using var outer =
                new Pen(
                    GoldDark,
                    6);

            g.DrawRectangle(
                outer,
                r.X + 2,
                r.Y + 2,
                r.Width - 5,
                r.Height - 5);

            // Ouro
            using var gold =
                new Pen(
                    Gold,
                    2);

            g.DrawRectangle(
                gold,
                r.X + 9,
                r.Y + 9,
                r.Width - 19,
                r.Height - 19);

            // Rebaixo
            using var dark =
                new Pen(
                    Color.FromArgb(
                        18,
                        10,
                        7),
                    2);

            g.DrawRectangle(
                dark,
                r.X + 14,
                r.Y + 14,
                r.Width - 29,
                r.Height - 29);

            DrawCorner(
                g,
                r.X + 10,
                r.Y + 10,
                1,
                1);

            DrawCorner(
                g,
                r.Right - 10,
                r.Y + 10,
                -1,
                1);

            DrawCorner(
                g,
                r.X + 10,
                r.Bottom - 10,
                1,
                -1);

            DrawCorner(
                g,
                r.Right - 10,
                r.Bottom - 10,
                -1,
                -1);
        }

        // =========================================================
        // CANTOS
        // =========================================================

        private static void DrawCorner(
            Graphics g,
            int x,
            int y,
            int dx,
            int dy)
        {
            using var pen =
                new Pen(
                    GoldBright,
                    2);

            g.DrawLine(
                pen,
                x,
                y,
                x + dx * 20,
                y);

            g.DrawLine(
                pen,
                x,
                y,
                x,
                y + dy * 20);

            g.DrawLine(
                pen,
                x,
                y,
                x + dx * 9,
                y + dy * 9);
        }

        // =========================================================
        // SELO
        // =========================================================

        private static void DrawSeal(
            Graphics g,
            int x,
            int y)
        {
            using var pen =
                new Pen(
                    Gold,
                    2);

            Point[] diamond =
            {
                new Point(
                    x,
                    y - 12),

                new Point(
                    x + 18,
                    y),

                new Point(
                    x,
                    y + 12),

                new Point(
                    x - 18,
                    y)
            };

            g.DrawPolygon(
                pen,
                diamond);

            using var brush =
                new SolidBrush(
                    GoldBright);

            g.FillEllipse(
                brush,
                x - 3,
                y - 3,
                6,
                6);
        }

        // =========================================================
        // LAYOUT
        // =========================================================

        private Rectangle GetEntityFrameRect()
        {
            int marginLeft = 55;
            int top = 125;
            int gap = 25;

            int availableWidth =
                Width -
                marginLeft -
                55;

            int entityWidth =
                (int)(
                    availableWidth *
                    0.37);

            int alarmX =
                marginLeft +
                entityWidth +
                gap;

            return new Rectangle(
                marginLeft,
                top,
                entityWidth,
                Height -
                top -
                65);
        }

        private Rectangle GetAlarmFrameRect()
        {
            Rectangle entity =
                GetEntityFrameRect();

            int gap = 25;

            int x =
                entity.Right +
                gap;

            return new Rectangle(
                x,
                125,
                Width -
                x -
                55,
                Height -
                125 -
                65);
        }

        private Rectangle GetEntityImageRect()
        {
            Rectangle frame =
                GetEntityFrameRect();

            return new Rectangle(
                frame.X + 27,
                frame.Y + 58,
                frame.Width - 54,
                frame.Height - 125);
        }

        // =========================================================
        // MOUSE
        // =========================================================

        private void CanvasMouseMove(
            object? sender,
            MouseEventArgs e)
        {
            bool hover =
                _dismissButtonRect.Contains(
                    e.Location);

            if (hover != _dismissHover)
            {
                _dismissHover =
                    hover;

                Cursor =
                    hover
                        ? Cursors.Hand
                        : Cursors.Default;

                Invalidate();
            }
        }

        private void CanvasMouseLeave(
            object? sender,
            EventArgs e)
        {
            _dismissHover = false;
            _dismissPressed = false;

            Cursor =
                Cursors.Default;

            Invalidate();
        }

        private void CanvasMouseDown(
            object? sender,
            MouseEventArgs e)
        {
            if (e.Button !=
                MouseButtons.Left)
            {
                return;
            }

            if (_dismissButtonRect.Contains(
                e.Location))
            {
                _dismissPressed = true;

                Invalidate();
            }
        }

        private void CanvasMouseUp(
            object? sender,
            MouseEventArgs e)
        {
            if (e.Button !=
                MouseButtons.Left)
            {
                return;
            }

            bool clicked =
                _dismissPressed &&
                _dismissButtonRect.Contains(
                    e.Location);

            _dismissPressed = false;

            Invalidate();

            if (clicked)
            {
                _dismiss();
            }
        }
    }
}