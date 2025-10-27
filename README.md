# 🧊 ClearBG — Transparent Overlay System for Unity

**ClearBG** is a GPU-swapchain-based transparency and UI compositing system designed for Unity.  
It allows developers to render **interactive UI overlays directly on the desktop background**, creating seamless transparent experiences — ideal for widgets, HUD-style tools, and real-time visualization layers.

---


> Straight talk: **Windows-only**. Works across **all Unity render pipelines and versions** and with **any Canvas render mode** (Screen Space — Overlay/Camera, World Space). The native overlay lives above the desktop and forwards input based on pixel alpha.

<p align="center">
  <img alt="status" src="https://img.shields.io/badge/status-alpha-critical">
  <img alt="unity" src="https://img.shields.io/badge/Unity-Any-blue">
  <img alt="pipelines" src="https://img.shields.io/badge/Pipelines-Built--in%2FURP%2FHDRP-informational">
  <img alt="platform" src="https://img.shields.io/badge/Platform-Windows-lightgrey">
</p>

## What It Does

**ClearBG** creates a transparent, always-on-top desktop overlay and renders your Unity content into it. You can target any monitor, read work-area/taskbar bounds, and toggle **pixel-alpha click-through** so mouse/keyboard events pass through fully transparent pixels to whatever is behind the overlay.

## Features

- ⚙️ **Auto-initialize (BeforeSplashScreen)** via settings, or start/stop at runtime.
- 🪟 **Always-on-top** toggle at runtime.
- 🖥️ **Multi-monitor**: set/get target monitor, get primary index.
- 🧭 **Screen & taskbar rectangles** exposed in both UI and world-space terms.
- 🖱️ **Pixel-alpha click-through** with configurable threshold.
- 📈 **Performance stats** (avg frame time) from the native side.
- 🧱 **Works with any Canvas render mode** (Overlay/Camera/World) and **all pipelines** (Built‑in/URP/HDRP).

## Installation

1. **Add the runtime scripts and the native DLL** to your project. The Windows DLL must be present at runtime (e.g., next to the exe or in `Plugins/x86_64`). The C# side calls the DLL via P/Invoke.
2. **Create the settings asset** (auto-created on first run if missing):  
   `Assets/Resources/ClearBGSettings.asset`.
3. **(Optional) Prefabs**: Provide your own overlay/diagnostic canvases if you want; the manager can also bootstrap a minimal setup.
4. **Build target**: **Windows Standalone** (x64).

## Quick Start

### Auto init (recommended for apps that are always overlay-driven)

```csharp
// In the ClearBG Settings asset:
// - AutoInitialize: true
// - AlwaysOnTop: true (if you want overlay to stay above everything)
// - TargetDisplay / TargetFPS / ClickThroughThreshold: set to taste
```

### Manual control from code

```csharp
using ClearBG.Runtime.Scripts.Managers;
using UnityEngine;

public class OverlayBoot : MonoBehaviour
{
    void Start()
    {
        // Ensure overlay is active
        ClearBgManager.ActivateClearBg();
        ClearBgManager.SetAlwaysOnTop(true);

        // Optional: target the primary monitor
        ClearBgManager.SetMonitorIndex(ClearBgManager.GetPrimaryMonitorIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            ClearBgManager.SetAlwaysOnTop(!ClearBgManager.IsAlwaysOnTop);

        if (Input.GetKeyDown(KeyCode.F2))
            ClearBgManager.DeactivateClearBg();

        ClearBgManager.GetPerformanceStats(out var cpuMs);
        // Use cpuMs for your own diagnostics/telemetry.
    }
}
```

## Public API (Summary)

### `ClearBgManager` (static)

- **Lifecycle**
  - `ActivateClearBg()` / `DeactivateClearBg()`
  - `Initialized : bool`
- **Z-Order**
  - `SetAlwaysOnTop(bool enable)`  
  - `IsAlwaysOnTop : bool`
- **Displays / Monitors**
  - `SetMonitorIndex(int index)`  
  - `int GetMonitorIndex { get; }`  
  - `int GetPrimaryMonitorIndex { get; }`
- **Geometry**
  - `GetTaskbarRect(out RectData ui, out RectData world)`  
  - `GetScreenRect(out RectData ui, out RectData world)`
- **Stats**
  - `GetPerformanceStats(out float avgCpuFrameMs)`
- **Settings Access**
  - `ClearBgSettings GetSettings()`

### `ClearBg` (MonoBehaviour)

Bridges to the native overlay DLL (initialize overlay window, per-frame update, click-through toggles, monitor selection, performance stats) and wires the Unity Camera for transparent output.

### `ClearBgSettings` (ScriptableObject)

- `AutoInitialize` / `DebugMode` / `AlwaysOnTop`  
- `ClickThroughThreshold` (0–1)  
- `TargetFPS` / `TargetDisplay`  
- Stored at `Assets/Resources/ClearBGSettings.asset`

## How It Works (High Level)

- A borderless transparent window is created on Windows and kept on top. Unity renders into a texture that is composed into that window.
- Each frame we can sample the pixel under the mouse and decide whether input should pass through (below the alpha threshold) or be captured by the overlay.
- Taskbar/work-area metrics are exposed so you can avoid covering critical OS UI or position things relative to safe regions.

## Limitations / Notes

- **Windows-only.** The native overlay depends on Win32/DWM.
- **Native DLL required.** If the DLL is missing or fails to initialize, the overlay will not start.
- **Security/Anti-cheat**: Desktop overlays can be flagged by some game anti-cheat systems. Use appropriately.

## Troubleshooting

- **Overlay doesn’t appear**
  - Verify **Windows build** (not macOS/Linux) and that the **DLL is present**.
  - Check logs from `ClearBgManager` for initialization errors.
- **Click-through not working as expected**
  - Tune `ClickThroughThreshold`. Ensure your rendered content actually has alpha < threshold where you expect pass‑through.
- **Wrong monitor**
  - Call `SetMonitorIndex()` explicitly. Use `GetPrimaryMonitorIndex` to fetch OS primary.

## Roadmap (suggested)

- Shared-texture / zero-copy paths (DXGI interop) for lower overhead.
- Region-based click-through masks to avoid per-frame sampling.
- Multiple overlay windows and window snapping policies.

🛠️ License

© 2025 Batuhan Kanbur.
All rights reserved.
This plugin may be used in both personal and commercial Unity projects.

🌟 Credits

Developed by Batuhan Kanbur

A high-performance overlay framework built for real-time, desktop-level UI rendering in Unity.


---
