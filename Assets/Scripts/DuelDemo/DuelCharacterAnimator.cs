// Project D | Codename: Zantetsuken — DuelCharacterAnimator.cs
// Procedural character animation for model rips that come with no animation clips.
// It reads the bones found by DuelRigFinder and, every frame, poses them from scratch:
//   * idle stance and run cycle with real two-bone leg IK (knees bend, feet plant, hips bob, torso leans and twists,
//     arms swing with the elbows bent), tuned per character "style" (knight, rebel, brute, sorcerer, dark mage);
//   * basic-attack, ability and summoner-spell clips (slashes, thrusts, smashes, palm blasts, raised-arm casts, sigils,
//     lunges, guards, prayer) chosen from the style and the ability shape;
//   * hit flinch, death (fall, lie, fade), respawn (rise), teleport channel (kneel and pray, then beam out) and arrival.
// Arms and legs are posed by aiming each bone segment at a direction, so it works on any skeleton whatever its bone axes.
// All numbers are placeholders to tune by eye; nothing here touches the deterministic duel rules.
using System;
using UnityEngine;
using ProjectD.Duel;

namespace ProjectD.DuelDemo
{
    public enum DuelStyle { Knight, Rebel, Brute, Sorcerer, DarkMage }

    public enum DuelClip { Slash, SlashWide, Thrust, Smash, Palm, CastForward, CastRaise, Sigil, Lunge, Guard, Pray }

    // ---------------------------------------------------------------------------------------------- pose data

    internal struct ArmPose
    {
        // Directions are in body space: F = 0 down, 90 forward, 180 up; L = degrees out to the side (negative = across the body).
        public float UF, UL, LF, LL;   // upper arm and forearm
        public float W;                // how strongly this pose overrides the idle/run arm (0..1)

        public static ArmPose A(float uf, float ul, float lf, float ll)
        {
            return new ArmPose { UF = uf, UL = ul, LF = lf, LL = ll, W = 1f };
        }

        public static ArmPose Lerp(ArmPose a, ArmPose b, float t)
        {
            float wa = a.W * (1f - t), wb = b.W * t, sum = wa + wb;
            var r = new ArmPose { W = Mathf.Lerp(a.W, b.W, t) };
            if (sum > 1e-5f)
            {
                r.UF = (a.UF * wa + b.UF * wb) / sum;
                r.UL = (a.UL * wa + b.UL * wb) / sum;
                r.LF = (a.LF * wa + b.LF * wb) / sum;
                r.LL = (a.LL * wa + b.LL * wb) / sum;
            }
            return r;
        }
    }

    internal struct ActPose
    {
        public ArmPose L, R;                 // L = arm [0] (-lateral), R = arm [1] (+lateral, the sword arm)
        public float Lean;                   // degrees forward
        public float Twist, HipTwist;        // degrees, positive = towards the character's right
        public float Drop, Fwd, Lift;        // fractions of leg length: crouch, shift forward, rise
        public float LegW, ZL, ZR;           // leg override weight and foot positions (fractions of leg length, forward +)

        public static ActPose Lerp(ActPose a, ActPose b, float t)
        {
            return new ActPose
            {
                L = ArmPose.Lerp(a.L, b.L, t),
                R = ArmPose.Lerp(a.R, b.R, t),
                Lean = Mathf.Lerp(a.Lean, b.Lean, t),
                Twist = Mathf.Lerp(a.Twist, b.Twist, t),
                HipTwist = Mathf.Lerp(a.HipTwist, b.HipTwist, t),
                Drop = Mathf.Lerp(a.Drop, b.Drop, t),
                Fwd = Mathf.Lerp(a.Fwd, b.Fwd, t),
                Lift = Mathf.Lerp(a.Lift, b.Lift, t),
                LegW = Mathf.Lerp(a.LegW, b.LegW, t),
                ZL = Mathf.Lerp(a.ZL, b.ZL, t),
                ZR = Mathf.Lerp(a.ZR, b.ZR, t)
            };
        }
    }

    internal sealed class DuelClipData
    {
        public readonly float Duration;
        private readonly float[] _t;
        private readonly ActPose[] _k;

        public DuelClipData(float[] times, ActPose[] keys)
        {
            _t = times;
            _k = keys;
            Duration = times[times.Length - 1];
        }

        public ActPose Eval(float time)
        {
            if (time <= _t[0]) return _k[0];
            for (int i = 1; i < _t.Length; i++)
            {
                if (time <= _t[i])
                {
                    float u = (time - _t[i - 1]) / (_t[i] - _t[i - 1]);
                    u = u * u * (3f - 2f * u);
                    return ActPose.Lerp(_k[i - 1], _k[i], u);
                }
            }
            return _k[_k.Length - 1];
        }
    }

    // ---------------------------------------------------------------------------------------------- clips

    internal static class DuelClips
    {
        private static readonly ArmPose None = default(ArmPose);
        private static DuelClipData[] _all;

        public static DuelClipData Get(DuelClip clip)
        {
            if (_all == null) Build();
            return _all[(int)clip];
        }

        public static ActPose DeathPose()
        {
            return K(ArmPose.A(105f, 52f, 112f, 58f), ArmPose.A(105f, 52f, 112f, 58f), -18f, 0f, 0.05f, 0f, 0f, 0f, 0f, 0f, 0f);
        }

        private static ActPose K(ArmPose r, ArmPose l, float lean, float twist, float drop, float fwd, float lift,
                                 float legW, float zl, float zr, float hip)
        {
            return new ActPose { R = r, L = l, Lean = lean, Twist = twist, Drop = drop, Fwd = fwd, Lift = lift, LegW = legW, ZL = zl, ZR = zr, HipTwist = hip };
        }

        private static ActPose Neutral() { return K(None, None, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f); }

        private static DuelClipData C(float[] t, ActPose[] k) { return new DuelClipData(t, k); }

        private static void Build()
        {
            _all = new DuelClipData[11];

            // Overhead slash with the sword arm: wind up behind the shoulder, cut down through, follow through.
            _all[(int)DuelClip.Slash] = C(new[] { 0f, 0.16f, 0.30f, 0.46f, 0.64f }, new[]
            {
                Neutral(),
                K(ArmPose.A(150f, 14f, 172f, 10f), None, -8f, 24f, 0.02f, 0f, 0f, 0.6f, 0.15f, -0.10f, 0f),
                K(ArmPose.A(68f, -4f, 82f, -6f), None, 20f, -26f, 0.08f, 0.22f, 0f, 1f, 0.50f, -0.35f, -10f),
                K(ArmPose.A(50f, 6f, 80f, 0f), None, 12f, -10f, 0.05f, 0.12f, 0f, 1f, 0.35f, -0.25f, -4f),
                Neutral()
            });

            // Wide horizontal sweep across the body.
            _all[(int)DuelClip.SlashWide] = C(new[] { 0f, 0.16f, 0.30f, 0.46f, 0.64f }, new[]
            {
                Neutral(),
                K(ArmPose.A(95f, 100f, 100f, 92f), None, 0f, 34f, 0.02f, 0f, 0f, 0.5f, 0.1f, -0.1f, 8f),
                K(ArmPose.A(92f, -35f, 95f, -50f), None, 10f, -34f, 0.05f, 0.15f, 0f, 1f, 0.35f, -0.25f, -12f),
                K(ArmPose.A(85f, -10f, 90f, -25f), None, 6f, -14f, 0.03f, 0.08f, 0f, 0.8f, 0.25f, -0.2f, -5f),
                Neutral()
            });

            // Thrust: draw the hand back to the hip, stab forward in a lunge.
            _all[(int)DuelClip.Thrust] = C(new[] { 0f, 0.14f, 0.26f, 0.42f, 0.60f }, new[]
            {
                Neutral(),
                K(ArmPose.A(10f, 12f, 85f, 10f), None, -4f, 14f, 0.04f, -0.08f, 0f, 0.5f, -0.1f, 0.1f, 6f),
                K(ArmPose.A(86f, 2f, 88f, 0f), ArmPose.A(40f, 22f, 90f, -10f), 16f, -10f, 0.06f, 0.38f, 0f, 1f, 0.65f, -0.45f, -8f),
                K(ArmPose.A(78f, 4f, 86f, 2f), None, 10f, -4f, 0.04f, 0.22f, 0f, 1f, 0.5f, -0.35f, -4f),
                Neutral()
            });

            // Two-handed smash: heave overhead, slam down.
            _all[(int)DuelClip.Smash] = C(new[] { 0f, 0.30f, 0.46f, 0.62f, 0.95f }, new[]
            {
                Neutral(),
                K(ArmPose.A(165f, 10f, 178f, 6f), ArmPose.A(165f, 16f, 178f, 12f), -14f, 8f, 0f, 0f, 0.10f, 0.5f, 0.10f, -0.10f, 4f),
                K(ArmPose.A(64f, 6f, 58f, -6f), ArmPose.A(64f, 10f, 60f, -10f), 30f, 0f, 0.16f, 0.28f, 0f, 1f, 0.45f, -0.35f, 0f),
                K(ArmPose.A(62f, 6f, 56f, -6f), ArmPose.A(62f, 10f, 58f, -10f), 28f, 0f, 0.15f, 0.26f, 0f, 1f, 0.45f, -0.35f, 0f),
                Neutral()
            });

            // Palm blast: gather, release forward with the leading hand, other hand balances.
            _all[(int)DuelClip.Palm] = C(new[] { 0f, 0.12f, 0.24f, 0.40f, 0.60f }, new[]
            {
                Neutral(),
                K(ArmPose.A(45f, 30f, 95f, 20f), ArmPose.A(45f, 30f, 95f, 20f), 4f, 0f, 0.03f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(86f, 6f, 90f, 4f), ArmPose.A(70f, 28f, 82f, 22f), 8f, 0f, 0.02f, 0.10f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(84f, 8f, 88f, 6f), ArmPose.A(68f, 30f, 80f, 24f), 6f, 0f, 0.02f, 0.08f, 0f, 0f, 0f, 0f, 0f),
                Neutral()
            });

            // Two-handed cast: draw in, push both palms out.
            _all[(int)DuelClip.CastForward] = C(new[] { 0f, 0.18f, 0.34f, 0.52f, 0.80f }, new[]
            {
                Neutral(),
                K(ArmPose.A(30f, 20f, 110f, 10f), ArmPose.A(30f, 20f, 110f, 10f), 6f, 0f, 0.05f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(88f, 4f, 90f, 2f), ArmPose.A(88f, 8f, 90f, 6f), 12f, 0f, 0.03f, 0.15f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(88f, 4f, 90f, 2f), ArmPose.A(88f, 8f, 90f, 6f), 10f, 0f, 0.03f, 0.12f, 0f, 0f, 0f, 0f, 0f),
                Neutral()
            });

            // Raise both arms to the sky, gather power, slam it forward. Used for ultimates and big area casts.
            _all[(int)DuelClip.CastRaise] = C(new[] { 0f, 0.25f, 0.50f, 0.75f, 0.95f, 1.20f }, new[]
            {
                Neutral(),
                K(ArmPose.A(15f, 40f, 25f, 45f), ArmPose.A(15f, 40f, 25f, 45f), 8f, 0f, 0.12f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(172f, 24f, 178f, 18f), ArmPose.A(172f, 24f, 178f, 18f), -12f, 0f, 0f, 0f, 0.12f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(172f, 24f, 178f, 18f), ArmPose.A(172f, 24f, 178f, 18f), -14f, 0f, 0f, 0f, 0.14f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(60f, 10f, 70f, 6f), ArmPose.A(60f, 10f, 70f, 6f), 18f, 0f, 0.12f, 0.15f, 0f, 0f, 0f, 0f, 0f),
                Neutral()
            });

            // Ground sigil: crouch and sweep both hands down in front.
            _all[(int)DuelClip.Sigil] = C(new[] { 0f, 0.20f, 0.45f, 0.65f, 0.85f }, new[]
            {
                Neutral(),
                K(ArmPose.A(40f, 22f, 80f, 16f), ArmPose.A(40f, 22f, 80f, 16f), 12f, 0f, 0.14f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(62f, 12f, 50f, 6f), ArmPose.A(62f, 12f, 50f, 6f), 18f, 0f, 0.18f, 0.05f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(62f, 12f, 50f, 6f), ArmPose.A(62f, 12f, 50f, 6f), 16f, 0f, 0.16f, 0.05f, 0f, 0f, 0f, 0f, 0f),
                Neutral()
            });

            // Lunge / dash: lean right over the front foot, arms swept back.
            _all[(int)DuelClip.Lunge] = C(new[] { 0f, 0.12f, 0.30f, 0.50f }, new[]
            {
                Neutral(),
                K(ArmPose.A(-35f, 8f, -20f, 6f), ArmPose.A(-35f, 10f, -20f, 8f), 30f, 0f, 0.10f, 0.45f, 0f, 1f, 0.70f, -0.60f, 0f),
                K(ArmPose.A(-30f, 8f, -15f, 6f), ArmPose.A(-30f, 10f, -15f, 8f), 24f, 0f, 0.08f, 0.35f, 0f, 1f, 0.60f, -0.50f, 0f),
                Neutral()
            });

            // Guard: shield arm up in front, crouch behind it.
            _all[(int)DuelClip.Guard] = C(new[] { 0f, 0.20f, 0.60f, 0.90f }, new[]
            {
                Neutral(),
                K(ArmPose.A(55f, -10f, 82f, -28f), ArmPose.A(98f, -6f, 104f, -20f), 4f, 0f, 0.10f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(55f, -10f, 82f, -28f), ArmPose.A(98f, -6f, 104f, -20f), 4f, 0f, 0.10f, 0f, 0f, 0f, 0f, 0f, 0f),
                Neutral()
            });

            // Prayer: hands together at the chest, bow, then lift the hands to the sky.
            _all[(int)DuelClip.Pray] = C(new[] { 0f, 0.30f, 0.60f, 0.80f, 1.10f }, new[]
            {
                Neutral(),
                K(ArmPose.A(48f, 4f, 104f, -42f), ArmPose.A(48f, 4f, 104f, -42f), 12f, 0f, 0.04f, 0f, 0f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(150f, 22f, 170f, 16f), ArmPose.A(150f, 22f, 170f, 16f), -8f, 0f, 0f, 0f, 0.10f, 0f, 0f, 0f, 0f),
                K(ArmPose.A(150f, 22f, 170f, 16f), ArmPose.A(150f, 22f, 170f, 16f), -8f, 0f, 0f, 0f, 0.10f, 0f, 0f, 0f, 0f),
                Neutral()
            });
        }
    }

    // ---------------------------------------------------------------------------------------------- styles

    internal sealed class StyleDef
    {
        public float StrideFrac = 0.55f;   // stride half-length as a fraction of leg length
        public float ArmSwing = 30f;       // degrees of arm swing while running
        public float Elbow = 80f;          // forearm bend while running
        public float Lean = 8f;            // forward lean at a run
        public float Bob = 0.035f;         // hip bob, fraction of leg length
        public float HoldR = 0.5f, HoldL = 0.5f;   // 0 = arm swings freely, 1 = arm stays in its idle (weapon) position
        public float Crouch = 0.04f;       // idle knee bend
        public float Spread = 1.10f;       // idle foot width
        public float ZL = 0.08f, ZR = -0.08f;      // idle foot stagger (forward +)
        public float KneeLift = 0.28f;     // how high the foot lifts on a step
        public float Hover = 0f;           // float height above the ground, fraction of leg length
        public float Dangle = 0f;          // how far the legs trail behind when hovering
        public bool Caster;
        public ArmPose IdleR, IdleL;
        public DuelClip[] Basic;
    }

    internal static class DuelStyles
    {
        public static DuelStyle ForModel(string id)
        {
            switch (id)
            {
                case "cecil":
                case "wol": return DuelStyle.Knight;
                case "firion": return DuelStyle.Rebel;
                case "garland": return DuelStyle.Brute;
                case "emperor": return DuelStyle.Sorcerer;
                case "golbez": return DuelStyle.DarkMage;
                default: return DuelStyle.Knight;
            }
        }

        public static StyleDef Get(DuelStyle s)
        {
            switch (s)
            {
                case DuelStyle.Rebel:   // Firion: agile, wide ready stance, quick alternating cuts
                    return new StyleDef
                    {
                        StrideFrac = 0.60f, ArmSwing = 38f, Elbow = 95f, Lean = 11f, Bob = 0.045f, HoldR = 0.40f, HoldL = 0.25f,
                        Crouch = 0.06f, Spread = 1.15f, ZL = 0.10f, ZR = -0.10f, KneeLift = 0.32f,
                        IdleR = ArmPose.A(18f, 18f, 65f, 8f), IdleL = ArmPose.A(16f, 22f, 60f, 10f),
                        Basic = new[] { DuelClip.SlashWide, DuelClip.Slash, DuelClip.Thrust }
                    };
                case DuelStyle.Brute:   // Garland: heavy, planted, two-handed greatsword
                    return new StyleDef
                    {
                        StrideFrac = 0.50f, ArmSwing = 16f, Elbow = 70f, Lean = 12f, Bob = 0.06f, HoldR = 0.85f, HoldL = 0.85f,
                        Crouch = 0.08f, Spread = 1.25f, ZL = 0.12f, ZR = -0.12f, KneeLift = 0.24f,
                        IdleR = ArmPose.A(38f, 8f, 100f, -30f), IdleL = ArmPose.A(38f, 12f, 100f, -35f),
                        Basic = new[] { DuelClip.Smash, DuelClip.SlashWide }
                    };
                case DuelStyle.Sorcerer:   // The Emperor: floats, glides, casts with open hands
                    return new StyleDef
                    {
                        StrideFrac = 0.35f, ArmSwing = 10f, Elbow = 40f, Lean = 14f, Bob = 0.02f, HoldR = 0.5f, HoldL = 0.5f,
                        Crouch = 0f, Spread = 1.0f, ZL = 0f, ZR = 0f, KneeLift = 0.1f, Hover = 0.22f, Dangle = 0.14f, Caster = true,
                        IdleR = ArmPose.A(14f, 22f, 55f, 14f), IdleL = ArmPose.A(14f, 22f, 55f, 14f),
                        Basic = new[] { DuelClip.Palm, DuelClip.CastForward }
                    };
                case DuelStyle.DarkMage:   // Golbez: slow, cloaked, forearms crossed in front, spreads his arms to cast
                    return new StyleDef
                    {
                        StrideFrac = 0.45f, ArmSwing = 12f, Elbow = 60f, Lean = 6f, Bob = 0.025f, HoldR = 0.8f, HoldL = 0.8f,
                        Crouch = 0.03f, Spread = 1.05f, ZL = 0.04f, ZR = -0.04f, KneeLift = 0.2f, Hover = 0.06f, Dangle = 0.05f, Caster = true,
                        IdleR = ArmPose.A(18f, 6f, 75f, -35f), IdleL = ArmPose.A(18f, 6f, 75f, -35f),
                        Basic = new[] { DuelClip.CastForward, DuelClip.Palm }
                    };
                default:   // Knight (Warrior of Light, Cecil): upright, sword ready, shield arm forward
                    return new StyleDef
                    {
                        StrideFrac = 0.55f, ArmSwing = 28f, Elbow = 85f, Lean = 7f, Bob = 0.035f, HoldR = 0.65f, HoldL = 0.50f,
                        Crouch = 0.04f, Spread = 1.12f, ZL = 0.10f, ZR = -0.08f, KneeLift = 0.28f,
                        IdleR = ArmPose.A(22f, 14f, 78f, 6f), IdleL = ArmPose.A(24f, 20f, 88f, -12f),
                        Basic = new[] { DuelClip.Slash, DuelClip.SlashWide }
                    };
            }
        }
    }

    // ---------------------------------------------------------------------------------------------- the animator

    public sealed class DuelCharacterAnimator : MonoBehaviour
    {
        public float Yaw;   // the holder's yaw (library Yaw Offset + any temporary turn), set by the controller every frame

        private const float DeathFall = 0.7f, DeathHold = 1.2f, DeathShrink = 0.5f;

        private DuelRig _rig;
        private Transform _holder;
        private StyleDef _style;
        private Transform _hips;
        private Vector3 _hipsLocalPos;
        private Transform[] _bones;
        private Quaternion[] _rest;

        private readonly float[] _legLat = new float[2];
        private readonly float[] _hAnkle = new float[2];
        private readonly float[] _legA = new float[2];
        private readonly float[] _legB = new float[2];
        private readonly Quaternion[] _footInFrame = new Quaternion[2];
        private float _legLen;

        private Vector3 _lastRoot;
        private float _speed, _walk, _phase;

        private DuelClipData _clip;
        private float _clipStart;
        private ActPose _lastAct, _xfadeFrom;
        private float _xfade = 1f;
        private int _basicIndex;

        private float _hitAt = -10f;
        private bool _dead;
        private float _deathAt;
        private float _riseAt = -10f;
        private float _arriveAt = -10f;
        private bool _channeling;
        private float _channelStart, _channelDuration = 1f;

        public bool IsDead { get { return _dead; } }
        /// <summary>0 standing, 1 running: lets capes and skirts trail back.</summary>
        public float Walk01 { get { return _walk; } }
        /// <summary>True while a dead unit's body should still be drawn (falling, lying, shrinking away).</summary>
        public bool DeathVisible { get { return !_dead || Time.time - _deathAt < DeathFall + DeathHold + DeathShrink; } }

        public bool Init(DuelRig rig, Transform holder, DuelStyle style)
        {
            _rig = rig;
            _holder = holder;
            _style = DuelStyles.Get(style);
            _hips = rig.Hips;
            _hipsLocalPos = _hips.localPosition;

            var list = new System.Collections.Generic.List<Transform>();
            AddBone(list, rig.Hips); AddBone(list, rig.Spine); AddBone(list, rig.Chest); AddBone(list, rig.Head);
            for (int i = 0; i < 2; i++)
            {
                AddBone(list, rig.UpperArm[i]); AddBone(list, rig.LowerArm[i]); AddBone(list, rig.Hand[i]);
                AddBone(list, rig.UpperLeg[i]); AddBone(list, rig.LowerLeg[i]); AddBone(list, rig.Foot[i]);
            }
            _bones = list.ToArray();
            _rest = new Quaternion[_bones.Length];
            for (int i = 0; i < _bones.Length; i++) _rest[i] = _bones[i].localRotation;

            Quaternion frame0 = holder.rotation * Quaternion.Euler(0f, rig.FrameYaw, 0f);
            Vector3 right0 = frame0 * Vector3.right;
            Vector3 hipsPos = _hips.position;
            float sumLen = 0f;
            for (int i = 0; i < 2; i++)
            {
                Vector3 thigh = rig.UpperLeg[i].position, knee = rig.LowerLeg[i].position, ankle = rig.Foot[i].position;
                _legLat[i] = Vector3.Dot(ankle - hipsPos, right0);
                _hAnkle[i] = hipsPos.y - ankle.y;
                _legA[i] = (knee - thigh).magnitude;
                _legB[i] = (ankle - knee).magnitude;
                _footInFrame[i] = Quaternion.Inverse(frame0) * rig.Foot[i].rotation;
                sumLen += _legA[i] + _legB[i];
                if (_legA[i] < 0.01f || _legB[i] < 0.01f || _hAnkle[i] <= 0.01f) return false;
            }
            _legLen = sumLen * 0.5f;
            _lastRoot = RootPosition();
            _lastAct = default(ActPose);
            return true;
        }

        private static void AddBone(System.Collections.Generic.List<Transform> list, Transform t)
        {
            if (t != null && !list.Contains(t)) list.Add(t);
        }

        private Vector3 RootPosition()
        {
            return _holder.parent != null ? _holder.parent.position : _holder.position;
        }

        // ------------------------------------------------------------------ triggers

        public void PlayBasic()
        {
            if (_dead) return;
            var clips = _style.Basic;
            PlayClip(clips[_basicIndex++ % clips.Length]);
        }

        public void PlayAbility(AbilityShape shape, int slot)
        {
            if (_dead) return;
            bool caster = _style.Caster;
            DuelClip c;
            switch (shape)
            {
                case AbilityShape.Cone: c = caster ? DuelClip.Palm : DuelClip.SlashWide; break;
                case AbilityShape.Line: c = caster ? DuelClip.CastForward : DuelClip.Thrust; break;
                case AbilityShape.Circle: c = caster ? DuelClip.CastRaise : DuelClip.Smash; break;
                case AbilityShape.Shield: c = DuelClip.Guard; break;
                case AbilityShape.Dash: c = DuelClip.Lunge; break;
                case AbilityShape.GroundCircle: c = DuelClip.Sigil; break;
                default: c = DuelClip.Slash; break;
            }
            if (slot == 3 && shape != AbilityShape.Dash && shape != AbilityShape.Shield && shape != AbilityShape.GroundCircle)
                c = caster ? DuelClip.CastRaise : DuelClip.Smash;   // the ultimate always looks big
            PlayClip(c);
        }

        /// <summary>Summoner spells by display name (Haste, Cure, Protect, Slow, Esuna, Warp, Bio, Ultima, Teleport, Pray, Warpstrike).</summary>
        public void PlaySpell(string name)
        {
            if (_dead || string.IsNullOrEmpty(name)) return;
            // Exact names only: other floating texts ("Slowed", "Cleansed", "Teleporting...") must not trigger a cast pose.
            switch (name)
            {
                case "Warp":
                case "Warpstrike dash": PlayClip(DuelClip.Lunge); PlayArrive(); break;
                case "Warpstrike":
                case "Bio": PlayClip(DuelClip.CastForward); break;
                case "Cure":
                case "Esuna":
                case "Pray": PlayClip(DuelClip.Pray); break;
                case "Protect": PlayClip(DuelClip.Guard); break;
                case "Haste":
                case "Ultima": PlayClip(DuelClip.CastRaise); break;
                case "Slow": PlayClip(DuelClip.Palm); break;
            }
        }

        public void PlayHit() { if (!_dead) _hitAt = Time.time; }

        public void PlayDeath()
        {
            if (_dead) return;
            _dead = true;
            _deathAt = Time.time;
            _clip = null;
            _channeling = false;
            _xfadeFrom = _lastAct;
            _xfade = 0f;
        }

        public void PlayRise()
        {
            _dead = false;
            _riseAt = Time.time;
            _clip = null;
            _channeling = false;
            _lastAct = default(ActPose);
            _xfade = 1f;
            _lastRoot = RootPosition();
        }

        public void PlayArrive() { _arriveAt = Time.time; }

        public void BeginChannel(float seconds)
        {
            if (_dead) return;
            _channeling = true;
            _channelStart = Time.time;
            _channelDuration = Mathf.Max(0.3f, seconds);
            _xfadeFrom = _lastAct;
            _xfade = 0f;
        }

        public void EndChannel(bool completed)
        {
            if (!_channeling) return;
            _channeling = false;
            _xfadeFrom = _lastAct;
            _xfade = 0f;
            if (completed) _arriveAt = Time.time;
        }

        private void PlayClip(DuelClip clip)
        {
            _clip = DuelClips.Get(clip);
            _clipStart = Time.time;
            _xfadeFrom = _lastAct;
            _xfade = 0f;
        }

        // ------------------------------------------------------------------ per-frame posing

        private void LateUpdate()
        {
            if (_rig == null || _holder == null || _hips == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            float now = Time.time;
            var st = _style;

            // ---- whole-body effects on the holder: fall, beam out, arrive, rise
            Vector3 scale = Vector3.one;
            float pitch = 0f;
            bool scaled = false;
            if (_dead)
            {
                float t = now - _deathAt;
                float f = Mathf.Clamp01(t / DeathFall);
                pitch = 88f * f * f;
                float shrink = Mathf.Clamp01((t - DeathFall - DeathHold) / DeathShrink);
                if (shrink > 0f) { scale = Vector3.one * Mathf.Max(0.001f, 1f - shrink); scaled = true; }
            }
            else
            {
                if (_channeling)
                {
                    float p = Mathf.Clamp01((now - _channelStart) / _channelDuration);
                    float s = p * p * p;
                    if (s > 0.01f) { scale = new Vector3(1f - 0.85f * s, 1f + 0.7f * s, 1f - 0.85f * s); scaled = true; }
                }
                float ta = now - _arriveAt;
                if (ta >= 0f && ta < 0.3f)
                {
                    float u = ta / 0.3f;
                    float k = 1f - (1f - u) * (1f - u) * (1f - u);
                    scale = Vector3.Scale(scale, Vector3.Lerp(new Vector3(0.15f, 1.8f, 0.15f), Vector3.one, k));
                    scaled = true;
                }
                float tr = now - _riseAt;
                if (tr >= 0f && tr < 0.5f)
                {
                    float u = tr / 0.5f;
                    float k = 1f - (1f - u) * (1f - u);
                    scale = scale * Mathf.Max(0.001f, k);
                    scaled = true;
                }
            }
            _holder.localRotation = Quaternion.Euler(0f, Yaw, 0f) * Quaternion.Euler(-pitch, 0f, 0f);
            _holder.localScale = scale;

            // ---- speed and gait phase from how far the unit actually moved
            Vector3 rootPos = RootPosition();
            Vector3 delta = rootPos - _lastRoot;
            delta.y = 0f;
            _lastRoot = rootPos;
            float raw = delta.magnitude / dt;
            if (raw > 25f) raw = _speed;   // a teleport jump, not running
            _speed = Mathf.Lerp(_speed, raw, 1f - Mathf.Exp(-14f * dt));
            float walkTarget = _dead ? 0f : Mathf.Clamp01((_speed - 0.15f) / 0.6f);
            _walk = Mathf.MoveTowards(_walk, walkTarget, dt * 7f);
            float stride = Mathf.Max(0.05f, st.StrideFrac * _legLen);
            float omega = Mathf.Clamp(Mathf.PI * _speed / (2f * stride), 0f, 20f);
            _phase = Mathf.Repeat(_phase + omega * dt, Mathf.PI * 2f);

            // ---- the current action
            ActPose act = default(ActPose);
            if (_dead)
            {
                act = DuelClips.DeathPose();
            }
            else if (_channeling)
            {
                act = DuelClips.Get(DuelClip.Pray).Eval(0.55f);
                act.Lean += Mathf.Sin(now * 14f) * 1.2f;
                act.Drop += 0.05f;
            }
            else if (_clip != null)
            {
                float t = now - _clipStart;
                if (t >= _clip.Duration) _clip = null; else act = _clip.Eval(t);
            }
            if (_xfade < 1f)
            {
                _xfade = Mathf.Min(1f, _xfade + dt / 0.09f);
                act = ActPose.Lerp(_xfadeFrom, act, _xfade);
            }
            _lastAct = act;

            float hit = Mathf.Max(0f, 1f - (now - _hitAt) / 0.28f);
            hit *= hit;

            // ---- frame
            Quaternion frame = _holder.rotation * Quaternion.Euler(0f, _rig.FrameYaw, 0f);
            Vector3 right = frame * Vector3.right;
            Vector3 fwd = frame * Vector3.forward;

            // ---- start from the as-imported pose
            for (int i = 0; i < _bones.Length; i++) _bones[i].localRotation = _rest[i];
            _hips.localPosition = _hipsLocalPos;
            Vector3 anchor = _hips.parent != null ? _hips.parent.TransformPoint(_hipsLocalPos) : _hips.position;

            float breathe = Mathf.Sin(now * 1.9f);
            float swing = Mathf.Sin(_phase);
            float lean = st.Lean * _walk + act.Lean - 11f * hit + 0.8f * breathe * (1f - _walk);
            float twist = 6f * swing * _walk + act.Twist + 6f * hit;
            float hipTwist = -4f * swing * _walk + act.HipTwist;

            bool legsFree = !_dead && !scaled;
            if (legsFree)
            {
                float bob = st.Bob * _legLen * 0.5f * (1f - Mathf.Cos(2f * _phase)) * _walk;
                float crouch = st.Crouch * _legLen * (1f - _walk * 0.5f);
                float drop = Mathf.Min(0.5f * _legLen, bob + crouch + (act.Drop + 0.04f * hit) * _legLen);
                float hover = st.Hover > 0f ? st.Hover * _legLen * (1f + 0.12f * Mathf.Sin(now * 2.3f)) : 0f;
                float lift = act.Lift * _legLen;
                Vector3 hipsPos = anchor + Vector3.up * (hover + lift - drop) + fwd * (act.Fwd * _legLen);
                _hips.position = hipsPos;
                float hipLean = _rig.Spine == null ? lean * 0.7f : 0f;
                _hips.rotation = Quaternion.AngleAxis(hipTwist, Vector3.up) * Quaternion.AngleAxis(hipLean, right) * _hips.rotation;
                PoseTorso(lean, twist, right);

                for (int i = 0; i < 2; i++)
                {
                    float th = _phase + (i == 0 ? 0f : Mathf.PI);
                    float s = Mathf.Sin(th), c = Mathf.Cos(th);
                    float zIdle = (i == 0 ? st.ZL : st.ZR) * _legLen;
                    float z = stride * s * _walk + zIdle * (1f - _walk);
                    float liftF = st.KneeLift * _legLen * Mathf.Max(0f, c) * _walk;
                    if (st.Hover > 0f)
                    {
                        z = -st.Dangle * _legLen * (0.6f + 0.4f * _walk) + 0.06f * _legLen * s * _walk;
                        liftF = 0f;
                    }
                    float lw = Mathf.Clamp01(act.LegW);
                    z = Mathf.Lerp(z, (i == 0 ? act.ZL : act.ZR) * _legLen, lw);
                    liftF *= 1f - lw;
                    float latK = Mathf.Lerp(st.Spread, 1f, _walk * 0.7f) + 0.06f * lw;
                    Vector3 target = anchor + right * (_legLat[i] * latK) + Vector3.up * (-_hAnkle[i] + hover + liftF) + fwd * z;
                    SolveLeg(i, target, fwd, frame);
                }
            }
            else
            {
                PoseTorso(lean, twist, right);
            }

            // ---- arms
            for (int i = 0; i < 2; i++)
            {
                float side = i == 1 ? 1f : -1f;
                ArmPose idle = i == 1 ? st.IdleR : st.IdleL;
                float hold = i == 1 ? st.HoldR : st.HoldL;
                ArmPose over = i == 1 ? act.R : act.L;
                float sw = Mathf.Sin(_phase + (i == 1 ? 0f : Mathf.PI));
                float k = _walk * (1f - hold);

                float runUF = 22f + st.ArmSwing * sw;
                float uf = Mathf.Lerp(idle.UF, runUF, k) + 1.5f * breathe * (1f - _walk);
                float ul = Mathf.Lerp(idle.UL, 10f, k);
                float lf = Mathf.Lerp(idle.LF, runUF + st.Elbow, k);
                float ll = Mathf.Lerp(idle.LL, 6f, k);

                float w = Mathf.Clamp01(over.W);
                uf = Mathf.Lerp(uf, over.UF, w) + 18f * hit;
                ul = Mathf.Lerp(ul, over.UL, w);
                lf = Mathf.Lerp(lf, over.LF, w) + 10f * hit;
                ll = Mathf.Lerp(ll, over.LL, w);

                Aim(_rig.UpperArm[i], _rig.LowerArm[i], frame * ArmDir(side, uf, ul));
                Aim(_rig.LowerArm[i], _rig.Hand[i], frame * ArmDir(side, lf, ll));
            }
        }

        private void PoseTorso(float lean, float twist, Vector3 right)
        {
            if (_rig.Spine != null)
            {
                float cs = _rig.Chest != null ? 0.6f : 1f;
                _rig.Spine.rotation = Quaternion.AngleAxis(twist * 0.5f, Vector3.up) * Quaternion.AngleAxis(lean * cs, right) * _rig.Spine.rotation;
            }
            if (_rig.Chest != null)
            {
                _rig.Chest.rotation = Quaternion.AngleAxis(twist * 0.5f, Vector3.up) * Quaternion.AngleAxis(lean * 0.4f, right) * _rig.Chest.rotation;
            }
            if (_rig.Head != null && _rig.Spine != null)
            {
                _rig.Head.rotation = Quaternion.AngleAxis(-lean * 0.5f, right) * _rig.Head.rotation;   // keep the head level
            }
        }

        /// <summary>Body-space direction of an arm segment (see ArmPose).</summary>
        private static Vector3 ArmDir(float side, float f, float l)
        {
            float fr = f * Mathf.Deg2Rad, lr = l * Mathf.Deg2Rad;
            float cl = Mathf.Cos(lr);
            return new Vector3(side * Mathf.Sin(lr), -cl * Mathf.Cos(fr), cl * Mathf.Sin(fr));
        }

        /// <summary>Rotates a bone so the segment from it to its child points along dir.</summary>
        private static void Aim(Transform bone, Transform child, Vector3 dir)
        {
            Vector3 cur = child.position - bone.position;
            if (cur.sqrMagnitude < 1e-10f || dir.sqrMagnitude < 1e-10f) return;
            bone.rotation = Quaternion.FromToRotation(cur.normalized, dir.normalized) * bone.rotation;
        }

        /// <summary>Two-bone IK: put the ankle on the target with the knee bending forward.</summary>
        private void SolveLeg(int i, Vector3 target, Vector3 fwd, Quaternion frame)
        {
            Transform thigh = _rig.UpperLeg[i], shin = _rig.LowerLeg[i], foot = _rig.Foot[i];
            Vector3 hip = thigh.position;
            Vector3 toTarget = target - hip;
            float dist = toTarget.magnitude;
            if (dist < 1e-4f) return;

            float a = _legA[i], b = _legB[i];
            float d = Mathf.Clamp(dist, Mathf.Abs(a - b) + 0.001f, a + b - 0.001f);
            Vector3 u = toTarget / dist;
            float cosA = Mathf.Clamp((a * a + d * d - b * b) / (2f * a * d), -1f, 1f);
            float angle = Mathf.Acos(cosA) * Mathf.Rad2Deg;

            Vector3 n = Vector3.Cross(u, fwd);
            if (n.sqrMagnitude < 1e-6f) n = frame * Vector3.right;
            n.Normalize();
            Vector3 d1 = Quaternion.AngleAxis(angle, n) * u;
            Vector3 d2 = Quaternion.AngleAxis(-angle, n) * u;
            Vector3 thighDir = Vector3.Dot(d1, fwd) >= Vector3.Dot(d2, fwd) ? d1 : d2;   // the knee goes forward

            Aim(thigh, shin, thighDir);
            Vector3 shinDir = target - shin.position;
            if (shinDir.sqrMagnitude > 1e-8f) Aim(shin, foot, shinDir.normalized);
            foot.rotation = frame * _footInFrame[i];   // keep the foot flat
        }
    }
}
