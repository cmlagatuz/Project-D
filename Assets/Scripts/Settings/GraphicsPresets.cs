// Project D  |  Codename: Zantetsuken
// GraphicsPresets.cs  (Settings — Low/Medium/High preset definitions)
//
// Maps the three quality presets to concrete per-option values, and detects when
// the current graphics settings match a preset (else they're "Custom"). Any option
// can be edited individually; changing one flips the preset to Custom. Frame-rate
// cap is deliberately NOT part of a preset match (players cap fps independently of
// visual quality). Pure C# — unit-testable.

namespace ProjectD.Settings
{
    public static class GraphicsPresets
    {
        /// <summary>Overwrite the per-option graphics fields for a preset (keeps frameRateCap).</summary>
        public static void Apply(GraphicsSettings g, QualityPreset preset)
        {
            switch (preset)
            {
                case QualityPreset.Low:
                    g.shadows = GraphicsLevel.Low;
                    g.textures = GraphicsLevel.Low;
                    g.effects = GraphicsLevel.Low;
                    g.modelDetail = GraphicsLevel.Low;
                    g.antiAliasing = false;
                    g.bloom = false;
                    g.renderScale = 0.75f;
                    break;
                case QualityPreset.Medium:
                    g.shadows = GraphicsLevel.Medium;
                    g.textures = GraphicsLevel.Medium;
                    g.effects = GraphicsLevel.Medium;
                    g.modelDetail = GraphicsLevel.Medium;
                    g.antiAliasing = true;
                    g.bloom = true;
                    g.renderScale = 0.9f;
                    break;
                case QualityPreset.High:
                    g.shadows = GraphicsLevel.High;
                    g.textures = GraphicsLevel.High;
                    g.effects = GraphicsLevel.High;
                    g.modelDetail = GraphicsLevel.High;
                    g.antiAliasing = true;
                    g.bloom = true;
                    g.renderScale = 1.0f;
                    break;
                case QualityPreset.Custom:
                default:
                    // leave individual values as-is
                    break;
            }
            if (preset != QualityPreset.Custom) g.preset = preset;
        }

        /// <summary>Return the preset the current values match, or Custom.</summary>
        public static QualityPreset Detect(GraphicsSettings g)
        {
            if (Matches(g, QualityPreset.Low)) return QualityPreset.Low;
            if (Matches(g, QualityPreset.Medium)) return QualityPreset.Medium;
            if (Matches(g, QualityPreset.High)) return QualityPreset.High;
            return QualityPreset.Custom;
        }

        /// <summary>Re-evaluate g.preset after an individual option changed.</summary>
        public static void RefreshPreset(GraphicsSettings g) => g.preset = Detect(g);

        private static bool Matches(GraphicsSettings g, QualityPreset preset)
        {
            var probe = new GraphicsSettings();
            Apply(probe, preset);
            return g.shadows == probe.shadows &&
                   g.textures == probe.textures &&
                   g.effects == probe.effects &&
                   g.modelDetail == probe.modelDetail &&
                   g.antiAliasing == probe.antiAliasing &&
                   g.bloom == probe.bloom &&
                   Approximately(g.renderScale, probe.renderScale);
        }

        private static bool Approximately(float a, float b) => System.Math.Abs(a - b) < 0.001f;
    }
}
