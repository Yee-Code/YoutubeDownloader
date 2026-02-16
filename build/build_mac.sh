#!/bin/bash

RID=${1:-osx-arm64}
APP_NAME="YoutubeDownloader"
OUTPUT_BASE="release/$RID"
PUBLISH_DIR="$OUTPUT_BASE/publish"
APP_DIR="$OUTPUT_BASE/$APP_NAME.app"
ICON_SOURCE="YoutubeDownloader.UI/App.ico"
ICON_ICNS="YoutubeDownloader.UI/App.icns"
VERSION=$(cat VERSION)

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

# Update app.manifest version
echo "Updating app.manifest version to $VERSION..."
sed -i '' "s/assemblyIdentity version=\"[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*\"/assemblyIdentity version=\"${VERSION}.0\"/" YoutubeDownloader.UI/app.manifest

# Publish the app
echo "Publishing for $RID..."
rm -rf "$PUBLISH_DIR"
dotnet publish YoutubeDownloader.UI/YoutubeDownloader.UI.csproj -c Release -r "$RID" --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded -p:Version=$VERSION -o "$PUBLISH_DIR"

# Create .app bundle structure
echo "Creating .app bundle in $APP_DIR..."
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# Copy published files
# Copy published files
cp "$PUBLISH_DIR/$APP_NAME.UI" "$APP_DIR/Contents/MacOS/"
cp "VERSION" "$APP_DIR/Contents/MacOS/"

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
    <string>$VERSION</string>
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
rm -rf "$PUBLISH_DIR"
