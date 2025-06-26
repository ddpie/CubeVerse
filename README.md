# CubeVerse - 立方体世界

[English](README_EN.md) | 中文

## 项目简介

CubeVerse 是一个完全使用 **Amazon Q Developer CLI** 通过自然语言对话开发的 3D 立方体世界游戏。

![游戏截图](Assets/Resources/showcase.png)
*CubeVerse 游戏截图 - 立方体风格的世界*

## 演示视频

https://www.bilibili.com/video/BV1bUKfzjEbr

### 视频说明

这段演示视频展示了如何使用 Amazon Q Developer CLI 通过自然语言对话来操作团结引擎/Unity开发游戏。在视频中，我们通过简单的自然语言指令实现了以下效果：

- 将游戏场景切换为冬季风格，开启降雪效果开关
- 调整地形生成参数，降低地形的起伏程度

所有这些操作都是通过与 Amazon Q Developer CLI 的对话完成的，而不是手动修改代码或调整参数。

## 游戏特性

### 🎮 核心玩法
- **第一人称视角控制**：流畅的玩家移动和视角控制
- **立方体世界生成**：程序化生成的立方体地形
- **物理交互**：真实的物理碰撞和重力系统

### 🌦️ 天气系统
- **雨天系统**：逼真的降雨效果，包含雨滴粒子和声音
- **雪天系统**：美丽的降雪效果，营造冬日氛围
- **闪电系统**：动态闪电效果，增强暴风雨体验
- **彩虹系统**：雨后彩虹，增加视觉美感
- **云朵系统**：动态云朵生成和移动

### 🌅 昼夜循环
- **日夜交替**：平滑的昼夜过渡效果
- **动态光照**：根据时间变化的环境光照
- **星空系统**：夜晚时的星空效果

### ⚡ 性能优化
- **对象池管理**：高效的对象复用机制
- **质量设置**：可调节的画质选项
- **LOD 系统**：距离相关的细节层次

## 开发工具

### Amazon Q Developer CLI + MCP Unity - 核心开发工具
本项目 **100% 使用 Amazon Q Developer CLI** 结合 **MCP Unity** 通过自然语言对话完成开发：

#### MCP Unity 集成
- **项目地址**：[https://github.com/CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity)
- **MCP 协议**：Model Context Protocol，实现 AI 与团结引擎/Unity 的直接交互
- **无缝集成**：Amazon Q Developer CLI 通过 MCP 直接操作团结引擎/Unity 编辑器
- **实时操作**：AI 可以直接创建、修改游戏对象和组件，无需手动操作

#### 开发特性
- **自然语言编程**：所有代码均通过与 AI 对话生成，无需手动编写
- **智能代码生成**：复杂的天气系统、物理交互全部由 AI 理解需求后自动生成
- **实时问题解决**：开发过程中遇到的所有技术问题都通过自然语言描述获得解决方案
- **代码优化建议**：AI 主动提供性能优化和代码改进建议
- **文档自动生成**：项目文档和代码注释全部由 AI 自动生成
- **引擎直接操作**：通过 MCP 协议，AI 可以直接在团结引擎/Unity 中创建场景、添加组件、设置参数

### 开发环境
- **游戏引擎**：团结引擎 1.5.x (Unity 2022.3 LTS)
- **编程语言**：C#（通过自然语言生成）
- **核心工具**：Amazon Q Developer CLI

## 安装和运行

### 环境要求
- 团结引擎 1.5.x (Unity 2022.3 LTS) 或更高版本
- Amazon Q Developer CLI（核心开发工具）
- Node.js 18+ （用于运行 MCP Unity/团结引擎 服务器）

### MCP 安装步骤
参考 https://github.com/CoderGamester/mcp-unity/blob/main/README.md 来配置 MCP

### 项目运行步骤
1. 使用 Unity Hub 打开项目
2. 等待团结引擎/Unity 导入所有资源
3. 在 Project 窗口中导航到 `Assets/Scenes` 文件夹
4. 双击打开 `SampleScene.scene` 场景文件
5. 点击编辑器顶部的播放按钮开始游戏

### 控制说明
- **WASD** - 移动
- **鼠标** - 视角控制
- **空格** - 跳跃

---

**使用 Amazon Q Developer CLI 开发，让游戏开发更高效！** 🚀
