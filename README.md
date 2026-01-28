# Stairs Are Hard

A physics-based wheelchair racing game built in Unity 6. Navigate obstacle courses, conquer stairs and ramps, and reach the goal in the fastest time possible.

## Features

- **Realistic Physics**: Momentum-based movement with slope gravity, drag, and stability systems
- **Stair Mechanics**: Smooth collision handling with tactile bump feedback
- **Multiple Ramp Types**: Standard, Boost, Slippery, and Sticky surfaces
- **Checkpoint System**: Progress saving with respawn points
- **Dual Input Support**: Keyboard (WASD/Arrows) and Gamepad
- **Dynamic Camera**: Momentum-delayed follow camera with collision avoidance

## Requirements

- Unity 6 (3D project)
- Input System package
- TextMeshPro package
- Cinemachine (optional)

## Quick Start

### Automated Setup (Recommended)

1. Create a new Unity 6 3D project
2. Copy the `Assets/` folder into your project
3. Install required packages via Package Manager
4. Create a new scene
5. Add an empty GameObject with the `SceneSetupHelper` script
6. Right-click the component and select **"Setup Complete Scene"**

This automatically creates: Player, Camera, Managers, Test Level, and UI.

### Manual Setup

See [SETUP.txt](SETUP.txt) for detailed manual configuration instructions.

## Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrow Keys | Left Stick |
| Brake | Space | A Button |
| Restart | R | Start |
| Pause | Escape | Select |

## Project Structure

```
Assets/Scripts/
├── Player/
│   ├── WheelchairController.cs   # Physics and movement
│   ├── WheelchairInput.cs        # Input handling
│   └── WheelchairCamera.cs       # Follow camera
├── Level/
│   ├── StairCollider.cs          # Stair physics
│   ├── RampCollider.cs           # Ramp effects
│   ├── GoalZone.cs               # Level completion
│   ├── FallZone.cs               # Out-of-bounds respawn
│   └── Checkpoint.cs             # Progress checkpoints
├── Core/
│   ├── GameManager.cs            # Game state management
│   ├── LevelManager.cs           # Level data and spawns
│   ├── GameUI.cs                 # UI display
│   └── SceneSetupHelper.cs       # Editor setup utility
├── WheelchairControls.inputactions
└── WheelchairRacing.asmdef
```

## Physics Configuration

Adjustable via Inspector on the WheelchairController component:

| Property | Default | Description |
|----------|---------|-------------|
| Forward Force | 30 | Acceleration power |
| Turn Torque | 15 | Turning speed |
| Max Speed | 20 | Speed cap (m/s) |
| Ground Drag | 2 | Friction on ground |
| Air Drag | 0.5 | Friction in air |
| Slope Gravity Multiplier | 1.5 | Extra gravity on slopes |

## Game States

- **Countdown**: 3-second pre-game countdown
- **Playing**: Active gameplay with running timer
- **Paused**: Time frozen, pause menu visible
- **Won**: Level completed, shows completion screen

## Creating Levels

1. Design your geometry (platforms, stairs, ramps)
2. Add `StairCollider` or `RampCollider` scripts to obstacles
3. Place `Checkpoint` triggers along the path
4. Add a `GoalZone` trigger at the end
5. Add a `FallZone` trigger below the level
6. Configure spawn points in `LevelManager`

## Namespaces

- `WheelchairRacing.Player` - Player control scripts
- `WheelchairRacing.Level` - Level mechanics
- `WheelchairRacing.Core` - Game management

## License

[Add your license here]
