# Flutter App – OW Tracker Desktop Integration Plan

**Purpose:** Connect the **Overwatch Personal Tracker** (OW Tracker) mobile app to the OW Tracker Desktop companion (Windows) so the phone receives real-time Overwatch queue notifications, especially **"Game found!"**, and can show local push notifications.

**Prerequisites:** OW Tracker Desktop is running on the user's PC (same WiFi as the phone), with monitoring active and the WebSocket server listening.

---

## 1. Decisions (v1)

These choices apply to the first implementation. They can be revisited later.


| Question                                  | Decision                                                                                                                                                | Notes                                                                                                                                                                                                                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Where does "Connect to desktop" live?** | **A) Dedicated screen** under Settings                                                                                                                  | Add a Settings section (e.g. "Desktop" or "Queue alerts") with a tile that navigates to a dedicated screen (e.g. `/settings/desktop` or `/connect-desktop`). Keeps the connection form, status, last-used address, and reconnect in one place without crowding Settings. |
| **Platform scope**                        | **Mobile only** (Android + iOS) for v1                                                                                                                  | The feature is going to be mobile only on this app (apart from the already in place desktop companion)                                                                                                                                                                   |
| **Reconnection in v1**                    | **Manual only**                                                                                                                                         | User taps Connect again after a disconnect. No auto-retry with backoff in v1; keeps the first implementation simple and predictable. Auto-retry can be added later.                                                                                                      |
| **Notification permission timing**        | **When user first opens the Connect-to-desktop screen** (or taps Connect)                                                                               | Request permission before connecting so the first "Game found!" is never missed because we're mid-permission flow. Proactive is better for this use case.                                                                                                                |
| **Notification title**                    | **"OW Tracker"**                                                                                                                                        | Use a fixed title "OW Tracker" for all desktop-related notifications. Avoids implying official Blizzard/Overwatch branding.                                                                                                                                              |
| **Desktop app and testing**               | Desktop app lives at the project root (`OW_Queue_Tracker`). Manual testing with the built desktop app is the primary way to verify. | Automated tests can use a mock WebSocket server if desired; real-server tests are optional.                                                                                                      |


---

## 2. User experience: linking desktop and mobile

### 2.1 Connection model: **manual entry** (no automatic discovery)

The desktop app does **not** discover or advertise itself to the phone. The user must **enter the PC's address in the Flutter app** so the phone knows where to connect. This keeps the desktop simple and works reliably on home networks without extra setup (no mDNS/Bonjour required).

**Flow:**

1. User starts **OW Tracker Desktop** on the PC (same WiFi as the phone).
2. User sees the **server address** on the desktop (see below).
3. User opens **Overwatch Personal Tracker** (OW Tracker) on their phone and goes to the "Connect to desktop" (or similar) screen.
4. User **enters the IP** (and optionally port) shown on the desktop, then taps **Connect**.
5. OW Tracker connects to `ws://<that_ip>:8080/` and shows connection status. When a game is found, the app shows a notification.

**Optional:** OW Tracker can **save the last-used address** so the user doesn't have to type it every time (useful if the PC's IP doesn't change often).

### 2.2 Where the user sees the address on the desktop

- **Main window:** The desktop app always shows a line like `**Server: 192.168.1.105:8080`** in the main UI (and `**Connected devices: 0**` or the current count). The user can read the IP:port from there or copy it if you add copy support later.
- **Tray:** If the window is minimized to the system tray, double-clicking the tray icon restores the window so the user can see the Server line again. The tray tooltip also shows status and the address.

So the **source of truth** for "what address to enter in the app" is the desktop's **Server** line (e.g. `192.168.1.105:8080`). Overwatch Personal Tracker should accept either:

- **IP only** (e.g. `192.168.1.105`) and assume port **8080**, or  
- **IP and port** (e.g. `192.168.1.105:8080` or separate fields), then build `ws://<ip>:<port>/`.

### 2.3 Overwatch Personal Tracker UX for "Connect to desktop"

- **Screen or section:** A **dedicated screen** (e.g. route `/settings/desktop` or `/connect-desktop`) reached from Settings via a tile like "Desktop" or "Queue alerts" (see **§1 Decisions**). On that screen:
  - A short explanation: "Enter the Server address shown in OW Tracker Desktop on your PC (e.g. 192.168.1.105 or 192.168.1.105:8080). PC and phone must be on the same WiFi."
  - **Input:** One field for "PC address" (IP, or IP:port), or two fields (IP + port, port defaulting to 8080).
  - **Connect** button that opens the WebSocket to `ws://<ip>:8080/` (or the entered port).
  - **Connection status:** "Connecting…", "Connected", "Disconnected", "Error: …" so the user knows whether the link is active.
- **Saved address:** Optionally save the last successful IP (and port) and pre-fill it next time, or offer a "Reconnect" using the last address.
- **Disconnect:** A way to disconnect and, if desired, clear the saved address.

### 2.4 When the address might change

- If the PC gets its IP via **DHCP** (typical at home), the IP can change after a router reboot or PC restart. In that case the user may need to look at the desktop again and re-enter the new address in Overwatch Personal Tracker.
- If the PC has a **static IP** or a **reserved DHCP** address, the same address can be reused and saving it in OW Tracker is especially useful.

### 2.5 No automatic discovery (for now)

- The plan does **not** assume mDNS/Bonjour or any automatic discovery. Connection is **manual entry only**.
- Automatic discovery could be a future enhancement (e.g. desktop advertises a service, phone discovers it); the protocol and message format below stay the same.

---

## 3. Protocol Summary


| Item         | Value                                                            |
| ------------ | ---------------------------------------------------------------- |
| **Protocol** | WebSocket (ws://)                                                |
| **Port**     | 8080 (default; desktop shows it in the Server line)              |
| **Path**     | `/` (root)                                                       |
| **Network**  | Local LAN only; PC and phone must be on the same WiFi            |
| **Server**   | Desktop app binds to the PC's local IP (e.g. `192.168.1.x:8080`) |


**Connection URL format:** `ws://<PC_IP>:8080/`  
Example: `ws://192.168.1.105:8080/`  
The user gets `<PC_IP>` and `8080` from the desktop app's **Server** line (e.g. `Server: 192.168.1.105:8080`).

---

## 4. Message Format (Desktop → Flutter)

All messages are JSON text frames.

### 4.1 On connect (welcome)

Sent once when the Flutter client connects.

```json
{
  "type": "connected",
  "data": {
    "message": "Connected to OW Tracker Desktop"
  },
  "timestamp": "2026-03-09T14:30:00.0000000Z"
}
```

### 4.2 Game state updates

Sent whenever the desktop detects a state change (e.g. user queues, game found, entering pregame).

```json
{
  "type": "searching",
  "data": {
    "state": "Searching",
    "message": "Searching for game..."
  },
  "timestamp": "2026-03-09T14:31:00.0000000Z"
}
```

`**type**` (string, lowercase, no underscores):

- `"connected"` – welcome message on connect
- `"idle"` – not in queue / in menus
- `"searching"` – in queue, waiting for a match
- `"gamefound"` – match found; **use this to trigger the main "Game found!" notification**
- `"matchstarting"` – loading into the game (entering pregame)

`**data.state`** – PascalCase state name (e.g. `"Searching"`, `"GameFound"`).  
`**data.message**` – User-facing message (e.g. `"Game found! Get ready!"`).  
`**timestamp**` – ISO 8601 UTC string.

---

## 5. Flutter Implementation Outline

### 5.1 Dependencies

- Use a WebSocket client package, e.g. `**web_socket_channel**` (or any stable ws client you prefer).
- For showing notifications when the app is in background or closed: `**flutter_local_notifications**` and/or `**firebase_messaging**` (if you already use FCM). For "game found" on the same device, local notifications are enough.
- For **mobile (Android + iOS)** use a WebSocket client and local notifications (see **§1** for platform scope). **Web is out of scope for v1**; gate or omit this feature on web.

### 5.2 Data model

Parse incoming JSON into a simple model, e.g.:

- `type`: string (`"connected"`, `"idle"`, `"searching"`, `"gamefound"`, `"matchstarting"`)
- `data.state`: string (optional)
- `data.message`: string (optional)
- `timestamp`: string (optional)

Handle missing fields safely (desktop may add fields later).

### 5.3 Connection flow

1. **Obtain server address**
  - User gets the address from the **desktop app's main window** (the **Server** line, e.g. `192.168.1.105:8080`).
  - In the Flutter app, user enters the PC IP (and optionally port, default 8080), or the full `ip:port` string. Optionally use a saved/last-used address.
  - Build URL: `ws://<ip>:<port>/` (e.g. `ws://192.168.1.105:8080/`).
2. **Connect**
  - Open WebSocket to `ws://<ip>:8080/`.
  - On open: expect a `type: "connected"` message; optionally show "Connected to desktop" in the UI.
3. **Listen for messages**
  - On each text frame: parse JSON, read `type` and `data.message`.
  - **When `type == "gamefound"`:**
    - Update in-app state (e.g. "Game found" badge/screen).
    - **Show a local push notification** with title like "Overwatch" and body `data.message` (e.g. "Game found! Get ready!") so the user is alerted even if the app is in background.
4. **Reconnection (v1: manual only)**
  - On disconnect or error: show status "Disconnected" / "Error: …". User taps **Connect** again to reconnect (no auto-retry in v1; see **§1**).
5. **Cleanup**
  - Close WebSocket when the user disconnects or leaves the "desktop connection" flow.

### 5.4 Notifications (Game found)

- **Foreground:** Update UI (e.g. banner, dialog, or dedicated "Game found!" screen).
- **Background / app not visible:** Show a system notification (local notification) with:
  - **Title:** **"OW Tracker"** (fixed for all desktop-related notifications; see **§1**).
  - Body: use `data.message` ("Game found! Get ready!").
  - Optional: tap opens the app.

**Permission:** Request notification permission **when the user first opens the Connect-to-desktop screen** (or when they tap Connect), so the first "Game found!" is never missed (see **§1**).

### 5.5 Optional: other states

- `**searching`** – e.g. show "Searching for game…" in the app.
- `**matchstarting**` – e.g. "Match starting soon."
- `**idle**` – e.g. "Ready to queue" or clear previous state.

These are optional; the critical one is `**gamefound**` for the "game found" notification.

---

## 6. Desktop App Reference (for context)

- The desktop app is a separate C# .NET 10 Windows project (OW Tracker Desktop).
- It captures the Overwatch screen, runs OCR, and detects: Idle, Searching, GameFound, MatchStarting.
- When the state changes, it broadcasts the JSON above to all connected WebSocket clients.
- The desktop app lives at the **project root** (`OW_Queue_Tracker`). Manual testing with the built desktop app is the primary way to verify integration; optional automated tests can use a mock WebSocket server (see **§1**).

---

## 7. Checklist for the Flutter project

- **Settings:** Add a section (e.g. "Desktop" or "Queue alerts") with a tile that navigates to the Connect-to-desktop screen (e.g. `/settings/desktop` or `/connect-desktop`).
- **Connect screen:** Form with PC address input (IP or IP:port), Connect button, connection status (Connecting / Connected / Disconnected / Error), optional save last-used address and Reconnect, Disconnect.
- **Request notification permission** when the user first opens the Connect-to-desktop screen (or on first Connect).
- **Notification title:** Use **"OW Tracker"** for desktop-related notifications.
- **Reconnection:** v1 = manual only (user taps Connect again after disconnect).
- **Platform:** Implement on **Android + iOS** only; gate or omit on web for v1.
- Add WebSocket client dependency and parse the JSON message format above.
- On `type: "gamefound"`, update UI and show local notification with title "OW Tracker" and body `data.message`.
- Handle disconnect and errors (network unavailable, desktop not running).
- Optionally handle `searching`, `matchstarting`, `idle` for richer in-app status.

---

## 8. Quick test

1. Run **OW Tracker Desktop** on the PC. In the main window, note the **Server** line (e.g. `Server: 192.168.1.105:8080`).
2. On Overwatch Personal Tracker, open the Connect to desktop screen, enter that IP (and port if not 8080), then tap Connect.
3. You should receive one message with `type: "connected"` and see "Connected" in the app.
4. Queue in Overwatch on the PC; you should receive messages with `type: "searching"`, then `type: "gamefound"` when a match is found.
5. Confirm Overwatch Personal Tracker shows a "Game found!" notification when `type == "gamefound"`.
