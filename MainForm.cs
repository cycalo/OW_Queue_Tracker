using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using OWTrackerDesktop.Models;
using OWTrackerDesktop.Services;

namespace OWTrackerDesktop;

public class MainForm : Form
{
    // Dark title bar (Windows 10 20H1+ / Windows 11)
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    private readonly OWWebSocketServer _webSocketServer;
    private readonly GameMonitor _gameMonitor;
    private NotifyIcon _trayIcon = null!;
    private Label _statusLabel = null!;
    private Label _serverLabel = null!;
    private Label _clientsLabel = null!;
    private Label _gameStateLabel = null!;
    private Button _startButton = null!;
    private Button _stopButton = null!;
    private Button _minimizeToTrayButton = null!;
    private bool _isExiting;
    private ComboBox _displayCombo = null!;
    private List<Screen> _screens = null!;

    private ToolStripMenuItem _startTrayMenuItem = null!;
    private ToolStripMenuItem _stopTrayMenuItem = null!;

    // Custom UI elements
    private Panel _statusCard = null!;
    private Panel _connectionCard = null!;
    private Panel _gameStateCard = null!;
    private Panel _controlsCard = null!;
    private Label _statusDot = null!;
    private Label _statusSubtextLabel = null!;
    private Label _mobileDot = null!;
    private Label _gameStateDot = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Icon? _appIcon;

    // Color palette
    private static readonly Color BgDeep = ColorTranslator.FromHtml("#0d1117");
    private static readonly Color BgCard = ColorTranslator.FromHtml("#161b22");
    private static readonly Color BorderCard = ColorTranslator.FromHtml("#30363d");
    private static readonly Color AccentOrange = ColorTranslator.FromHtml("#F99E1A");
    private static readonly Color StatusGreen = ColorTranslator.FromHtml("#3fb950");
    private static readonly Color StatusAmber = ColorTranslator.FromHtml("#d29922");
    private static readonly Color StatusRed = ColorTranslator.FromHtml("#f85149");
    private static readonly Color StatusBlue = ColorTranslator.FromHtml("#58a6ff");
    private static readonly Color TextPrimary = ColorTranslator.FromHtml("#e6edf3");
    private static readonly Color TextSecondary = ColorTranslator.FromHtml("#8b949e");
    private static readonly Color TextMuted = ColorTranslator.FromHtml("#484f58");
    private static readonly Color BtnHover = ColorTranslator.FromHtml("#1f2937");
    private static readonly Color BgCardLight = ColorTranslator.FromHtml("#1c2333");

    public MainForm()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        DoubleBuffered = true;

        Text = "Overwatch Queue Tracker";
        Size = new Size(480, 620);
        MinimumSize = new Size(480, 620);
        MaximumSize = new Size(480, 620);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = BgDeep;
        ForeColor = TextPrimary;
        Font = new Font("Segoe UI", 11f);

        _webSocketServer = new OWWebSocketServer(8080);
        _gameMonitor = new GameMonitor(_webSocketServer, pollIntervalMs: 2000);
        _gameMonitor.StateChanged += (prev, curr) =>
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(UpdateStatus));
            }
        };

        BuildHeader();
        BuildStatusCard();
        BuildConnectionCard();
        BuildGameStateCard();
        BuildControlsCard();
        BuildBottomBar();
        LoadAppIcon();
        BuildTrayIcon();

        Load += OnFormLoad;
        FormClosing += OnFormClosing;
        HandleCreated += OnHandleCreated;
    }

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
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDark, size);
    }

    private void BuildHeader()
    {
        var headerPanel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(480, 70),
            BackColor = Color.Transparent
        };
        headerPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, headerPanel.Width, headerPanel.Height),
                Color.FromArgb(30, AccentOrange), Color.FromArgb(0, AccentOrange),
                LinearGradientMode.Vertical);
            g.FillRectangle(brush, 0, 0, headerPanel.Width, headerPanel.Height);
            using var linePen = new Pen(Color.FromArgb(60, AccentOrange), 1);
            g.DrawLine(linePen, 20, headerPanel.Height - 1, headerPanel.Width - 20, headerPanel.Height - 1);
        };

        _titleLabel = new Label
        {
            Text = "OVERWATCH QUEUE TRACKER",
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        _subtitleLabel = new Label
        {
            Text = "Desktop Companion",
            Font = new Font("Segoe UI", 10f),
            ForeColor = TextSecondary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(26, 42)
        };

        var accentBar = new Panel
        {
            Size = new Size(4, 36),
            Location = new Point(16, 16),
            BackColor = AccentOrange
        };

        headerPanel.Controls.Add(accentBar);
        headerPanel.Controls.Add(_titleLabel);
        headerPanel.Controls.Add(_subtitleLabel);
        Controls.Add(headerPanel);
    }

    private Panel CreateCard(int y, int height)
    {
        var card = new RoundedPanel
        {
            Location = new Point(16, y),
            Size = new Size(432, height),
            BackColor = BgCard,
            BorderColor = BorderCard,
            CornerRadius = 10
        };
        Controls.Add(card);
        return card;
    }

    private void BuildStatusCard()
    {
        _statusCard = CreateCard(80, 60);

        var monitorIcon = new Label
        {
            Text = "\u25CF",
            Font = new Font("Segoe UI", 18f),
            ForeColor = StatusGreen,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 14)
        };
        _statusDot = monitorIcon;

        _statusLabel = new Label
        {
            Text = "Monitoring: —",
            Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 18)
        };

        _statusSubtextLabel = new Label
        {
            Text = "Keep Overwatch visible — do not minimize while searching for a match",
            Font = new Font("Segoe UI", 9f),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 38)
        };

        _statusCard.Controls.Add(monitorIcon);
        _statusCard.Controls.Add(_statusLabel);
        _statusCard.Controls.Add(_statusSubtextLabel);
    }

    private void BuildConnectionCard()
    {
        // Match the visual structure of the Monitoring card:
        // dot on the left, primary line, then a secondary line of text.
        _connectionCard = CreateCard(150, 60);

        _mobileDot = new Label
        {
            Text = "\u25CF",
            Font = new Font("Segoe UI", 18f),
            ForeColor = StatusRed,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 14)
        };

        // Primary line: mobile app connection status (mirrors Monitoring importance)
        _clientsLabel = new Label
        {
            Text = "Mobile App Disconnected",
            Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = TextSecondary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 18)
        };

        // Secondary line: server address beneath it
        _serverLabel = new Label
        {
            Text = "Server: —",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = TextSecondary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 38)
        };

        _connectionCard.Controls.Add(_mobileDot);
        _connectionCard.Controls.Add(_clientsLabel);
        _connectionCard.Controls.Add(_serverLabel);
    }

    private void BuildGameStateCard()
    {
        _gameStateCard = CreateCard(240, 68);

        var sectionLabel = new Label
        {
            Text = "GAME STATE",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 10)
        };

        _gameStateDot = new Label
        {
            Text = "\u25CF",
            Font = new Font("Segoe UI", 14f),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 32)
        };

        _gameStateLabel = new Label
        {
            Text = "Idle",
            Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(38, 34)
        };

        _gameStateCard.Controls.Add(sectionLabel);
        _gameStateCard.Controls.Add(_gameStateDot);
        _gameStateCard.Controls.Add(_gameStateLabel);
    }

    private void BuildControlsCard()
    {
        _controlsCard = CreateCard(318, 140);

        var captureLabel = new Label
        {
            Text = "DISPLAY CAPTURE",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 10)
        };

        _displayCombo = new ComboBox
        {
            Location = new Point(16, 30),
            Size = new Size(400, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10.5f),
            BackColor = BgDeep,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat
        };
        PopulateDisplayCombo();
        _displayCombo.SelectedIndexChanged += OnDisplaySelectionChanged;

        _startButton = CreateStyledButton("Start Monitoring", 16, 68, 195, 36, StatusGreen);
        _startButton.Click += OnStartMonitoring;
        _startButton.Enabled = true;

        _stopButton = CreateStyledButton("Stop Monitoring", 221, 68, 195, 36, StatusRed);
        _stopButton.Click += OnStopMonitoring;
        _stopButton.Enabled = false;

        _minimizeToTrayButton = CreateStyledButton("Minimize to System Tray", 16, 112, 400, 24, TextMuted);
        _minimizeToTrayButton.FlatAppearance.BorderSize = 0;
        _minimizeToTrayButton.Font = new Font("Segoe UI", 9.5f);
        _minimizeToTrayButton.ForeColor = TextSecondary;
        _minimizeToTrayButton.Click += (_, _) => MinimizeToTray();

        _controlsCard.Controls.Add(captureLabel);
        _controlsCard.Controls.Add(_displayCombo);
        _controlsCard.Controls.Add(_startButton);
        _controlsCard.Controls.Add(_stopButton);
        _controlsCard.Controls.Add(_minimizeToTrayButton);
    }

    private Button CreateStyledButton(string text, int x, int y, int w, int h, Color accentColor)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, h),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, accentColor.R, accentColor.G, accentColor.B),
            ForeColor = accentColor,
            Font = new Font("Segoe UI Semibold", 10.5f),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(80, accentColor.R, accentColor.G, accentColor.B);
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B);
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, accentColor.R, accentColor.G, accentColor.B);
        return btn;
    }

    private void BuildBottomBar()
    {
        var instructionsButton = CreateStyledButton("Instructions", 16, 472, 132, 34, TextSecondary);
        instructionsButton.FlatAppearance.BorderColor = BorderCard;
        instructionsButton.ForeColor = TextSecondary;
        instructionsButton.BackColor = Color.FromArgb(10, 255, 255, 255);
        instructionsButton.Click += OnInstructions;

        var aboutButton = CreateStyledButton("About", 160, 472, 132, 34, TextSecondary);
        aboutButton.FlatAppearance.BorderColor = BorderCard;
        aboutButton.ForeColor = TextSecondary;
        aboutButton.BackColor = Color.FromArgb(10, 255, 255, 255);
        aboutButton.Click += OnAbout;

        var exitButton = CreateStyledButton("Exit", 304, 472, 144, 34, StatusRed);
        exitButton.Click += OnExitClick;

        var versionLabel = new Label
        {
            Text = "v1.0  \u2022  Not affiliated with Blizzard Entertainment",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(432, 20),
            Location = new Point(16, 515)
        };

        Controls.Add(instructionsButton);
        Controls.Add(aboutButton);
        Controls.Add(exitButton);
        Controls.Add(versionLabel);
    }

    private void LoadAppIcon()
    {
        string? baseDir = Path.GetDirectoryName(Application.ExecutablePath);
        string iconPath = Path.Combine(baseDir ?? "", "playstore-icon.png");
        if (!File.Exists(iconPath))
            iconPath = Path.Combine(AppContext.BaseDirectory, "playstore-icon.png");
        if (!File.Exists(iconPath))
            iconPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "playstore-icon.png");
        if (!File.Exists(iconPath))
            return;
        try
        {
            using (var bmp = new Bitmap(iconPath))
            {
                _appIcon = (Icon)Icon.FromHandle(bmp.GetHicon()).Clone();
            }
            if (_appIcon != null)
                Icon = _appIcon;
        }
        catch { /* ignore */ }
    }

    private void BuildTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon ?? SystemIcons.Application,
            Text = "Overwatch Queue Tracker",
            Visible = true
        };

        var openItem = new ToolStripMenuItem("Open");
        openItem.Click += (_, _) => RestoreFromTray();

        var startItem = new ToolStripMenuItem("Start Monitoring");
        startItem.Click += (_, _) => { _gameMonitor.Start(); UpdateStatus(); SyncTrayMenu(); };

        var stopItem = new ToolStripMenuItem("Stop Monitoring");
        stopItem.Click += (_, _) => { _gameMonitor.Stop(); UpdateStatus(); SyncTrayMenu(); };

        var aboutItem = new ToolStripMenuItem("About");
        aboutItem.Click += OnAbout;

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExit;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(openItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(startItem);
        contextMenu.Items.Add(stopItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(aboutItem);
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (_, _) => RestoreFromTray();

        _startTrayMenuItem = startItem;
        _stopTrayMenuItem = stopItem;
    }

    private void PopulateDisplayCombo()
    {
        _screens = new List<Screen>(Screen.AllScreens);
        _displayCombo.Items.Clear();
        for (int i = 0; i < _screens.Count; i++)
        {
            var screen = _screens[i];
            string label = screen.Primary
                ? $"Primary - {screen.Bounds.Width}\u00d7{screen.Bounds.Height}"
                : $"Display {i + 1} - {screen.Bounds.Width}\u00d7{screen.Bounds.Height}";
            _displayCombo.Items.Add(label);
        }
        if (_screens.Count > 0)
        {
            _displayCombo.SelectedIndex = 0;
            ScreenCapture.TargetScreen = _screens[0];
        }
    }

    private void OnDisplaySelectionChanged(object? sender, EventArgs e)
    {
        if (_displayCombo.SelectedIndex >= 0 && _displayCombo.SelectedIndex < _screens.Count)
            ScreenCapture.TargetScreen = _screens[_displayCombo.SelectedIndex];
    }

    private void OnFormLoad(object? sender, EventArgs e)
    {
        try
        {
            _webSocketServer.Start();
            _gameMonitor.Start();

            OWWebSocketServer.GetCurrentStateOnConnect = () =>
                new GameStateEvent(_gameMonitor.CurrentState);

            OWWebSocketServer.OnConnectionCountChanged = () =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateStatus));
            };

            UpdateStatus();
            SyncTrayMenu();
            _startButton.Enabled = false;
            _stopButton.Enabled = true;

            // Prevent the display capture dropdown from being focused (and highlighted) on first open
            ActiveControl = _stopButton;

            _trayIcon.ShowBalloonTip(
                3000,
                "Overwatch Queue Tracker",
                $"Monitoring started. Server: {_webSocketServer.LocalIP}:{_webSocketServer.Port}",
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start services:\n{ex.Message}",
                "Overwatch Queue Tracker - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Close();
        }
    }

    private void UpdateStatus()
    {
        bool monitoring = _gameMonitor.IsMonitoring;
        int clientCount = _webSocketServer.GetConnectedClientCount();
        bool mobileConnected = clientCount > 0;
        string ip = _webSocketServer.LocalIP;
        int port = _webSocketServer.Port;
        var state = _gameMonitor.CurrentState;

        // Simple diagnostic logging to help debug connection behaviour
        System.Diagnostics.Debug.WriteLine($"[OW Desktop] Monitoring={monitoring}, ConnectedClients={clientCount}, MobileConnected={mobileConnected}");
        Console.WriteLine($"[OW Desktop] Monitoring={monitoring}, ConnectedClients={clientCount}, MobileConnected={mobileConnected}");

        // Status indicator
        _statusLabel.Text = monitoring ? "Monitoring Active" : "Monitoring Paused";
        _statusLabel.ForeColor = monitoring ? StatusGreen : StatusAmber;
        _statusDot.ForeColor = monitoring ? StatusGreen : StatusAmber;

        // Connection info
        _serverLabel.Text = $"Server:  {ip}:{port}";
        _serverLabel.ForeColor = TextSecondary;

        _clientsLabel.Text = mobileConnected ? "Mobile App Connected" : "Mobile App Disconnected";
        _clientsLabel.ForeColor = mobileConnected ? StatusGreen : StatusRed;
        _mobileDot.ForeColor = mobileConnected ? StatusGreen : StatusRed;

        // Game state with color coding
        (string stateText, Color stateColor) = state switch
        {
            GameState.Searching => ("Searching for game\u2026", StatusBlue),
            GameState.GameFound => ("Game Found!", AccentOrange),
            GameState.MatchStarting => ("Match Starting", StatusGreen),
            GameState.Idle => ("Idle", TextMuted),
            _ => (state.ToString(), TextMuted)
        };

        _gameStateLabel.Text = stateText;
        _gameStateLabel.ForeColor = stateColor;
        _gameStateDot.ForeColor = stateColor;

        // Pulse the game state card border for GameFound
        if (_gameStateCard is RoundedPanel rp)
        {
            rp.BorderColor = state == GameState.GameFound ? AccentOrange :
                             state == GameState.Searching ? StatusBlue :
                             state == GameState.MatchStarting ? StatusGreen : BorderCard;
            rp.Invalidate();
        }

        // Pulse status card border
        if (_statusCard is RoundedPanel sp)
        {
            sp.BorderColor = monitoring ? Color.FromArgb(60, StatusGreen) : BorderCard;
            sp.Invalidate();
        }

        _trayIcon.Text = $"Overwatch Queue Tracker \u2014 {(monitoring ? "Active" : "Paused")} | {(mobileConnected ? "Mobile connected" : "Mobile disconnected")}";
    }

    private void SyncTrayMenu()
    {
        bool monitoring = _gameMonitor.IsMonitoring;
        _startTrayMenuItem.Enabled = !monitoring;
        _stopTrayMenuItem.Enabled = monitoring;
        _startButton.Enabled = !monitoring;
        _stopButton.Enabled = monitoring;
    }

    private void OnStartMonitoring(object? sender, EventArgs e)
    {
        _gameMonitor.Start();
        UpdateStatus();
        SyncTrayMenu();
        _trayIcon.ShowBalloonTip(2000, "Monitoring Started", "Game monitoring is active.", ToolTipIcon.Info);
    }

    private void OnStopMonitoring(object? sender, EventArgs e)
    {
        _gameMonitor.Stop();
        UpdateStatus();
        SyncTrayMenu();
        _trayIcon.ShowBalloonTip(2000, "Monitoring Stopped", "Game monitoring is paused.", ToolTipIcon.Info);
    }

    private void MinimizeToTray()
    {
        Hide();
        _trayIcon.ShowBalloonTip(1500, "Overwatch Queue Tracker", "Running in system tray. Double-click to open.", ToolTipIcon.Info);
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        BringToFront();
        Activate();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isExiting)
            return;
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            MinimizeToTray();
        }
    }

    private void OnInstructions(object? sender, EventArgs e)
    {
        using var form = new InstructionsForm();
        form.ShowDialog(this);
    }

    private void OnAbout(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "Overwatch Queue Tracker v1.0\n\n" +
            "Companion app for Overwatch Personal Tracker phone app (OW Tracker).\n" +
            "Detects Overwatch game states and sends\n" +
            "real-time notifications to your phone.\n\n" +
            "Not affiliated with Blizzard Entertainment.",
            "About Overwatch Queue Tracker",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        ExitApplication();
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        ExitApplication();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        OWWebSocketServer.GetCurrentStateOnConnect = null;
        OWWebSocketServer.OnConnectionCountChanged = null;
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _appIcon?.Dispose();
        _gameMonitor.Stop();
        _webSocketServer.Stop();
        Application.Exit();
    }
}

internal class RoundedPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = ColorTranslator.FromHtml("#30363d");

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 10;

    public RoundedPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreateRoundedRectPath(rect, CornerRadius);

        using var fillBrush = new SolidBrush(BackColor);
        g.FillPath(fillBrush, path);

        using var borderPen = new Pen(BorderColor, 1.2f);
        g.DrawPath(borderPen, path);
    }

    private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
