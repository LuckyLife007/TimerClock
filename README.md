# TimerClock

A professional dual-purpose application that combines countdown timer and clock functionality with a clean, customizable display interface. Perfect for presentations, time management, and any scenario requiring visible time tracking.

## Features

### Display Modes
- **Timer Mode**: Configurable countdown timer (1-200 minutes)
- **Clock Mode**: Real-time digital clock display
- **Auto Mode**: Smart switching between timer and clock displays

### Visual Alerts
- Warning indicators when timer approaches completion (customizable threshold)
- Negative time tracking with visual feedback
- Synchronized animations across control and display windows

### Customization
- Adjustable opacity for both control panel and display window
- Always-on-top option for both windows
- Configurable auto-switch interval in Auto mode
- Drag-and-drop window positioning
- Window state management (minimize, maximize, restore)

## Release Process

To build and release the application:

### Method 1: Using Visual Studio

1. Open the solution in Visual Studio
2. Change the build configuration to "Release"
3. Build the solution (Build > Build Solution)
4. The release build will be available in the `bin\Release\net8.0-windows` directory

### Method 2: Using PowerShell Script

1. Open PowerShell as Administrator
2. Navigate to the project directory
3. Run the CreateRelease.ps1 script: `.\CreateRelease.ps1`
4. This will create a self-contained release in the `Release\PublishOutput` directory
5. A basic installer package will be created as a ZIP file in the `Release` directory

### Method 3: Create a Professional Installer

1. Install Inno Setup (https://jrsoftware.org/isinfo.php)
2. Build the release version using Method 1 or Method 2
3. Run Inno Setup Compiler and open the `TimerClockSetup.iss` script
4. Compile the script to create an installer executable
5. The installer will be available in the `Release\Installer` directory

## Installation Methods

### Standard Installation (Recommended)
1. Download the latest `TimerClockSetup.exe` from the Releases page
2. Run the installer and follow the on-screen instructions
3. Launch TimerClock from the Start menu or desktop shortcut

### Portable Installation
1. Download the self-contained `TimerClock-[version].zip` package
2. Extract to your preferred location
3. Run `TimerClockApp.exe`

### Developer Installation
1. Clone the repository: `git clone [repository-url]`
2. Open `TimerClockSolution.sln` in Visual Studio
3. Build and run the solution

## System Requirements

### Minimum Requirements
- Operating System: Windows 10 or newer
- Architecture: x64
- Memory: 50MB RAM
- Storage: 100MB free space

### Development Requirements
- Visual Studio 2022 or newer
- .NET 8.0 SDK
- Windows 10/11 SDK
- Inno Setup (optional, for building installer)

## User Guide

### Basic Usage
1. Launch TimerClock
2. Set desired timer duration (1-200 minutes)
3. Choose mode:
   - Timer: Countdown mode
   - Clock: Real-time display
   - Auto: Alternates between modes

### Customization
- Access settings via the gear icon
- Adjust opacity, warning times, and display preferences
- Position windows as needed with drag-and-drop

### Keyboard Shortcuts
- Space: Play/Pause timer
- Esc: Reset timer
- Alt+Enter: Toggle fullscreen (display window)

## Support

For support, bug reports, or feature requests:
1. Check the [Issues](issues) page
2. Create a new issue with detailed information
3. Follow the contributing guidelines

## License

This project is licensed under the MIT License - see [LICENSE.txt](LICENSE.txt) for details.

## Acknowledgments

- Contributors and maintainers
- .NET Community
- User feedback and suggestions