// Project D  |  Codename: Zantetsuken
// PingWheel.cs  (Comms UI — radial ping wheel + quick-chat)
//
// A 6-slot radial ping wheel (hold to open, drag to a slot, release to fire) plus a
// quick-chat strip. Selecting a slot raises OnPing with the PingType; the broadcaster
// (server) rate-limits and delivers to allies. Also fires OnSfxCue for the ping sound.
// The wheel is rebuilt from QuickChat.DefaultWheel so slots stay in sync with design.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Comms;

namespace ProjectD.CommsUI
{
    public sealed class PingWheel : MonoBehaviour
    {
        [SerializeField] private RectTransform wheelRoot;      // container shown while open
        [SerializeField] private Button slotPrefab;            // one radial slot
        [SerializeField] private float radius = 150f;
        [SerializeField] private RectTransform quickChatStrip;
        [SerializeField] private Button quickChatButtonPrefab;

        public event System.Action<PingType> OnPing;
        public event System.Action<string> OnQuickChat;       // phrase key
        public event System.Action<string> OnSfxCue;

        private readonly List<Button> _slots = new();
        private bool _open;

        private void Start()
        {
            BuildWheel();
            BuildQuickChat();
            SetOpen(false);
        }

        private void BuildWheel()
        {
            if (wheelRoot == null || slotPrefab == null) return;
            var types = QuickChat.DefaultWheel;
            int n = types.Length;
            for (int i = 0; i < n; i++)
            {
                var slot = Instantiate(slotPrefab, wheelRoot);
                slot.gameObject.SetActive(true);
                // Position clockwise from the top.
                float ang = (Mathf.PI * 2f * i / n) - Mathf.PI / 2f;
                var rt = (RectTransform)slot.transform;
                rt.anchoredPosition = new Vector2(Mathf.Cos(ang) * radius, -Mathf.Sin(ang) * radius);
                var label = slot.GetComponentInChildren<Text>();
                if (label) label.text = QuickChat.Label(types[i]);
                var t = types[i];
                slot.onClick.AddListener(() => FirePing(t));
                _slots.Add(slot);
            }
        }

        private void BuildQuickChat()
        {
            if (quickChatStrip == null || quickChatButtonPrefab == null) return;
            foreach (var key in QuickChat.QuickChatPhrases)
            {
                var b = Instantiate(quickChatButtonPrefab, quickChatStrip);
                b.gameObject.SetActive(true);
                var label = b.GetComponentInChildren<Text>();
                if (label) label.text = key.Replace("chat.", "").Replace("_", " ");
                var k = key;
                b.onClick.AddListener(() => OnQuickChat?.Invoke(k));
            }
        }

        public void Toggle() => SetOpen(!_open);

        public void SetOpen(bool open)
        {
            _open = open;
            if (wheelRoot) wheelRoot.gameObject.SetActive(open);
        }

        private void FirePing(PingType type)
        {
            OnPing?.Invoke(type);
            OnSfxCue?.Invoke($"sfx_ping_{type.ToString().ToLowerInvariant()}");
            SetOpen(false);
        }
    }
}
