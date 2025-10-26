# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimerClock is a WPF desktop application that provides countdown timer and digital clock functionality with a split control/display architecture. The app targets .NET 8.0 with Windows-specific WPF UI.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Build for Release (optimized, no debug symbols)
dotnet build -c Release

# Run the application
dotnet run

# Clean build artifacts
dotnet clean
```

## Architecture

### Two-Window Design Pattern

The application uses a dual-window architecture:
- **ControlPanel**: Small control window for user input and timer management
- **DisplayWindow**: Larger display window showing the time in large format

Both windows communicate through a shared `ControlPanelViewModel` that is passed from ControlPanel to DisplayWindow during initialization (see ControlPanel.xaml.cs:26).

### MVVM Pattern with Service Layer

The application follows MVVM architecture with a clear separation:

**View Models:**
- `ControlPanelViewModel`: Central coordination point for all application state, owns the services and manages their lifecycle

**Services (ServiceClasses.cs):**
- `TimerService`: Manages countdown timer state, tick events, and warning/negative states
- `ClockService`: Provides current time updates every second
- `DisplayManager`: Handles display mode switching (Timer/Clock/Auto) and auto-switching logic

**Views:**
- `ControlPanel.xaml/.cs`: Control panel window
- `DisplayWindow.xaml/.cs`: Display window with WPF animations
- `SettingsPanel.xaml/.cs`: Settings dialog

### State Management and Data Flow

1. User interacts with ControlPanel (clicks Start, Pause, etc.)
2. Commands in ControlPanelViewModel update service states
3. Services raise PropertyChanged events
4. ViewModel listens to service events and re-raises PropertyChanged for bound properties
5. Both windows update via data binding to the shared ViewModel

### Display States and Animation

Timer has three visual states (DisplayState.cs):
- **Normal**: Standard countdown display
- **Warning**: Activated when timer drops below warning threshold (configurable via Settings, default 30 seconds)
- **Negative**: Activated when timer counts below zero (continues into negative time)

Visual indicators are controlled through WPF Storyboard animations in DisplayWindow.xaml.cs:58-112.

## Key Components

### ControlPanelViewModel (ControlPanelViewModel.cs)

The central coordinator that:
- Owns service instances (TimerService, ClockService, DisplayManager)
- Provides ICommand implementations for UI actions (Start, PlayPause, Reset, mode switching)
- Listens to PropertyChanged events from services and propagates changes to UI
- Manages proper disposal of services
- Updates DisplayState based on timer conditions (lines 114-135)

### Services Layer (ServiceClasses.cs)

**ISettings Interface:**
Abstracts application settings access to enable testability. DefaultSettings implementation wraps Properties.Settings.Default.

**DispatcherHelper:**
Static helper for safely invoking operations on the UI thread. Handles both test (no Dispatcher) and runtime environments.

**TimerService:**
- Uses System.Timers.Timer with 1-second intervals
- Supports Start, Pause, Resume, Stop, Reset operations
- Calculates IsWarning state based on configurable threshold from settings
- Continues counting into negative time (doesn't stop at zero)
- All state changes wrapped in DispatcherHelper.InvokeOnDispatcher

**ClockService:**
Simple service that updates CurrentTime every second.

**DisplayManager:**
- Manages three display modes: "Timer", "Clock", "Auto"
- In Auto mode, alternates between timer and clock based on AutoSwitchInterval setting
- Raises DisplayTypeChanged event when ShowingClock changes

### Window Management

**ControlPanel (ControlPanel.xaml.cs):**
- Creates and owns DisplayWindow instance (line 26)
- Implements window lifetime management: closing either window shuts down the app (lines 65-74)
- Applies opacity settings with mouse hover transparency effects (lines 55-63)
- Opens SettingsPanel as modal dialog and applies settings on save (lines 38-53)

**DisplayWindow (DisplayWindow.xaml.cs):**
- Receives ViewModel reference via constructor
- Manages warning and negative time animations via Storyboard
- Implements size state management (minimum/normal/maximized) with _normalSize tracking (lines 215-291)
- Updates animations based on ViewModel state changes (lines 114-204)

### Settings System

User-scoped settings stored in Properties/Settings.settings:
- **ControlOpacity/DisplayOpacity**: Window transparency (0.0-1.0)
- **ControlTopmost/DisplayTopmost**: Always-on-top behavior per window
- **WarningTime**: Seconds before zero to activate warning state (10-300)
- **AutoSwitchInterval**: Seconds between timer/clock in Auto mode (5-60)
- **LastTimerMinutes**: Persists last-used timer duration (1-200)

Settings are accessed through ISettings interface for testability.

## Code Patterns and Conventions

### Property Change Notifications

Services and ViewModels implement INotifyPropertyChanged. Pattern:
```csharp
private bool _field;
public bool Property
{
    get => _field;
    private set
    {
        if (_field != value)
        {
            _field = value;
            OnPropertyChanged(nameof(Property));
        }
    }
}
```

### Disposal Pattern

All services implement IDisposable with the standard pattern:
- Store _disposed flag
- Implement protected Dispose(bool disposing)
- Unsubscribe event handlers before disposing resources
- Call GC.SuppressFinalize in public Dispose()

### Thread Safety

All timer callbacks and property changes must be marshalled to UI thread using DispatcherHelper.InvokeOnDispatcher.

### Dependency Injection

Services accept optional ISettings parameter for dependency injection, defaulting to DefaultSettings() for production use.

## Common Development Patterns

### Adding a New Setting

1. Add setting to Properties/Settings.settings (XML)
2. Add property to ISettings interface in ServiceClasses.cs
3. Implement property in DefaultSettings class
4. Add UI controls to SettingsPanel.xaml
5. Wire up load/save in SettingsPanel.xaml.cs
6. Use via _settings field in service that needs it

### Adding a New Command

1. Add ICommand property to ControlPanelViewModel
2. Initialize in constructor: `NewCommand = new RelayCommand(ExecuteMethod)`
3. Bind to UI element in ControlPanel.xaml: `Command="{Binding NewCommand}"`
4. Implement ExecuteMethod in ViewModel

### Adding Animation States

1. Update DisplayState enum if needed (DisplayState.cs)
2. Add animation setup in DisplayWindow.SetupAnimations()
3. Update DisplayWindow.UpdateAnimationStates() to handle new state
4. Update ControlPanelViewModel.UpdateDisplayState() logic

## Important Notes

- Both windows must stay in sync through shared ViewModel - never create separate ViewModels
- Timer continues into negative time by design (doesn't stop at zero)
- Settings changes require window recreation or manual property updates to take effect
- Always use DispatcherHelper for any timer callback operations
- Window Topmost and Opacity settings support per-window configuration
