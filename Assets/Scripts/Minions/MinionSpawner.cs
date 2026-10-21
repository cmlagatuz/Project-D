// Project D | Codename: Zantetsuken — MinionSpawner.cs (GCD 8.3)
using System.Collections.Generic; using UnityEngine; using ProjectD.Data;
namespace ProjectD.Gameplay.Minions
{
    public sealed class MinionSpawner : MonoBehaviour
    {
        [SerializeField] private NeutralMonsterDefinition meleeGoblin, rangedGoblin, goblinMage, armoredGoblin, goblinWarlord;
        [SerializeField] private float waveInterval = 30f; [SerializeField] private int meleePerWave = 3, rangedPerWave = 3, siegeEveryNthWave = 3;
        private int _waveNumber; private bool _laneSealDown;
        public void MarkLaneSealDestroyed()=>_laneSealDown=true;
        public List<NeutralMonsterDefinition> BuildNextWave(float t)
        {
            _waveNumber++; var w=new List<NeutralMonsterDefinition>();
            for(int i=0;i<meleePerWave;i++) w.Add(meleeGoblin);
            for(int i=0;i<rangedPerWave;i++) w.Add(rangedGoblin);
            if(siegeEveryNthWave>0 && _waveNumber%siegeEveryNthWave==0){ bool late=t>15f*60f; w.Add(late&&armoredGoblin!=null?armoredGoblin:goblinMage); }
            if(_laneSealDown && goblinWarlord!=null) w.Add(goblinWarlord);
            return w;
        }
        public float WaveInterval => waveInterval; public int CurrentWave => _waveNumber;
    }
}
