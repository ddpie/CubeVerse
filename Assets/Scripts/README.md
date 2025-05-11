# CubeVerse 项目

## 云朵系统说明

在本次更新中，我们为CubeVerse项目添加了白色方块组成的云朵系统。云朵系统具有以下特点：

1. 云朵由白色方块组成，与项目整体风格一致
2. 云朵会在玩家周围的天空中自动生成
3. 云朵会随风缓慢移动，增加场景的动态效果
4. 云朵没有碰撞体，玩家可以穿过它们
5. 当玩家移动到新区域时，远处的云朵会被销毁，新的云朵会在玩家周围生成

## 实现方式

云朵系统通过以下两个脚本实现：

1. `CloudGenerator.cs` - 主要的云朵生成和管理脚本
2. 在 `GameManager.cs` 中添加了云朵系统的初始化代码

云朵系统会自动使用与地形生成相同的方块预制体，确保视觉风格的一致性。

## 参数调整

如果需要调整云朵的外观和行为，可以在GameManager中修改以下参数：

- `cloudCount` - 云朵的数量
- `minCloudHeight` / `maxCloudHeight` - 云朵生成的高度范围
- `cloudSpawnRadius` - 云朵生成的半径范围
- `minCubesPerCloud` / `maxCubesPerCloud` - 每朵云的方块数量范围
- `cloudDensity` - 云朵的密度
- `cloudColor` - 云朵的颜色（默认为白色）
