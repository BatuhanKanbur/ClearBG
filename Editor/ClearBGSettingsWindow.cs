using ClearBG.Runtime.Scripts.Behaviours;
using ClearBG.Runtime.Scripts.Structures;
using UnityEditor;
using UnityEngine;

namespace ClearBG.Editor
{
    public class ClearBgSettingsWindow : EditorWindow
    {
        private const string FirstOpenKey = "ClearBGSettings_FirstOpen";
        private ClearBgSettings _settings;
        private readonly Vector2 _windowSize = new Vector2(450, 650);

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            if (EditorPrefs.HasKey(FirstOpenKey)) return;
            EditorPrefs.SetBool(FirstOpenKey, true);
            Open();
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
            _settings.CanvasAutoConvert = EditorGUILayout.Toggle("Canvas Auto Convert", _settings.CanvasAutoConvert);
            EditorGUILayout.LabelField("Automatically converts newly created Canvases to overlay compatible format.",
                EditorStyles.miniLabel);
            _settings.TargetFPS = EditorGUILayout.IntField("Target FPS", _settings.TargetFPS);
            EditorGUILayout.LabelField("Limits the overlay rendering to the specified frames per second.",
                EditorStyles.miniLabel);
            _settings.TargetDisplay = EditorGUILayout.IntField("Target Display", _settings.TargetDisplay);
            EditorGUILayout.LabelField("Specifies which monitor the overlay should render on.", EditorStyles.miniLabel);
            GUILayout.Space(15);
            if (GUILayout.Button("Save Settings", GUILayout.Height(35)))
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Convert All Canvases in Project", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Convert All Canvases",
                        "This will add ClearBGCanvas component to all Canvas objects in the current scenes and all prefabs in the project. Are you sure you want to proceed?",
                        "Yes", "No"))
                {
                    ConvertAllCanvases();
                }
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

        private static void ConvertAllCanvases()
        {
            var targetType = typeof(ClearBgCanvas);
            var addedCount = 0;
            var skippedCount = 0;
            var sceneCanvases = FindObjectsOfType<Canvas>(true);
            foreach (var canvas in sceneCanvases)
            {
                if (canvas.GetComponent(targetType) == null)
                {
                    Undo.AddComponent(canvas.gameObject, targetType);
                    addedCount++;
                }
                else
                    skippedCount++;
            }
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

            foreach (var guid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                if (scenePath == currentScenePath)
                    continue;
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath,
                    UnityEditor.SceneManagement.OpenSceneMode.Additive);
                var rootObjects = scene.GetRootGameObjects();
                foreach (var rootObj in rootObjects)
                {
                    var canvases = rootObj.GetComponentsInChildren<Canvas>(true);
                    foreach (var canvas in canvases)
                    {
                        if (canvas.GetComponent(targetType) == null)
                        {
                            Undo.AddComponent(canvas.gameObject, targetType);
                            EditorUtility.SetDirty(canvas.gameObject);
                            addedCount++;
                        }
                        else
                            skippedCount++;
                    }
                }
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                var prefabChanged = false;
                var canvases = prefab.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in canvases)
                {
                    if (canvas.GetComponent(targetType) == null)
                    {
                        Undo.AddComponent(canvas.gameObject, targetType);
                        EditorUtility.SetDirty(canvas.gameObject);
                        prefabChanged = true;
                        addedCount++;
                    }
                    else
                        skippedCount++;
                }

                if (!prefabChanged) continue;
                EditorUtility.SetDirty(prefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ {addedCount} ClearBGCanvas was added to Canvas, ⏩ {skippedCount} already existed.");
        }
    }
}
