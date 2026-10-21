// Project D  |  Codename: Zantetsuken
// ManifestationDemoDriver.cs  (World Rift UI — simulates a Manifestation fight)
//
// Spawns a Manifestation, then simulates both teams contesting it so the HUD shows the
// contest bar shifting, the boss dropping, a claim banner + summon strike, and the buff
// timer — on a loop, so the whole HUD flow is visible.

using UnityEngine;
using ProjectD.WorldBoss;
using ProjectD.WorldRiftUI;

namespace ProjectD.WorldRiftUI
{
    public sealed class ManifestationDemoDriver : MonoBehaviour
    {
        [SerializeField] private ManifestationHudView hud;
        [SerializeField] private string manifestationName = "Ifrit";

        private ManifestationBoss _boss;
        private float _clock;
        private float _dmgTimer;

        private void Start()
        {
            if (hud == null) return;
            _boss = new ManifestationBoss(new ManifestationConfig
            {
                manifestationId = "summon_ifrit", firstSpawnSeconds = 3f, respawnSeconds = 8f,
                baseHp = 4000f, hpPerMinute = 0f, buffDurationSeconds = 6f, summonStrikeDamage = 900f, summonStrikeRadius = 8f
            });
            hud.Bind(_boss, manifestationName);
        }

        private void Update()
        {
            _clock += Time.deltaTime;
            _boss.Tick(_clock);
            hud.SetClock(_clock);

            // While the boss is alive, both teams chip it — red slightly ahead, but blue
            // sometimes steals the finishing blow to show the contest.
            if (_boss.State == BossState.Alive)
            {
                _dmgTimer -= Time.deltaTime;
                if (_dmgTimer <= 0f)
                {
                    _dmgTimer = 0.25f;
                    _boss.DealDamage(BossTeam.Red, Random.Range(120f, 220f), _clock);
                    _boss.DealDamage(BossTeam.Blue, Random.Range(140f, 260f), _clock);   // blue trends ahead
                }
            }
        }
    }
}
