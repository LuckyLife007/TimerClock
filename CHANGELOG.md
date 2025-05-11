# Changelog

All notable changes to the TimerClock application will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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