// Project D  |  Codename: Zantetsuken
// MapEnvDemoDriver.cs  (Map — play-in-editor sandbox)
//
// Drives a demo: a movable "local player" marker, two river pits, and the
// ObjectiveDirector + ProximityBgmController. Move the marker (WASD / arrows) into a
// pit and the pit's current objective theme plays for YOU only; leave and it fades.
// Buttons kill the summon/divine (advancing the sequence/rotation) and flip which
// side the duo occupies (summon pit follows them; divine pit stays opposite).

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Audio;

namespace ProjectD.Map
{
    public sealed class MapEnvDemoDriver : MonoBehaviour
    {
        [SerializeField] private ObjectiveDirector director;
        [SerializeField] private ProximityBgmController bgm;
        [SerializeField] private Transform localPlayer;
        [SerializeField] private Transform topPit, bottomPit;
        [SerializeField] private float moveSpeed = 12f;

        [Header("HUD")]
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button summonKillButton, divineKillButton, flipDuoButton;

        private RiverPit _duoPit = RiverPit.Bottom;

        private void Start()
        {
            if (bgm && localPlayer) bgm.SetLocalPlayer(localPlayer);
            if (director) director.Initialize(_duoPit);
            if (director) { director.Timers.SetNextSpawn("pit_summon", 1f); director.Timers.SetNextSpawn("pit_divine", 1f); }

            if (summonKillButton) summonKillButton.onClick.AddListener(() => director?.NotifySummonKilled());
            if (divineKillButton) divineKillButton.onClick.AddListener(() => director?.NotifyDivineKilled());
            if (flipDuoButton) flipDuoButton.onClick.AddListener(FlipDuo);
        }

        private void Update()
        {
            if (director) director.Tick(Time.deltaTime);

            if (localPlayer)
            {
                float h = Input.GetAxisRaw("Horizontal"), v = Input.GetAxisRaw("Vertical");
                localPlayer.position += new Vector3(h, 0, v) * (moveSpeed * Time.deltaTime);
            }

            if (statusLabel && director)
            {
                var L = director.Layout;
                statusLabel.text =
                    $"Duo pit: {_duoPit}   |   Summon pit: {L.SummonPit} ({director.SummonObjectiveNow})   |   " +
                    $"Divine pit: {L.DivinePit} ({director.DivineObjectiveNow})\n" +
                    $"Move into a pit to hear its theme (yours only). Clock {director.Clock:0}s";
            }
        }

        private void FlipDuo()
        {
            _duoPit = _duoPit == RiverPit.Bottom ? RiverPit.Top : RiverPit.Bottom;
            if (director)
            {
                director.Initialize(_duoPit);
                director.Timers.SetNextSpawn("pit_summon", director.Clock + 1f);
                director.Timers.SetNextSpawn("pit_divine", director.Clock + 1f);
            }
        }
    }
}
