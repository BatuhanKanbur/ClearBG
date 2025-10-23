using UnityEditor;
using UnityEngine;

namespace ClearBG.Runtime.Scripts.Structures
{
    public class ClearBgSettings : ScriptableObject
    {
        public bool AutoInitialize = true;
        public bool DebugMode = false;
        public bool AlwaysOnTop = true;
        [Range(0f,1f)]public float ClickThroughThreshold = 0.1f;
        public int TargetFPS = 60;
        public int TargetDisplay = 0;
        private static ClearBgSettings _instance;
        private const string ResourcePath = "Assets/Resources/ClearBGSettings.asset";
        public static ClearBgSettings GetOrCreateSettings()
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<ClearBgSettings>(ResourcePath);
            if (settings) return settings;
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            settings = CreateInstance<ClearBgSettings>();
            AssetDatabase.CreateAsset(settings, ResourcePath);
            AssetDatabase.SaveAssets();
            return settings;
#else
        return Resources.Load<ClearBgSettings>("ClearBgSettings");
#endif
        }
    }
}
