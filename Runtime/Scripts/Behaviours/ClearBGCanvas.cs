using UnityEngine;

namespace ClearBG.Runtime.Scripts.Behaviours
{
    [RequireComponent(typeof(Canvas))]
    public class ClearBgCanvas : MonoBehaviour
    {
        [HideInInspector]public Canvas canvas;
        private void Awake()
        {
            canvas.worldCamera ??= Camera.main;
        }

        private void OnValidate()
        {
            if(canvas) return;
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            if (canvas.worldCamera) canvas.planeDistance = canvas.worldCamera.nearClipPlane + 0.01f;
        }
    }
}
