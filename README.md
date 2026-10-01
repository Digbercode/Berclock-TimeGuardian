# BerClock

> **A modern desktop timekeeper for Windows.**

BerClock is a Windows desktop application that brings alarms, timers, stopwatch tools and world clocks together in a dark interface inspired by ancient/relic aesthetics.

The project combines a practical time-management toolkit with a distinctive gold-and-purple visual identity.

## ✨ Features

- ⏰ **Alarms**
  - Custom labels
  - Alarm groups
  - Weekday scheduling
  - Enable/disable alarms
  - Custom audio or built-in system sounds
  - Configurable duration
  - Fade-in and fade-out
- ⌛ **Timer**
  - Hours, minutes and seconds
  - Start, pause and reset controls
- ⏱️ **Stopwatch**
  - Start, pause and reset
  - Millisecond display
- 🌎 **World Clock**
  - Rio de Janeiro
  - New York
  - London
  - Tokyo
- 🖥️ **Windows system tray**
- 🎨 **Custom UI**
  - Dark background
  - Relic-inspired gold accents
  - Purple/dark visual language
- 🔔 **Dedicated alarm window**
  - Full-screen-style presentation
  - Countdown
  - Audio playback
  - Optional fade-out
  - Visual assets/animations

## 🛠️ Technology

- **C#**
- **.NET 10**
- **Windows Forms**
- **WPF** for the alarm presentation window
- **NAudio** for audio playback
- JSON-based application data

## 📁 Project structure

```text
BerClock/
├── Win7AlarmClassic/
│   ├── Models/
│   ├── Services/
│   ├── UI/
│   ├── Assets/
│   ├── Program.cs
│   └── *.csproj
├── docs/
│   ├── screenshot.png
│   └── demo.gif
├── .editorconfig
├── .gitignore
└── README.md
```

> The repository can keep the existing `Win7AlarmClassic` project namespace while the product itself is presented publicly as **BerClock**.

## 🚀 Running locally

### Requirements

- Windows
- .NET 10 SDK
- Visual Studio 2026, JetBrains Rider or another compatible .NET IDE

### Clone

```bash
git clone https://github.com/SEU-USUARIO/BerClock.git
cd BerClock
```

### Build

```bash
dotnet restore
dotnet build
```

### Run

```bash
dotnet run --project Win7AlarmClassic
```

## 📦 Publishing a Windows build

For a self-contained Windows x64 build:

```bash
dotnet publish Win7AlarmClassic -c Release -r win-x64 --self-contained true
```

The resulting files can be distributed as a Windows application package.

## 🎨 Visual identity

BerClock was designed around a relic/ancient-timekeeper concept rather than the usual flat modern clock interface.

Core visual references:

- Background: `#0C0A10`
- Gold: `#E8BE48`
- Highlight gold: `#FFDA69`

The goal is a UI with a more physical, ornamental and collectible feel.

## 🖼️ Screenshots

Place screenshots in `docs/` and reference them here:

```md
![BerClock](docs/screenshot.png)
```

A short animated demonstration can also be added:

```md
![BerClock Demo](docs/demo.gif)
```

## 🗺️ Roadmap

Possible future improvements:

- [ ] More alarm visualization themes
- [ ] Additional world-clock locations
- [ ] Improved animated alarm assets
- [ ] More audio controls
- [ ] Portable distribution package
- [ ] Installer
- [ ] Automatic update mechanism
- [ ] Additional customization options

## 🤝 Development

BerClock is an independently developed desktop project.

Development may involve different programming tools and assistants as part of the development workflow. The repository's source code, decisions and final implementation should be treated according to the actual project history and applicable licenses.

## 📄 License

Choose and add a license before publishing the repository publicly.

If the project contains code, assets or components derived from another project, verify that project's license and attribution requirements first.

---

**BerClock** — *Time, forged in gold.*
