using ClearBG.Runtime.Scripts.Structures;
using UnityEditor;
using UnityEngine;

namespace ClearBG.Editor
{
    internal static class ClearBgSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateClearBgSettingsProvider()
        {
            return new SettingsProvider("Project/Clear BG", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    var settings = ClearBgSettings.GetOrCreateSettings();

                    EditorGUILayout.Space(10);

                    // HEADER BAR
                    Rect headerRect = EditorGUILayout.GetControlRect(false, 60);
                    DrawGradientRect(headerRect, new Color(0.1f, 0.5f, 0.9f, 1f), new Color(0.2f, 0.7f, 1f, 1f));

                    GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 20,
                        normal = { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter
                    };
                    GUI.Label(headerRect, "Clear BG Overlay Plugin", headerStyle);

                    EditorGUILayout.Space(5);

                    // DESCRIPTION
                    GUIStyle descStyle = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14
                    };
                    EditorGUILayout.LabelField("A professional plugin to manage desktop overlays easily.", descStyle);

                    EditorGUILayout.Space(5);

                    // DEVELOPER NAME
                    GUIStyle devStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 15
                    };
                   

                    // ENGINEERING QUOTE
                    GUIStyle quoteStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        wordWrap = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13
                    };
                    EditorGUILayout.LabelField("\"Engineering is the art of directing the great sources of power in nature for the use and convenience of man.\"", quoteStyle);
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.LabelField("Batuhan Kanbur", devStyle);

                    EditorGUILayout.Space(5);

                    // WEBSITE LINK
                    GUIStyle linkStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        normal = { textColor = Color.cyan },
                        hover = { textColor = Color.blue }
                    };
                    Rect linkRect = EditorGUILayout.GetControlRect();
                    GUI.Label(linkRect, "www.batuhankanbur.com", linkStyle);
                    if (Event.current.type == EventType.MouseDown && linkRect.Contains(Event.current.mousePosition))
                    {
                        Application.OpenURL("https://www.batuhankanbur.com");
                    }

                    EditorGUILayout.Space(5);
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
                    settings.CanvasAutoConvert = EditorGUILayout.Toggle("Canvas Auto Convert", settings.CanvasAutoConvert);
                    EditorGUILayout.LabelField("Automatically converts newly created Canvases to overlay compatible format.", EditorStyles.miniLabel);
                    settings.TargetFPS = EditorGUILayout.IntField("Target FPS", settings.TargetFPS);
                    EditorGUILayout.LabelField("Limits the overlay rendering to the specified frames per second.", EditorStyles.miniLabel);
                    settings.TargetDisplay = EditorGUILayout.IntField("Target Display", settings.TargetDisplay);
                    EditorGUILayout.LabelField("Specifies which monitor the overlay should render on.", EditorStyles.miniLabel);

                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                    }
                }
            };
        }
        private static void DrawGradientRect(Rect position, Color topColor, Color bottomColor)
        {
            Texture2D tex = new Texture2D(1, 2);
            tex.SetPixels(new Color[] { topColor, bottomColor });
            tex.Apply();
            GUI.DrawTexture(position, tex);
            Object.DestroyImmediate(tex);
        }
    }
}
