// Project D  |  Codename: Zantetsuken
// MissionCard.cs  (Live-ops UI — one mission/quest tile)
//
// Shows a mission's title, description, a progress bar (progress/target), its rewards
// (XP / KP / chest fragments), and a CLAIM button that lights up when complete. Bind a
// MissionDefinition + MissionState; raises OnClaim so the screen grants + refreshes.

using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.LiveOps;

namespace ProjectD.LiveOpsUI
{
    public sealed class MissionCard : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image cadenceTag;
        [SerializeField] private Text cadenceLabel;      // DAILY / WEEKLY / EVENT
        [SerializeField] private Text titleText;
        [SerializeField] private Text descText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;      // "2 / 3"
        [SerializeField] private Text rewardText;        // "+500 XP  +20 KP"
        [SerializeField] private Button claimButton;
        [SerializeField] private Text claimLabel;

        public string MissionId { get; private set; }
        public event System.Action<string> OnClaim;

        private static readonly Color Daily = new Color(0.35f, 0.62f, 0.92f);
        private static readonly Color Weekly = new Color(0.65f, 0.40f, 0.90f);
        private static readonly Color Event = new Color(0.95f, 0.72f, 0.30f);

        public void Bind(MissionDefinition def, MissionState state)
        {
            MissionId = def.missionId;
            if (titleText) titleText.text = def.title;
            if (descText) descText.text = def.description;

            if (cadenceLabel) cadenceLabel.text = def.cadence.ToString().ToUpperInvariant();
            if (cadenceTag) cadenceTag.color = def.cadence switch { MissionCadence.Daily => Daily, MissionCadence.Weekly => Weekly, _ => Event };

            int prog = state != null ? state.progress : 0;
            float pct = def.targetProgress > 0 ? (float)prog / def.targetProgress : 0f;
            if (progressFill) progressFill.fillAmount = Mathf.Clamp01(pct);
            if (progressText) progressText.text = $"{prog} / {def.targetProgress}";
            if (rewardText) rewardText.text = FormatRewards(def.rewards);

            bool claimable = state != null && state.completed && !state.claimed;
            bool claimed = state != null && state.claimed;
            if (claimButton)
            {
                claimButton.interactable = claimable;
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(() => OnClaim?.Invoke(MissionId));
            }
            if (claimLabel) claimLabel.text = claimed ? "CLAIMED" : (claimable ? "CLAIM" : "IN PROGRESS");
            if (background) background.color = claimed ? new Color(0.11f,0.16f,0.13f,0.95f) : new Color(0.13f,0.15f,0.21f,0.95f);
        }

        private static string FormatRewards(MissionReward[] rewards)
        {
            if (rewards == null || rewards.Length == 0) return "";
            var sb = new StringBuilder();
            foreach (var r in rewards)
            {
                if (sb.Length > 0) sb.Append("   ");
                sb.Append(r.kind switch
                {
                    MissionRewardKind.AccountXp => $"+{r.amount} XP",
                    MissionRewardKind.KP => $"+{r.amount} KP",
                    _ => $"+{r.amount} \u25C6 Fragment"
                });
            }
            return sb.ToString();
        }
    }
}
