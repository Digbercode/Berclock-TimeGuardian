using Win7AlarmClassic.Models;

namespace Win7AlarmClassic.UI;

public sealed class AlarmEditForm : Form
{
    private readonly DateTimePicker _time = new()
    {
        Format = DateTimePickerFormat.Time,
        ShowUpDown = true,
        Width = 110
    };

    private readonly TextBox _label = new() { Width = 260 };
    private readonly TextBox _group = new() { Width = 260 };
    private readonly ComboBox _builtIn = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 180
    };

    private readonly TextBox _customPath = new()
    {
        Width = 300,
        ReadOnly = true
    };

    private readonly CheckBox _fadeIn = new() { Text = "Fade-in", AutoSize = true };
    private readonly NumericUpDown _fadeInSeconds = new()
    {
        Minimum = 1, Maximum = 120, Value = 3, Width = 65
    };

    private readonly CheckBox _fadeOut = new() { Text = "Fade-out", AutoSize = true, Checked = true };
    private readonly NumericUpDown _fadeOutSeconds = new()
    {
        Minimum = 1, Maximum = 120, Value = 3, Width = 65
    };

    private readonly NumericUpDown _duration = new()
    {
        Minimum = 0, Maximum = 86400, Value = 0, Width = 80
    };

    private readonly Dictionary<CheckBox, AlarmDays> _dayChecks = new();
    private string? _customSound;

    public Alarm Alarm { get; private set; }

    public AlarmEditForm(Alarm? alarm = null)
    {
        Alarm = alarm is null ? new Alarm() : Clone(alarm);

        Text = alarm is null ? "Novo alarme" : "Editar alarme";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(610, 520);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(240, 240, 240);

        _time.Value = DateTime.Today.Add(Alarm.ParsedTime.ToTimeSpan());
        _label.Text = Alarm.Label;
        _group.Text = string.IsNullOrWhiteSpace(Alarm.Group) ? "Geral" : Alarm.Group;

        _builtIn.Items.AddRange(Enum.GetNames<BuiltInSound>());
        _builtIn.SelectedItem = Alarm.BuiltInSound.ToString();

        _customPath.Text = string.IsNullOrWhiteSpace(Alarm.CustomSoundPath)
            ? "Nenhum arquivo personalizado"
            : Path.GetFileName(Alarm.CustomSoundPath);

        _customSound = Alarm.CustomSoundPath;

        _fadeIn.Checked = Alarm.FadeInEnabled;
        _fadeInSeconds.Value = Math.Clamp(Alarm.FadeInSeconds, 1, 120);
        _fadeOut.Checked = Alarm.FadeOutEnabled;
        _fadeOutSeconds.Value = Math.Clamp(Alarm.FadeOutSeconds, 1, 120);
        _duration.Value = Math.Clamp(Alarm.DurationSeconds, 0, 86400);

        BuildUi();
    }

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 8
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 8; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 2 ? 75 : 48));

        Controls.Add(layout);

        AddRow(layout, 0, "Horário:", _time);
        AddRow(layout, 1, "Nome:", _label);
        AddRow(layout, 2, "Dias:", BuildDaysPanel());
        AddRow(layout, 3, "Grupo:", _group);
        AddRow(layout, 4, "Som:", BuildSoundPanel());
        AddRow(layout, 5, "Duração:", BuildDurationPanel());
        AddRow(layout, 6, "Fade:", BuildFadePanel());

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        var ok = new ClassicButton { Text = "OK", Width = 95 };
        var cancel = new ClassicButton
        {
            Text = "Cancelar",
            Width = 95,
            DialogResult = DialogResult.Cancel
        };

        ok.Click += (_, _) => ValidateAndAccept();

        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 1, 7);

        AcceptButton = ok;
        CancelButton = cancel;

        _fadeIn.CheckedChanged += (_, _) => _fadeInSeconds.Enabled = _fadeIn.Checked;
        _fadeOut.CheckedChanged += (_, _) => _fadeOutSeconds.Enabled = _fadeOut.Checked;
        _fadeInSeconds.Enabled = _fadeIn.Checked;
        _fadeOutSeconds.Enabled = _fadeOut.Checked;
    }

    private Control BuildDaysPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        foreach (var (text, flag) in new[]
        {
            ("Seg", AlarmDays.Monday),
            ("Ter", AlarmDays.Tuesday),
            ("Qua", AlarmDays.Wednesday),
            ("Qui", AlarmDays.Thursday),
            ("Sex", AlarmDays.Friday),
            ("Sáb", AlarmDays.Saturday),
            ("Dom", AlarmDays.Sunday)
        })
        {
            var cb = new CheckBox
            {
                Text = text,
                AutoSize = true,
                Checked = Alarm.Days.HasFlag(flag),
                Margin = new Padding(4, 3, 4, 3)
            };

            _dayChecks[cb] = flag;
            panel.Controls.Add(cb);
        }

        return panel;
    }

    private Control BuildSoundPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        var browse = new ClassicButton
        {
            Text = "Escolher áudio",
            Width = 125
        };

        browse.Click += (_, _) => ChooseSound();

        panel.Controls.Add(_builtIn);
        panel.Controls.Add(browse);
        panel.Controls.Add(_customPath);

        return panel;
    }

    private Control BuildDurationPanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill };

        panel.Controls.Add(_duration);
        panel.Controls.Add(new Label
        {
            Text = "segundos (0 = manual)",
            AutoSize = true,
            Padding = new Padding(4, 7, 0, 0)
        });

        return panel;
    }

    private Control BuildFadePanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true
        };

        panel.Controls.Add(_fadeIn);
        panel.Controls.Add(_fadeInSeconds);
        panel.Controls.Add(new Label
        {
            Text = "s",
            AutoSize = true,
            Padding = new Padding(2, 7, 8, 0)
        });

        panel.Controls.Add(_fadeOut);
        panel.Controls.Add(_fadeOutSeconds);
        panel.Controls.Add(new Label
        {
            Text = "s",
            AutoSize = true,
            Padding = new Padding(2, 7, 0, 0)
        });

        return panel;
    }

    private static void AddRow(TableLayoutPanel panel, int row, string caption, Control control)
    {
        panel.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, row);

        panel.Controls.Add(control, 1, row);
    }

    private void ChooseSound()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Arquivos de áudio (*.wav;*.mp3;*.aac;*.m4a;*.wma)|*.wav;*.mp3;*.aac;*.m4a;*.wma|Todos os arquivos|*.*",
            Title = "Escolher som do alarme"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _customSound = dialog.FileName;
            _customPath.Text = Path.GetFileName(dialog.FileName);
        }
    }

    private void ValidateAndAccept()
    {
        if (string.IsNullOrWhiteSpace(_label.Text))
        {
            MessageBox.Show(this, "Informe um nome para o alarme.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_group.Text))
            _group.Text = "Geral";

        Alarm.Days = AlarmDays.None;

        foreach (var pair in _dayChecks)
            if (pair.Key.Checked)
                Alarm.Days |= pair.Value;

        if (Alarm.Days == AlarmDays.None)
        {
            MessageBox.Show(this, "Selecione pelo menos um dia da semana.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Alarm.Label = _label.Text.Trim();
        Alarm.Group = _group.Text.Trim();
        Alarm.ParsedTime = TimeOnly.FromDateTime(_time.Value);
        Alarm.BuiltInSound = Enum.Parse<BuiltInSound>(
            _builtIn.SelectedItem?.ToString() ?? nameof(BuiltInSound.Asterisk));

        Alarm.CustomSoundPath = _customSound;
        Alarm.DurationSeconds = (int)_duration.Value;
        Alarm.FadeInEnabled = _fadeIn.Checked;
        Alarm.FadeInSeconds = (int)_fadeInSeconds.Value;
        Alarm.FadeOutEnabled = _fadeOut.Checked;
        Alarm.FadeOutSeconds = (int)_fadeOutSeconds.Value;

        DialogResult = DialogResult.OK;
        Close();
    }

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
}
