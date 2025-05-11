# CubeVerse 项目

## 云朵系统说明

在本次更新中，我们为CubeVerse项目添加了白色方块组成的云朵系统。云朵系统具有以下特点：

1. 云朵由白色方块组成，与项目整体风格一致
2. 云朵会在玩家周围的天空中自动生成
3. 云朵会随风缓慢移动，增加场景的动态效果
4. 云朵没有碰撞体，玩家可以穿过它们
5. 当玩家移动到新区域时，远处的云朵会被销毁，新的云朵会在玩家周围生成

## 雨滴系统说明

我们还添加了与方块风格一致的雨滴系统。雨滴系统具有以下特点：

1. 雨滴由细长的半透明蓝色方块组成，与项目整体风格一致
2. 雨滴会从玩家上方的天空中落下
3. 雨滴没有碰撞体，不会与玩家或地形产生物理交互
4. 玩家可以通过按R键切换雨的开关状态
5. 雨的强度可以通过代码调整
6. 雨滴系统使用对象池技术，性能开销小

## 实现方式

云朵系统通过以下两个脚本实现：

1. `CloudGenerator.cs` - 主要的云朵生成和管理脚本
2. 在 `GameManager.cs` 中添加了云朵系统的初始化代码

雨滴系统通过以下三个脚本实现：

1. `RainSystem.cs` - 主要的雨滴生成和管理脚本
2. `RainManager.cs` - 雨滴系统的管理器
3. 在 `GameManager.cs` 中添加了雨滴系统的初始化和控制代码
4. 在 `PlayerController.cs` 中添加了玩家控制雨的功能

## 参数调整

如果需要调整云朵的外观和行为，可以在GameManager中修改以下参数：

- `cloudCount` - 云朵的数量
- `minCloudHeight` / `maxCloudHeight` - 云朵生成的高度范围
- `cloudSpawnRadius` - 云朵生成的半径范围
- `minCubesPerCloud` / `maxCubesPerCloud` - 每朵云的方块数量范围
- `cloudDensity` - 云朵的密度
- `cloudColor` - 云朵的颜色（默认为白色）

如果需要调整雨滴的外观和行为，可以在GameManager和RainManager中修改以下参数：

- `enableWeatherSystem` - 是否启用天气系统
- `startWithRain` - 是否在游戏开始时下雨
- `defaultRainIntensity` - 默认雨的强度（0-1）
- `maxRainDrops` - 最大雨滴数量
- `rainSpawnRadius` - 雨滴生成半径
- `rainHeight` - 雨滴生成高度
- `rainColor` - 雨滴颜色
- `rainDropScale` - 雨滴大小
- `rainSpeed` - 雨滴下落速度
- `rainDirection` - 雨的方向（角度）
