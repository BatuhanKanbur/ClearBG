using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClearBG.Runtime.Scripts.Managers;
using ClearBG.Runtime.Scripts.Structures;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ClearBG.Runtime.Scripts.Behaviours
{
    public class ClearBg : MonoBehaviour
    {
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern bool InitOverlay(int monitorIndex);
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern void OverlayUpdate();
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern void BlitRawRGBA(IntPtr data, int width, int height);
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern void SetMonitorIndex(int monitorIndex);
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern MonitorData GetMonitorData(int monitorIndex);
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern void SetOverlayActive(bool enable);
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        #endif
        private static extern bool IsOverlayActive();
        
        public int TargetMonitor { get; private set; }

        private Texture2D _rtTex;
        private Color32[] _pixelBuffer;
        private GCHandle _handle;
        public bool Initialized { get; private set; }
        public Camera Camera {get; private set;}
        public Canvas Canvas { get; private set; }
        private ClearBgSettings _settings;
        private CameraClearFlags _clearFlags;
        private Color _backgroundColor;
        public MonitorData MonitorData { get; private set; }

        public void Initialize()
        {
            if (Initialized) return;
            DontDestroyOnLoad(gameObject);
            _settings = ClearBgSettings.GetOrCreateSettings();
            var monitorIndex = Mathf.Clamp(_settings.TargetDisplay, 0, Display.displays.Length - 1);
            TargetMonitor = monitorIndex;
            #if !UNITY_EDITOR
            if (!InitOverlay(monitorIndex))
            {
                Debug.LogError("<color=red>Overlay initialization failed.</color>");
                return;
            }
            #endif

            Debug.Log("<color=green>Overlay plugin initialized.</color>");
            Prepare();
        }
        private async void Prepare()
        {
            Camera = await GetMainCamera();
            #if!UNITY_EDITOR
            _rtTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            _pixelBuffer = new Color32[Screen.width * Screen.height];
            _handle = GCHandle.Alloc(_pixelBuffer, GCHandleType.Pinned);
            _clearFlags = Camera.clearFlags;
            _backgroundColor = Camera.backgroundColor;
            Camera.clearFlags = CameraClearFlags.Color;
            Camera.backgroundColor = Color.clear;
            Camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            #endif
            SceneManager.sceneLoaded += OnActiveSceneChanged;
            var firstCanvas = FindObjectsOfType<ClearBgCanvas>(true).FirstOrDefault(c => !c.transform.parent);
            if (firstCanvas) Canvas ??= firstCanvas.canvas;
            Canvas ??= FindObjectOfType<Canvas>();
            if (_settings.CanvasAutoConvert)
                ConvertAllCanvas();
            Application.targetFrameRate = _settings.TargetFPS;
            #if UNITY_EDITOR
            MonitorData = ClearBgManager.GetDefaultMonitorData();
            #else
            MonitorData = GetMonitorData(TargetMonitor);
            #endif
            Initialized = true;
            if(_settings.CanvasAutoConvert)
                ConvertAllCanvas();
            Debug.Log("<color=green>Transparent plugin fully initialized!</color>");
        }
        private void ConvertAllCanvas()
        {
            var canvases = FindObjectsOfType<ClearBgCanvas>(true);
            Canvas ??= canvases.FirstOrDefault(c => !c.transform.parent)?.canvas;
            Canvas ??= FindObjectOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                canvas.canvas.renderMode = RenderMode.ScreenSpaceCamera;
                if (!canvas.canvas.worldCamera)
                    canvas.canvas.worldCamera = Camera;
                canvas.canvas.planeDistance = Camera.nearClipPlane + 0.01f;
            }
        }
        public void ChangeMonitor(int monitorIndex)
        {
            #if UNITY_EDITOR
            Debug.Log("<color=yellow>Monitor changing is disabled in the editor. </color>");
            return;
            #endif
            if (!Initialized) return;
            var targetMonitor = Mathf.Clamp(monitorIndex, 0, Display.displays.Length - 1);
            TargetMonitor = targetMonitor;
            MonitorData = GetMonitorData(TargetMonitor);
            SetMonitorIndex(TargetMonitor);
        }
        public void SetOverlayEnabled(bool enable)
        {
            #if UNITY_EDITOR
            Debug.Log("<color=yellow>Overlay enabling/disabling is disabled in the editor. </color>");
            return;
            #endif
            if (enable)
            {
                _rtTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                _pixelBuffer = new Color32[Screen.width * Screen.height];
                _handle = GCHandle.Alloc(_pixelBuffer, GCHandleType.Pinned);
                Camera.clearFlags = CameraClearFlags.Color;
                Camera.backgroundColor = Color.clear;
                Camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                MonitorData = GetMonitorData(TargetMonitor);
            }
            else
            {
                if (Camera && Camera.targetTexture)
                {
                    Camera.targetTexture.Release();
                    Camera.targetTexture = null;
                }
                if (_handle.IsAllocated)
                    _handle.Free();
                if (_rtTex)
                    Destroy(_rtTex);
                _rtTex = null;
                _pixelBuffer = null;
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
                MonitorData = monitorData;
                Camera.clearFlags = _clearFlags;
                Camera.backgroundColor = _backgroundColor;
            }
            SetOverlayActive(enable);
        }
        private void OnDestroy()
        {
            if (!Initialized) return;
            SceneManager.sceneLoaded -= OnActiveSceneChanged;
            #if UNITY_EDITOR
            Debug.Log("<color=red>ClearBG has been disposed.</color>");
            return;
            #endif
            if (_handle.IsAllocated)
                _handle.Free();
        }
        private void OnActiveSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Initialized) return;
            if (!Camera)
                Camera = Camera.main;
            Canvas = FindObjectsOfType<ClearBgCanvas>().FirstOrDefault(c => !c.transform.parent).canvas;
            Canvas ??= FindObjectOfType<Canvas>();
            if (_settings.CanvasAutoConvert)
                ConvertAllCanvas();
        }
        private async Task<Camera> GetMainCamera()
        {
            var start = Time.time;
            while (!Camera && Time.time - start < 5f)
            {
                Camera = Camera.main;
                if (Camera)
                    break;
                await Task.Yield();
            }
            if (!Camera)
                Debug.LogError("<color=red>Main Camera does not exist.</color>");
            return Camera;
        }
        private void Update()
        {
            if (!Initialized || !Camera || !Camera.targetTexture) return;
            #if UNITY_EDITOR
            return;
            #endif
            if(!IsOverlayActive()) return;
            OverlayUpdate();
            AsyncGPUReadback.Request(Camera.targetTexture, 0, TextureFormat.RGBA32, OnCompleteReadback);
        }
        private void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || _pixelBuffer == null || !_handle.IsAllocated) return;
            try
            {
                request.GetData<Color32>().CopyTo(_pixelBuffer);
                BlitRawRGBA(_handle.AddrOfPinnedObject(), _rtTex.width, _rtTex.height);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ClearBG] GPU Readback skipped: {ex.Message}");
            }
        }
    }
}
