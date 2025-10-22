using System.Runtime.InteropServices;
using UnityEngine;

namespace ClearBG.Runtime.Scripts.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorData
    {
        public int screenWidth;
        public int screenHeight;
        public int monitorLeft;
        public int monitorTop;
        public int monitorRight;
        public int monitorBottom;
        public int workLeft;
        public int workTop;
        public int workRight;
        public int workBottom;
        public int taskbarLeft;
        public int taskbarTop;
        public int taskbarRight;
        public int taskbarBottom;
        public int taskbarEdge;
    }
}