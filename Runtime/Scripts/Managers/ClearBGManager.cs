using ClearBG.Runtime.Scripts.Behaviours;
using ClearBG.Runtime.Scripts.Structures;
using UnityEngine;

namespace ClearBG.Runtime.Scripts.Managers
{
    /// <summary>
    /// Core runtime manager for the <b>ClearBG</b> system.
    /// <para>
    /// Handles initialization, activation, and configuration of the ClearBG overlay,
    /// managing all runtime interactions with the <see cref="ClearBg"/> component.
    /// </para>
    /// <para>
    /// ⚙️ <b>Runtime Behavior:</b>
    /// Automatically initializes before the Unity splash screen if auto-initialization is enabled.
    /// Works exclusively during builds (not in the Unity Editor), and relies on <b>World Space UI Canvases</b>
    /// for proper rendering.
    /// </para>
    /// <para>
    /// 🧠 <b>Editor Notes:</b>
    /// In the Unity Editor, monitor and taskbar calculations are <b>approximate</b>
    /// and should not be used for precise UI positioning.
    /// </para>
    /// <para>
    /// 🌈 <b>Compatibility:</b>
    /// - Works with all Render Pipelines (Built-in, URP, HDRP).<br/>
    /// - Supports DirectX and OpenGL graphics APIs.<br/>
    /// - Currently available on <b>Windows only</b>.
    /// </para>
    /// <para>
    /// 🧩 <b>Technical Details:</b>
    /// Operates using a <b>GPU swapchain</b>-based compositing model for transparent overlay rendering.
    /// </para>
    /// </summary>
    public static class ClearBgManager
    {
        private static ClearBg _clearBg;
        private static DebugCanvas _debugCanvas;
        private static ClearBgSettings _settings;
        /// <summary>
        /// Indicates whether the ClearBG system has been successfully initialized.
        /// Returns <c>true</c> only if a valid <see cref="ClearBg"/> instance and camera are available.
        /// </summary>
        public static bool Initialized => _clearBg && _clearBg.Camera && _clearBg.Initialized;
        /// <summary>
        /// Initializes the ClearBG system before the Unity splash screen is displayed.
        /// Loads settings, creates the ClearBG instance if auto-initialization is enabled,
        /// and optionally spawns a debug canvas when <b>Debug Mode</b> is active.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _settings = ClearBgSettings.GetOrCreateSettings();
            if (_clearBg) return;
            if(_settings.DebugMode) CreateDebugCanvas();
            if (!_settings.AutoInitialize) return;
            var clearBgInstance = new GameObject("ClearBG");
            _clearBg = clearBgInstance.AddComponent<ClearBg>();
            _clearBg.Initialize();
        }
        /// <summary>
        /// Creates the Debug Canvas overlay if Debug Mode is enabled.
        /// Used for visualizing ClearBG boundaries and diagnostics at runtime.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateDebugCanvas()
        {
            if (!_settings.DebugMode || _debugCanvas) return;
            var prefab = Resources.Load<GameObject>("Debug_Canvas");
            if (prefab)
            {
                _debugCanvas = Object.Instantiate(prefab).GetComponent<DebugCanvas>();
                Debug.Log("<color=yellow>Debug mode is enabled.</color>");
            }
            else
            {
                Debug.Log("<color=red>Debug mode could not be started because the source files could not be found.</color>");
            }
        }
        /// <summary>
        /// Enables or disables the always on top.
        /// </summary>
        /// <param name="value">Set to <c>true</c> to enable , <c>false</c> to disable it.</param>
        public static void SetAlwaysOnTop(bool value)
        {
            if (!_clearBg) return;
            _settings.AlwaysOnTop = value;
            _clearBg.SetAlwaysOnTopEnabled(value);
        }
        /// <summary>
        /// Activates the ClearBG overlay if it exists,
        /// or creates and initializes a new one if necessary.
        /// </summary>
        public static void ActivateClearBg()
        {
            if (_clearBg)
            {
                _clearBg.SetOverlayEnabled(true);
                return;
            }
            var clearBgInstance = new GameObject("ClearBG");
            _clearBg = clearBgInstance.AddComponent<ClearBg>();
            _clearBg.Initialize();
        }
        /// <summary>
        /// Deactivates the ClearBG overlay, hiding all related rendering layers.
        /// </summary>
        public static void DeactivateClearBg()
        {
            if (!_clearBg) return;
            _clearBg.SetOverlayEnabled(false);
        }
        /// <summary>
        /// Sets the target monitor index for the ClearBG overlay.
        /// Useful in multi-monitor setups.
        /// </summary>
        /// <param name="index">The zero-based index of the monitor to target.</param>
        public static void SetMonitorIndex(int index)
        {
            if (!_clearBg) return;
            _clearBg.ChangeMonitor(index);
        }
        /// <summary>
        /// Returns the index of the currently targeted monitor.
        /// Returns -1 if ClearBG is not initialized.
        /// </summary>
        public static int GetMonitorIndex => _clearBg ? _clearBg.TargetMonitor : -1;
        /// <summary>
        /// Retrieves performance statistics for the ClearBG overlay rendering.
        /// </summary>
        /// <param name="cpuTimeMs">Output parameter for CPU time in milliseconds.</param>
        /// <param name="cpuFeature">Output parameter for GPU time in milliseconds.</param
        public static void GetPerformanceStats(out float cpuTimeMs, out int cpuFeature)
        {
            cpuTimeMs = -2;
            cpuFeature = -2;
            if (!_clearBg) return;
            _clearBg.GetPerformance(out cpuTimeMs, out cpuFeature);
        }
        
        
         /// <summary>
         /// Retrieves the active <see cref="ClearBgSettings"/> instance.
         /// </summary>
        public static ClearBgSettings GetSettings() => _settings;
        /// <summary>
        /// Retrieves the taskbar area in both UI-space and world-space coordinates.
        /// Returns fallback estimates when the ClearBG camera is unavailable.
        /// </summary>
        /// <param name="uiRect">Output rectangle in UI-space.</param>
        /// <param name="worldRect">Output rectangle in world-space.</param>
        public static void GetTaskbarRect(out RectData uiRect, out RectData worldRect)
        {
            uiRect = new RectData();
            worldRect = new RectData();
            if (!_clearBg || !_clearBg.Camera)
            {
                uiRect.Left = -Screen.width / 2f;
                uiRect.Right = Screen.width / 2f;
                uiRect.Top = -Screen.height / 2f + 40;
                uiRect.Bottom = -Screen.height / 2f;
                uiRect.Size = new Vector2(uiRect.Right - uiRect.Left, uiRect.Top - uiRect.Bottom);
                var cam = Camera.main;
                if (cam)
                {
                    var screenBL = new Vector3(0, 0, cam.nearClipPlane + 1f);
                    var screenTR = new Vector3(Screen.width, 40, cam.nearClipPlane + 1f);
                    var worldBL = cam.ScreenToWorldPoint(screenBL);
                    var worldTR = cam.ScreenToWorldPoint(screenTR);
                    worldRect.Left = worldBL.x;
                    worldRect.Right = worldTR.x;
                    worldRect.Bottom = worldBL.y;
                    worldRect.Top = worldTR.y;
                    worldRect.Size = new Vector2(worldRect.Right - worldRect.Left, worldRect.Top - worldRect.Bottom);
                }
                else
                {
                    worldRect = uiRect;
                }
                return;
            }
            var uiCamera = _clearBg.Camera;
            var data = _clearBg.MonitorData;
            var workW = data.workRight - data.workLeft;
            var workH = data.workBottom - data.workTop;
            var scaleX = data.screenWidth / (float)workW;
            var scaleY = data.screenHeight / (float)workH;
            var tbLeft = (data.taskbarLeft - data.workLeft) * scaleX;
            var tbTop = (data.taskbarTop - data.workTop) * scaleY;
            var tbRight = (data.taskbarRight - data.workLeft) * scaleX;
            var tbBottom = (data.taskbarBottom - data.workTop) * scaleY;
            var screenTopLeftWorld = new Vector3(tbLeft, data.screenHeight - tbTop, 1f);
            var screenBottomRightWorld = new Vector3(tbRight, data.screenHeight - tbBottom, 1f);
            var worldTopLeft = uiCamera.ScreenToWorldPoint(screenTopLeftWorld);
            var worldBottomRight = uiCamera.ScreenToWorldPoint(screenBottomRightWorld);
            worldRect.Left = worldTopLeft.x;
            worldRect.Top = worldTopLeft.y;
            worldRect.Right = worldBottomRight.x;
            worldRect.Bottom = worldBottomRight.y;
            worldRect.Size = new Vector2(worldRect.Right - worldRect.Left, worldRect.Top - worldRect.Bottom);
            var canvasRect = _clearBg.Canvas.GetComponent<RectTransform>();
            var cameraParam = (_clearBg.Canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : uiCamera;
            var screenTopLeftUI = new Vector2(tbLeft, data.screenHeight - tbTop);
            var screenBottomRightUI = new Vector2(tbRight, data.screenHeight - tbBottom);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenTopLeftUI, cameraParam, out var localTopLeft);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenBottomRightUI, cameraParam, out var localBottomRight);
            uiRect.Left = localTopLeft.x;
            uiRect.Top = localTopLeft.y + data.taskbarEdge;
            uiRect.Right = localBottomRight.x;
            uiRect.Bottom = localBottomRight.y + data.taskbarEdge;
            uiRect.Size = new Vector2(uiRect.Right - uiRect.Left, uiRect.Top - uiRect.Bottom);
        }
        /// <summary>
        /// Retrieves the screen boundaries in both UI-space and world-space coordinates.
        /// Returns fallback values if the ClearBG system or camera is not initialized.
        /// </summary>
        /// <param name="uiLimits">Output rectangle in UI-space.</param>
        /// <param name="worldLimits">Output rectangle in world-space.</param>
        public static void GetScreenRect(out RectData uiLimits, out RectData worldLimits)
        {
            uiLimits = new RectData();
            worldLimits = new RectData();
            if (!_clearBg || !_clearBg.Camera)
            {
                uiLimits = new RectData
                {
                    Left = -Screen.width / 2f,
                    Right = Screen.width / 2f,
                    Top = Screen.height / 2f,
                    Bottom = -Screen.height / 2f,
                    Size = new Vector2(Screen.width, Screen.height)
                };
                var cam = Camera.main;
                if (cam)
                {
                    var screenBL = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane + 1f));
                    var screenTR = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane + 1f));
                    worldLimits = new RectData
                    {
                        Left = screenBL.x,
                        Right = screenTR.x,
                        Bottom = screenBL.y,
                        Top = screenTR.y,
                        Size = new Vector2(screenTR.x - screenBL.x, screenTR.y - screenBL.y)
                    };
                }
                else
                {
                    worldLimits = uiLimits;
                }
                return;
            }
            var monitorData = _clearBg.MonitorData;
            var uiCamera = _clearBg.Camera;
            var workW = monitorData.workRight - monitorData.workLeft;
            var workH = monitorData.workBottom - monitorData.workTop;
            var scaleX = monitorData.screenWidth / (float)workW;
            var scaleY = monitorData.screenHeight / (float)workH;
            var left   = (monitorData.workLeft - monitorData.monitorLeft) * scaleX;
            var top    = (monitorData.workTop - monitorData.monitorTop) * scaleY;
            var right  = (monitorData.workRight - monitorData.monitorLeft) * scaleX;
            var bottom = (monitorData.workBottom - monitorData.monitorTop) * scaleY;
            uiLimits = new RectData
            {
                Left     =  - (monitorData.screenWidth / 2f) + left,
                Right    =  - (monitorData.screenWidth / 2f) + right,
                Top      =    (monitorData.screenHeight / 2f) - top,
                Bottom   =    (monitorData.screenHeight / 2f) - bottom,
                Size     =  new Vector2(scaleX, scaleY)
            };
            var canvasRect = _clearBg.Canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, new Vector2(left, monitorData.screenHeight - bottom), _clearBg.Camera, out var blLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, new Vector2(right, monitorData.screenHeight - top), _clearBg.Camera, out var trLocal);
            var bl = uiCamera.ScreenToWorldPoint(new Vector3(left, monitorData.screenHeight - bottom, 1f));
            var tr = uiCamera.ScreenToWorldPoint(new Vector3(right, monitorData.screenHeight - top, 1f));
            worldLimits = new RectData
            {
                Left     = bl.x,
                Right    = tr.x,
                Top      = tr.y,
                Bottom   = bl.y,
                Size     =  new Vector2(tr.x - bl.x, tr.y - bl.y)
            };
        }
        /// <summary>
        /// Generates a default <see cref="MonitorData"/> structure
        /// based on Unity's <see cref="Screen"/> class.
        /// Primarily used in the Editor as a fallback when monitor data is unavailable.
        /// </summary>
        public static MonitorData GetDefaultMonitorData()
        {
            var monitorData = new MonitorData
            {
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                monitorLeft = 0,
                monitorTop = 0,
                monitorRight = Screen.width,
                monitorBottom = Screen.height,
                workLeft = 0,
                workTop = 0,
                workRight = Screen.width,
                workBottom = Screen.height - 40,
                taskbarLeft = 0,
                taskbarTop = Screen.height - 40,
                taskbarRight = Screen.width,
                taskbarBottom = Screen.height,
                taskbarEdge = 40
            };
            return monitorData;
        }

    }
}
