// Project D | Codename: Zantetsuken — DuelHumanoidPose.cs
// Raw model rips have no animation clips, so they stand in a T-pose. When a model has a valid Humanoid avatar (set
// automatically for champion models by the import tool) this component bends its muscles directly: arms lowered, a
// simple walk swing, an attack swing, and a little breathing. It does nothing if the model already has an
// Animator Controller (real animations win) or if the avatar is not Humanoid.
using System;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public sealed class DuelHumanoidPose : MonoBehaviour
    {
        public float Speed01;    // 0 = standing, 1 = running (set by the controller every frame)
        public float Punch01;    // 1 right after an attack, fades to 0

        private HumanPoseHandler _handler;
        private HumanPose _rest;
        private HumanPose _pose;
        private float _phase;
        private float _walk;

        private int _lArmDown, _rArmDown, _lArmFwd, _rArmFwd, _lShoulder, _rShoulder, _lLeg, _rLeg, _spine, _lForearm, _rForearm;

        /// <summary>Adds the poser to the animator's object if it is a usable Humanoid with no controller; otherwise returns null.</summary>
        public static DuelHumanoidPose TryAttach(Animator animator)
        {
            if (animator == null || animator.runtimeAnimatorController != null) return null;
            var avatar = animator.avatar;
            if (avatar == null || !avatar.isValid || !avatar.isHuman) return null;
            var pose = animator.gameObject.AddComponent<DuelHumanoidPose>();
            if (!pose.Init(animator)) { Destroy(pose); return null; }
            return pose;
        }

        private bool Init(Animator animator)
        {
            try
            {
                _handler = new HumanPoseHandler(animator.avatar, animator.transform);
                _handler.GetHumanPose(ref _rest);   // the as-imported (T) pose
                _handler.GetHumanPose(ref _pose);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Project D] Humanoid pose unavailable for " + animator.name + ": " + e.Message);
                return false;
            }

            _lArmDown = Muscle("Left Arm Down-Up");
            _rArmDown = Muscle("Right Arm Down-Up");
            _lArmFwd = Muscle("Left Arm Front-Back");
            _rArmFwd = Muscle("Right Arm Front-Back");
            _lShoulder = Muscle("Left Shoulder Down-Up");
            _rShoulder = Muscle("Right Shoulder Down-Up");
            _lLeg = Muscle("Left Upper Leg Front-Back");
            _rLeg = Muscle("Right Upper Leg Front-Back");
            _spine = Muscle("Spine Front-Back");
            _lForearm = Muscle("Left Forearm Stretch");
            _rForearm = Muscle("Right Forearm Stretch");
            return _rest.muscles != null && _rest.muscles.Length > 0;
        }

        private static int Muscle(string name)
        {
            var names = HumanTrait.MuscleName;
            for (int i = 0; i < names.Length; i++) if (names[i] == name) return i;
            return -1;
        }

        private void Set(int index, float value)
        {
            if (index >= 0 && index < _pose.muscles.Length) _pose.muscles[index] = Mathf.Clamp(value, -1f, 1f);
        }

        private void LateUpdate()
        {
            if (_handler == null) return;
            _walk = Mathf.MoveTowards(_walk, Speed01, Time.deltaTime * 8f);
            _phase += Time.deltaTime * 9f * _walk;
            float swing = Mathf.Sin(_phase) * _walk;
            float breathe = Mathf.Sin(Time.time * 2f) * (1f - _walk);

            Array.Copy(_rest.muscles, _pose.muscles, _rest.muscles.Length);
            _pose.bodyPosition = _rest.bodyPosition;
            _pose.bodyRotation = _rest.bodyRotation;

            // Arms down from the T-pose (negative Down-Up lowers them); the right arm swings up when attacking.
            Set(_lArmDown, -0.85f);
            Set(_rArmDown, -0.85f + Punch01 * 1.5f);
            Set(_lShoulder, -0.4f);
            Set(_rShoulder, -0.4f);
            Set(_lArmFwd, 0.30f * swing + 0.10f * breathe);
            Set(_rArmFwd, -0.30f * swing + Punch01 * 0.5f);
            Set(_lForearm, 0.15f);
            Set(_rForearm, 0.15f);

            // Walk: swing the thighs in opposite directions.
            Set(_lLeg, 0.55f * swing);
            Set(_rLeg, -0.55f * swing);
            Set(_spine, 0.04f * breathe + Punch01 * 0.15f);

            _handler.SetHumanPose(ref _pose);
        }

        private void OnDestroy()
        {
            if (_handler != null) _handler.Dispose();
        }
    }
}
