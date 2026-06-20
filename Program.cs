using OWTrackerDesktop.Services;

namespace OWTrackerDesktop;

static class Program
{
    private const string SingleInstanceMutexName = @"Local\OWTrackerDesktop_SingleInstance_9f2c1a8e";

    [STAThread]
    static void Main()
    {
        AppLocalizer.SetLanguage(GameLanguageStore.LoadOrDefault().Id);

        using var mutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                AppLocalizer.T("already_running"),
                AppLocalizer.T("window_title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
