# Overwatch Queue Tracker

**Desktop app version:** 1.2

Get a notification on your phone when Overwatch finds a match — even if you're alt-tabbed or away from your PC.

This is a **companion app for Overwatch Personal Tracker (OW Tracker)** on your phone. It is **not** a standalone app and does **not** work without OW Tracker installed.

## What this app does

- Watches the queue banner on your Overwatch screen ("Searching…", "Game Found!", etc.)
- Sends queue updates to OW Tracker on your phone over your local Wi‑Fi
- Triggers a **"Game found!"** notification on your phone so you don't miss a pop

## What this app does NOT do

- Does **not** log match results or update character stats on your phone
- Does **not** know which hero or account you are playing
- Does **not** send anything **after** a match ends
- Does **not** replace the main OW Tracker app — it only alerts you during queue

## Requirements

- Windows 10 or 11 (64‑bit)
- Overwatch installed on your PC
- **Overwatch Personal Tracker (OW Tracker)** on your phone
- Phone and PC on the **same Wi‑Fi network**
- Overwatch text language matching the **Game Language** you pick in this app (install the matching Windows OCR language pack if prompted)

## Quick start

1. Start `OWTrackerDesktop.exe` and click **Start** to begin monitoring.
2. Choose your **network adapter**, **display**, and **Game Language** if needed. Confirm the **Server** line shows your PC's IP and port.
3. On your phone, open OW Tracker → **Desktop** tab → **Scan QR code**, and scan the QR on your PC.
4. Queue in Overwatch. Your phone updates when you are **Searching**, **Game Found**, and **Match Starting**.
5. Keep Overwatch **visible and not minimized** while in queue.

## No "Game found!" notification?

Work through this list:

- **Same Wi‑Fi** — phone and PC must be on the same network (not mobile data or guest Wi‑Fi).
- **QR scanned** — in OW Tracker, open the **Desktop** tab and scan the QR from the PC app.
- **Monitoring active** — the PC app must be running with **Start** pressed; check that **Mobile connected** shows on the PC.
- **Overwatch visible** — do not minimize the game; use **Borderless Windowed** or **Fullscreen**.
- **Correct language** — pick the **Game Language** that matches Overwatch's text language and install the Windows OCR pack if the app asks.
- **Firewall** — allow the app through Windows Firewall if the phone cannot connect.

## Privacy & safety

The PC app reads a small region of your Overwatch window to detect queue text. It does not access your Blizzard account, game files, or other applications. See the VirusTotal scan below if you want a third-party check.

## Download & install

1. Open the latest release and download **`OWTrackerDesktop.exe`**.
   - [OW Queue Tracker Desktop releases](https://github.com/cycalo/OW_Queue_Tracker/releases)
2. Run the app and click **Instructions** for a quick setup guide.

### Virus scan results

[VirusTotal scan results](https://www.virustotal.com/gui/file/f89effe7b823f92dc47d0452d7fcf27a36cb4cb696790f21dff30cc78eaa0c3f/detection)

### Hashes

- **MD5:** `61B96B721954B786CB431685192BAAA6`
- **SHA-256:** `F89EFFE7B823F92DC47D0452D7FCF27A36CB4CB696790F21DFF30CC78EAA0C3F`

## License & disclaimer

- License: **MIT**
- Not affiliated with Blizzard Entertainment.
