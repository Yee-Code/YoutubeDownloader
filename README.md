# YouTube Downloader

A simple yet powerful console application for downloading YouTube videos, built with C# and .NET 10.0. This tool acts as a wrapper around the popular `yt-dlp` command-line utility, providing an easy-to-use interface for downloading high-quality videos.

## Features

- **High Quality**: Downloads the best available video and audio quality and merges them.
- **Format**: Automatically merges output into MP4 format.
- **Interactive**: Simple command-line interface to input URLs.
- **Cross-Platform**: Runs on any platform that supports .NET and the required dependencies (Windows, macOS, Linux).

## Prerequisites

Before running this application, you must have the following installed on your system:

1.  **[yt-dlp](https://github.com/yt-dlp/yt-dlp)**: The core engine used for downloading videos.
    *   **macOS**: `brew install yt-dlp`
    *   **Windows/Linux**: Follow instructions on their GitHub page.
2.  **[ffmpeg](https://ffmpeg.org/)** (Recommended): Required for merging separate video and audio streams (best quality).
    *   **macOS**: `brew install ffmpeg`
3.  **.NET 10.0 SDK**: To build and run the application.

## Installation

1.  Clone the repository:
    ```bash
    git clone https://github.com/yourusername/YoutubeDownloader.git
    cd YoutubeDownloader
    ```

2.  Build the project:
    ```bash
    dotnet build
    ```

## Usage

1.  Run the application:
    ```bash
    dotnet run
    ```

2.  When prompted, paste the YouTube URL you want to download:
    ```
    Enter YouTube URL (or 'exit' to quit): https://www.youtube.com/watch?v=example
    ```

3.  The video will be downloaded to the current directory with the title as the filename.

4.  To exit the program, type `exit`.

## Troubleshooting

-   **"yt-dlp is not found"**: Ensure `yt-dlp` is installed and added to your system's PATH.
-   **"ffmpeg is not found"**: If you download high resolutions (1080p+), video and audio might be separate. Install `ffmpeg` to allow the downloader to merge them automatically.
