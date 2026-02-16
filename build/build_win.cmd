@echo off
setlocal enabledelayedexpansion

:: Configuration
set "PROJECT_NAME=YoutubeDownloader"
set "PROJECT_PATH=YoutubeDownloader.UI\YoutubeDownloader.UI.csproj"
set "BINARY_NAME=YoutubeDownloader.UI"
set "VERSION_FILE=VERSION"

:: Arguments
set "RID=%1"
if "%RID%"=="" set "RID=win-x64"

set "OUTPUT_DIR=%2"
if "%OUTPUT_DIR%"=="" set "OUTPUT_DIR=release\%RID%"

:: Get Version
if exist "%VERSION_FILE%" (
    set /p APP_VERSION=<"%VERSION_FILE%"
) else (
    set "APP_VERSION=0.0.0"
)

echo Building %PROJECT_NAME% v%APP_VERSION% for %RID%...

:: Update app.manifest version
echo Updating app.manifest version...
powershell -Command "(Get-Content 'YoutubeDownloader.UI/app.manifest') -replace 'assemblyIdentity version=\"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\"', 'assemblyIdentity version=\"%APP_VERSION%.0\"' | Set-Content 'YoutubeDownloader.UI/app.manifest'"

:: Clean previous build (only if default output)
if "%OUTPUT_DIR%"=="." (
    if exist "%PROJECT_NAME%.exe" (
        del "%PROJECT_NAME%.exe"
        del "%PROJECT_NAME%.pdb"
    )
)

:: Restore explicitly to avoid ambiguity issues
dotnet restore "%PROJECT_PATH%" -r %RID%
if %ERRORLEVEL% NEQ 0 (
    echo Restore failed!
    exit /b %ERRORLEVEL%
)

:: Build and Publish
set "PUBLISH_DIR=%OUTPUT_DIR%\publish_tmp"
dotnet publish "%PROJECT_PATH%" ^
    --no-restore ^
    -c Release ^
    -r %RID% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:DebugType=embedded ^
    -p:Version=%APP_VERSION% ^
    -o "%PUBLISH_DIR%"

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

:: Move executable and clean up
if exist "%PUBLISH_DIR%\%BINARY_NAME%.exe" (
    move /Y "%PUBLISH_DIR%\%BINARY_NAME%.exe" "%OUTPUT_DIR%\%PROJECT_NAME%.exe" >nul
)

if exist "%PUBLISH_DIR%" (
    rmdir /s /q "%PUBLISH_DIR%"
)

echo.
echo Build successful! Executable: %OUTPUT_DIR%\%PROJECT_NAME%.exe
