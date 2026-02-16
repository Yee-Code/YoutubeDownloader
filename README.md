# YouTube Downloader

A cross-platform YouTube video downloader built with .NET 10 and `yt-dlp`, with both Avalonia UI and CLI modes.

## Architecture (Refactored)

The project is now split with SOLID-oriented boundaries:

- `YoutubeDownloader.Core/Abstractions`: interfaces (`IDownloadClient`, `IDependencyService`, `IProcessRunner`, `IExecutableLocator`, `IProgressParser`)
- `YoutubeDownloader.Core/Infrastructure`: OS/process adapters (`SystemProcessRunner`, `SystemExecutableLocator`)
- `YoutubeDownloader.Core/Services`: business/application services (`DownloadClient`, `DependencyService`, `DefaultProgressParser`, `DependencyGuidance`)
- `YoutubeDownloader.Core/Composition`: default wiring (`CoreServiceFactory`)
- `YoutubeDownloader.UI`: presentation layer only (ViewModel + UI), depends on abstractions

This design enables easier future extension:

- swap process execution strategy without touching UI
- replace `yt-dlp` engine with a different provider
- add platform-specific behavior by implementing abstractions
- improve testability with fake implementations

## Features

- Best-quality download via `bestvideo+bestaudio/best`
- MP4 merge via `ffmpeg`
- Real-time progress + ETA parsing
- Dependency detection (`yt-dlp`, `ffmpeg`, `node`)
- Privacy-by-default settings persistence (URL not saved)
- Cross-platform: Windows/macOS/Linux

## Prerequisites

1. [.NET 10 SDK](https://dotnet.microsoft.com/download)
2. [yt-dlp](https://github.com/yt-dlp/yt-dlp)
3. [ffmpeg](https://ffmpeg.org/) (recommended)
4. Node.js (recommended for some sites/scenarios)

Examples:

- macOS: `brew install yt-dlp ffmpeg node`
- Windows: `winget install yt-dlp Gyan.FFmpeg OpenJS.NodeJS`

## Build

```bash
dotnet build YoutubeDownloader.sln
```

## Run

UI:

```bash
dotnet run --project YoutubeDownloader.UI/YoutubeDownloader.UI.csproj
```

CLI:

```bash
dotnet run --project YoutubeDownloader.UI/YoutubeDownloader.UI.csproj -- "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
```

## Tests

```bash
dotnet test YoutubeDownloader.sln --nologo
```

## Troubleshooting

- `yt-dlp` not found: verify `yt-dlp --version` works in terminal.
- video/audio merge failed: check `ffmpeg` installation.
- some videos fail/throttle: install Node.js.
