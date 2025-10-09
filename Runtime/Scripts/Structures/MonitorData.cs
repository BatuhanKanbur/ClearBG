using System.Runtime.InteropServices;

namespace ClearBG.Runtime.Scripts.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorData
    {
        public int screenWidth, screenHeight;
        public int monitorLeft, monitorTop, monitorRight, monitorBottom;
        public int workLeft, workTop, workRight, workBottom;
        public int taskbarLeft, taskbarTop, taskbarRight, taskbarBottom;
        public int taskbarEdge;
    }
}