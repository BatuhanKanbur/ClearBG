using ClearBG.Runtime.Scripts.Behaviours;
using ClearBG.Runtime.Scripts.Structures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClearBG.Editor
{
    public class ClearBgSettingsWindow : EditorWindow
    {
        private const string FirstOpenKey = "ClearBGSettings_FirstOpen";
        private ClearBgSettings _settings;
        private readonly Vector2 _windowSize = new(450, 800);

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            if (EditorPrefs.HasKey(FirstOpenKey)) return;
            EditorPrefs.SetBool(FirstOpenKey, true);
            ConfigurePlayerSettings();
            Open();
        }
        private static void ConfigurePlayerSettings()
        {
            Debug.Log("<color=cyan>[ClearBG] Configuring Player Settings for transparent overlay...</color>");
            var changedSomething = false;
            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64))
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Auto Graphics API disabled</color>");
            }
            var currentAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            if (currentAPIs.Length == 0 || currentAPIs[0] != GraphicsDeviceType.Direct3D11)
            {
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Graphics API set to Direct3D11</color>");
            }
            if (PlayerSettings.useFlipModelSwapchain)
            {
                PlayerSettings.useFlipModelSwapchain = false;
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ DXGI Flip Model disabled (CRITICAL for transparency!)</color>");
            }
            if (PlayerSettings.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Fullscreen Mode set to Fullscreen Window</color>");
            }
            if (!PlayerSettings.runInBackground)
            {
                PlayerSettings.runInBackground = true;
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Run In Background enabled</color>");
            }
            if (PlayerSettings.resizableWindow)
            {
                PlayerSettings.resizableWindow = false;
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Resizable Window disabled</color>");
            }
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                PlayerSettings.colorSpace = ColorSpace.Linear;
                changedSomething = true;
                Debug.Log("<color=green>[ClearBG] ✓ Color Space set to Linear</color>");
            }
            if (PlayerSettings.SplashScreen.show)
                PlayerSettings.SplashScreen.show = false;
            if (changedSomething)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("<color=cyan>[ClearBG] ✓✓✓ Player Settings configured successfully! ✓✓✓</color>");
                Debug.Log("<color=yellow>[ClearBG] You may need to restart Unity Editor for all changes to take effect.</color>");
            }
            else
            {
                Debug.Log("<color=green>[ClearBG] Player Settings already configured correctly!</color>");
            }
        }
        [MenuItem("Clear BG/Settings")]
        public static void Open()
        {
            var window = GetWindow<ClearBgSettingsWindow>("Clear BG Settings");
            window.minSize = window._windowSize;
            window.maxSize = window._windowSize;
            window.position = new Rect(100, 100, window._windowSize.x, window._windowSize.y);
            window.Show();
        }
        
        [MenuItem("Clear BG/Configure Player Settings")]
        public static void ConfigureSettings()
        {
            ConfigurePlayerSettings();
        }

        private void OnEnable()
        {
            _settings = ClearBgSettings.GetOrCreateSettings();
            minSize = _windowSize;
            maxSize = _windowSize;
        }

        private void OnGUI()
        {
            if (!Mathf.Approximately(position.width, _windowSize.x) ||
                !Mathf.Approximately(position.height, _windowSize.y))
                position = new Rect(position.x, position.y, _windowSize.x, _windowSize.y);
            if (!_settings)
            {
                EditorGUILayout.HelpBox("Settings asset not found.", MessageType.Warning);
                if (GUILayout.Button("Create Settings"))
                    _settings = ClearBgSettings.GetOrCreateSettings();
                return;
            }
            var headerRect = EditorGUILayout.GetControlRect(false, 70);
            DrawGradientRect(headerRect, new Color(0.1f, 0.5f, 0.9f, 1f), new Color(0.2f, 0.7f, 1f, 1f));
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(headerRect, "Clear BG Overlay Plugin", headerStyle);
            EditorGUILayout.Space();
            var descStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            EditorGUILayout.LabelField("A professional plugin to manage desktop overlays easily.", descStyle);
            EditorGUILayout.Space(5);
            var devStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            };

            var quoteStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13
            };
            EditorGUILayout.LabelField(
                "\"Engineering is the art of directing the great sources of power in nature for the use and convenience of man.\"",
                quoteStyle);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Batuhan Kanbur", devStyle);
            EditorGUILayout.Space(5);
            var linkStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.cyan },
                hover = { textColor = Color.blue }
            };
            var linkRect = EditorGUILayout.GetControlRect();
            GUI.Label(linkRect, "www.batuhankanbur.com", linkStyle);
            if (Event.current.type == EventType.MouseDown && linkRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://www.batuhankanbur.com");
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Player Settings Configuration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("ClearBG requires specific Player Settings to work correctly.", EditorStyles.miniLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("This will configure:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • Graphics API → Direct3D11", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • DXGI Flip Model → OFF (critical!)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • Fullscreen Mode → Fullscreen Window", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("  • Run In Background → ON", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Remember, this plugin does not work in editor mode, only in builds!",
                EditorStyles.miniLabel);
            _settings.AutoInitialize = EditorGUILayout.Toggle("Auto Initialize", _settings.AutoInitialize);
            EditorGUILayout.LabelField("Automatically initializes the overlay when game starts.",
                EditorStyles.miniLabel);
            _settings.DebugMode = EditorGUILayout.Toggle("Debug Mode", _settings.DebugMode);
            EditorGUILayout.LabelField("Enables debug logs for troubleshooting purposes.", EditorStyles.miniLabel);
            _settings.AlwaysOnTop = EditorGUILayout.Toggle("Always On Top", _settings.AlwaysOnTop);
            EditorGUILayout.LabelField("Keeps the overlay window always on top of other windows.",
                EditorStyles.miniLabel);
            _settings.ClickThrough = EditorGUILayout.Toggle("Click Through", _settings.ClickThrough);
            EditorGUILayout.LabelField("Allows clicks to pass through the overlay to windows beneath it.",
                EditorStyles.miniLabel);
            _settings.TargetFPS = EditorGUILayout.IntField("Target FPS", _settings.TargetFPS);
            EditorGUILayout.LabelField("Limits the overlay rendering to the specified frames per second.",
                EditorStyles.miniLabel);
            _settings.TargetDisplay = EditorGUILayout.IntField("Target Display", _settings.TargetDisplay);
            EditorGUILayout.LabelField("Specifies which monitor the overlay should render on.", EditorStyles.miniLabel);
            EditorGUILayout.Space(20);
            if (GUILayout.Button("Save Settings", GUILayout.Height(35)))
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("🔧 Auto Configure Player Settings", GUILayout.Height(40)))
            {
                ConfigureSettings();
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawGradientRect(Rect pos, Color topColor, Color bottomColor)
        {
            var tex = new Texture2D(1, 2);
            tex.SetPixels(new Color[] { topColor, bottomColor });
            tex.Apply();
            GUI.DrawTexture(pos, tex);
            DestroyImmediate(tex);
        }
    }
}