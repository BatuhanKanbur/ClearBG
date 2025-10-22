using ClearBG.Runtime.Scripts.Structures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ClearBG.Editor
{
    internal static class ClearBgSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateClearBgSettingsProvider()
        {
            return new SettingsProvider("Project/Clear BG", SettingsScope.Project)
            {
                guiHandler = (_) =>
                {
                    var settings = ClearBgSettings.GetOrCreateSettings();
                    EditorGUILayout.Space(10);
                    var headerRect = EditorGUILayout.GetControlRect(false, 60);
                    DrawGradientRect(headerRect, new Color(0.1f, 0.5f, 0.9f, 1f), new Color(0.2f, 0.7f, 1f, 1f));
                    var headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 20,
                        normal = { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter
                    };
                    GUI.Label(headerRect, "Clear BG Overlay Plugin", headerStyle);
                    EditorGUILayout.Space(5);
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
                    EditorGUILayout.LabelField("\"Engineering is the art of directing the great sources of power in nature for the use and convenience of man.\"", quoteStyle);
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
                    EditorGUILayout.LabelField("  • Color Space → Linear", EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Remember, this plugin does not work in editor mode, only in builds!", EditorStyles.miniLabel);
                    settings.AutoInitialize = EditorGUILayout.Toggle("Auto Initialize", settings.AutoInitialize);
                    EditorGUILayout.LabelField("Automatically initializes the overlay when game starts.", EditorStyles.miniLabel);
                    settings.DebugMode = EditorGUILayout.Toggle("Debug Mode", settings.DebugMode);
                    EditorGUILayout.LabelField("Enables debug logs for troubleshooting.", EditorStyles.miniLabel);
                    settings.AlwaysOnTop = EditorGUILayout.Toggle("Always On Top", settings.AlwaysOnTop);
                    EditorGUILayout.LabelField("Keeps the overlay window always on top of other windows.", EditorStyles.miniLabel);
                    settings.ClickThrough = EditorGUILayout.Toggle("Click Through", settings.ClickThrough);
                    EditorGUILayout.LabelField("Allows clicks to pass through the overlay to windows beneath it.", EditorStyles.miniLabel);
                    settings.TargetFPS = EditorGUILayout.IntField("Target FPS", settings.TargetFPS);
                    EditorGUILayout.LabelField("Limits the overlay rendering to the specified frames per second.", EditorStyles.miniLabel);
                    settings.TargetDisplay = EditorGUILayout.IntField("Target Display", settings.TargetDisplay);
                    EditorGUILayout.LabelField("Specifies which monitor the overlay should render on.", EditorStyles.miniLabel);
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                    }
                    EditorGUILayout.Space(5);
                    if (GUILayout.Button("🔧 Auto Configure Player Settings", GUILayout.Height(35)))
                    {
                        ConfigurePlayerSettings();
                    }
                }
            };
        }
        
        /// <summary>
        /// Automatically configures Player Settings for optimal ClearBG performance.
        /// </summary>
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
                EditorUtility.DisplayDialog(
                    "ClearBG Configuration Complete", 
                    "Player Settings have been configured successfully!\n\n" +
                    "✓ Graphics API: Direct3D11\n" +
                    "✓ DXGI Flip Model: OFF\n" +
                    "✓ Fullscreen Mode: Fullscreen Window\n" +
                    "✓ Run In Background: ON\n" +
                    "✓ Color Space: Linear\n\n" +
                    "You may need to restart Unity Editor for all changes to take effect.",
                    "OK"
                );
                Debug.Log("<color=cyan>[ClearBG] ✓✓✓ Player Settings configured successfully! ✓✓✓</color>");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "ClearBG Configuration", 
                    "Player Settings are already configured correctly!",
                    "OK"
                );
                Debug.Log("<color=green>[ClearBG] Player Settings already configured correctly!</color>");
            }
        }
        
        private static void DrawGradientRect(Rect position, Color topColor, Color bottomColor)
        {
            var tex = new Texture2D(1, 2);
            tex.SetPixels(new Color[] { topColor, bottomColor });
            tex.Apply();
            GUI.DrawTexture(position, tex);
            Object.DestroyImmediate(tex);
        }
    }
}