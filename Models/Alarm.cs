using System.Text.Json.Serialization;

namespace Win7AlarmClassic.Models;

[Flags]
public enum AlarmDays
{
    None = 0,
    Monday = 1 << 0,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6
}

public enum BuiltInSound
{
    Asterisk,
    Exclamation,
    Beep,
    Hand
}

public sealed class Alarm
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = "Alarme";
    public string Group { get; set; } = "Geral";
    public string Time { get; set; } = "07:00";
    public AlarmDays Days { get; set; } =
        AlarmDays.Monday | AlarmDays.Tuesday | AlarmDays.Wednesday |
        AlarmDays.Thursday | AlarmDays.Friday;
    public bool Enabled { get; set; } = true;

    public string? CustomSoundPath { get; set; }
    public BuiltInSound BuiltInSound { get; set; } = BuiltInSound.Asterisk;

    // Duração do toque. Zero = toca até o usuário dispensar.
    public int DurationSeconds { get; set; } = 0;

    // Fade configurável.
    public bool FadeInEnabled { get; set; } = false;
    public int FadeInSeconds { get; set; } = 3;
    public bool FadeOutEnabled { get; set; } = true;
    public int FadeOutSeconds { get; set; } = 3;

    [JsonIgnore]
    public TimeOnly ParsedTime
    {
        get => TimeOnly.TryParse(Time, out var result) ? result : new TimeOnly(7, 0);
        set => Time = value.ToString("HH:mm");
    }

    public bool IsScheduledFor(DayOfWeek day)
    {
        var flag = day switch
        {
            DayOfWeek.Monday => AlarmDays.Monday,
            DayOfWeek.Tuesday => AlarmDays.Tuesday,
            DayOfWeek.Wednesday => AlarmDays.Wednesday,
            DayOfWeek.Thursday => AlarmDays.Thursday,
            DayOfWeek.Friday => AlarmDays.Friday,
            DayOfWeek.Saturday => AlarmDays.Saturday,
            DayOfWeek.Sunday => AlarmDays.Sunday,
            _ => AlarmDays.None
        };

        return Days.HasFlag(flag);
    }
}
