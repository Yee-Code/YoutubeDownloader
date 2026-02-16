# YouTube Downloader

A cross-platform YouTube video downloader built with .NET 10 and `yt-dlp`, with both Avalonia UI and CLI modes.
![img.png](img.png)

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
- **Error: `yt-dlp` not found**
  - Ensure `yt-dlp` is installed and you can run `yt-dlp --version` in the terminal.
  - Make sure its directory is added to the system's PATH environment variable.

- **Video has only audio or only video?**
  - This is usually due to missing `ffmpeg`. `yt-dlp` requires `ffmpeg` to merge high-quality video and audio streams. Please install `ffmpeg` to resolve this.

- **Some videos fail/throttle**
  - Try to install Node.js.

- **Slow download speed?**
  - This may depend on YouTube server limits or your network conditions. The program uses `yt-dlp` default configurations.

- **App is damaged and can't be opened? (macOS)**
  - This is a common macOS security feature for apps not signed by an identified developer.
  - To fix this, run the following command in Terminal:
    ```bash
    sudo xattr -rd com.apple.quarantine /Applications/YoutubeDownloader.app
    ```

