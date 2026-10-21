// Project D  |  Codename: Zantetsuken
// ScoreboardRow.cs  (PostGame UI — one player row on the scoreboard)
//
// Shows a player's champion, name (self/MVP accented), KDA, CS, damage, gold, and a
// vote button (for the MVP vote). Bind a PlayerMatchStats + flags. Raises OnVote so
// the results view can tally MVP votes.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.PostGame;

namespace ProjectD.PostGameUI
{
    public sealed class ScoreboardRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image championSwatch;
        [SerializeField] private Text nameText;
        [SerializeField] private Text kdaText;
        [SerializeField] private Text csText;
        [SerializeField] private Text damageText;
        [SerializeField] private Text goldText;
        [SerializeField] private GameObject mvpCrown;
        [SerializeField] private Button voteButton;
        [SerializeField] private Text voteButtonLabel;
        [SerializeField] private Text voteCountText;

        public string PlayerId { get; private set; }
        public event System.Action<string> OnVote;

        public void Bind(PlayerMatchStats p, bool isLocal, bool isMvp, bool canVote, int voteCount)
        {
            PlayerId = p.playerId;
            if (championSwatch && championSwatch.sprite == null) championSwatch.color = ChampionTint(p.championId);
            if (nameText)
            {
                nameText.text = p.displayName;
                nameText.color = isLocal ? new Color(0.55f, 0.85f, 1f) : (isMvp ? new Color(0.95f, 0.80f, 0.32f) : Color.white);
            }
            if (kdaText) kdaText.text = $"{p.kills} / {p.deaths} / {p.assists}";
            if (csText) csText.text = p.minionsSlain.ToString();
            if (damageText) damageText.text = Compact(p.championDamage);
            if (goldText) goldText.text = Compact(p.goldEarned);
            if (mvpCrown) mvpCrown.SetActive(isMvp);

            if (voteButton)
            {
                voteButton.gameObject.SetActive(canVote);
                voteButton.onClick.RemoveAllListeners();
                string capture = p.playerId;
                voteButton.onClick.AddListener(() => OnVote?.Invoke(capture));
            }
            if (voteButtonLabel) voteButtonLabel.text = "VOTE MVP";
            if (voteCountText) voteCountText.text = voteCount > 0 ? $"{voteCount} \u2605" : "";

            if (background)
                background.color = isLocal ? new Color(0.20f, 0.28f, 0.42f, 0.95f)
                    : new Color(0.13f, 0.15f, 0.21f, 0.9f);
        }

        public void SetVoteCount(int count, bool votingDone)
        {
            if (voteCountText) voteCountText.text = count > 0 ? $"{count} \u2605" : "";
            if (voteButton && votingDone) voteButton.interactable = false;
        }

        private static string Compact(int v) => v >= 1000 ? $"{v / 1000f:0.0}k" : v.ToString();

        private static Color ChampionTint(string championId)
        {
            if (string.IsNullOrEmpty(championId)) return new Color(0.4f, 0.4f, 0.5f);
            int h = 0; foreach (char c in championId) h = h * 31 + c;
            Random.State s = Random.state; Random.InitState(h);
            var col = Color.HSVToRGB(Random.value, 0.5f, 0.85f);
            Random.state = s;
            return col;
        }
    }
}
