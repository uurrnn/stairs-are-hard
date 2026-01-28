# CLAUDE.md

This file provides guidance for Claude Code when working with this project.

## Project Overview

**Stairs Are Hard** is a Unity 6 physics-based wheelchair racing game. Players control a wheelchair through obstacle courses featuring stairs, ramps, and checkpoints, racing to reach the goal zone.

## Architecture

The codebase uses a **modular, event-driven architecture** with three namespaces:

- `WheelchairRacing.Player` - Input and movement (`Assets/Scripts/Player/`)
- `WheelchairRacing.Level` - Level mechanics (`Assets/Scripts/Level/`)
- `WheelchairRacing.Core` - Game state and UI (`Assets/Scripts/Core/`)

**Key Patterns:**
- Singleton pattern for `GameManager` and `LevelManager`
- Component-based MonoBehaviours attached to GameObjects
- Event system for state changes (`OnGameStateChanged`, `OnTimerUpdated`, etc.)

## Key Files

| File | Purpose |
|------|---------|
| `Core/GameManager.cs` | Central game state, timer, respawning, checkpoint tracking |
| `Core/LevelManager.cs` | Level data, spawn points, progress tracking |
| `Player/WheelchairController.cs` | Rigidbody physics: forces, drag, slopes, stability |
| `Player/WheelchairInput.cs` | Input System handling for keyboard/gamepad |
| `Player/WheelchairCamera.cs` | Third-person follow camera with momentum delay |
| `Level/StairCollider.cs` | Stair physics with ramp overlay and bump effects |
| `Level/RampCollider.cs` | Ramp types: Standard, Boost, Slippery, Sticky |
| `Level/GoalZone.cs` | Level completion trigger |
| `Level/Checkpoint.cs` | Progress save points |
| `Core/SceneSetupHelper.cs` | Editor utility for automated scene creation |

## Build & Run

This is a Unity project - open it in Unity 6 Editor:

1. Open Unity Hub and load the project
2. Ensure Input System and TextMeshPro packages are installed
3. Open a scene or use `SceneSetupHelper` to create one
4. Press Play to test

## Code Style

- C# with Unity conventions
- MonoBehaviour components with `[SerializeField]` for Inspector exposure
- `[Header("Section")]` attributes to organize Inspector fields
- Regions (`#region`) to organize code sections
- XML documentation comments on public methods
- Assembly definition (`WheelchairRacing.asmdef`) groups all scripts

## Common Tasks

**Adding a new level mechanic:**
1. Create script in `Assets/Scripts/Level/`
2. Use namespace `WheelchairRacing.Level`
3. Inherit from `MonoBehaviour`
4. Use trigger colliders with `OnTriggerEnter`/`OnTriggerStay`
5. Reference `GameManager.Instance` for game state interactions

**Modifying physics:**
- Edit `WheelchairController.cs`
- Physics uses `Rigidbody.AddForce()` and `AddTorque()` in `FixedUpdate`
- Ground detection via `Physics.Raycast`

**Adding UI elements:**
- Edit `GameUI.cs` in `Assets/Scripts/Core/`
- Uses Unity UI with TextMeshPro components
- Subscribe to GameManager events for updates

## Dependencies

- Unity Input System (required)
- TextMeshPro (required)
- Cinemachine (optional)

## Testing

No automated test suite. Test by running in Unity Editor:
- WASD/Arrows to move
- Space to brake
- R to restart
- Escape to pause
