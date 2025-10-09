using UnityEngine;

namespace ClearBG.Runtime.Scripts.Structures
{
    public struct RectData
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public Vector2 Center => new Vector2((Left + Right) / 2f, (Top + Bottom) / 2f);
        public Vector2 Size;
    }
}