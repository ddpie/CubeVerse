# CubeVerse

一个基于方块的3D世界，具有动态天气系统和日夜循环。

## 天气系统

### 雨滴系统
- 可以通过`RainManager`控制雨滴的生成和停止
- 雨滴具有可调节的强度、颜色和大小

### 雪花系统
- 可以通过`SnowManager`控制雪花的生成和停止
- 雪花具有可调节的强度、颜色和大小

### 闪电系统
- 在下雨时随机生成闪电效果
- 闪电伴随着延迟的雷声
- 可以通过`LightningManager`手动触发闪电

## 日夜循环
- 可以通过`DayNightManager`控制日夜切换
- 夜晚会显示星星
- 光照和环境色会随日夜变化

## 使用方法

### 天气控制
```csharp
// 通过GameManager控制天气
GameManager.Instance.StartRain();
GameManager.Instance.StopRain();
GameManager.Instance.ToggleRain();
GameManager.Instance.SetRainIntensity(0.8f);

GameManager.Instance.StartSnow();
GameManager.Instance.StopSnow();
GameManager.Instance.ToggleSnow();
GameManager.Instance.SetSnowIntensity(0.5f);

// 手动触发闪电
GameManager.Instance.TriggerLightning();
```

### 日夜控制
```csharp
// 通过GameManager控制日夜
GameManager.Instance.SetDay();
GameManager.Instance.SetNight();
GameManager.Instance.ToggleDayNight();
```

## 注意事项
- 闪电系统需要在Resources/Sounds文件夹中放置名为"Thunder"的音效文件
- 闪电只会在下雨时触发
- 夜晚时闪电会更加频繁
