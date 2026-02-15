@echo off
setlocal enabledelayedexpansion

:: Configuration
set "PROJECT_NAME=YoutubeDownloader"
set "PROJECT_PATH=YoutubeDownloader.UI\YoutubeDownloader.UI.csproj"
set "BINARY_NAME=YoutubeDownloader.UI"
set "VERSION_FILE=VERSION"

:: Get Version
if exist "%VERSION_FILE%" (
    set /p APP_VERSION=<"%VERSION_FILE%"
) else (
    set "APP_VERSION=0.0.0"
)

echo Building %PROJECT_NAME% v%APP_VERSION% for Windows...

:: Clean previous build
if exist "%PROJECT_NAME%.exe" (
    del "%PROJECT_NAME%.exe"
    del "%PROJECT_NAME%.pdb"
)

:: Restore explicitly to avoid ambiguity issues
dotnet restore "%PROJECT_PATH%" -r win-x64
if %ERRORLEVEL% NEQ 0 (
    echo Restore failed!
    exit /b %ERRORLEVEL%
)

:: Build and Publish
dotnet publish "%PROJECT_PATH%" ^
    --no-restore ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:DebugType=embedded ^
    -p:Version=%APP_VERSION% ^
    -o "."

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

:: Rename output
if exist "%BINARY_NAME%.exe" (
    move /Y "%BINARY_NAME%.exe" "%PROJECT_NAME%.exe" >nul
    if exist "%BINARY_NAME%.pdb" move /Y "%BINARY_NAME%.pdb" "%PROJECT_NAME%.pdb" >nul
)

echo.
echo Build successful! Executable: %PROJECT_NAME%.exe
