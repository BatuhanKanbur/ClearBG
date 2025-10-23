using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClearBG.Runtime.Scripts.Managers;
using ClearBG.Runtime.Scripts.Structures;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClearBG.Runtime.Scripts.Behaviours
{
    public class ClearBg : MonoBehaviour
    {
        #if UNITY_STANDALONE_WIN
        [DllImport("TransparentPlugin.dll")]
        private static extern bool InitOverlay(int monitorIndex);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void SetAlwaysOnTop(bool enable);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void OverlayUpdate();
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void SetMonitorIndex(int monitorIndex);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern MonitorData GetMonitorData(int monitorIndex);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void SetOverlayActive(bool enable);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern bool IsOverlayActive();
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void GetPerformanceStats(out float avgFrameTime);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern int GetPrimaryMonitorIndex();
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void SetClickThrough(bool enable);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern void UpdateClickThroughFromAlpha(float alphaValue,float alphaThreshold);
        
        [DllImport("TransparentPlugin.dll")]
        private static extern bool IsClickThroughEnabled();
        #endif
        public int TargetMonitor { get; private set; }
        public bool Initialized { get; private set; }
        public Camera Camera { get; private set; }
        public Canvas Canvas { get; private set; }
        public MonitorData MonitorData { get; private set; }
        private ClearBgSettings _settings;
        private CameraClearFlags _originalClearFlags;
        private Color _originalBackgroundColor;
        private RenderTexture _originalTargetTexture;
        private Texture2D _pixelReadTexture;
        private Rect _pixelReadRect = new Rect(0, 0, 1, 1);
        private Coroutine _clickThroughCoroutine;
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
            SetAlwaysOnTop(_settings.AlwaysOnTop);
            #endif
            Debug.Log("<color=green>Overlay plugin initialized.</color>");
            Prepare();
        }

        private async void Prepare()
        {
            Camera = await GetMainCamera();
            if (!Camera)
            {
                Debug.LogError("<color=red>Cannot initialize without main camera!</color>");
                return;
            }
            _originalClearFlags = Camera.clearFlags;
            _originalBackgroundColor = Camera.backgroundColor;
            _originalTargetTexture = Camera.targetTexture;
            #if !UNITY_EDITOR
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(0, 0, 0, 0);
            Camera.allowHDR = false;
            Camera.targetTexture = null;
            _pixelReadTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            #endif
            SceneManager.sceneLoaded += OnActiveSceneChanged;
            var prefab = Resources.Load<GameObject>("ClearBG_Canvas");
            if (!prefab)
            {
                Debug.LogError("<color=red>ClearBG Canvas prefab missing!</color>");
                return;
            }
            Canvas = Instantiate(prefab).GetComponent<Canvas>();
            DontDestroyOnLoad(Canvas);
            Canvas.worldCamera = Camera;
            Canvas.planeDistance = Camera.nearClipPlane + 0.01f;
            Application.targetFrameRate = _settings.TargetFPS;
            Application.runInBackground = true;
            #if UNITY_EDITOR
            MonitorData = ClearBgManager.GetDefaultMonitorData();
            #else
            MonitorData = GetMonitorData(TargetMonitor);
            #endif
            Initialized = true;
            Debug.Log("<color=green>ClearBG fully initialized with DWM transparency + Pixel-based clickthrough!</color>");
            #if !UNITY_EDITOR
            _clickThroughCoroutine = StartCoroutine(ClickThroughLoop());
            #endif
        }
        public void ChangeMonitor(int monitorIndex)
        {
            #if UNITY_EDITOR
            Debug.Log("<color=yellow>Monitor changing is disabled in the editor.</color>");
            return;
            #endif
            if (!Initialized) return;
            var targetMonitor = Mathf.Clamp(monitorIndex, 0, Display.displays.Length - 1);
            TargetMonitor = targetMonitor;
            SetMonitorIndex(TargetMonitor);
            MonitorData = GetMonitorData(TargetMonitor);
            Debug.Log($"<color=cyan>Switched to monitor {TargetMonitor}</color>");
        }

        public void SetOverlayEnabled(bool enable)
        {
            #if UNITY_EDITOR
            Debug.Log("<color=yellow>Overlay toggle is disabled in the editor.</color>");
            return;
            #endif
            if (!Initialized) return;
            if (enable)
            {
                Camera.clearFlags = CameraClearFlags.SolidColor;
                Camera.backgroundColor = new Color(0, 0, 0, 0);
                Camera.allowHDR = false;
                Camera.targetTexture = null;
                if (_pixelReadTexture == null)
                {
                    _pixelReadTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                }
                SetOverlayActive(true);
                SetAlwaysOnTop(_settings.AlwaysOnTop);
                MonitorData = GetMonitorData(TargetMonitor);
                if (_clickThroughCoroutine == null)
                {
                    _clickThroughCoroutine = StartCoroutine(ClickThroughLoop());
                }
                Debug.Log("<color=green>Overlay enabled (DWM transparency active)</color>");
            }
            else
            {
                Camera.clearFlags = _originalClearFlags;
                Camera.backgroundColor = _originalBackgroundColor;
                Camera.targetTexture = _originalTargetTexture;
                if (_clickThroughCoroutine != null)
                {
                    StopCoroutine(_clickThroughCoroutine);
                    _clickThroughCoroutine = null;
                }
                if (_pixelReadTexture)
                {
                    Destroy(_pixelReadTexture);
                    _pixelReadTexture = null;
                }
                SetOverlayActive(false);
                MonitorData = ClearBgManager.GetDefaultMonitorData();
                Debug.Log("<color=yellow>Overlay disabled (normal window mode)</color>");
            }
        }
        public void SetAlwaysOnTopEnabled(bool enable)
        {
            #if UNITY_EDITOR
            Debug.Log("<color=yellow>Always on top is disabled in the editor.</color>");
            return;
            #endif
            if (!Initialized) return;
            SetAlwaysOnTop(enable);
        }

        public void GetPerformance(out float avgFrameTime)
        {
            #if UNITY_EDITOR
            avgFrameTime = -1f;
            return;
            #endif
            if (!Initialized)
            {
                avgFrameTime = 0f;
                return;
            }
            GetPerformanceStats(out avgFrameTime);
        }
        private void OnDestroy()
        {
            if (!Initialized) return;
            SceneManager.sceneLoaded -= OnActiveSceneChanged;
            #if !UNITY_EDITOR
            if (_clickThroughCoroutine != null)
            {
                StopCoroutine(_clickThroughCoroutine);
                _clickThroughCoroutine = null;
            }
            if (_pixelReadTexture)
            {
                Destroy(_pixelReadTexture);
                _pixelReadTexture = null;
            }
            if (Camera)
            {
                Camera.clearFlags = _originalClearFlags;
                Camera.backgroundColor = _originalBackgroundColor;
                Camera.targetTexture = _originalTargetTexture;
            }
            #endif
            Debug.Log("<color=red>ClearBG disposed.</color>");
        }
        private void OnActiveSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Initialized) return;
            if (!Camera)
                Camera = Camera.main;
        }
        private async Task<Camera> GetMainCamera()
        {
            var start = Time.time;
            while (!Camera && Time.time - start < 5f)
            {
                Camera = Camera.main;
                if (Camera) break;
                await Task.Yield();
            }
            if (!Camera)
                Debug.LogError("<color=red>Main Camera does not exist.</color>");
            return Camera;
        }
        private IEnumerator ClickThroughLoop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!Initialized || !Camera || !IsOverlayActive() || !_pixelReadTexture)
                    continue;
                var mousePos = Input.mousePosition;
                mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width - 1);
                mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height - 1);
                _pixelReadRect.x = mousePos.x;
                _pixelReadRect.y = mousePos.y;
                _pixelReadTexture.ReadPixels(_pixelReadRect, 0, 0, false);
                _pixelReadTexture.Apply();
                var pixelColor = _pixelReadTexture.GetPixel(0, 0);
                var alphaValue = pixelColor.a;
                UpdateClickThroughFromAlpha(alphaValue,_settings.ClickThroughThreshold);
            }
        }
        private void LateUpdate()
        {
            #if UNITY_EDITOR
            return;
            #endif
            if (!Initialized || !Camera) return;
            if (!IsOverlayActive()) return;
            OverlayUpdate();
        }
    }
}