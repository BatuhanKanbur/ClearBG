using System;
using UnityEngine;

namespace ClearBG.Runtime.Scripts.Behaviours
{
    public class ClearBg3DInputTest : MonoBehaviour
    {
        private bool _isDragging = false;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void OnMouseDown()
        {
            _isDragging = true;
        }

        private void OnMouseUp()
        {
            _isDragging = false;
        }

        private void Update()
        {
            if (!_isDragging) return;
            var targetPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = -3;
            transform.position = targetPos;
        }
    }
}
