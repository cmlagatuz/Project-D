// Project D  |  Codename: Zantetsuken
// WeaponSocket.cs  (Art pipeline rig marker — held-weapon attach point)
//
// Sits on the bone/child transform where a champion's weapon is attached (e.g. the
// hand or weapon-tip socket). AbilityAnimator looks this up via
// GetComponentInChildren<WeaponSocket>() to know where to toggle the weapon trail
// VFX during "active" ability frames. Deliberately minimal — this is a marker +
// optional tip reference for effects, not simulation logic.

using UnityEngine;

namespace ProjectD.Art.Animation
{
    public sealed class WeaponSocket : MonoBehaviour
    {
        [Tooltip("Point effects should spawn from, e.g. the blade tip. Defaults to this transform if unset.")]
        [SerializeField] private Transform tip;

        public Transform Tip => tip != null ? tip : transform;
    }
}
