using UnityEngine;
using Unity.XR.Oculus;

namespace InkRidge.Core
{
    /// <summary>
    /// One-time Quest runtime performance bootstrap.
    /// Sets display refresh rate, CPU/GPU levels and Fixed Foveated Rendering
    /// before the first scene loads. Registering via RuntimeInitializeOnLoadMethod
    /// keeps it independent of scene setup order.
    /// </summary>
    public static class VRPerformanceBootstrap
    {
        // Quest 3 safe refresh rates: 72 (default), 90 (headset default OS value).
        private const float TargetRefreshRate = 72f;

        // FFR levels: 0 off, 1 low, 2 medium, 3 high, 4 high-top.
        private const int FoveationLevel = 2;

        // CPU/GPU levels: 0 low, 1 medium, 2 high, 3 fixed-high.
        private const int CpuLevel = 2;
        private const int GpuLevel = 2;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Application.targetFrameRate = 72;
            QualitySettings.vSyncCount = 0;

            Performance.TrySetDisplayRefreshRate(TargetRefreshRate);
            Performance.TrySetCPULevel(CpuLevel);
            Performance.TrySetGPULevel(GpuLevel);

            if (Utils.SetFoveationLevel(FoveationLevel))
            {
                Debug.Log("[VRPerf] FFR level " + FoveationLevel + " set");
            }
        }
    }
}
