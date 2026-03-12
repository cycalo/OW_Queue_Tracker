using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace OWTrackerDesktop;

public class InstructionsForm : Form
{
    private static readonly Color BgDeep = ColorTranslator.FromHtml("#0d1117");
    private static readonly Color BgCard = ColorTranslator.FromHtml("#161b22");
    private static readonly Color BorderCard = ColorTranslator.FromHtml("#30363d");
    private static readonly Color AccentOrange = ColorTranslator.FromHtml("#F99E1A");
    private static readonly Color TextPrimary = ColorTranslator.FromHtml("#e6edf3");
    private static readonly Color TextSecondary = ColorTranslator.FromHtml("#8b949e");
    private static readonly Color TextMuted = ColorTranslator.FromHtml("#484f58");
    private static readonly Color StatusGreen = ColorTranslator.FromHtml("#3fb950");

    public InstructionsForm()
    {
        DoubleBuffered = true;
        Text = "How to get queue notifications on your phone";
        Size = new Size(600, 740);
        MinimumSize = new Size(600, 740);
        MaximumSize = new Size(600, 740);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = BgDeep;
        ForeColor = TextPrimary;
        Font = new Font("Segoe UI", 11f);

        HandleCreated += OnHandleCreated;
        LoadAppIcon();

        var headerPanel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(500, 54),
            BackColor = Color.Transparent
        };
        headerPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, headerPanel.Width, headerPanel.Height),
                Color.FromArgb(25, AccentOrange), Color.FromArgb(0, AccentOrange),
                LinearGradientMode.Vertical);
            g.FillRectangle(brush, 0, 0, headerPanel.Width, headerPanel.Height);
            using var linePen = new Pen(Color.FromArgb(50, AccentOrange), 1);
            g.DrawLine(linePen, 16, headerPanel.Height - 1, headerPanel.Width - 16, headerPanel.Height - 1);
        };

        var accentBar = new Panel
        {
            Size = new Size(4, 26),
            Location = new Point(16, 14),
            BackColor = AccentOrange
        };

        var titleLabel = new Label
        {
            Text = "SETUP INSTRUCTIONS",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(28, 14)
        };

        headerPanel.Controls.Add(accentBar);
        headerPanel.Controls.Add(titleLabel);
        Controls.Add(headerPanel);

        // Scrollable content area under the header so text never overflows
        var contentPanel = new Panel
        {
            Location = new Point(0, headerPanel.Bottom),
            Size = new Size(ClientSize.Width, ClientSize.Height - headerPanel.Height - 70),
            BackColor = Color.Transparent,
            AutoScroll = true
        };
        Controls.Add(contentPanel);

        var steps = new (string number, string title, string body)[]
        {
            ("1", "Keep this app running",
             "Overwatch Queue Tracker must be open on your PC."),
            ("2", "Keep Overwatch visible",
             "Do not minimize Overwatch. The game must be visible on screen for the tracker to work."),
            ("3", "Same network",
             "Your phone and PC need to be on the same WiFi network."),
            ("4", "Find the server address",
             "Look for Server followed by an IP address. You'll enter this on your phone."),
            ("5", "Open the mobile app",
             "Open OW Tracker on your phone and select the Desktop tab."),
            ("6", "Enter the IP address",
             "Type in the server IP address. You can ignore the :8080 part. Tap Connect."),
            ("7", "Done",
             "When a game is found, your phone will show a notification.")
        };

        int yPos = 12;
        foreach (var (number, stepTitle, body) in steps)
        {
            var stepPanel = new Panel
            {
                Location = new Point(16, yPos),
                Size = new Size(452, 56),
                BackColor = Color.Transparent
            };

            var numLabel = new Label
            {
                Text = number,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AccentOrange,
                BackColor = BgCard,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(28, 28),
                Location = new Point(0, 4)
            };

            var stepTitleLabel = new Label
            {
                Text = stepTitle,
                Font = new Font("Segoe UI Semibold", 11f),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(36, 2)
            };

            var bodyLabel = new Label
            {
                Text = body,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(410, 32),
                Location = new Point(36, 22)
            };

            stepPanel.Controls.Add(numLabel);
            stepPanel.Controls.Add(stepTitleLabel);
            stepPanel.Controls.Add(bodyLabel);
            contentPanel.Controls.Add(stepPanel);
            yPos += stepPanel.Height + 6;
        }

        yPos += 4;
        var troubleLabel = new Label
        {
            Text = "TROUBLESHOOTING",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, yPos)
        };
        contentPanel.Controls.Add(troubleLabel);
        yPos += 22;

        var troubleItems = new[]
        {
            "Make sure this app is running on your PC.",
            "Double-check the IP address. Your router may change the IP address periodically.",
            "Confirm your phone and PC are on the same WiFi.",
            "Do not minimize Overwatch — it must be visible on screen to work.",
            "Be in Fullscreen or Borderless Windowed mode in Overwatch."
        };

        foreach (var item in troubleItems)
        {
            var bulletLabel = new Label
            {
                Text = $"\u2022  {item}",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(24, yPos)
            };
            contentPanel.Controls.Add(bulletLabel);
            yPos += 20;
        }

        // Action button pinned at the bottom of the dialog
        var okButton = new Button
        {
            Text = "Got it",
            Size = new Size(130, 40),
            Location = new Point(ClientSize.Width - 130 - 24, ClientSize.Height - 40 - 16),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, StatusGreen.R, StatusGreen.G, StatusGreen.B),
            ForeColor = StatusGreen,
            Font = new Font("Segoe UI Semibold", 11f),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.OK
        };
        okButton.FlatAppearance.BorderColor = Color.FromArgb(80, StatusGreen.R, StatusGreen.G, StatusGreen.B);
        okButton.FlatAppearance.BorderSize = 1;
        okButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, StatusGreen.R, StatusGreen.G, StatusGreen.B);
        okButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, StatusGreen.R, StatusGreen.G, StatusGreen.B);
        okButton.Click += (_, _) => Close();

        Controls.Add(okButton);
        AcceptButton = okButton;
        CancelButton = okButton;
    }

    // Match dark title bar + app icon used by the main window
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private void OnHandleCreated(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || DesignMode)
            return;
        TrySetDarkTitleBar();
    }

    private void TrySetDarkTitleBar()
    {
        int useDark = 1;
        int size = sizeof(int);
        if (DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, size) != 0)
        {
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDark, size);
        }
    }

    private void LoadAppIcon()
    {
        // Try a few likely locations for the shared app icon
        string? baseDir = Path.GetDirectoryName(Application.ExecutablePath);
        string[] candidates =
        {
            Path.Combine(baseDir ?? string.Empty, "playstore-icon.png"),
            Path.Combine(AppContext.BaseDirectory, "playstore-icon.png"),
            Path.Combine(AppContext.BaseDirectory, "assets", "playstore-icon.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "playstore-icon.png")
        };

        string? iconPath = candidates.FirstOrDefault(File.Exists);
        if (iconPath is null)
            return;

        try
        {
            using var bmp = new Bitmap(iconPath);
            Icon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
        }
        catch
        {
            // If anything goes wrong, just keep the default icon.
        }
    }
}
