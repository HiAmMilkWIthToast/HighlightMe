# HighlightMe

A WPF desktop app for quickly finding and highlighting files on your Windows desktop.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Desktop-0078D4?style=flat&logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

## What it does

Search for files on your desktop and get visual feedback showing you exactly where they are. The app highlights matching files with a glow effect and bouncing arrows so you can spot them instantly.

**Search features:**
- Real-time filtering as you type
- Search inside text files (< 1MB)
- Search history

**Visual highlights:**
- Glow effect around matching desktop icons
- Bouncing arrows pointing to files
- Auto-highlight for newly added files

**File management:**
- Preview images and text files
- Add notes to files
- Lock files to prevent deletion
- Hide/show files
- Organize with color categories

**Themes:** Dark, Light, Ocean, Forest, Sunset

## Getting Started

**Requirements:**
- Windows 10/11
- [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

**Build from source:**
```bash
git clone https://github.com/HiAmMilkWIthToast/HighlightMe.git
```
Open `HighlightMe.csproj` in Visual Studio 2022+ and build.

## How to use

- Type in the search box to find files
- Toggle desktop glow, arrows, or new item detection from the toolbar
- Right-click any file for actions (open, preview, copy path, add note, lock, hide)
- Double-click to open
- Click the gear icon for theme and layout settings

## Project layout

```
HighlightMe/
├── Views/          # XAML windows and dialogs
├── ViewModels/     # MVVM view models
├── Models/         # Data models
├── Services/       # Business logic
├── Themes/         # Theme resources
├── Converters/     # Value converters
└── Helpers/        # Utilities
```

## Built with

- WPF (.NET 8.0)
- MVVM pattern

## License

MIT - see [LICENSE](LICENSE)

## Contributing

1. Fork the repo
2. Create a feature branch
3. Make your changes
4. Open a PR

Make sure `bin/`, `obj/`, and `.vs/` are in your `.gitignore` (they should be already).

---

Questions or issues? Open an issue on GitHub.
