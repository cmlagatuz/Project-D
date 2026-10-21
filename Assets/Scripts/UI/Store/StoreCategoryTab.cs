// Project D  |  Codename: Zantetsuken
// StoreCategoryTab.cs  (Gold Shop UI — one category tab, e.g. "All" / "Boots" / "Exclusive")
//
// Distinct from any KP-store equivalent — this lives in ProjectD.UI.Store and is wired
// up by Editor/StoreScreenBuilder.cs. Field names below are load-bearing: the builder
// assigns them via SerializedObject.FindProperty("label"/"background"/"underline").

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ProjectD.UI.Store
{
    public sealed class StoreCategoryTab : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private Image underline;

        public string CategoryId { get; private set; } = "";
        public bool IsSelected { get; private set; }
        public event System.Action<string> OnClicked;

        private static readonly Color SelectedBg = new Color(0.18f, 0.20f, 0.28f);
        private static readonly Color UnselectedBg = new Color(0.12f, 0.14f, 0.20f);

        public void Bind(string categoryId, string displayText)
        {
            CategoryId = categoryId;
            if (label) label.text = displayText;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (background) background.color = selected ? SelectedBg : UnselectedBg;
            if (underline) underline.gameObject.SetActive(selected);
        }

        public void OnPointerClick(PointerEventData e) => OnClicked?.Invoke(CategoryId);
    }
}
