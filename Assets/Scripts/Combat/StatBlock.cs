// Project D | Codename: Zantetsuken — StatBlock.cs (server-authoritative runtime stats)
using System.Collections.Generic; using ProjectD.Data;
namespace ProjectD.Gameplay.Combat
{
    public sealed class StatBlock
    {
        public float BaseMaxHealth, BaseAttackDamage, BaseMagicPower, BaseArmor, BaseMagicResist, BaseMoveSpeed, BaseAttackSpeed;
        private float _fH,_fAd,_fAp,_fAr,_fMr,_fAdapt,_pMove,_pAtk,_haste;
        public float MaxHealth => BaseMaxHealth + _fH;
        public float AttackDamage => BaseAttackDamage + _fAd + _fAdapt;
        public float MagicPower => BaseMagicPower + _fAp + _fAdapt;
        public float Armor => BaseArmor + _fAr;
        public float MagicResist => BaseMagicResist + _fMr;
        public float MoveSpeed => BaseMoveSpeed * (1f + _pMove);
        public float AttackSpeed => BaseAttackSpeed * (1f + _pAtk);
        public float AbilityHaste => _haste;
        public float CooldownAfterHaste(float baseCd) => baseCd * (100f / (100f + _haste));
        public void ResetModifiers(){ _fH=_fAd=_fAp=_fAr=_fMr=_fAdapt=_pMove=_pAtk=_haste=0f; }
        public void ApplyBuffs(IEnumerable<BuffDefinition> buffs)
        {
            ResetModifiers();
            foreach (var b in buffs){ if(b==null) continue;
                _fH+=b.maxHealthBonus; _fAd+=b.attackDamageBonus; _fAp+=b.magicPowerBonus; _fAr+=b.armorBonus; _fMr+=b.magicResistanceBonus; _fAdapt+=b.adaptivePowerBonus;
                _pMove+=b.movementSpeedPercent; _pAtk+=b.attackSpeedPercent; _haste+=b.abilityHaste; }
        }
    }
}
