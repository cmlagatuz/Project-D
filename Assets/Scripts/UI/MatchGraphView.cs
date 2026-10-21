// Project D  |  Codename: Zantetsuken
// MatchGraphView.cs  (Graphs UI — gold/XP-over-time line + breakdown bars)
//
// Draws the post-game gold (and XP) difference line from MatchGraphModel using a
// LineRenderer-free approach (UI Image segments), plus a stacked gold-source breakdown
// bar from EconomyBreakdown. The zero line is centered; blue lead goes up, red down.
// Bind a MatchGraphModel + EconomyBreakdown.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Graphs;

namespace ProjectD.GraphsUI
{
    public sealed class MatchGraphView : MonoBehaviour
    {
        [Header("Gold-diff line")]
        [SerializeField] private RectTransform graphArea;      // the plotting rect
        [SerializeField] private Image segmentPrefab;          // a thin image reused as a segment
        [SerializeField] private Text peakBlueText;
        [SerializeField] private Text peakRedText;
        [SerializeField] private Text finalDiffText;

        [Header("Gold breakdown")]
        [SerializeField] private RectTransform breakdownBar;   // horizontal stacked bar
        [SerializeField] private Image breakdownSegmentPrefab;

        private readonly List<Image> _segments = new();

        public void Bind(MatchGraphModel graph, EconomyBreakdown breakdown)
        {
            DrawGoldLine(graph);
            DrawBreakdown(breakdown);
        }

        private void DrawGoldLine(MatchGraphModel graph)
        {
            foreach (var s in _segments) if (s) Destroy(s.gameObject);
            _segments.Clear();
            if (graphArea == null || segmentPrefab == null || graph.Gold.Count < 2) return;

            var samples = graph.Gold;
            float w = graphArea.rect.width, h = graphArea.rect.height;
            int maxAbs = 1;
            foreach (var s in samples) maxAbs = Mathf.Max(maxAbs, Mathf.Abs(s.Diff));

            Vector2 Prev = default; bool first = true;
            for (int i = 0; i < samples.Count; i++)
            {
                float x = w * i / (samples.Count - 1) - w / 2f;
                float y = (samples[i].Diff / (float)maxAbs) * (h / 2f);
                var p = new Vector2(x, y);
                if (!first) DrawSegment(Prev, p, samples[i].Diff >= 0);
                Prev = p; first = false;
            }

            if (peakBlueText) peakBlueText.text = $"Blue peak +{graph.PeakGoldLeadBlue()}g";
            if (peakRedText) peakRedText.text = $"Red peak +{graph.PeakGoldLeadRed()}g";
            if (finalDiffText)
            {
                int d = graph.FinalGoldDiff;
                finalDiffText.text = d == 0 ? "Even gold" : (d > 0 ? $"Blue +{d}g" : $"Red +{-d}g");
                finalDiffText.color = d >= 0 ? new Color(0.4f,0.65f,1f) : new Color(1f,0.45f,0.45f);
            }
        }

        private void DrawSegment(Vector2 a, Vector2 b, bool blueSide)
        {
            var seg = Instantiate(segmentPrefab, graphArea);
            seg.gameObject.SetActive(true);
            var rt = (RectTransform)seg.transform;
            Vector2 dir = b - a; float len = dir.magnitude;
            rt.sizeDelta = new Vector2(len, 4f);
            rt.anchoredPosition = a + dir * 0.5f;
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            seg.color = blueSide ? new Color(0.4f,0.65f,1f) : new Color(1f,0.45f,0.45f);
            _segments.Add(seg);
        }

        private void DrawBreakdown(EconomyBreakdown breakdown)
        {
            if (breakdownBar == null || breakdownSegmentPrefab == null || breakdown == null) return;
            foreach (Transform c in breakdownBar) if (c != breakdownBar) Destroy(c.gameObject);

            var fractions = breakdown.GoldFractions();
            float x = 0f, w = breakdownBar.rect.width;
            foreach (var kv in fractions)
            {
                var seg = Instantiate(breakdownSegmentPrefab, breakdownBar);
                seg.gameObject.SetActive(true);
                var rt = (RectTransform)seg.transform;
                float segW = kv.Value * w;
                rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 0.5f); rt.sizeDelta = new Vector2(segW, 0); rt.anchoredPosition = new Vector2(x, 0);
                seg.color = SourceColor(kv.Key);
                x += segW;
            }
        }

        private static Color SourceColor(GoldSource s) => s switch
        {
            GoldSource.Minions => new Color(0.4f,0.7f,0.5f), GoldSource.Kills => new Color(0.9f,0.5f,0.4f),
            GoldSource.Assists => new Color(0.9f,0.75f,0.4f), GoldSource.Objectives => new Color(0.6f,0.45f,0.85f),
            GoldSource.Structures => new Color(0.5f,0.6f,0.9f), GoldSource.Passive => new Color(0.5f,0.55f,0.6f),
            _ => new Color(0.7f,0.7f,0.4f)
        };
    }
}
