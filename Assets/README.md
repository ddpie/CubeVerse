# CubeVerse Assets Documentation

| [English](README_EN.md) | 中文 |
|:---:|:---:|

CubeVerse 是一个功能丰富的3D立方体世界游戏，完全通过 Amazon Q Developer CLI 自然语言对话开发。本文档介绍游戏资源的组织结构和核心系统的使用方法。

## 📁 资源结构

```
Assets/
├── Scenes/          # 游戏场景文件
├── Scripts/         # 核心脚本系统
├── Prefabs/         # 预制体资源
├── Resources/       # 运行时资源
│   ├── Sounds/      # 音效文件
│   └── showcase.png # 游戏截图
└── README.md        # 本文档
```

## 🎮 核心系统

### 🌦️ 天气系统

#### 雨天系统 (RainSystem & RainManager)
- **逼真降雨效果**：包含雨滴粒子和环境音效
- **可调节参数**：强度、颜色、大小、密度
- **智能优化**：基于对象池的高效渲染

#### 雪天系统 (SnowSystem & SnowManager)
- **美丽降雪效果**：营造冬日氛围
- **动态参数**：雪花大小、飘落速度、密度控制
- **季节切换**：支持实时天气转换

#### 闪电系统 (LightningSystem & LightningManager)
- **动态闪电效果**：增强暴风雨体验
- **音效同步**：闪电伴随延迟雷声
- **智能触发**：雨天时自动随机生成，夜晚频率更高

#### 彩虹系统 (RainbowSystem & RainbowManager)
- **雨后彩虹**：增加视觉美感和真实感
- **动态渲染**：基于天气条件自动显示/隐藏

#### 云朵系统 (CloudGenerator & CloudManager)
- **动态云朵**：程序化生成和移动
- **天空渲染**：增强环境氛围

### 🌅 昼夜循环系统 (DayNightManager)
- **平滑过渡**：日夜交替的自然过渡效果
- **动态光照**：环境光照随时间变化
- **星空系统**：夜晚时的璀璨星空效果

### 🎯 游戏核心

#### 玩家控制 (PlayerController & PlayerSetup)
- **第一人称视角**：流畅的移动和视角控制
- **物理交互**：真实的碰撞和重力系统

#### 世界生成 (CubeGenerator)
- **程序化地形**：动态生成立方体世界
- **可调参数**：地形起伏、密度、材质

### ⚡ 性能优化

#### 对象池管理 (ObjectPoolManager)
- **高效复用**：减少内存分配和垃圾回收
- **智能管理**：自动扩容和回收机制

#### 质量设置 (QualitySettings)
- **可调画质**：适配不同性能设备
- **LOD系统**：距离相关的细节层次优化

## 🔧 API 使用指南

### 天气控制
```csharp
// 雨天控制
GameManager.Instance.StartRain();           // 开始下雨
GameManager.Instance.StopRain();            // 停止下雨
GameManager.Instance.ToggleRain();          // 切换雨天状态
GameManager.Instance.SetRainIntensity(0.8f); // 设置雨量强度 (0.0-1.0)

// 雪天控制
GameManager.Instance.StartSnow();           // 开始下雪
GameManager.Instance.StopSnow();            // 停止下雪
GameManager.Instance.ToggleSnow();          // 切换雪天状态
GameManager.Instance.SetSnowIntensity(0.5f); // 设置雪量强度 (0.0-1.0)

// 闪电控制
GameManager.Instance.TriggerLightning();    // 手动触发闪电
```

### 昼夜控制
```csharp
// 时间控制
GameManager.Instance.SetDay();              // 切换到白天
GameManager.Instance.SetNight();            // 切换到夜晚
GameManager.Instance.ToggleDayNight();      // 切换昼夜状态
```

### 世界生成
```csharp
// 地形控制 (通过 CubeGenerator)
// 可调整地形起伏程度、生成密度等参数
```

## 📋 开发注意事项

### 音效资源
- 闪电系统需要在 `Resources/Sounds/` 文件夹中放置名为 "Thunder" 的音效文件
- 支持常见音频格式：.wav, .mp3, .ogg

### 系统依赖
- 闪电效果只在雨天时自动触发
- 夜晚时闪电频率会自动增加
- 彩虹系统依赖雨天结束后的光照条件

### 性能建议
- 使用对象池管理器来优化粒子系统性能
- 根据目标平台调整质量设置
- 大型场景建议启用LOD系统

## 🚀 扩展开发

本项目完全通过 Amazon Q Developer CLI 开发，支持通过自然语言对话继续扩展功能：
- 新增天气类型（如雾、霜、冰雹等）
- 扩展昼夜循环（如黄昏、黎明过渡）
- 增加季节系统
- 添加更多环境交互

---

*本文档随项目开发持续更新。如需了解更多信息，请参考项目根目录的 README.md 文件。*
