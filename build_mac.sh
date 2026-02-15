#!/bin/bash

APP_NAME="YoutubeDownloader"
PUBLISH_DIR="bin/Release/net10.0/osx-arm64/publish"
APP_DIR="$APP_NAME.app"
ICON_SOURCE="YoutubeDownloader.UI/App.ico"
ICON_ICNS="YoutubeDownloader.UI/App.icns"

# Convert ico to temporary pngs and then to icns using iconutil
mkdir -p YoutubeDownloader.UI/App.iconset
ffmpeg -y -i "$ICON_SOURCE" YoutubeDownloader.UI/App.iconset/icon_256x256.png
sips -z 16 16     YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_16x16.png
sips -z 32 32     YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_16x16@2x.png
sips -z 32 32     YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_32x32.png
sips -z 64 64     YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_32x32@2x.png
sips -z 128 128   YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_128x128.png
sips -z 256 256   YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_128x128@2x.png
sips -z 256 256   YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_256x256.png
sips -z 512 512   YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_256x256@2x.png
sips -z 512 512   YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_512x512.png
sips -z 1024 1024 YoutubeDownloader.UI/App.iconset/icon_256x256.png --out YoutubeDownloader.UI/App.iconset/icon_512x512@2x.png

iconutil -c icns YoutubeDownloader.UI/App.iconset -o "$ICON_ICNS"
rm -rf YoutubeDownloader.UI/App.iconset

# Publish the app
echo "Publishing..."
rm -rf "$PUBLISH_DIR"
dotnet publish YoutubeDownloader.UI/YoutubeDownloader.UI.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PUBLISH_DIR"

# Create .app bundle structure
echo "Creating .app bundle..."
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# Copy published files
cp -a "$PUBLISH_DIR/." "$APP_DIR/Contents/MacOS/"

# Copy icon
cp "$ICON_ICNS" "$APP_DIR/Contents/Resources/App.icns"

# Create Info.plist
echo "Creating Info.plist..."
cat > "$APP_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>com.leeyu.$APP_NAME</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>CFBundleExecutable</key>
    <string>YoutubeDownloader.UI</string>
    <key>CFBundleIconFile</key>
    <string>App</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

echo "Build complete: $APP_DIR"
