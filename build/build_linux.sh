#!/bin/bash

RID=${1:-linux-x64}
APP_NAME="YoutubeDownloader"
OUTPUT_BASE="release/$RID"
VERSION=$(cat VERSION)

# Update app.manifest version
echo "Updating app.manifest version to $VERSION..."
sed -i "s/assemblyIdentity version=\"[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*\"/assemblyIdentity version=\"${VERSION}.0\"/" YoutubeDownloader.UI/app.manifest

# Publish the app
# Publish the app
echo "Publishing for $RID..."
PUBLISH_DIR="$OUTPUT_BASE/publish_tmp"
rm -rf "$OUTPUT_BASE"
dotnet publish YoutubeDownloader.UI/YoutubeDownloader.UI.csproj -c Release -r "$RID" --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded -p:Version=$VERSION -o "$PUBLISH_DIR"

# Move executable and clean up
if [ -f "$PUBLISH_DIR/$APP_NAME.UI" ]; then
    echo "Moving executable..."
    mkdir -p "$OUTPUT_BASE"
    mv "$PUBLISH_DIR/$APP_NAME.UI" "$OUTPUT_BASE/$APP_NAME"
fi
rm -rf "$PUBLISH_DIR"

echo "Build complete: $OUTPUT_BASE/$APP_NAME"
