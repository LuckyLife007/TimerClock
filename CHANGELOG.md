# Changelog

All notable changes to the TimerClock application will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-10-27

### Added
- **Window Management**
  - Window resize controls (Shrink, Maximize, Restore buttons) for Display Window
  - Window size state tracking and management
  - Improved drag handle functionality for both windows

- **Enhanced Settings**
  - Separate opacity controls for Control Panel and Display Window
  - Independent "Always on Top" toggles for each window
  - Better settings UI with improved layout and value formatting

- **Animation System Improvements**
  - Centralized animation logic using DisplayState enum (Normal, Warning, Negative)
  - Reusable animation styles in Resources/Animations.xaml
  - XAML data triggers for declarative animation control
  - Proper warning animation timing (blinks once every 4 seconds)
  - Continuous pulsing for negative time animation

- **Documentation**
  - Added CLAUDE.md with development guidelines and architecture notes

### Fixed
- **Critical Bug**: Settings dialog requiring two clicks to close (duplicate event handler)
- Warning and negative animations displaying incorrectly (overlapping indicators)
- Animation behavior not matching intended timing specifications
- Code-behind animation logic causing maintenance issues

### Changed
- Moved animation logic from procedural code-behind to declarative XAML triggers
- Consolidated duplicate animation code into single resource dictionary
- Improved animation maintainability and testability

### Removed
- Debug "QQQ" statements from DisplayWindow.xaml.cs (8 instances)
- Duplicate logger implementation in Services/Logger.cs (105 lines)
- Unused using directives from App.xaml.cs
- Excessive empty lines and code cleanup
- Redundant animation code from window classes (290 lines)

## [1.0.0] - 2025-05-11

### Added
- Core Timer Features
  - Countdown timer with range of 1-200 minutes
  - Real-time digital clock display
  - Auto mode with configurable switch interval
  - Last used timer duration persistence
  
- User Interface
  - Dual-window design with separate control panel and display
  - Modern, clean interface with intuitive controls
  - Custom application icon for brand recognition
  
- Visual Feedback
  - Warning animations with customizable threshold (10-300 seconds)
  - Negative time tracking with distinctive visual indication
  - Synchronized animations between control and display windows
  
- Customization Options
  - Independent opacity controls for both windows (0-100%)
  - Always-on-top toggles for each window
  - Drag-and-drop window positioning
  - Window state management (minimize/maximize/restore)
  
- Installation & Distribution
  - Professional installer package
  - Self-contained deployment option
  - Support for Windows 10 and newer

### Technical Implementation
- Built with .NET 8.0
- WPF MVVM architecture for maintainable codebase
- Efficient resource management with proper disposal
- Thread-safe UI updates using dispatcher
- Persistent user settings