# YouTube Downloader

A cross-platform YouTube video downloader built with .NET 10, integrating the power of `yt-dlp`. It offers a clean and modern Graphical User Interface (Avalonia UI) and an efficient Command Line Interface (CLI).

## Key Features

- **High Quality Downloads**: Automatically selects the best video and audio quality (`bestvideo+bestaudio/best`).
- **Auto-Merge**: Automatically merges video and audio into MP4 format after download.
- **Modern Interface**: Built with Avalonia UI, supporting smooth animations and Acrylic transparency effects (macOS/Windows).
- **Real-time Progress**: Displays download progress bars, estimated time remaining, and detailed logs.
- **Smart Memory**: Automatically remembers the last used download path, window size, and position.
- **Dual Mode Support**: Provides both Graphical User Interface (GUI) and Command Line Interface (CLI).
- **Cross-Platform**: Fully supports Windows, macOS, and Linux.

## Prerequisites

Before running this application, ensure your system has the following components installed:

1.  **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (Required runtime environment)
2.  **[yt-dlp](https://github.com/yt-dlp/yt-dlp)** (Core download engine, must be added to the system PATH)
    - **macOS**: `brew install yt-dlp`
    - **Windows/Linux**: Refer to the official installation guide.
3.  **[ffmpeg](https://ffmpeg.org/)** (Highly recommended for merging high-quality video and audio)
    - **macOS**: `brew install ffmpeg`
    - **Windows**: `winget install ffmpeg` or install manually and add to PATH.

## Build and Run

### 1. Clone the Repository

```bash
git clone https://github.com/Yee-Code/YoutubeDownloader.git
cd YoutubeDownloader
```

### 2. Build the Project

```bash
dotnet build YoutubeDownloader.sln
```

### 3. Run the Application

#### Desktop UI (Recommended)

Suitable for general users, providing an intuitive experience.

```bash
dotnet run --project YoutubeDownloader.UI/YoutubeDownloader.UI.csproj
```

**Features:**
- Paste a YouTube URL directly to download.
- Use the `...` button to select the download location.
- Supports window dragging and resizing, and automatically remembers your preferences.

#### Command Line Interface (CLI)

Suitable for script automation or terminal users.

```bash
dotnet run --project YoutubeDownloader.CLI/YoutubeDownloader.CLI.csproj
```

**Instructions:**
- Enter a YouTube URL after startup.
- If no output path is specified, it defaults to the current directory.
- Type `exit` to quit.

## Troubleshooting

- **Error: `yt-dlp` not found**
  - Ensure `yt-dlp` is installed and you can run `yt-dlp --version` in the terminal.
  - Make sure its directory is added to the system's PATH environment variable.

- **Video has only audio or only video?**
  - This is usually due to missing `ffmpeg`. `yt-dlp` requires `ffmpeg` to merge high-quality video and audio streams. Please install `ffmpeg` to resolve this.

- **Slow download speed?**
  - This may depend on YouTube server limits or your network conditions. The program uses `yt-dlp` default configurations.
- **App is damaged and can't be opened? (macOS)**
  - This is a common macOS security feature for apps not signed by an identified developer.
  - To fix this, run the following command in Terminal:
    ```bash
    sudo xattr -rd com.apple.quarantine YoutubeDownloader.app
    ```

