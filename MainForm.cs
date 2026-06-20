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
    private PictureBox _connectionQrPicture = null!;
    private ComboBox _qrIpCombo = null!;
    private string _lastRenderedQrUri = "";
    private bool _qrIpComboPopulating;
    private ToolTip _toolTip = null!;
    private Label _clientsLabel = null!;
    private Label _gameStateLabel = null!;
    private Button _startButton = null!;
    private Button _stopButton = null!;
    private Button _minimizeToTrayButton = null!;
    private bool _isExiting;
    private ComboBox _displayCombo = null!;
    private ComboBox _gameLanguageCombo = null!;
    private List<Screen> _screens = null!;

    private ToolStripMenuItem _startTrayMenuItem = null!;
    private ToolStripMenuItem _stopTrayMenuItem = null!;

    // Custom UI elements
    private Panel _statusCard = null!;
    private Panel _connectionCard = null!;
    private Panel _gameStateCard = null!;
    private Panel _controlsCard = null!;
    private Label _statusDot = null!;
    private Label _mobileDot = null!;
    private Label _gameStateDot = null!;
    private Label _gameStateSectionLabel = null!;
    private Label _captureLabel = null!;
    private Label _languageLabel = null!;
    private Label _versionLabel = null!;
    private Button _instructionsButton = null!;
    private Button _aboutButton = null!;
    private Button _exitButton = null!;
    private ToolStripMenuItem _openTrayMenuItem = null!;
    private ToolStripMenuItem _aboutTrayMenuItem = null!;
    private ToolStripMenuItem _exitTrayMenuItem = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Icon? _appIcon;

    private const int WindowWidth = 480;
    private const int WindowHeight = 704;

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

        Text = AppLocalizer.T("window_title");
        Size = new Size(WindowWidth, WindowHeight);
        MinimumSize = new Size(WindowWidth, WindowHeight);
        MaximumSize = new Size(WindowWidth, WindowHeight);
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
        _gameMonitor.PersistentFailure += OnMonitorPersistentFailure;

        _toolTip = new ToolTip { AutoPopDelay = 20000, InitialDelay = 400, ReshowDelay = 200 };

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
            Text = AppLocalizer.T("window_title").ToUpperInvariant(),
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        _subtitleLabel = new Label
        {
            Text = AppLocalizer.T("app_subtitle"),
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
        _statusCard = CreateCard(80, 52);

        var monitorIcon = new Label
        {
            Text = "\u25CF",
            Font = new Font("Segoe UI", 18f),
            ForeColor = StatusGreen,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 12)
        };
        _statusDot = monitorIcon;

        _statusLabel = new Label
        {
            Text = "Monitoring: —",
            Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 16)
        };

        _statusCard.Controls.Add(monitorIcon);
        _statusCard.Controls.Add(_statusLabel);
    }

    private void BuildConnectionCard()
    {
        _connectionCard = CreateCard(140, 132);

        _mobileDot = new Label
        {
            Text = "\u25CF",
            Font = new Font("Segoe UI", 18f),
            ForeColor = StatusRed,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 14)
        };

        _clientsLabel = new Label
        {
            Text = AppLocalizer.T("mobile_disconnected"),
            Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = TextSecondary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 18)
        };

        _serverLabel = new Label
        {
            Text = "Server: —",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = TextSecondary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(44, 38)
        };

        _qrIpCombo = new ComboBox
        {
            Location = new Point(44, 58),
            Size = new Size(252, 28),
            DropDownWidth = 400,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5f),
            BackColor = BgDeep,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat,
            IntegralHeight = false
        };
        _qrIpCombo.SelectedIndexChanged += OnAdvertisedIpComboChanged;

        _connectionQrPicture = new PictureBox
        {
            Location = new Point(302, 10),
            Size = new Size(114, 114),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        _connectionCard.Controls.Add(_mobileDot);
        _connectionCard.Controls.Add(_clientsLabel);
        _connectionCard.Controls.Add(_serverLabel);
        _connectionCard.Controls.Add(_qrIpCombo);
        _connectionCard.Controls.Add(_connectionQrPicture);
    }

    private void BuildGameStateCard()
    {
        _gameStateCard = CreateCard(280, 68);

        var sectionLabel = new Label
        {
            Text = AppLocalizer.T("game_state_section"),
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

        _gameStateSectionLabel = sectionLabel;

        _gameStateCard.Controls.Add(sectionLabel);
        _gameStateCard.Controls.Add(_gameStateDot);
        _gameStateCard.Controls.Add(_gameStateLabel);
    }

    private void BuildControlsCard()
    {
        _controlsCard = CreateCard(356, 200);

        _captureLabel = new Label
        {
            Text = AppLocalizer.T("display_capture"),
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

        _languageLabel = new Label
        {
            Text = AppLocalizer.T("language_label"),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(16, 66)
        };

        _gameLanguageCombo = new ComboBox
        {
            Location = new Point(16, 84),
            Size = new Size(400, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10.5f),
            BackColor = BgDeep,
            ForeColor = TextPrimary,
            FlatStyle = FlatStyle.Flat
        };
        PopulateGameLanguageCombo();
        _gameLanguageCombo.SelectedIndexChanged += OnGameLanguageSelectionChanged;

        _startButton = CreateStyledButton(AppLocalizer.T("start_monitoring"), 16, 120, 195, 36, StatusGreen);
        _startButton.Click += OnStartMonitoring;
        _startButton.Enabled = true;

        _stopButton = CreateStyledButton(AppLocalizer.T("stop_monitoring"), 221, 120, 195, 36, StatusRed);
        _stopButton.Click += OnStopMonitoring;
        _stopButton.Enabled = false;

        _minimizeToTrayButton = CreateStyledButton(AppLocalizer.T("minimize_tray"), 16, 164, 400, 28, TextMuted);
        _minimizeToTrayButton.FlatAppearance.BorderSize = 0;
        _minimizeToTrayButton.Font = new Font("Segoe UI", 9.5f);
        _minimizeToTrayButton.ForeColor = TextSecondary;
        _minimizeToTrayButton.Click += (_, _) => MinimizeToTray();

        _controlsCard.Controls.Add(_captureLabel);
        _controlsCard.Controls.Add(_displayCombo);
        _controlsCard.Controls.Add(_languageLabel);
        _controlsCard.Controls.Add(_gameLanguageCombo);
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
        _instructionsButton = CreateStyledButton(AppLocalizer.T("instructions"), 16, 0, 132, 34, TextSecondary);
        _instructionsButton.FlatAppearance.BorderColor = BorderCard;
        _instructionsButton.ForeColor = TextSecondary;
        _instructionsButton.BackColor = Color.FromArgb(10, 255, 255, 255);
        _instructionsButton.Click += OnInstructions;

        _aboutButton = CreateStyledButton(AppLocalizer.T("about"), 160, 0, 132, 34, TextSecondary);
        _aboutButton.FlatAppearance.BorderColor = BorderCard;
        _aboutButton.ForeColor = TextSecondary;
        _aboutButton.BackColor = Color.FromArgb(10, 255, 255, 255);
        _aboutButton.Click += OnAbout;

        _exitButton = CreateStyledButton(AppLocalizer.T("exit"), 304, 0, 144, 34, StatusRed);
        _exitButton.Click += OnExitClick;

        _versionLabel = new Label
        {
            Text = AppLocalizer.T("version_disclaimer"),
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(432, 20)
        };

        Controls.Add(_instructionsButton);
        Controls.Add(_aboutButton);
        Controls.Add(_exitButton);
        Controls.Add(_versionLabel);
    }

    private void LayoutFooter()
    {
        int buttonY = ClientSize.Height - 88;
        int versionY = ClientSize.Height - 44;

        _instructionsButton.Location = new Point(16, buttonY);
        _aboutButton.Location = new Point(160, buttonY);
        _exitButton.Location = new Point(304, buttonY);
        _versionLabel.Location = new Point(16, versionY);
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
            Text = AppLocalizer.T("window_title"),
            Visible = true
        };

        _openTrayMenuItem = new ToolStripMenuItem(AppLocalizer.T("tray_open"));
        _openTrayMenuItem.Click += (_, _) => RestoreFromTray();

        _startTrayMenuItem = new ToolStripMenuItem(AppLocalizer.T("tray_start"));
        _startTrayMenuItem.Click += (_, _) => { _gameMonitor.Start(); UpdateStatus(); SyncTrayMenu(); };

        _stopTrayMenuItem = new ToolStripMenuItem(AppLocalizer.T("tray_stop"));
        _stopTrayMenuItem.Click += (_, _) => { _gameMonitor.Stop(); UpdateStatus(); SyncTrayMenu(); };

        _aboutTrayMenuItem = new ToolStripMenuItem(AppLocalizer.T("tray_about"));
        _aboutTrayMenuItem.Click += OnAbout;

        _exitTrayMenuItem = new ToolStripMenuItem(AppLocalizer.T("tray_exit"));
        _exitTrayMenuItem.Click += OnExit;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_openTrayMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_startTrayMenuItem);
        contextMenu.Items.Add(_stopTrayMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_aboutTrayMenuItem);
        contextMenu.Items.Add(_exitTrayMenuItem);

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    private void ApplyLocalization()
    {
        Text = AppLocalizer.T("window_title");
        _titleLabel.Text = AppLocalizer.T("window_title").ToUpperInvariant();
        _subtitleLabel.Text = AppLocalizer.T("app_subtitle");
        _gameStateSectionLabel.Text = AppLocalizer.T("game_state_section");
        _captureLabel.Text = AppLocalizer.T("display_capture");
        _languageLabel.Text = AppLocalizer.T("language_label");
        _startButton.Text = AppLocalizer.T("start_monitoring");
        _stopButton.Text = AppLocalizer.T("stop_monitoring");
        _minimizeToTrayButton.Text = AppLocalizer.T("minimize_tray");
        _instructionsButton.Text = AppLocalizer.T("instructions");
        _aboutButton.Text = AppLocalizer.T("about");
        _exitButton.Text = AppLocalizer.T("exit");
        _versionLabel.Text = AppLocalizer.T("version_disclaimer");

        _openTrayMenuItem.Text = AppLocalizer.T("tray_open");
        _startTrayMenuItem.Text = AppLocalizer.T("tray_start");
        _stopTrayMenuItem.Text = AppLocalizer.T("tray_stop");
        _aboutTrayMenuItem.Text = AppLocalizer.T("tray_about");
        _exitTrayMenuItem.Text = AppLocalizer.T("tray_exit");
        _trayIcon.Text = AppLocalizer.T("window_title");

        int displayIndex = _displayCombo.SelectedIndex;
        PopulateDisplayCombo();
        if (displayIndex >= 0 && displayIndex < _displayCombo.Items.Count)
            _displayCombo.SelectedIndex = displayIndex;

        UpdateStatus();
    }

    private void PopulateDisplayCombo()
    {
        _screens = new List<Screen>(Screen.AllScreens);
        _displayCombo.Items.Clear();
        for (int i = 0; i < _screens.Count; i++)
        {
            var screen = _screens[i];
            string label = screen.Primary
                ? $"{AppLocalizer.T("display_primary")} - {screen.Bounds.Width}\u00d7{screen.Bounds.Height}"
                : $"{AppLocalizer.T("display_label", i + 1)} - {screen.Bounds.Width}\u00d7{screen.Bounds.Height}";
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

    private void PopulateGameLanguageCombo()
    {
        var saved = GameLanguageStore.LoadOrDefault();
        _gameLanguageCombo.Items.Clear();

        int selectedIndex = 0;
        for (int i = 0; i < GameLanguageCatalog.All.Count; i++)
        {
            var language = GameLanguageCatalog.All[i];
            _gameLanguageCombo.Items.Add(language.DisplayName);
            if (language.Id == saved.Id)
                selectedIndex = i;
        }

        if (_gameLanguageCombo.Items.Count > 0)
            _gameLanguageCombo.SelectedIndex = selectedIndex;
    }

    private void OnGameLanguageSelectionChanged(object? sender, EventArgs e)
    {
        if (_gameLanguageCombo.SelectedIndex < 0 ||
            _gameLanguageCombo.SelectedIndex >= GameLanguageCatalog.All.Count)
            return;

        var language = GameLanguageCatalog.All[_gameLanguageCombo.SelectedIndex];
        try
        {
            _gameMonitor.SetGameLanguage(language);
            AppLocalizer.SetLanguage(language.Id);
            ApplyLocalization();
        }
        catch (Exception ex)
        {
            _trayIcon.ShowBalloonTip(
                8000,
                AppLocalizer.T("language_pack_title"),
                ex.Message,
                ToolTipIcon.Warning);
            PopulateGameLanguageCombo();
        }
    }

    private void PopulateAdvertisedIpCombo()
    {
        _qrIpComboPopulating = true;
        try
        {
            _qrIpCombo.Items.Clear();
            var choices = NetworkAddressHelper.GetRankedLanIpv4Choices();
            foreach (var c in choices)
                _qrIpCombo.Items.Add(c);

            if (choices.Count == 0)
            {
                _qrIpCombo.Enabled = false;
                return;
            }

            _qrIpCombo.Enabled = true;
            var current = _webSocketServer.AdvertisedLanIP;
            var idx = 0;
            for (var i = 0; i < choices.Count; i++)
            {
                if (choices[i].Address == current)
                    idx = i;
            }

            _qrIpCombo.SelectedIndex = idx;
        }
        finally
        {
            _qrIpComboPopulating = false;
        }
    }

    private void OnAdvertisedIpComboChanged(object? sender, EventArgs e)
    {
        if (_qrIpComboPopulating || _qrIpCombo.SelectedItem is not NetworkAddressHelper.LanIpv4Choice choice)
            return;

        _webSocketServer.TrySetAdvertisedLanIp(choice.Address);
        _lastRenderedQrUri = "";
        UpdateStatus();
    }

    private void OnMonitorPersistentFailure(string message)
    {
        if (!IsHandleCreated || IsDisposed)
            return;
        BeginInvoke(() =>
        {
            _trayIcon.ShowBalloonTip(10000, AppLocalizer.T("monitoring_warning_title"), message, ToolTipIcon.Warning);
        });
    }

    private void OnFormLoad(object? sender, EventArgs e)
    {
        LayoutFooter();

        try
        {
            _webSocketServer.Start();
            if (!_webSocketServer.IsRunning)
            {
                MessageBox.Show(
                    AppLocalizer.T("ws_failed_body"),
                    AppLocalizer.T("ws_failed_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            _gameMonitor.Start();

            OWWebSocketServer.GetCurrentStateOnConnect = () =>
                new GameStateEvent(_gameMonitor.CurrentState);

            OWWebSocketServer.OnConnectionCountChanged = () =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateStatus));
            };

            PopulateAdvertisedIpCombo();
            UpdateStatus();
            SyncTrayMenu();
            _startButton.Enabled = false;
            _stopButton.Enabled = true;

            // Prevent the display capture dropdown from being focused (and highlighted) on first open
            ActiveControl = _stopButton;

            _trayIcon.ShowBalloonTip(
                3000,
                AppLocalizer.T("balloon_startup_title"),
                AppLocalizer.T("balloon_startup_body"),
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                AppLocalizer.T("error_services_body", ex.Message),
                AppLocalizer.T("error_services_title"),
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

        System.Diagnostics.Debug.WriteLine(
            $"[OW Desktop] Monitoring={monitoring}, Clients={clientCount}, Advertised={ip}:{port}, Mobile={(mobileConnected ? "yes" : "no")}");

        // Status indicator
        _statusLabel.Text = monitoring
            ? AppLocalizer.T("monitoring_active")
            : AppLocalizer.T("monitoring_paused");
        _statusLabel.ForeColor = monitoring ? StatusGreen : StatusAmber;
        _statusDot.ForeColor = monitoring ? StatusGreen : StatusAmber;

        _serverLabel.Text = $"{AppLocalizer.T("server_prefix")}  {ip}:{port}";
        _serverLabel.ForeColor = TextSecondary;

        string wsUri = _webSocketServer.GetConnectionWebSocketUri();
        _toolTip.SetToolTip(_connectionQrPicture,
            string.IsNullOrEmpty(wsUri) ? "" : AppLocalizer.T("qr_tooltip"));

        if (wsUri != _lastRenderedQrUri)
        {
            _lastRenderedQrUri = wsUri;
            var previous = _connectionQrPicture.Image;
            _connectionQrPicture.Image = string.IsNullOrEmpty(wsUri)
                ? null
                : DesktopConnectionQrBitmap.Create(wsUri);
            previous?.Dispose();
        }

        _clientsLabel.Text = mobileConnected
            ? AppLocalizer.T("mobile_connected")
            : AppLocalizer.T("mobile_disconnected");
        _clientsLabel.ForeColor = mobileConnected ? StatusGreen : StatusRed;
        _mobileDot.ForeColor = mobileConnected ? StatusGreen : StatusRed;

        // Game state with color coding
        (string stateText, Color stateColor) = state switch
        {
            GameState.Searching => (AppLocalizer.T("state_searching"), StatusBlue),
            GameState.GameFound => (AppLocalizer.T("state_game_found"), AccentOrange),
            GameState.MatchStarting => (AppLocalizer.T("state_match_starting"), StatusGreen),
            GameState.Idle => (AppLocalizer.T("state_idle"), TextMuted),
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

        string status = monitoring ? AppLocalizer.T("tray_status_active") : AppLocalizer.T("tray_status_paused");
        string mobile = mobileConnected
            ? AppLocalizer.T("tray_mobile_connected")
            : AppLocalizer.T("tray_mobile_disconnected");
        _trayIcon.Text = AppLocalizer.T("tray_summary", status, mobile);
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
        _trayIcon.ShowBalloonTip(
            2000,
            AppLocalizer.T("monitoring_started_title"),
            AppLocalizer.T("monitoring_started_body"),
            ToolTipIcon.Info);
    }

    private void OnStopMonitoring(object? sender, EventArgs e)
    {
        _gameMonitor.Stop();
        UpdateStatus();
        SyncTrayMenu();
        _trayIcon.ShowBalloonTip(
            2000,
            AppLocalizer.T("monitoring_stopped_title"),
            AppLocalizer.T("monitoring_stopped_body"),
            ToolTipIcon.Info);
    }

    private void MinimizeToTray()
    {
        Hide();
        _trayIcon.ShowBalloonTip(
            1500,
            AppLocalizer.T("tray_minimized_title"),
            AppLocalizer.T("tray_minimized_body"),
            ToolTipIcon.Info);
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
            PromptExitAndCloseIfConfirmed();
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
            AppLocalizer.T("about_body"),
            AppLocalizer.T("about_title"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        PromptExitAndCloseIfConfirmed();
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        PromptExitAndCloseIfConfirmed();
    }

    private void PromptExitAndCloseIfConfirmed()
    {
        var result = MessageBox.Show(
            AppLocalizer.T("confirm_exit_body"),
            AppLocalizer.T("confirm_exit_title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (result == DialogResult.Yes)
            ExitApplication();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        OWWebSocketServer.ReleaseStaticState();
        _lastRenderedQrUri = "";
        _connectionQrPicture.Image?.Dispose();
        _connectionQrPicture.Image = null;
        _toolTip.Dispose();
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
