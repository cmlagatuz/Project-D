// Project D  |  Codename: Zantetsuken
// ChampionCard.cs  (Champion Select grid tile)
//
// A single selectable champion tile in the roster grid. Shows the champion name,
// role, and a difficulty pip; raises OnClicked with its champion id; and can be
// dimmed when banned/taken. Portraits are swapped in when art is available.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectD.Data;

namespace ProjectD.UI.ChampionSelect
{
    public sealed class ChampionCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text roleLabel;
        [SerializeField] private Image portrait;
        [SerializeField] private Image frame;
        [SerializeField] private Image dimOverlay;

        public string ChampionId { get; private set; }

        public event System.Action<string> OnClicked;
        public event System.Action<string> OnHovered;

        public void Bind(ChampionDefinition def)
        {
            ChampionId = def.championId;
            if (nameLabel != null) nameLabel.text = def.displayName;
            if (roleLabel != null) roleLabel.text = def.primaryRole.ToString();
            if (portrait != null && def.modelPrefab == null)
                portrait.color = RoleColor(def.primaryRole); // placeholder swatch until art
        }

        public void SetSelectable(bool selectable)
        {
            if (dimOverlay != null)
            {
                dimOverlay.gameObject.SetActive(!selectable);
                dimOverlay.color = new Color(0f, 0f, 0f, selectable ? 0f : 0.62f);
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (frame != null)
                frame.color = highlighted ? new Color(0.36f, 0.75f, 1f) : new Color(1f, 1f, 1f, 0.25f);
        }

        public void OnPointerClick(PointerEventData e) => OnClicked?.Invoke(ChampionId);
        public void OnPointerEnter(PointerEventData e) => OnHovered?.Invoke(ChampionId);

        public static Color RoleColor(ProjectD.Core.ChampionRole role) => role switch
        {
            ProjectD.Core.ChampionRole.Vanguard => new Color(0.35f, 0.55f, 0.95f),
            ProjectD.Core.ChampionRole.Fighter  => new Color(0.90f, 0.55f, 0.35f),
            ProjectD.Core.ChampionRole.Assassin => new Color(0.85f, 0.35f, 0.45f),
            ProjectD.Core.ChampionRole.Marksman => new Color(0.90f, 0.78f, 0.35f),
            ProjectD.Core.ChampionRole.Mage     => new Color(0.65f, 0.40f, 0.90f),
            ProjectD.Core.ChampionRole.Support  => new Color(0.40f, 0.80f, 0.55f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
    }
}
