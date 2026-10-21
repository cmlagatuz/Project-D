// Project D | Codename: Zantetsuken — CrystalCore.cs (GCD 5.1 victory)
using UnityEngine;
namespace ProjectD.Gameplay.Map
{
    public sealed class CrystalCore : Structure
    {
        public delegate void CoreDestroyed(ProjectD.Core.Team losingTeam);
        public static event CoreDestroyed OnCoreDestroyed;
        protected override void OnDestroyed(){ base.OnDestroyed(); OnCoreDestroyed?.Invoke(Team); Debug.Log($"[Project D] Crystal Core down. Team {Team} loses."); }
    }
}
