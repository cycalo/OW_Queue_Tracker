using System.Drawing;
using OWTrackerDesktop.Models;
using OWTrackerDesktop.Services;

namespace OWTrackerDesktop;

public class MainForm : Form
{
    private readonly OWWebSocketServer _webSocketServer;
    private readonly GameMonitor _gameMonitor;
    private readonly NotifyIcon _trayIcon;
    private readonly Label _statusLabel;
    private readonly Label _serverLabel;
    private readonly Label _clientsLabel;
    private readonly Label _gameStateLabel;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Button _minimizeToTrayButton;
    private bool _isExiting;
    private readonly ComboBox _displayCombo;
    private List<Screen> _screens = null!;

    public MainForm()
    {
        Text = "OW Tracker Desktop";
        Size = new Size(420, 358);
        MinimumSize = new Size(380, 320);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        _webSocketServer = new OWWebSocketServer(8080);
        _gameMonitor = new GameMonitor(_webSocketServer, pollIntervalMs: 2000);
        _gameMonitor.StateChanged += (prev, curr) =>
        {
            // Marshall updates back onto the UI thread
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(UpdateStatus));
            }
        };

        _statusLabel = new Label
        {
            Text = "Monitoring: —",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        _serverLabel = new Label
        {
            Text = "Server: —",
            Location = new Point(20, 50),
            AutoSize = true
        };

        _clientsLabel = new Label
        {
            Text = "Mobile App Disconnected",
            Location = new Point(20, 78),
            AutoSize = true
        };

        _gameStateLabel = new Label
        {
            Text = "Current state: Idle",
            Location = new Point(20, 106),
            AutoSize = true
        };

        var captureLabel = new Label
        {
            Text = "Display Capture:",
            Location = new Point(20, 132),
            AutoSize = true
        };

        _displayCombo = new ComboBox
        {
            Location = new Point(20, 150),
            Size = new Size(340, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        PopulateDisplayCombo();
        _displayCombo.SelectedIndexChanged += OnDisplaySelectionChanged;

        _startButton = new Button
        {
            Text = "Start Monitoring",
            Location = new Point(20, 178),
            Size = new Size(160, 36),
            Enabled = true
        };
        _startButton.Click += OnStartMonitoring;

        _stopButton = new Button
        {
            Text = "Stop Monitoring",
            Location = new Point(200, 178),
            Size = new Size(160, 36),
            Enabled = false
        };
        _stopButton.Click += OnStopMonitoring;

        _minimizeToTrayButton = new Button
        {
            Text = "Minimize to System Tray",
            Location = new Point(20, 223),
            Size = new Size(340, 36)
        };
        _minimizeToTrayButton.Click += (_, _) => MinimizeToTray();

        var instructionsButton = new Button
        {
            Text = "Instructions",
            Location = new Point(20, 271),
            Size = new Size(100, 32)
        };
        instructionsButton.Click += OnInstructions;

        var aboutButton = new Button
        {
            Text = "About",
            Location = new Point(130, 271),
            Size = new Size(100, 32)
        };
        aboutButton.Click += OnAbout;

        var exitButton = new Button
        {
            Text = "Exit",
            Location = new Point(240, 271),
            Size = new Size(100, 32)
        };
        exitButton.Click += OnExitClick;

        Controls.Add(_statusLabel);
        Controls.Add(_serverLabel);
        Controls.Add(_clientsLabel);
        Controls.Add(_gameStateLabel);
        Controls.Add(captureLabel);
        Controls.Add(_displayCombo);
        Controls.Add(_startButton);
        Controls.Add(_stopButton);
        Controls.Add(_minimizeToTrayButton);
        Controls.Add(instructionsButton);
        Controls.Add(aboutButton);
        Controls.Add(exitButton);

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "OW Tracker Desktop",
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

        Load += OnFormLoad;
        FormClosing += OnFormClosing;
    }

    private ToolStripMenuItem _startTrayMenuItem = null!;
    private ToolStripMenuItem _stopTrayMenuItem = null!;

    private void PopulateDisplayCombo()
    {
        _screens = new List<Screen>(Screen.AllScreens);
        _displayCombo.Items.Clear();
        for (int i = 0; i < _screens.Count; i++)
        {
            var screen = _screens[i];
            string label = screen.Primary
                ? $"Primary - {screen.Bounds.Width}×{screen.Bounds.Height}"
                : $"Display {i + 1} - {screen.Bounds.Width}×{screen.Bounds.Height}";
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

            _trayIcon.ShowBalloonTip(
                3000,
                "OW Tracker Desktop",
                $"Monitoring started. Server: {_webSocketServer.LocalIP}:{_webSocketServer.Port}",
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start services:\n{ex.Message}",
                "OW Tracker Desktop - Error",
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

        string stateText = state switch
        {
            GameState.Searching => "Searching for game…",
            GameState.GameFound => "Game found!",
            GameState.MatchStarting => "Match starting",
            GameState.Idle => "Idle / not in queue",
            _ => state.ToString()
        };

        _statusLabel.Text = $"Monitoring: {(monitoring ? "Active" : "Paused")}";
        _serverLabel.Text = $"Server: {ip}:{port}";
        _clientsLabel.Text = mobileConnected ? "Mobile App Connected" : "Mobile App Disconnected";
        _gameStateLabel.Text = $"Current state: {stateText}";

        _trayIcon.Text = $"OW Tracker Desktop — {(monitoring ? "Active" : "Paused")} | {(mobileConnected ? "Mobile connected" : "Mobile disconnected")}";
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
        _trayIcon.ShowBalloonTip(1500, "OW Tracker Desktop", "Running in system tray. Double-click to open.", ToolTipIcon.Info);
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
            "OW Tracker Desktop v1.0\n\n" +
            "Companion app for Overwatch Personal Tracker (OW Tracker).\n" +
            "Detects Overwatch game states and sends\n" +
            "real-time notifications to your phone.\n\n" +
            "Not affiliated with Blizzard Entertainment.",
            "About OW Tracker Desktop",
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
        _gameMonitor.Stop();
        _webSocketServer.Stop();
        Application.Exit();
    }
}
