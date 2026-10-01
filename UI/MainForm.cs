using System.Diagnostics;
using System.Media;

using Win7AlarmClassic.Models;
using Win7AlarmClassic.Services;

namespace Win7AlarmClassic.UI;

public sealed class MainForm : Form
{
    // =========================================================
    // SERVIÇOS
    // =========================================================

    private readonly AlarmService _alarmService;
    private readonly AudioService _audioService;

    // =========================================================
    // BANDEJA
    // =========================================================

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;

    private bool _reallyExit;

    // =========================================================
    // TIMERS
    // =========================================================

    private readonly System.Windows.Forms.Timer _clockTimer;
    private readonly System.Windows.Forms.Timer _toolTimer;
    private readonly System.Windows.Forms.Timer _worldClockTimer;

    // =========================================================
    // TEMPORIZADOR
    // =========================================================

    private bool _timerRunning;
    private TimeSpan _timerRemaining = TimeSpan.Zero;
    private DateTime _timerLastTick;

    // =========================================================
    // CRONÔMETRO
    // =========================================================

    private readonly Stopwatch _stopwatch = new();
    private bool _stopwatchRunning;
    private TimeSpan _stopwatchAccumulated = TimeSpan.Zero;

    // =========================================================
    // INTERFACE
    // =========================================================

    private readonly Panel _titleBar = new();
    private readonly Panel _sidebar = new();
    private readonly Panel _content = new();

    private readonly Panel _pageHeader = new();
    private readonly Panel _pageHost = new();

    private readonly Label _pageTitle = new();

    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly Dictionary<string, Label> _worldClockLabels = new();

    private Panel? _currentPage;

    private const int SidebarWidth = 220;
    private const int TitleBarHeight = 54;
    private const int PageHeaderHeight = 74;

    // =========================================================
    // CORES
    // =========================================================

    private static readonly Color Background =
        Color.FromArgb(12, 10, 16);

    private static readonly Color SidebarColor =
        Color.FromArgb(20, 16, 27);

    private static readonly Color SidebarDark =
        Color.FromArgb(15, 12, 21);

    private static readonly Color PanelColor =
        Color.FromArgb(30, 24, 39);

    private static readonly Color PanelHover =
        Color.FromArgb(48, 37, 61);

    private static readonly Color PanelSelected =
        Color.FromArgb(62, 43, 78);

    private static readonly Color Gold =
        Color.FromArgb(232, 190, 72);

    private static readonly Color GoldBright =
        Color.FromArgb(255, 218, 105);

    private static readonly Color GoldDark =
        Color.FromArgb(126, 91, 28);

    private static readonly Color TextPrimary =
        Color.FromArgb(245, 240, 225);

    private static readonly Color TextSecondary =
        Color.FromArgb(170, 158, 180);

    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public MainForm(
        AlarmService alarmService,
        AudioService audioService)
    {
        _alarmService = alarmService;
        _audioService = audioService;

        Text = "BerClock";
        StartPosition = FormStartPosition.CenterScreen;

        MinimumSize = new Size(1050, 650);
        ClientSize = new Size(1250, 750);

        BackColor = Background;

        Font = new Font(
            "Segoe UI",
            9F);

        // Double buffering SOMENTE no próprio formulário.
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        UpdateStyles();

        // =====================================================
        // BANDEJA DO WINDOWS
        // =====================================================

        _trayMenu =
            new ContextMenuStrip();

        var openItem =
            new ToolStripMenuItem(
                "Abrir BerClock");

        openItem.Click +=
            (_, _) =>
                ShowFromTray();

        var exitItem =
            new ToolStripMenuItem(
                "Sair");

        exitItem.Click +=
            (_, _) =>
                ExitApplication();

        _trayMenu.Items.Add(
            openItem);

        _trayMenu.Items.Add(
            new ToolStripSeparator());

        _trayMenu.Items.Add(
            exitItem);

        _trayIcon =
            new NotifyIcon
            {
                Text = "BerClock",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };

        _trayIcon.DoubleClick +=
            (_, _) =>
                ShowFromTray();

        // -----------------------------------------------------
        // RELÓGIO
        // -----------------------------------------------------

        _clockTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 250
            };

        _clockTimer.Tick +=
            (_, _) =>
                UpdateClock();

        // -----------------------------------------------------
        // TEMPORIZADOR / CRONÔMETRO
        // -----------------------------------------------------

        _toolTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 50
            };

        _toolTimer.Tick +=
            ToolTimer_Tick;

        // -----------------------------------------------------
        // RELÓGIO MUNDIAL
        // -----------------------------------------------------

        _worldClockTimer =
            new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

        _worldClockTimer.Tick +=
            (_, _) =>
                UpdateWorldClocks();

        // -----------------------------------------------------
        // ALARMES
        // -----------------------------------------------------

        _alarmService.AlarmTriggered +=
            AlarmService_AlarmTriggered;

        // -----------------------------------------------------
        // INTERFACE
        // -----------------------------------------------------

        BuildInterface();

        FormClosing +=
            MainForm_FormClosing;

        Load +=
            (_, _) =>
            {
                LoadWindowIcon();

                LoadTrayIcon();

                _clockTimer.Start();

                ShowPage("Início");
            };
    }

    // =========================================================
    // ÍCONE DA JANELA
    // =========================================================

    private void LoadWindowIcon()
    {
        try
        {
            string path =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "BerClock.ico");

            if (!File.Exists(path))
                return;

            using var stream =
                new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read);

            using var temp =
                new Icon(stream);

            Icon =
                (Icon)temp.Clone();
        }
        catch
        {
        }
    }

    // =========================================================
    // ÍCONE DA BANDEJA
    // =========================================================

    private void LoadTrayIcon()
    {
        try
        {
            string path =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "BerClock.ico");

            if (!File.Exists(path))
                return;

            using var stream =
                new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read);

            using var temp =
                new Icon(stream);

            _trayIcon.Icon =
                (Icon)temp.Clone();
        }
        catch
        {
        }
    }

    // =========================================================
    // BANDEJA
    // =========================================================

    private void ShowFromTray()
    {
        if (IsDisposed ||
            Disposing)
        {
            return;
        }

        Show();

        if (WindowState ==
            FormWindowState.Minimized)
        {
            WindowState =
                FormWindowState.Normal;
        }

        ShowInTaskbar = true;

        Activate();
        BringToFront();
    }

    private void ExitApplication()
    {
        if (_reallyExit)
            return;

        _reallyExit = true;

        _trayIcon.Visible = false;

        Close();
    }

    // =========================================================
    // INTERFACE PRINCIPAL
    // =========================================================

    private void BuildInterface()
    {
        SuspendLayout();

        // -----------------------------------------------------
        // TITLE BAR
        // -----------------------------------------------------

        _titleBar.Dock = DockStyle.Top;
        _titleBar.Height = TitleBarHeight;
        _titleBar.BackColor =
            Color.FromArgb(23, 20, 26);

        // -----------------------------------------------------
        // SIDEBAR
        // -----------------------------------------------------

        _sidebar.Dock = DockStyle.Left;
        _sidebar.Width = SidebarWidth;
        _sidebar.BackColor = SidebarDark;

        // -----------------------------------------------------
        // CONTENT
        // -----------------------------------------------------

        _content.Dock = DockStyle.Fill;
        _content.BackColor = Background;
        _content.Padding = Padding.Empty;

        // -----------------------------------------------------
        // HEADER DA PÁGINA
        // -----------------------------------------------------

        _pageHeader.Dock = DockStyle.Top;
        _pageHeader.Height = PageHeaderHeight;
        _pageHeader.BackColor = Background;

        _pageHeader.Padding =
            new Padding(
                30,
                0,
                30,
                0);

        // -----------------------------------------------------
        // HOST DAS PÁGINAS
        // -----------------------------------------------------

        _pageHost.Dock = DockStyle.Fill;
        _pageHost.BackColor = Background;

        _pageHost.Padding =
            new Padding(
                30,
                0,
                30,
                30);

        // -----------------------------------------------------
        // HIERARQUIA
        // -----------------------------------------------------

        Controls.Add(_content);
        Controls.Add(_sidebar);
        Controls.Add(_titleBar);

        _content.Controls.Add(_pageHost);
        _content.Controls.Add(_pageHeader);

        BuildTitleBar();
        BuildPageHeader();
        BuildSidebar();

        ResumeLayout(true);
    }

    // =========================================================
    // TITLE BAR
    // =========================================================

    private void BuildTitleBar()
    {
        var logo =
            new Label
            {
                Text = "BER CLOCK",
                AutoSize = true,
                Font =
                    new Font(
                        "Segoe UI",
                        14F,
                        FontStyle.Bold),
                ForeColor = Gold,
                Location =
                    new Point(
                        20,
                        14)
            };

        _titleBar.Controls.Add(logo);
    }

    // =========================================================
    // PAGE HEADER
    // =========================================================

    private void BuildPageHeader()
    {
        _pageTitle.Dock = DockStyle.Fill;

        _pageTitle.Text = "INÍCIO";

        _pageTitle.TextAlign =
            ContentAlignment.MiddleLeft;

        _pageTitle.Font =
            new Font(
                "Segoe UI",
                21F,
                FontStyle.Bold);

        _pageTitle.ForeColor =
            TextPrimary;

        _pageHeader.Controls.Add(
            _pageTitle);
    }

    // =========================================================
    // SIDEBAR
    // =========================================================

    private void BuildSidebar()
    {
        _sidebar.Padding =
            new Padding(
                12,
                18,
                12,
                18);

        var logoPanel =
            new Panel
            {
                Dock = DockStyle.Top,
                Height = 145,
                BackColor = SidebarColor
            };

        logoPanel.Paint +=
            (_, e) =>
            {
                using var pen =
                    new Pen(
                        GoldDark,
                        1);

                e.Graphics.DrawLine(
                    pen,
                    15,
                    logoPanel.Height - 1,
                    logoPanel.Width - 15,
                    logoPanel.Height - 1);
            };

        var symbol =
            new Label
            {
                Text = "☥",
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                Font =
                    new Font(
                        "Segoe UI Symbol",
                        38F,
                        FontStyle.Bold),
                ForeColor = GoldBright
            };

        var name =
            new Label
            {
                Text = "BER CLOCK",
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                Font =
                    new Font(
                        "Segoe UI",
                        12F,
                        FontStyle.Bold),
                ForeColor = TextPrimary
            };

        logoPanel.Controls.Add(name);
        logoPanel.Controls.Add(symbol);

        // Dock.Top trabalha de baixo para cima.
        _sidebar.Controls.Add(
            CreateNavigationButton(
                "Relógio Mundial",
                "◉"));

        _sidebar.Controls.Add(
            CreateNavigationButton(
                "Cronômetro",
                "◷"));

        _sidebar.Controls.Add(
            CreateNavigationButton(
                "Temporizador",
                "⌛"));

        _sidebar.Controls.Add(
            CreateNavigationButton(
                "Alarmes",
                "♢"));

        _sidebar.Controls.Add(
            CreateNavigationButton(
                "Início",
                "☥"));

        _sidebar.Controls.Add(
            logoPanel);
    }

    private Button CreateNavigationButton(
        string text,
        string icon)
    {
        var button =
            new Button
            {
                Text =
                    $"   {icon}   {text}",

                Dock = DockStyle.Top,

                Height = 52,

                FlatStyle =
                    FlatStyle.Flat,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F,
                        FontStyle.Bold),

                ForeColor =
                    TextSecondary,

                BackColor =
                    Color.Transparent,

                Cursor =
                    Cursors.Hand,

                Padding =
                    new Padding(
                        8,
                        0,
                        0,
                        0),

                TabStop = false
            };

        button.FlatAppearance.BorderSize = 0;

        button.FlatAppearance.MouseOverBackColor =
            PanelHover;

        button.FlatAppearance.MouseDownBackColor =
            PanelSelected;

        button.Click +=
            (_, _) =>
                ShowPage(text);

        _navButtons[text] = button;

        return button;
    }

    // =========================================================
    // TROCA DE PÁGINA
    // =========================================================

    private void ShowPage(
        string pageName)
    {
        if (IsDisposed ||
            Disposing)
        {
            return;
        }

        _pageHost.SuspendLayout();

        try
        {
            _worldClockTimer.Stop();
            _worldClockLabels.Clear();

            if (_currentPage != null)
            {
                _pageHost.Controls.Remove(
                    _currentPage);

                _currentPage.Dispose();

                _currentPage = null;
            }

            foreach (var button in
                     _navButtons.Values)
            {
                button.ForeColor =
                    TextSecondary;

                button.BackColor =
                    Color.Transparent;
            }

            if (_navButtons.TryGetValue(
                    pageName,
                    out var selected))
            {
                selected.ForeColor =
                    GoldBright;

                selected.BackColor =
                    PanelSelected;
            }

            _pageTitle.Text =
                pageName.ToUpperInvariant();

            var page =
                pageName switch
                {
                    "Início" =>
                        BuildHomePage(),

                    "Alarmes" =>
                        BuildAlarmsPage(),

                    "Temporizador" =>
                        BuildTimerPage(),

                    "Cronômetro" =>
                        BuildStopwatchPage(),

                    "Relógio Mundial" =>
                        BuildWorldClockPage(),

                    _ =>
                        BuildHomePage()
                };

            page.Dock =
                DockStyle.Fill;

            page.Margin =
                Padding.Empty;

            page.Padding =
                Padding.Empty;

            _currentPage = page;

            _pageHost.Controls.Add(page);

            page.BringToFront();

            _pageHost.PerformLayout();
        }
        finally
        {
            _pageHost.ResumeLayout(true);
        }

        UpdateClock();
    }

    // =========================================================
    // PÁGINA BASE
    // =========================================================

    private Panel CreatePage()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
    }

    // =========================================================
    // HOME
    // =========================================================

    private Panel BuildHomePage()
    {
        var page =
            CreatePage();

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,
                RowCount = 2,

                BackColor = Background,

                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                285));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100F));

        var clockCard =
            CreateCard();

        clockCard.Dock =
            DockStyle.Fill;

        clockCard.Margin =
            new Padding(
                0,
                0,
                0,
                15);

        var currentTime =
            new Label
            {
                Text = "CURRENT TIME",
                Dock = DockStyle.Top,
                Height = 35,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),

                ForeColor = GoldDark
            };

        var clock =
            new Label
            {
                Dock = DockStyle.Fill,

                Text =
                    DateTime.Now.ToString(
                        "HH:mm:ss"),

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Consolas",
                        58F,
                        FontStyle.Bold),

                ForeColor = GoldBright,

                Tag = "MAIN_CLOCK"
            };

        var date =
            new Label
            {
                Dock = DockStyle.Bottom,
                Height = 38,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        11F),

                ForeColor = TextSecondary,

                Text =
                    DateTime.Now.ToString(
                        "dddd, dd 'de' MMMM 'de' yyyy"),

                Tag = "MAIN_DATE"
            };

        clockCard.Controls.Add(clock);
        clockCard.Controls.Add(date);
        clockCard.Controls.Add(currentTime);

        var info =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 3,
                RowCount = 1,

                BackColor = Background,

                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        for (int i = 0; i < 3; i++)
        {
            info.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.3333F));
        }

        info.Controls.Add(
            CreateInfoCard(
                "PRÓXIMO ALARME",
                GetNextAlarmText()),
            0,
            0);

        info.Controls.Add(
            CreateInfoCard(
                "TOTAL DE ALARMES",
                _alarmService.Alarms.Count.ToString()),
            1,
            0);

        info.Controls.Add(
            CreateInfoCard(
                "ALARMES ATIVOS",
                _alarmService.Alarms
                    .Count(a => a.Enabled)
                    .ToString()),
            2,
            0);

        layout.Controls.Add(
            clockCard,
            0,
            0);

        layout.Controls.Add(
            info,
            0,
            1);

        page.Controls.Add(layout);

        return page;
    }

    private Panel CreateInfoCard(
        string title,
        string value)
    {
        var card =
            CreateCard();

        card.Dock =
            DockStyle.Fill;

        card.Margin =
            new Padding(
                0,
                0,
                12,
                0);

        var titleLabel =
            new Label
            {
                Text = title,

                Dock = DockStyle.Top,
                Height = 32,

                Padding =
                    new Padding(
                        15,
                        12,
                        15,
                        0),

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),

                ForeColor =
                    TextSecondary
            };

        var valueLabel =
            new Label
            {
                Text = value,

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        17F,
                        FontStyle.Bold),

                ForeColor =
                    GoldBright,

                AutoEllipsis = true
            };

        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);

        return card;
    }

    // =========================================================
    // ALARMES
    // =========================================================

    private Panel BuildAlarmsPage()
    {
        var page =
            CreatePage();

        var toolbar =
            new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Background
            };

        var newButton =
            new Button
            {
                Text = "+ NOVO ALARME",

                Width = 170,
                Height = 38,

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right,

                Location =
                    new Point(
                        Math.Max(
                            0,
                            toolbar.ClientSize.Width - 170),
                        5),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor = Gold,
                ForeColor = Background,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                Cursor = Cursors.Hand,

                TabStop = false
            };

        newButton.FlatAppearance.BorderSize = 0;

        toolbar.Controls.Add(newButton);

        toolbar.Resize +=
            (_, _) =>
            {
                newButton.Left =
                    toolbar.ClientSize.Width -
                    newButton.Width -
                    5;
            };

        newButton.Click +=
            (_, _) =>
            {
                using var form =
                    new AlarmEditForm();

                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    _alarmService.Add(
                        form.Alarm);

                    ShowPage("Alarmes");
                }
            };

        var scroll =
            new Panel
            {
                Dock = DockStyle.Fill,

                AutoScroll = true,

                BackColor = Background,

                Padding =
                    new Padding(
                        0,
                        5,
                        5,
                        10)
            };

        var alarms =
            _alarmService.Alarms
                .OrderBy(
                    a => a.ParsedTime)
                .ToList();

        if (alarms.Count == 0)
        {
            var empty =
                new Label
                {
                    Text =
                        "Nenhum alarme criado ainda.\r\n\r\n" +
                        "Clique em + NOVO ALARME para começar.",

                    AutoSize = false,

                    Width = 650,
                    Height = 180,

                    Location =
                        new Point(
                            0,
                            0),

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Segoe UI",
                            14F),

                    ForeColor =
                        TextSecondary
                };

            scroll.Controls.Add(empty);
        }
        else
        {
            int y = 0;

            foreach (var alarm in alarms)
            {
                var card =
                    CreateAlarmCard(alarm);

                card.Location =
                    new Point(
                        0,
                        y);

                scroll.Controls.Add(card);

                y +=
                    card.Height +
                    12;
            }

            void ResizeAlarmCards()
            {
                int width =
                    Math.Max(
                        650,
                        scroll.ClientSize.Width -
                        5);

                foreach (Control control in
                         scroll.Controls)
                {
                    if (control is Panel card)
                        card.Width = width;
                }
            }

            scroll.Resize +=
                (_, _) =>
                    ResizeAlarmCards();

            ResizeAlarmCards();
        }

        page.Controls.Add(scroll);
        page.Controls.Add(toolbar);

        return page;
    }

    private Panel CreateAlarmCard(
        Alarm alarm)
    {
        var card =
            CreateCard();

        card.Height = 120;

        card.Margin =
            Padding.Empty;

        var time =
            new Label
            {
                Text = alarm.Time,

                Location =
                    new Point(
                        18,
                        18),

                Width = 175,
                Height = 55,

                Font =
                    new Font(
                        "Consolas",
                        28F,
                        FontStyle.Bold),

                ForeColor =
                    alarm.Enabled
                        ? Gold
                        : TextSecondary,

                TextAlign =
                    ContentAlignment.MiddleLeft
            };

        var name =
            new Label
            {
                Text =
                    string.IsNullOrWhiteSpace(
                        alarm.Label)
                        ? "Sem nome"
                        : alarm.Label,

                Location =
                    new Point(
                        200,
                        14),

                Width = 330,
                Height = 30,

                Font =
                    new Font(
                        "Segoe UI",
                        13F,
                        FontStyle.Bold),

                ForeColor = TextPrimary,

                AutoEllipsis = true
            };

        var group =
            new Label
            {
                Text =
                    $"Grupo: {alarm.Group}",

                Location =
                    new Point(
                        200,
                        44),

                Width = 330,
                Height = 22,

                Font =
                    new Font(
                        "Segoe UI",
                        8F),

                ForeColor = GoldDark,

                AutoEllipsis = true
            };

        var days =
            new Label
            {
                Text =
                    FormatAlarmDays(
                        alarm.Days),

                Location =
                    new Point(
                        200,
                        70),

                Width = 360,
                Height = 25,

                Font =
                    new Font(
                        "Segoe UI",
                        9F),

                ForeColor = TextSecondary
            };

        var buttons =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Right,

                Width = 275,

                Height = 120,

                FlowDirection =
                    FlowDirection.LeftToRight,

                WrapContents = false,

                Padding =
                    new Padding(
                        0,
                        43,
                        8,
                        0),

                BackColor =
                    Color.Transparent
            };

        var toggle =
            new Button
            {
                Text =
                    alarm.Enabled
                        ? "● ATIVO"
                        : "○ DESATIVADO",

                Width = 110,
                Height = 34,

                FlatStyle =
                    FlatStyle.Flat,

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),

                Cursor = Cursors.Hand,

                BackColor =
                    alarm.Enabled
                        ? Color.FromArgb(
                            60,
                            90,
                            60)
                        : Color.FromArgb(
                            65,
                            55,
                            60),

                ForeColor =
                    alarm.Enabled
                        ? Color.FromArgb(
                            150,
                            220,
                            140)
                        : TextSecondary,

                TabStop = false
            };

        toggle.FlatAppearance.BorderSize = 0;

        toggle.Click +=
            (_, _) =>
            {
                _alarmService.Toggle(
                    alarm.Id,
                    !alarm.Enabled);

                ShowPage("Alarmes");
            };

        var edit =
            CreateSmallButton("EDITAR");

        edit.Width = 75;

        edit.Click +=
            (_, _) =>
                EditAlarm(alarm);

        var delete =
            CreateSmallButton("✕");

        delete.Width = 42;

        delete.Click +=
            (_, _) =>
            {
                var result =
                    MessageBox.Show(
                        this,
                        $"Excluir o alarme \"{alarm.Label}\"?",
                        "BerClock",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (result ==
                    DialogResult.Yes)
                {
                    _alarmService.Remove(
                        alarm.Id);

                    ShowPage("Alarmes");
                }
            };

        buttons.Controls.Add(toggle);
        buttons.Controls.Add(edit);
        buttons.Controls.Add(delete);

        card.Controls.Add(buttons);
        card.Controls.Add(time);
        card.Controls.Add(name);
        card.Controls.Add(group);
        card.Controls.Add(days);

        return card;
    }

    private void EditAlarm(
        Alarm alarm)
    {
        using var form =
            new AlarmEditForm(alarm);

        if (form.ShowDialog(this) ==
            DialogResult.OK)
        {
            _alarmService.Update(
                form.Alarm);

            ShowPage("Alarmes");
        }
    }

    // =========================================================
    // TEMPORIZADOR
    // =========================================================

    private Panel BuildTimerPage()
    {
        var page =
            CreatePage();

        var outer =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,
                RowCount = 3,

                BackColor = Background,

                Padding =
                    new Padding(
                        0,
                        0,
                        0,
                        0)
            };

        outer.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                20F));

        outer.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                210));

        outer.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                80F));

        var spacerTop =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Background
            };

        var spacerBottom =
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Background
            };

        var card =
            CreateCard();

        card.Dock = DockStyle.Fill;

        card.Padding =
            new Padding(
                25);

        var title =
            new Label
            {
                Text = "TEMPORIZADOR",

                Dock = DockStyle.Top,
                Height = 30,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                ForeColor = GoldDark
            };

        var display =
            new Label
            {
                Text =
                    FormatTimerTime(
                        _timerRemaining),

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Consolas",
                        48F,
                        FontStyle.Bold),

                ForeColor = GoldBright,

                Tag = "TIMER_DISPLAY"
            };

        var controls =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,

                Height = 48,

                FlowDirection =
                    FlowDirection.LeftToRight,

                WrapContents = false,

                Anchor =
                    AnchorStyles.Bottom,

                Padding =
                    new Padding(
                        0,
                        4,
                        0,
                        0)
            };

        var hours =
            CreateNumberInput(
                99);

        var minutes =
            CreateNumberInput(
                59);

        var seconds =
            CreateNumberInput(
                59);

        controls.Controls.Add(
            CreateToolLabel("HORAS"));

        controls.Controls.Add(hours);

        controls.Controls.Add(
            CreateToolLabel("MIN"));

        controls.Controls.Add(minutes);

        controls.Controls.Add(
            CreateToolLabel("SEG"));

        controls.Controls.Add(seconds);

        var buttons =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,

                Height = 45,

                FlowDirection =
                    FlowDirection.LeftToRight,

                WrapContents = false
            };

        var start =
            CreateSmallButton("INICIAR");

        var pause =
            CreateSmallButton("PAUSAR");

        var reset =
            CreateSmallButton("ZERAR");

        start.Width = 100;
        pause.Width = 100;
        reset.Width = 100;

        start.Click +=
            (_, _) =>
            {
                if (_timerRunning)
                    return;

                if (_timerRemaining ==
                    TimeSpan.Zero)
                {
                    _timerRemaining =
                        new TimeSpan(
                            (int)hours.Value,
                            (int)minutes.Value,
                            (int)seconds.Value);

                    if (_timerRemaining ==
                        TimeSpan.Zero)
                    {
                        MessageBox.Show(
                            this,
                            "Defina um tempo antes de iniciar.",
                            "BerClock",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }
                }

                _timerLastTick =
                    DateTime.Now;

                _timerRunning = true;

                _toolTimer.Start();
            };

        pause.Click +=
            (_, _) =>
            {
                _timerRunning = false;

                if (!_stopwatchRunning)
                    _toolTimer.Stop();
            };

        reset.Click +=
            (_, _) =>
            {
                _timerRunning = false;

                _timerRemaining =
                    TimeSpan.Zero;

                hours.Value = 0;
                minutes.Value = 0;
                seconds.Value = 0;

                if (!_stopwatchRunning)
                    _toolTimer.Stop();

                UpdateToolDisplays(
                    page);
            };

        buttons.Controls.Add(start);
        buttons.Controls.Add(pause);
        buttons.Controls.Add(reset);

        card.Controls.Add(display);
        card.Controls.Add(buttons);
        card.Controls.Add(controls);
        card.Controls.Add(title);

        outer.Controls.Add(
            spacerTop,
            0,
            0);

        outer.Controls.Add(
            card,
            0,
            1);

        outer.Controls.Add(
            spacerBottom,
            0,
            2);

        page.Controls.Add(outer);

        return page;
    }

    private NumericUpDown CreateNumberInput(
        int maximum)
    {
        return new NumericUpDown
        {
            Minimum = 0,
            Maximum = maximum,
            Width = 70,
            Height = 30,
            Font =
                new Font(
                    "Segoe UI",
                    10F)
        };
    }

    // =========================================================
    // CRONÔMETRO
    // =========================================================

    private Panel BuildStopwatchPage()
    {
        var page =
            CreatePage();

        var layout =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,
                RowCount = 3,

                BackColor = Background
            };

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                20F));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                210));

        layout.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                80F));

        var card =
            CreateCard();

        card.Dock = DockStyle.Fill;

        card.Padding =
            new Padding(25);

        var title =
            new Label
            {
                Text = "CRONÔMETRO",

                Dock = DockStyle.Top,
                Height = 30,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                ForeColor = GoldDark
            };

        var display =
            new Label
            {
                Text =
                    FormatStopwatchTime(
                        _stopwatchAccumulated),

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Consolas",
                        48F,
                        FontStyle.Bold),

                ForeColor = GoldBright,

                Tag = "STOPWATCH_DISPLAY"
            };

        var buttons =
            new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,

                Height = 45,

                FlowDirection =
                    FlowDirection.LeftToRight,

                WrapContents = false
            };

        var start =
            CreateSmallButton("INICIAR");

        var pause =
            CreateSmallButton("PAUSAR");

        var reset =
            CreateSmallButton("ZERAR");

        start.Width = 100;
        pause.Width = 100;
        reset.Width = 100;

        start.Click +=
            (_, _) =>
            {
                if (_stopwatchRunning)
                    return;

                _stopwatch.Restart();

                _stopwatchRunning = true;

                _toolTimer.Start();
            };

        pause.Click +=
            (_, _) =>
            {
                if (!_stopwatchRunning)
                    return;

                _stopwatch.Stop();

                _stopwatchAccumulated +=
                    _stopwatch.Elapsed;

                _stopwatch.Reset();

                _stopwatchRunning = false;

                if (!_timerRunning)
                    _toolTimer.Stop();

                UpdateToolDisplays(page);
            };

        reset.Click +=
            (_, _) =>
            {
                _stopwatchRunning = false;

                _stopwatch.Stop();
                _stopwatch.Reset();

                _stopwatchAccumulated =
                    TimeSpan.Zero;

                if (!_timerRunning)
                    _toolTimer.Stop();

                UpdateToolDisplays(page);
            };

        buttons.Controls.Add(start);
        buttons.Controls.Add(pause);
        buttons.Controls.Add(reset);

        card.Controls.Add(display);
        card.Controls.Add(buttons);
        card.Controls.Add(title);

        layout.Controls.Add(
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Background
            },
            0,
            0);

        layout.Controls.Add(
            card,
            0,
            1);

        layout.Controls.Add(
            new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Background
            },
            0,
            2);

        page.Controls.Add(layout);

        return page;
    }

    // =========================================================
    // RELÓGIO MUNDIAL
    // =========================================================

    private Panel BuildWorldClockPage()
    {
        var page =
            CreatePage();

        var grid =
            new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 2,
                RowCount = 2,

                BackColor = Background,

                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

        grid.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                50F));

        grid.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                50F));

        grid.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                50F));

        grid.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                50F));

        AddWorldClock(
            grid,
            "RIO DE JANEIRO",
            "E. South America Standard Time",
            0,
            0);

        AddWorldClock(
            grid,
            "NEW YORK",
            "Eastern Standard Time",
            1,
            0);

        AddWorldClock(
            grid,
            "LONDON",
            "GMT Standard Time",
            0,
            1);

        AddWorldClock(
            grid,
            "TOKYO",
            "Tokyo Standard Time",
            1,
            1);

        page.Controls.Add(grid);

        UpdateWorldClocks();

        _worldClockTimer.Start();

        return page;
    }

    private void AddWorldClock(
        TableLayoutPanel grid,
        string city,
        string timeZoneId,
        int column,
        int row)
    {
        var card =
            CreateCard();

        card.Dock = DockStyle.Fill;

        card.Margin =
            new Padding(7);

        card.Padding =
            new Padding(15);

        var cityLabel =
            new Label
            {
                Text = city,

                Dock = DockStyle.Top,
                Height = 30,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold),

                ForeColor = GoldDark
            };

        var timeLabel =
            new Label
            {
                Text = "--:--:--",

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Consolas",
                        34F,
                        FontStyle.Bold),

                ForeColor = GoldBright,

                Tag = timeZoneId
            };

        var dateLabel =
            new Label
            {
                Text = "",

                Dock = DockStyle.Bottom,
                Height = 28,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Font =
                    new Font(
                        "Segoe UI",
                        9F),

                ForeColor = TextSecondary,

                Tag = "WORLD_DATE"
            };

        card.Controls.Add(timeLabel);
        card.Controls.Add(dateLabel);
        card.Controls.Add(cityLabel);

        grid.Controls.Add(
            card,
            column,
            row);

        _worldClockLabels[city] =
            timeLabel;
    }

    private void UpdateWorldClocks()
    {
        if (IsDisposed ||
            Disposing)
        {
            return;
        }

        foreach (var pair in
                 _worldClockLabels.ToList())
        {
            var timeLabel =
                pair.Value;

            if (timeLabel.IsDisposed)
                continue;

            if (timeLabel.Tag is not string zoneId)
                continue;

            TimeZoneInfo zone;

            try
            {
                zone =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        zoneId);
            }
            catch
            {
                continue;
            }

            var local =
                TimeZoneInfo.ConvertTime(
                    DateTimeOffset.Now,
                    zone);

            string time =
                local.ToString(
                    "HH:mm:ss");

            if (timeLabel.Text != time)
                timeLabel.Text = time;

            if (timeLabel.Parent is not Control card)
                continue;

            string date =
                local.ToString(
                    "dddd, dd/MM/yyyy");

            foreach (Control child in
                     card.Controls)
            {
                if (child is Label label &&
                    label.Tag is string tag &&
                    tag == "WORLD_DATE")
                {
                    if (label.Text != date)
                        label.Text = date;
                }
            }
        }
    }

    // =========================================================
    // CARD
    // =========================================================

    private Panel CreateCard()
    {
        var card =
            new Panel
            {
                BackColor = PanelColor,

                Margin = Padding.Empty
            };

        card.Paint +=
            (_, e) =>
            {
                if (card.Width <= 1 ||
                    card.Height <= 1)
                {
                    return;
                }

                using var pen =
                    new Pen(
                        GoldDark,
                        1);

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    card.Width - 1,
                    card.Height - 1);
            };

        return card;
    }

    // =========================================================
    // BOTÕES
    // =========================================================

    private Button CreateSmallButton(
        string text)
    {
        var button =
            new Button
            {
                Text = text,

                Width = 85,
                Height = 34,

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    PanelHover,

                ForeColor =
                    TextPrimary,

                Font =
                    new Font(
                        "Segoe UI",
                        8F,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand,

                TabStop = false
            };

        button.FlatAppearance.BorderSize = 0;

        button.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(
                80,
                65,
                50);

        return button;
    }

    private Label CreateToolLabel(
        string text)
    {
        return new Label
        {
            Text = text,

            AutoSize = true,

            Height = 30,

            Padding =
                new Padding(
                    8,
                    7,
                    8,
                    0),

            Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold),

            ForeColor =
                TextSecondary
        };
    }

    // =========================================================
    // DIAS
    // =========================================================

    private static string FormatAlarmDays(
        AlarmDays days)
    {
        var result =
            new List<string>();

        if (days.HasFlag(AlarmDays.Monday))
            result.Add("SEG");

        if (days.HasFlag(AlarmDays.Tuesday))
            result.Add("TER");

        if (days.HasFlag(AlarmDays.Wednesday))
            result.Add("QUA");

        if (days.HasFlag(AlarmDays.Thursday))
            result.Add("QUI");

        if (days.HasFlag(AlarmDays.Friday))
            result.Add("SEX");

        if (days.HasFlag(AlarmDays.Saturday))
            result.Add("SÁB");

        if (days.HasFlag(AlarmDays.Sunday))
            result.Add("DOM");

        return result.Count == 0
            ? "Uma vez"
            : string.Join(
                "   ",
                result);
    }

    // =========================================================
    // RELÓGIO PRINCIPAL
    // =========================================================

    private void UpdateClock()
    {
        if (IsDisposed ||
            Disposing ||
            _currentPage == null)
        {
            return;
        }

        string time =
            DateTime.Now.ToString(
                "HH:mm:ss");

        string date =
            DateTime.Now.ToString(
                "dddd, dd 'de' MMMM 'de' yyyy");

        UpdateClockControls(
            _currentPage,
            time,
            date);
    }

    private void UpdateClockControls(
        Control parent,
        string time,
        string date)
    {
        if (parent.IsDisposed)
            return;

        foreach (Control control in
                 parent.Controls)
        {
            if (control.IsDisposed)
                continue;

            if (control is Label label &&
                label.Tag is string tag)
            {
                if (tag == "MAIN_CLOCK")
                {
                    if (label.Text != time)
                        label.Text = time;
                }
                else if (tag == "MAIN_DATE")
                {
                    if (label.Text != date)
                        label.Text = date;
                }
            }

            if (control.HasChildren)
            {
                UpdateClockControls(
                    control,
                    time,
                    date);
            }
        }
    }

    // =========================================================
    // PRÓXIMO ALARME
    // =========================================================

    private string GetNextAlarmText()
    {
        var now =
            DateTime.Now;

        var currentTime =
            TimeOnly.FromDateTime(now);

        var alarm =
            _alarmService.Alarms
                .Where(
                    a =>
                        a.Enabled &&
                        a.IsScheduledFor(
                            now.DayOfWeek))
                .Select(
                    a => new
                    {
                        Alarm = a,
                        Time = a.ParsedTime
                    })
                .OrderBy(
                    x =>
                        x.Time < currentTime
                            ? x.Time.AddHours(24)
                            : x.Time)
                .FirstOrDefault();

        if (alarm is null)
            return "Nenhum";

        return string.IsNullOrWhiteSpace(
                   alarm.Alarm.Label)
            ? alarm.Alarm.Time
            : $"{alarm.Alarm.Time} - {alarm.Alarm.Label}";
    }

    // =========================================================
    // TIMER DAS FERRAMENTAS
    // =========================================================

    private void ToolTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_timerRunning)
        {
            var now =
                DateTime.Now;

            var elapsed =
                now - _timerLastTick;

            _timerLastTick = now;

            _timerRemaining -= elapsed;

            if (_timerRemaining <=
                TimeSpan.Zero)
            {
                _timerRemaining =
                    TimeSpan.Zero;

                _timerRunning = false;

                SystemSounds.Exclamation.Play();

                MessageBox.Show(
                    this,
                    "O temporizador terminou.",
                    "BerClock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        if (_currentPage != null &&
            !_currentPage.IsDisposed)
        {
            UpdateToolDisplays(
                _currentPage);
        }

        if (!_timerRunning &&
            !_stopwatchRunning)
        {
            _toolTimer.Stop();
        }
    }

    private void UpdateToolDisplays(
        Control parent)
    {
        if (parent.IsDisposed)
            return;

        foreach (Control control in
                 parent.Controls)
        {
            if (control.IsDisposed)
                continue;

            if (control is Label label &&
                label.Tag is string tag)
            {
                if (tag == "TIMER_DISPLAY")
                {
                    string value =
                        FormatTimerTime(
                            _timerRemaining);

                    if (label.Text != value)
                        label.Text = value;
                }
                else if (tag ==
                         "STOPWATCH_DISPLAY")
                {
                    var elapsed =
                        _stopwatchAccumulated;

                    if (_stopwatchRunning)
                    {
                        elapsed +=
                            _stopwatch.Elapsed;
                    }

                    string value =
                        FormatStopwatchTime(
                            elapsed);

                    if (label.Text != value)
                        label.Text = value;
                }
            }

            if (control.HasChildren)
            {
                UpdateToolDisplays(
                    control);
            }
        }
    }

    // =========================================================
    // FORMATAÇÃO
    // =========================================================

    private static string FormatTimerTime(
        TimeSpan time)
    {
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        return
            $"{(int)time.TotalHours:00}:" +
            $"{time.Minutes:00}:" +
            $"{time.Seconds:00}";
    }

    private static string FormatStopwatchTime(
        TimeSpan time)
    {
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        return
            $"{(int)time.TotalHours:00}:" +
            $"{time.Minutes:00}:" +
            $"{time.Seconds:00}." +
            $"{time.Milliseconds / 10:00}";
    }

    // =========================================================
    // ALARME DISPARADO
    // =========================================================

    private void AlarmService_AlarmTriggered(
        object? sender,
        AlarmTriggeredEventArgs e)
    {
        if (IsDisposed ||
            Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(
                    new Action(
                        () =>
                            AlarmService_AlarmTriggered(
                                sender,
                                e)));
            }
            catch
            {
            }

            return;
        }

        try
        {
            var ring =
                new AlarmRingWindow(
                    e.Alarm,
                    _audioService);

            ring.Show();
        }
        catch
        {
            SystemSounds.Exclamation.Play();
        }
    }

    // =========================================================
    // FECHAMENTO
    // =========================================================

    private void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        // -----------------------------------------------------
        // CLIQUE NO X
        // -----------------------------------------------------
        // Não encerra o programa.
        // Apenas esconde a janela e mantém os alarmes ativos.

        if (!_reallyExit)
        {
            e.Cancel = true;

            Hide();

            ShowInTaskbar = false;

            return;
        }

        // -----------------------------------------------------
        // SAÍDA REAL
        // -----------------------------------------------------

        _clockTimer.Stop();
        _toolTimer.Stop();
        _worldClockTimer.Stop();

        _stopwatch.Stop();

        _alarmService.AlarmTriggered -=
            AlarmService_AlarmTriggered;

        _trayIcon.Visible = false;

        _trayIcon.Dispose();

        _trayMenu.Dispose();
    }
}
