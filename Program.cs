using Win7AlarmClassic.Services;
using Win7AlarmClassic.UI;

namespace Win7AlarmClassic;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var repository = new JsonAlarmRepository();
        using var alarmService = new AlarmService(repository);
        using var audioService = new AudioService();

        Application.Run(new MainForm(alarmService, audioService));
    }
}
