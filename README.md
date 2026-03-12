# OW Tracker Desktop Companion

Automatically detect Overwatch game states and notify the **Overwatch Personal Tracker** (OW Tracker) mobile app over your local network.

## Features

- Automatic queue detection (Searching, Game Found, Match Starting)
- Real-time sync with mobile app via WebSocket
- Runs in the system tray; main window shows server address, status, and display selector
- Local network only — no data leaves your WiFi

## Requirements

- Windows 10 or 11 (64-bit)
- .NET 10 Runtime (included if building from source with the SDK)
- Overwatch
- Overwatch Personal Tracker (OW Tracker) on the same WiFi network
- English language pack installed (for OCR)

## Building from Source

From the project root (`OW_Queue_Tracker`):

```bash
dotnet restore
dotnet build
dotnet run
```

### Publish a single-file executable

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The executable will be in `bin/Release/net10.0-windows10.0.19041.0/win-x64/publish/`.

## Usage

1. Launch the app — it starts in the system tray and opens the main window.
2. The main window shows **Server:** with your PC’s IP and port (e.g. `192.168.1.x:8080`). Use **Display Capture** if you have multiple monitors.
3. Open **Overwatch Personal Tracker** (OW Tracker) on your phone and connect to that address.
4. Launch Overwatch and queue for a match. Your phone receives notifications when:
   - **Searching** — you entered the queue
   - **Game Found** — a match was found
   - **Match Starting** — loading into the game

You can **Minimize to System Tray** and use **Instructions** for step-by-step connection help.

### Tray menu

- **Open** — restore the main window (or double-click the tray icon)
- **Start Monitoring** / **Stop Monitoring** — toggle screen scanning
- **About** — version info
- **Exit** — quit the app

## Troubleshooting

**Overwatch Personal Tracker won’t connect**

- Ensure both devices are on the same WiFi network.
- Check Windows Firewall allows inbound connections on port 8080.
- Use the Server address shown in the main window (or after opening from the tray).

**Game states not detected**

- Overwatch must be in **Borderless Windowed** or **Fullscreen** on the selected display.
- The game language must be set to **English**.
- Screen capture uses relative regions calibrated for 16:9; it works across common resolutions (e.g. 2560×1440).

## How it works

The app captures two small screen regions every 2 seconds and runs Windows built-in OCR on them:

| Region            | What it detects              |
|-------------------|------------------------------|
| Top-center banner | "SEARCHING" / "GAME FOUND"   |
| Center overlay    | "ENTERING PREGAME"           |

When a state change is detected, a JSON message is broadcast to all connected WebSocket clients.

## Privacy

- All communication stays on your local network.
- No internet connection required after installation.
- No data collection or telemetry.

## License

MIT

## Disclaimer

Not affiliated with Blizzard Entertainment.
