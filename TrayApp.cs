using System.Drawing;
using OWTrackerDesktop.Services;

namespace OWTrackerDesktop;

public class TrayApp : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly OWWebSocketServer _webSocketServer;
    private readonly GameMonitor _gameMonitor;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _toggleItem;

    public TrayApp()
    {
        _statusItem = new ToolStripMenuItem("Status: Initializing...") { Enabled = false };
        _toggleItem = new ToolStripMenuItem("Pause Monitoring");
        _toggleItem.Click += OnToggleMonitoring;

        var aboutItem = new ToolStripMenuItem("About");
        aboutItem.Click += OnAbout;

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExit;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_statusItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_toggleItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(aboutItem);
        contextMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "OW Tracker Desktop",
            ContextMenuStrip = contextMenu
        };
        _trayIcon.DoubleClick += (_, _) => ShowStatus();

        _webSocketServer = new OWWebSocketServer(8080);
        _gameMonitor = new GameMonitor(_webSocketServer, pollIntervalMs: 2000);
        _gameMonitor.StateChanged += (_, _) => UpdateStatus();

        StartServices();
    }

    private void StartServices()
    {
        try
        {
            _webSocketServer.Start();
            _gameMonitor.Start();
            UpdateStatus();

            _trayIcon.ShowBalloonTip(
                3000,
                "OW Tracker Desktop",
                $"Monitoring active. Server: {_webSocketServer.LocalIP}:{_webSocketServer.Port}",
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start services:\n{ex.Message}",
                "OW Tracker Desktop - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            ExitThread();
        }
    }

    private void UpdateStatus()
    {
        bool monitoring = _gameMonitor.IsMonitoring;
        bool mobileConnected = _webSocketServer.GetConnectedClientCount() > 0;

        _statusItem.Text = $"Status: {(monitoring ? "Active" : "Paused")} | {(mobileConnected ? "Mobile connected" : "Mobile disconnected")}";
        _trayIcon.Text = $"OW Tracker Desktop\n" +
                         $"{(monitoring ? "Monitoring" : "Paused")} | {(mobileConnected ? "Mobile connected" : "Mobile disconnected")}\n" +
                         $"{_webSocketServer.LocalIP}:{_webSocketServer.Port}";
    }

    private void OnToggleMonitoring(object? sender, EventArgs e)
    {
        if (_gameMonitor.IsMonitoring)
        {
            _gameMonitor.Stop();
            _toggleItem.Text = "Resume Monitoring";
            _trayIcon.ShowBalloonTip(2000, "Paused", "Game monitoring paused", ToolTipIcon.Info);
        }
        else
        {
            _gameMonitor.Start();
            _toggleItem.Text = "Pause Monitoring";
            _trayIcon.ShowBalloonTip(2000, "Resumed", "Game monitoring active", ToolTipIcon.Info);
        }
        UpdateStatus();
    }

    private void ShowStatus()
    {
        string ip = _webSocketServer.LocalIP;
        int port = _webSocketServer.Port;
        bool mobileConnected = _webSocketServer.GetConnectedClientCount() > 0;
        bool monitoring = _gameMonitor.IsMonitoring;

        MessageBox.Show(
            $"OW Tracker Desktop v1.0\n\n" +
            $"Server Address: {ip}:{port}\n" +
            $"{(mobileConnected ? "Mobile App Connected" : "Mobile App Disconnected")}\n" +
            $"Monitoring: {(monitoring ? "Active" : "Paused")}\n\n" +
            $"Connect your mobile app to: {ip}:{port}",
            "OW Tracker Desktop - Status",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
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
        _gameMonitor.Stop();
        _webSocketServer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        ExitThread();
    }
}
