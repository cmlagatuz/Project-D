// Project D  |  Codename: Zantetsuken
// DeviceCapabilities.cs  (Settings — device-aware option availability)
//
// Detects what the current MOBILE device supports so the settings menu only offers
// valid choices (GCD 14.4 device tiers). Frame-rate options are gated by the panel's
// max refresh rate; quality presets are suggested from RAM / GPU tier. The pure
// logic (which fps are valid for a given refresh rate, tier -> preset) is
// engine-agnostic and unit-testable; the Unity probe fills the raw numbers.

using System.Collections.Generic;

namespace ProjectD.Settings
{
    public struct DeviceInfo
    {
        public int MaxRefreshRateHz;   // panel refresh (e.g. 60, 90, 120, 144)
        public int SystemMemoryMB;     // total RAM
        public int GpuMemoryMB;        // graphics memory (0 if unknown)
        public int ProcessorCount;     // logical cores
        public bool IsMobile;          // Android/iOS
    }

    public enum DeviceTier { Low, Mid, High }

    public static class DeviceCapabilities
    {
        /// <summary>Frame-rate caps valid for a device: never above the panel refresh, plus 30.</summary>
        public static List<FrameRateCap> SupportedFrameRates(int maxRefreshHz)
        {
            var all = new[] { FrameRateCap.FPS30, FrameRateCap.FPS60, FrameRateCap.FPS90, FrameRateCap.FPS120, FrameRateCap.FPS144 };
            var outList = new List<FrameRateCap>();
            foreach (var f in all)
                if ((int)f <= System.Math.Max(30, maxRefreshHz)) outList.Add(f);
            if (outList.Count == 0) outList.Add(FrameRateCap.FPS30);
            return outList;
        }

        /// <summary>Highest fps the device can actually present (clamped to a known cap).</summary>
        public static FrameRateCap MaxSupported(int maxRefreshHz)
        {
            var list = SupportedFrameRates(maxRefreshHz);
            return list[list.Count - 1];
        }

        /// <summary>Classify the device into a tier from RAM + cores (mobile-oriented thresholds).</summary>
        public static DeviceTier ClassifyTier(in DeviceInfo d)
        {
            // Simple, tunable heuristic. Adjust thresholds after real device profiling.
            int ram = d.SystemMemoryMB;
            int cores = d.ProcessorCount;
            if (ram >= 6000 && cores >= 8) return DeviceTier.High;
            if (ram >= 3500 && cores >= 6) return DeviceTier.Mid;
            return DeviceTier.Low;
        }

        /// <summary>Recommended default quality preset for a tier.</summary>
        public static QualityPreset RecommendedPreset(DeviceTier tier) => tier switch
        {
            DeviceTier.High => QualityPreset.High,
            DeviceTier.Mid => QualityPreset.Medium,
            _ => QualityPreset.Low
        };

        /// <summary>Recommended default frame cap for a tier, clamped to the panel.</summary>
        public static FrameRateCap RecommendedFrameRate(DeviceTier tier, int maxRefreshHz)
        {
            var supported = SupportedFrameRates(maxRefreshHz);
            FrameRateCap want = tier switch
            {
                DeviceTier.High => FrameRateCap.FPS120,
                DeviceTier.Mid => FrameRateCap.FPS60,
                _ => FrameRateCap.FPS30
            };
            // Clamp to the highest supported that is <= want, else the min supported.
            FrameRateCap best = supported[0];
            foreach (var f in supported) if ((int)f <= (int)want) best = f;
            return best;
        }
    }
}
