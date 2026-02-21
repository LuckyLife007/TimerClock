# PowerShell Script to build and prepare the TimerClock application for release
$ErrorActionPreference = "Stop"

# Define paths
$projectPath = ".\TimerClockApp.csproj"
$outputPath = ".\Release"
$publishPath = "$outputPath\PublishOutput"
$installerPath = "$outputPath\Installer"

# Create output directories
Write-Host "Creating output directories..."
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
New-Item -ItemType Directory -Force -Path $publishPath | Out-Null
New-Item -ItemType Directory -Force -Path $installerPath | Out-Null

# Build the project in Release mode
Write-Host "Building project in Release mode..."
# Note: Run this from Visual Studio Developer Command Prompt for MSBuild to work
# msbuild $projectPath /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild

# Alternatively, use dotnet CLI
dotnet publish $projectPath -c Release -o $publishPath --self-contained true -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Create simple installer directory structure
Write-Host "Creating installer package..."
New-Item -ItemType Directory -Force -Path "$installerPath\TimerClock" | Out-Null

# Copy all published files to installer directory
Copy-Item "$publishPath\*" -Destination "$installerPath\TimerClock" -Recurse -Force

# Create shortcut (optional - skip if it fails)
Write-Host "Creating shortcut..."
try {
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$installerPath\TimerClock\TimerClock.lnk")
    $Shortcut.TargetPath = "$installerPath\TimerClock\TimerClockApp.exe"
    $Shortcut.Save()
    Write-Host "Shortcut created successfully."
} catch {
    Write-Host "Warning: Could not create shortcut. Continuing anyway..." -ForegroundColor Yellow
}

# Create zip archive
Write-Host "Creating ZIP archive..."
Compress-Archive -Path "$installerPath\TimerClock\*" -DestinationPath "$outputPath\TimerClock-2.0.1.zip" -Force

Write-Host "Release preparation complete!"
Write-Host "Release files are available at: $outputPath"
Write-Host "Installer ZIP: $outputPath\TimerClock-2.0.1.zip"
Write-Host ""
Write-Host "Note: To create a proper installer, consider using tools like Inno Setup, WiX Toolset, or NSIS." 