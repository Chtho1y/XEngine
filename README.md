## XEngine
**你的star是我的动力**  
**↓**  
<img src="https://img.shields.io/github/stars/Chtho1y/XEngine?style=social">

<p align="center">
    <img src="XEngine/XEngine.png" alt="XEngine Logo" width="384" height="384">
</p>

<h3 align="center"><strong>XEngine</strong></h3>

<p align="center">
  <strong>The Ultimate Unity Framework Solution</strong>
    <br>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/Unity%20Ver-2021.3.20++-blue.svg?style=flat-square" alt="Unity Version" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/license/Chtho1y/XEngine" alt="License" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/last-commit/Chtho1y/XEngine" alt="Last Commit" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/issues/Chtho1y/XEngine" alt="Issues" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/languages/top/Chtho1y/XEngine" alt="Top Language" />
  </a>
  <br>
</p>

## Introduction

XEngine is a robust and beginner-friendly Unity framework that provides a comprehensive solution for cross-platform development. Whether you are an individual developer or part of a team, XEngine offers a clean codebase, clear documentation, and high performance, making it an excellent choice for commercial-grade projects.

## Why Choose XEngine?

1. **Easy to Use**: Get started with XEngine in just 5 minutes. The framework offers a clean and organized codebase, making it easy to understand and extend. You can quickly remove or replace modules you don't need, thanks to its high cohesion and low coupling design.
2. **Commercial-Grade Performance**: Leveraging XLua for hot updates, XEngine is optimized for mobile platforms and has been validated in high-DAU commercial games. It features next-gen HybridCLR for hot updates, the best Luban configuration tables, and the YooAsset resource framework, ensuring efficient resource management and memory usage.
3. **Cross-Platform Support**: XEngine supports multiple platforms, including Steam, WeChat Minigame, and AppStore. The framework has already been used in projects available on these platforms.
4. **Modular Design**: From asset bundles to UI components, XEngine's modular architecture allows you to pick and choose the features you need for your project, enhancing flexibility and scalability.

## Quick Start Guide

To get a quick overview of how to run XEngine on various platforms, refer to our [Platform Runthrough Guide](Books/99-各平台运行RunAble.md). Here are some key modules:

* [Introduction](Books/0-介绍.md): An overview of the framework.
* [Framework Overview](Books/2-框架概览.md): Detailed look at the framework's architecture.
* [Resource Module](Books/3-1-资源模块.md): Insights into resource management.
* [Event Module](Books/3-2-事件模块.md): Explanation of the event handling system.
* [Memory Pool Module](Books/3-3-内存池模块.md): Details on memory pooling mechanisms.
* [Object Pool Module](Books/3-4-对象池模块.md): Description of the object pooling system.
* [Configuration Module](Books/3-6-配置表模块.md): Guide to using configuration tables.
* [Flow Module](Books/3-7-流程模块.md): Overview of the business logic flow.
* [UI Module](Books/3-5-UI模块.md): Guide to developing commercial-grade UI.

## <strong>Project Structure
```
XEngine
├── Editor
│ ├── AssetBundleBuilder
│ │ ├── AssetBundleBuilder
│ │ ├── AssetBundleBuilderOptions
│ │ ├── AssetBundleBuilderWindow
│ │ ├── Res2BundleBuilder
│ ├── UIBuilder
│ │ ├── ComponentType
│ │ ├── LuaCodeGenerator
│ │ ├── UIBuilderWindow
│ ├── EmmyLuaService
│ ├── GameEditor
│ ├── MissingScriptsAndEventsFinder
│ ├── ProtoTool
├── Engine
│ ├── Bundle
│ │ ├── AssetBundleUpdater
│ │ ├── BundleInfo
│ │ ├── BundleManager
│ │ ├── CryptoTool
│ │ ├── PathProtocol
│ │ ├── ResourceMode
│ │ ├── VersionInfo
│ ├── Lua
│ │ ├── LuaLifecycle
│ │ ├── XLuaSimulator
│ ├── Pool
│ │ ├── GameObjectPool
│ │ ├── GameObjectPoolManager
│ │ ├── ObjectPool
│ ├── Utils
│ │ ├── AnimationEventHandler
│ │ ├── CollisionBehaviour
│ │ ├── GameUtil
│ │ ├── HttpUtil
│ │ ├── LogPrinter
│ │ ├── MonoSingleton
│ │ ├── ParticleScaler
│ │ ├── Singleton
│ │ ├── StringUtil
│ │ ├── TimeUtil
│ │ ├── TriggerBehaviour
│ │ ├── TweenUtil
│ │ ├── UniTaskUtil
│ │ ├── UniWebViewUtil
│ │ ├── GameManager
│ │ ├── UnityEventHelper
├── Main
│ ├── BundleServerConfig
│ ├── GameLoaderProgress
│ ├── GameRoot
├── UI
│ ├── DragButton
│ ├── EmptyGraphic
│ ├── Irregular
│ ├── ScreenFit
│ ├── TextEx
│ ├── Transition
│ ├── UIEvent
│ ├── UIUtil
│ ├── XButton
│ ├── XButtonGroup
│ ├── XComponent
│ ├── XDragButton
│ ├── XDropdown
│ ├── XImage
│ ├── XInputField
│ ├── XRawImage
│ ├── XRectTransform
│ ├── XScrollBar
│ ├── XScrollRect
│ ├── XSlider
│ ├── XText
│ ├── XToggle
│ ├── XTransform
```

## Recommended Third-Party Plugins

To ensure the best experience with XEngine, we recommend using the following third-party plugins. Please purchase and import them as needed:
   - [Sirenix](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041): For enhanced editor functionalities.

## Open Source Projects We Recommend

* [YooAsset](https://github.com/tuyoogame/YooAsset): A commercial-grade resource management system validated in high-DAU games.
* [JEngine](https://github.com/JasonXuDeveloper/JEngine): A solution for hot updates in Unity games.
* [HybridCLR](https://github.com/focus-creative-games/hybridclr): A near-perfect native C# hot update solution for Unity, featuring complete functionality, zero cost, high performance, and low memory usage.
* [Fantasy](https://github.com/qq362946/Fantasy): A commercial-grade server framework that's simple and easy to use, derived from ETServer.
* [GameNetty](https://github.com/ALEXTANGXIAO/GameNetty): A front-end and back-end solution package from ET8.1, providing a minimal client size and almost zero-cost integration into your framework.

## Community and Support

Join our community to discuss and get support for XEngine:
* [QQ Group: 574763941](https://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=C_grV7Zwbegcjlk79wDdvkh8PtRKPkDU&authKey=pwnX5CZ%2FWmWD4D5tRFbHyOy6WHXJ99L%2B%2BCzZH%2B33lH9Qx1Z5AtbVEZXIhEwYqFHq&noverify=0&group_code=574763941)

## Buy Me a Milk Tea

If XEngine has helped you, consider [buying me a milk tea](Books/Donate.md). Your support will enable us to improve and develop faster.
