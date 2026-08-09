# aud.io

**Every device. Every app. One mixer.**

aud.io is a free Windows 10/11 audio mixer. It automatically discovers active speakers, headphones, Bluetooth devices, monitors, USB interfaces, and applications that create audio sessions.

## What works

- Automatic output-device discovery
- Master volume and mute for every output
- One-click default output switching
- Automatic per-app audio-session discovery
- Separate app volume and mute controls
- Minimal, DJ Neon, and Compact designs
- System-tray operation; closing the window keeps the mixer running
- Automatic refresh when apps and devices appear or disappear

## Run from source

Install the free [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then run:

```powershell
dotnet restore
dotnet run
```

## Make a downloadable app

Push this folder to a public GitHub repository. Every push is compile-checked. To publish a downloadable version, create and push a tag such as `v0.1.0`; GitHub will build `aud.io.exe` and attach a zip to a new GitHub Release.

```powershell
git tag v0.1.0
git push origin v0.1.0
```

## Current limitation

Windows exposes per-app volume and mute through its public Core Audio API. Assigning a specific app to a different physical output uses a separate Windows routing system and is planned for a later release.

## License

MIT — free to use, modify, and share.
