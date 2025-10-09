using ClearBG.Runtime.Scripts.Behaviours;
using UnityEditor;
using UnityEngine;

namespace ClearBG.Editor
{
    [CustomEditor(typeof(Canvas))]
    public class ClearBgCanvasProvider : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var canvas = (Canvas)target;
            var controller = canvas.GetComponent<ClearBgCanvas>();
            if (!controller)
            {
                EditorGUILayout.HelpBox("⚠️ UI Canvases require the ClearBGCanvas component to appear in the overlay area. Please review the documentation for details.", MessageType.Error);
                if (GUILayout.Button("Set as Clear BG Canvas"))
                {
                    controller = canvas.gameObject.AddComponent<ClearBgCanvas>();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✅ The canvas is set to appear in the overlay area.", MessageType.Info);
            }
        }
    }
}
