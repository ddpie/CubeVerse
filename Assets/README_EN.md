# CubeVerse Assets Documentation

| English | [中文](README.md) |
|:---:|:---:|

CubeVerse is a feature-rich 3D cube world game developed entirely through natural language conversations using Amazon Q Developer CLI. This documentation covers the organization of game assets and usage of core systems.

## 📁 Asset Structure

```
Assets/
├── Scenes/          # Game scene files
├── Scripts/         # Core script systems
├── Prefabs/         # Prefab resources
├── Resources/       # Runtime resources
│   ├── Sounds/      # Audio files
│   └── showcase.png # Game screenshot
└── README.md        # This documentation
```

## 🎮 Core Systems

### 🌦️ Weather System

#### Rain System (RainSystem & RainManager)
- **Realistic Rain Effects**: Includes raindrop particles and ambient audio
- **Adjustable Parameters**: Intensity, color, size, and density
- **Smart Optimization**: Efficient rendering based on object pooling

#### Snow System (SnowSystem & SnowManager)
- **Beautiful Snow Effects**: Creates winter atmosphere
- **Dynamic Parameters**: Snowflake size, falling speed, density control
- **Season Switching**: Supports real-time weather transitions

#### Lightning System (LightningSystem & LightningManager)
- **Dynamic Lightning Effects**: Enhances storm experience
- **Audio Synchronization**: Lightning accompanied by delayed thunder
- **Smart Triggering**: Automatic random generation during rain, higher frequency at night

#### Rainbow System (RainbowSystem & RainbowManager)
- **Post-Rain Rainbow**: Adds visual beauty and realism
- **Dynamic Rendering**: Automatically shows/hides based on weather conditions

#### Cloud System (CloudGenerator & CloudManager)
- **Dynamic Clouds**: Procedural generation and movement
- **Sky Rendering**: Enhances environmental atmosphere

### 🌅 Day-Night Cycle System (DayNightManager)
- **Smooth Transitions**: Natural day-night transition effects
- **Dynamic Lighting**: Environmental lighting changes with time
- **Star System**: Brilliant starry sky effects at night

### 🎯 Game Core

#### Player Control (PlayerController & PlayerSetup)
- **First-Person Perspective**: Smooth movement and camera control
- **Physics Interaction**: Realistic collision and gravity systems

#### World Generation (CubeGenerator)
- **Procedural Terrain**: Dynamic cube world generation
- **Adjustable Parameters**: Terrain elevation, density, materials

### ⚡ Performance Optimization

#### Object Pool Management (ObjectPoolManager)
- **Efficient Reuse**: Reduces memory allocation and garbage collection
- **Smart Management**: Automatic expansion and recycling mechanisms

#### Quality Settings (QualitySettings)
- **Adjustable Graphics**: Adapts to different performance devices
- **LOD System**: Distance-based level of detail optimization

## 🔧 API Usage Guide

### Weather Control
```csharp
// Rain Control
GameManager.Instance.StartRain();           // Start rain
GameManager.Instance.StopRain();            // Stop rain
GameManager.Instance.ToggleRain();          // Toggle rain state
GameManager.Instance.SetRainIntensity(0.8f); // Set rain intensity (0.0-1.0)

// Snow Control
GameManager.Instance.StartSnow();           // Start snow
GameManager.Instance.StopSnow();            // Stop snow
GameManager.Instance.ToggleSnow();          // Toggle snow state
GameManager.Instance.SetSnowIntensity(0.5f); // Set snow intensity (0.0-1.0)

// Lightning Control
GameManager.Instance.TriggerLightning();    // Manually trigger lightning
```

### Day-Night Control
```csharp
// Time Control
GameManager.Instance.SetDay();              // Switch to day
GameManager.Instance.SetNight();            // Switch to night
GameManager.Instance.ToggleDayNight();      // Toggle day-night state
```

### World Generation
```csharp
// Terrain Control (via CubeGenerator)
// Adjustable terrain elevation, generation density, and other parameters
```

## 📋 Development Notes

### Audio Resources
- Lightning system requires a "Thunder" audio file in the `Resources/Sounds/` folder
- Supports common audio formats: .wav, .mp3, .ogg

### System Dependencies
- Lightning effects only trigger automatically during rain
- Lightning frequency automatically increases at night
- Rainbow system depends on lighting conditions after rain ends

### Performance Recommendations
- Use Object Pool Manager to optimize particle system performance
- Adjust quality settings based on target platform
- Enable LOD system for large scenes

## 🚀 Extension Development

This project is developed entirely through Amazon Q Developer CLI and supports continued feature expansion through natural language conversations:
- Add new weather types (fog, frost, hail, etc.)
- Extend day-night cycle (dusk, dawn transitions)
- Add seasonal systems
- Include more environmental interactions

---

*This documentation is continuously updated with project development. For more information, please refer to the README.md file in the project root directory.*
