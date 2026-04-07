using System.Threading;

namespace OWTrackerDesktop;

static class Program
{
    private const string SingleInstanceMutexName = @"Local\OWTrackerDesktop_SingleInstance_9f2c1a8e";

    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Overwatch Queue Tracker is already running.",
                "Overwatch Queue Tracker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
