// Project D | Codename: Zantetsuken — Structure.cs (GCD 6.2)
using UnityEngine; using ProjectD.Core; using ProjectD.Gameplay.Combat;
namespace ProjectD.Gameplay.Map
{
    public class Structure : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId; [SerializeField] private Team team = Team.Blue;
        [SerializeField] private float maxHealth = 5000f, armor = 100f, magicResist = 100f;
        [SerializeField] private float backdoorMitigation = 0.5f; [SerializeField] private Structure[] prerequisites;
        private float _health; public bool AllyMinionNearby { get; set; }
        public int EntityId => entityId; public Team Team => team; public bool IsAlive => _health > 0f; public float CurrentHealth => _health; public float MaxHealth => maxHealth;
        private void Awake()=>_health=maxHealth;
        public bool IsTargetable(){ if(prerequisites==null)return true; foreach(var p in prerequisites) if(p!=null&&p.IsAlive) return false; return true; }
        public float ReceiveDamage(in DamageInstance dmg)
        {
            if(!IsAlive||!IsTargetable()) return 0f;
            float raw=DamageSystem.Resolve(dmg,armor,magicResist,_health,maxHealth); if(!AllyMinionNearby) raw*=(1f-backdoorMitigation);
            _health=Mathf.Max(0f,_health-raw); if(_health<=0f) OnDestroyed(); return raw;
        }
        protected virtual void OnDestroyed(){}
    }
}
