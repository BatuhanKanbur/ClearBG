# 🧊 ClearBG — Transparent Overlay System for Unity

**ClearBG** is a GPU-swapchain-based transparency and UI compositing system designed for Unity.  
It allows developers to render **interactive UI overlays directly on the desktop background**, creating seamless transparent experiences — ideal for widgets, HUD-style tools, and real-time visualization layers.

---

## 🚀 Features

- 🖥️ **Transparent GPU Swapchain Rendering** — renders your Unity scene or UI as a true desktop overlay.  
- 💡 **World Space Canvas Support Only** — designed specifically for world-space UI rendering.  
- 🔄 **Automatic Initialization** — loads before the Unity splash screen if enabled in settings.  
- 🧰 **Editor Simulation Support** — provides approximate monitor and taskbar data for previewing in the Unity Editor.  
- 🌈 **Universal Render Pipeline Compatibility** — works across Built-in, URP, and HDRP.  
- ⚙️ **Graphics API Agnostic** — supports both DirectX and OpenGL.  
- 🪟 **Windows Only (for now)** — current build pipeline supports Windows targets exclusively.  
- 🧩 **Debug Overlay System** — optional debug canvas for visualizing overlay boundaries and diagnostics.

---

## 🧠 How It Works

ClearBG operates by creating a **GPU Swapchain Layer** behind Unity’s main render surface.  
Instead of rendering to the game window, it composites a transparent buffer that directly overlays the Windows desktop.  
This allows any Unity UI rendered in World Space to appear as part of the desktop environment.

---

## 🏗️ Setup & Usage

### 1. Add ClearBG to Your Project

Place the `ClearBG` folder inside your `Assets` directory.  
The typical structure should look like this:

Assets/
├── ClearBG/
│ ├── Runtime/
│ │ ├── Scripts/
│ │ ├── Resources/
│ │ └── Shaders/
│ ├── Editor/
│ ├── Plugins/
│ │ ├── x86/
│ │ └── x86_64/
│ └── ClearBG.asmdef


> 🧩 Native DLLs (for GPU swapchain and OS-level composition) should be placed under:
> `Assets/ClearBG/Plugins/x86_64/` and marked as **"Windows Only"** in import settings.

---

### 2. Canvas Configuration

ClearBG **only supports World Space canvases**.  
For proper rendering:
- Set your Canvas **Render Mode** to `World Space`.  
- Avoid using Screen Space - Overlay or Camera modes.  
- Ensure UI elements are positioned within visible world-space boundaries.

---

### 3. Initialization

ClearBG can auto-initialize before the Unity splash screen:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
private static void Initialize()
{
    _settings = ClearBgSettings.GetOrCreateSettings();
    if (_settings.AutoInitialize)
    {
        var clearBgInstance = new GameObject("ClearBG");
        _clearBg = clearBgInstance.AddComponent<ClearBg>();
        _clearBg.Initialize();
    }
}


Or manually, by calling:

ClearBgManager.ActivateClearBg();


To disable it at runtime:

ClearBgManager.DeactivateClearBg();

🧭 API Overview
ClearBgManager
Method	Description
ActivateClearBg()	Enables or creates the ClearBG overlay.
DeactivateClearBg()	Disables the ClearBG overlay.
SetMonitorIndex(int index)	Targets a specific monitor in multi-monitor setups.
GetMonitorIndex	Returns the currently active monitor index.
GetTaskbarRect(out RectData ui, out RectData world)	Returns taskbar bounds in both UI-space and world-space.
GetScreenRect(out RectData ui, out RectData world)	Returns full screen boundaries in UI-space and world-space.
GetDefaultMonitorData()	Provides fallback monitor info (used in Editor).
GetSettings()	Retrieves the current runtime settings object.

🧪 Debug Mode
Enable Debug Mode in ClearBgSettings to visualize overlay boundaries.
This spawns a runtime-only Debug_Canvas prefab (auto-loaded from Resources/Debug_Canvas.prefab).
Ideal for alignment testing and scaling validation during development.

⚙️ Platform & Technical Details
Category	Status
OS	Windows (Build only)
Editor Support	Simulated (approximate monitor data)
Graphics APIs	DirectX, OpenGL
Render Pipelines	Built-in, URP, HDRP
Architecture	GPU Swapchain Overlay
Dependencies	None (self-contained runtime)

🧩 Tips & Recommendations
Always use World Space UI for accurate scaling.
Avoid enabling Post Processing on the ClearBG camera.
Use ClearBgManager.Initialized to verify system readiness before querying monitor data.
Use GetScreenRect() or GetTaskbarRect() to dynamically align your UI elements with desktop regions.

🧠 Known Limitations
Editor preview provides approximate results — accurate only in runtime builds.
Currently supports Windows only (macOS & Linux support planned).
Unity splash screen cannot be rendered through ClearBG’s transparent layer.

🛠️ License

© 2025 Batuhan Kanbur.
All rights reserved.
This plugin may be used in both personal and commercial Unity projects.

🌟 Credits

Developed by Batuhan Kanbur

A high-performance overlay framework built for real-time, desktop-level UI rendering in Unity.


---