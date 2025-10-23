using System.Threading;
using System.Threading.Tasks;
using ClearBG.Runtime.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ClearBG.Runtime.Scripts.Behaviours
{
    public class DebugCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform leftIndicator, rightIndicator, topIndicator, bottomIndicator,taskbarIndicator;
        [SerializeField] private Text cpuPerformanceText;
        [SerializeField] private Toggle toggleOverlay;
        [SerializeField] private Transform cubeObject;
        private CancellationTokenSource _cts = new();

        private void Start()
        {
            _cts = new CancellationTokenSource();
            cubeObject.transform.SetParent(null);
            cubeObject.transform.localScale = Vector3.one;
            toggleOverlay.isOn = ClearBgManager.GetSettings().AlwaysOnTop;
            UpdateRects();
        }

        private void Update()
        {
            cubeObject.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
            if(Time.frameCount % ClearBgManager.GetSettings().TargetFPS * 2 != 0) return;
            ClearBgManager.GetPerformanceStats( out var cpuMs);
            if (Mathf.Approximately(cpuMs, -2f))
                cpuPerformanceText.text = "ClearBG Not Activated!";
            else if(Mathf.Approximately(cpuMs, -1f))
                cpuPerformanceText.text = "ClearBG does not work on Editor Mode!";
            else
                cpuPerformanceText.text = $"CPU Latency : {Mathf.Clamp(cpuMs,0,Mathf.Infinity):0} ms";
        }
        private async void UpdateRects()
        {
            try
            {
                await Task.Delay(100, cancellationToken: _cts.Token);
                var cubeRandomPosIndex = 0;
                while (!_cts.IsCancellationRequested)
                {
                    ClearBgManager.GetScreenRect(out var screenUILimits, out var screenWorldLimits);
                    ClearBgManager.GetTaskbarRect(out var taskbarUIPos, out var taskBarWorldPos);
                    leftIndicator.anchoredPosition = new Vector2(screenUILimits.Left, 0);
                    rightIndicator.anchoredPosition = new Vector2(screenUILimits.Right, 0);
                    topIndicator.anchoredPosition = new Vector2(0, screenUILimits.Top);
                    bottomIndicator.anchoredPosition = new Vector2(0, screenUILimits.Bottom);
                    taskbarIndicator.anchoredPosition = taskbarUIPos.Center;
                    taskbarIndicator.sizeDelta = new Vector2(taskbarUIPos.Size.x, taskbarUIPos.Size.y);
                    if (cubeRandomPosIndex > 4) cubeRandomPosIndex = 0;
                    switch (cubeRandomPosIndex)
                    {
                        case 0:
                            cubeObject.position = new Vector3(screenWorldLimits.Left + 0.5f, (screenWorldLimits.Top + screenWorldLimits.Bottom) / 2f, 0);
                            break;
                        case 1:
                            cubeObject.position = new Vector3(screenWorldLimits.Right - 0.5f, (screenWorldLimits.Top + screenWorldLimits.Bottom) / 2f, 0);
                            break;
                        case 2:
                            cubeObject.position = new Vector3((screenWorldLimits.Left + screenWorldLimits.Right) / 2f, screenWorldLimits.Top - 0.5f, 0);
                            break;
                        case 3:
                            cubeObject.position = new Vector3((screenWorldLimits.Left + screenWorldLimits.Right) / 2f, screenWorldLimits.Bottom + 0.5f, 0);
                            break;
                        case 4:
                            cubeObject.position = taskBarWorldPos.Center;
                            break;
                    }

                    await Task.Delay(5000, cancellationToken: _cts.Token);
                    cubeRandomPosIndex++;
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        public void LeftMonitor()
        {
            ClearBgManager.SetMonitorIndex(ClearBgManager.GetMonitorIndex - 1);
        }
        public void RightMonitor()
        {
            ClearBgManager.SetMonitorIndex(ClearBgManager.GetMonitorIndex + 1);
        }
        public void ActivateOverlay()
        {
            ClearBgManager.ActivateClearBg();
            UpdateRects();
        }
        public void DeactivateOverlay()
        {
            ClearBgManager.DeactivateClearBg();
        }
        public void ToggleOverlay()
        {
            if(ClearBgManager.GetSettings().AlwaysOnTop != toggleOverlay.isOn)
                ClearBgManager.SetAlwaysOnTop(toggleOverlay.isOn);
        }
    }
}
