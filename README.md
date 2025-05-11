# TimerClock Application

A simple application that functions as both a timer and a clock with a display window.

## Features

- Timer mode: Set a countdown timer
- Clock mode: Display the current time
- Auto mode: Toggle between timer and clock
- Warning animation when time is running low
- Negative time indication
- Adjustable opacity settings
- Customizable display options

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

## Installation

Double-click the installer executable and follow the on-screen instructions.

## System Requirements

- Windows 10 or newer
- .NET 8.0 Runtime (included in self-contained deployments)

## License

See LICENSE.txt for details. 