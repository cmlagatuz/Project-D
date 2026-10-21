// Project D  |  Codename: Zantetsuken
// AnimatorControllerGenerator.cs  (Editor-only)
//
// Generates an AnimatorController per champion from its ChampionAnimationSet:
//   - Base layer: Locomotion blend (Idle <-> Run via 'Speed'), plus Spawn/Death/Recall.
//   - Ability states: Q/W/E/R (and charge/recast where present), entered by triggers
//     (TrigQ/TrigW/TrigE/TrigR) and exiting back to Locomotion when done.
//   - An additive 'Upper' layer slot for passive auras / guard holds.
//
// State names equal the clip names in the set, so once the artist drops the FBX
// animation clips in (matching those names) the controller is ready. Missing clips
// simply leave an empty state you can assign later.
//
//   Tools > Project D > Generate Animator Controllers

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.EditorTools
{
    public static class AnimatorControllerGenerator
    {
        private const string AnimDir = "Assets/GameData/Animations";
        private const string OutDir = "Assets/GameData/Animations/Controllers";

        [MenuItem("Tools/Project D/Generate Animator Controllers", priority = 66)]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(OutDir))
                AssetDatabase.CreateFolder(AnimDir, "Controllers");

            int made = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:ChampionAnimationSet", new[] { AnimDir }))
            {
                var set = AssetDatabase.LoadAssetAtPath<ChampionAnimationSet>(AssetDatabase.GUIDToAssetPath(guid));
                if (set == null) continue;
                BuildController(set);
                made++;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Generated {made} AnimatorController(s) in {OutDir}.\n\n" +
                "States are named after clips in each set — drop matching FBX clips to fill them.\n" +
                "Assign the controller to the champion prefab's Animator.", "OK");
            Debug.Log($"[Project D] Generated {made} animator controllers.");
        }

        private static void BuildController(ChampionAnimationSet set)
        {
            string path = $"{OutDir}/AC_{set.championId}.controller";
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Parameters
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ac.AddParameter("AttackSpeed", AnimatorControllerParameterType.Float);
            foreach (var p in new[] { "TrigQ", "TrigW", "TrigE", "TrigR", "TrigAttack",
                                       "TrigHit", "TrigStun", "TrigKnockup", "TrigDeath", "TrigRecall", "AbilityDone" })
                ac.AddParameter(p, AnimatorControllerParameterType.Trigger);

            var baseLayer = ac.layers[0];
            var sm = baseLayer.stateMachine;

            // Locomotion: 1D blend Idle<->Run on Speed.
            var loco = sm.AddState("Locomotion", new Vector3(300, 0, 0));
            var blend = new BlendTree { name = "LocomotionBlend", blendType = BlendTreeType.Simple1D, blendParameter = "Speed" };
            AssetDatabase.AddObjectToAsset(blend, ac);
            blend.AddChild(MakeStub(set.idleClip), 0f);
            blend.AddChild(MakeStub(set.runClip), 1f);
            loco.motion = blend;
            sm.defaultState = loco;

            // Utility states
            AddSimple(sm, set.spawnClip, new Vector3(0, -120, 0), loco);
            var death = AddSimple(sm, set.deathClip, new Vector3(600, 120, 0), null);
            AddTransition(loco, death, "TrigDeath");
            var recall = AddSimple(sm, set.recallClip, new Vector3(600, -120, 0), loco);
            AddTransition(loco, recall, "TrigRecall");

            // Ability states, each returns to Locomotion on AbilityDone.
            AddAbility(sm, ac, set.abilityQ, "TrigQ", loco, new Vector3(300, 160, 0));
            AddAbility(sm, ac, set.abilityW, "TrigW", loco, new Vector3(300, 240, 0));
            AddAbility(sm, ac, set.abilityE, "TrigE", loco, new Vector3(300, 320, 0));
            AddAbility(sm, ac, set.ultimate, "TrigR", loco, new Vector3(300, 400, 0));

            // Additive upper-body layer for passive auras / guard holds.
            var upper = new AnimatorControllerLayer
            {
                name = "Upper", defaultWeight = 1f, blendingMode = AnimatorLayerBlendingMode.Additive,
                stateMachine = new AnimatorStateMachine { name = "Upper", hideFlags = HideFlags.HideInHierarchy }
            };
            AssetDatabase.AddObjectToAsset(upper.stateMachine, ac);
            var upEmpty = upper.stateMachine.AddState("UpperEmpty");
            upper.stateMachine.defaultState = upEmpty;
            ac.AddLayer(upper);

            EditorUtility.SetDirty(ac);
        }

        private static AnimatorState AddAbility(AnimatorStateMachine sm, AnimatorController ac, AbilityAnimation anim,
                                                string trigger, AnimatorState back, Vector3 pos)
        {
            if (anim == null || string.IsNullOrEmpty(anim.clipName)) return null;
            var st = sm.AddState(anim.clipName, pos);
            st.motion = MakeStub(anim.clipName);

            var enter = back.AddTransition(st);
            enter.AddCondition(AnimatorConditionMode.If, 0, trigger);
            enter.hasExitTime = false; enter.duration = 0.05f;

            if (anim.loop)
            {
                // Loop states exit on AbilityDone (server/gameplay drives it).
                var exit = st.AddTransition(back);
                exit.AddCondition(AnimatorConditionMode.If, 0, "AbilityDone");
                exit.hasExitTime = false; exit.duration = 0.08f;
            }
            else
            {
                var exit = st.AddTransition(back);
                exit.hasExitTime = true; exit.exitTime = 0.95f; exit.duration = 0.08f;
            }
            return st;
        }

        private static AnimatorState AddSimple(AnimatorStateMachine sm, string clip, Vector3 pos, AnimatorState backOrNull)
        {
            var st = sm.AddState(string.IsNullOrEmpty(clip) ? "state" : clip, pos);
            st.motion = MakeStub(clip);
            if (backOrNull != null)
            {
                var t = st.AddTransition(backOrNull);
                t.hasExitTime = true; t.exitTime = 0.95f; t.duration = 0.1f;
            }
            return st;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, string trigger)
        {
            var t = from.AddTransition(to);
            t.AddCondition(AnimatorConditionMode.If, 0, trigger);
            t.hasExitTime = false; t.duration = 0.05f;
        }

        // A placeholder clip so states are valid before the FBX clips are dropped in.
        private static AnimationClip MakeStub(string name)
        {
            var clip = new AnimationClip { name = string.IsNullOrEmpty(name) ? "empty" : name, legacy = false };
            return clip;
        }

        // ---- Neutral (summon / monster / minion) controllers ---- //
        private const string NeutralDir = "Assets/GameData/Animations/Neutrals";

        [MenuItem("Tools/Project D/Generate Neutral Controllers", priority = 69)]
        public static void GenerateNeutrals()
        {
            if (!AssetDatabase.IsValidFolder(OutDir))
                AssetDatabase.CreateFolder(AnimDir, "Controllers");

            int made = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:NeutralAnimationSet", new[] { NeutralDir }))
            {
                var set = AssetDatabase.LoadAssetAtPath<NeutralAnimationSet>(AssetDatabase.GUIDToAssetPath(guid));
                if (set == null) continue;
                BuildNeutralController(set);
                made++;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D",
                $"Generated {made} neutral AnimatorController(s) in {OutDir}.", "OK");
            Debug.Log($"[Project D] Generated {made} neutral controllers.");
        }

        private static void BuildNeutralController(NeutralAnimationSet set)
        {
            string path = $"{OutDir}/AC_neutral_{set.neutralId}.controller";
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
            foreach (var p in new[] { "TrigAttack", "TrigSigA", "TrigSigB", "TrigSigUlt",
                                       "TrigEnrage", "TrigDeath", "TrigReward", "AbilityDone" })
                ac.AddParameter(p, AnimatorControllerParameterType.Trigger);

            var sm = ac.layers[0].stateMachine;

            var loco = sm.AddState("Locomotion", new Vector3(300, 0, 0));
            var blend = new BlendTree { name = "NeutralBlend", blendType = BlendTreeType.Simple1D, blendParameter = "Speed" };
            AssetDatabase.AddObjectToAsset(blend, ac);
            blend.AddChild(MakeStub(set.idleClip), 0f);
            blend.AddChild(MakeStub(set.moveClip), 1f);
            loco.motion = blend;
            sm.defaultState = loco;

            AddSimple(sm, set.spawnClip, new Vector3(0, -120, 0), loco);
            if (set.basicAttacks != null && set.basicAttacks.Length > 0)
                AddNeutralState(sm, set.basicAttacks[0], "TrigAttack", loco, new Vector3(300, 120, 0), false);
            if (set.signatureA != null) AddNeutralState(sm, set.signatureA.clipName, "TrigSigA", loco, new Vector3(300, 190, 0), set.signatureA.loop);
            if (set.signatureB != null) AddNeutralState(sm, set.signatureB.clipName, "TrigSigB", loco, new Vector3(300, 260, 0), set.signatureB.loop);
            if (set.signatureUltimate != null) AddNeutralState(sm, set.signatureUltimate.clipName, "TrigSigUlt", loco, new Vector3(300, 330, 0), set.signatureUltimate.loop);
            if (!string.IsNullOrEmpty(set.enrageClip)) AddNeutralState(sm, set.enrageClip, "TrigEnrage", loco, new Vector3(600, 60, 0), false);
            if (!string.IsNullOrEmpty(set.rewardGrantClip)) AddNeutralState(sm, set.rewardGrantClip, "TrigReward", loco, new Vector3(600, -60, 0), false);
            var death = AddSimple(sm, set.deathClip, new Vector3(600, 180, 0), null);
            AddTransition(loco, death, "TrigDeath");

            EditorUtility.SetDirty(ac);
        }

        private static void AddNeutralState(AnimatorStateMachine sm, string clip, string trigger,
                                            AnimatorState back, Vector3 pos, bool loop)
        {
            if (string.IsNullOrEmpty(clip)) return;
            var st = sm.AddState(clip, pos);
            st.motion = MakeStub(clip);
            var enter = back.AddTransition(st);
            enter.AddCondition(AnimatorConditionMode.If, 0, trigger);
            enter.hasExitTime = false; enter.duration = 0.05f;
            var exit = st.AddTransition(back);
            if (loop) { exit.AddCondition(AnimatorConditionMode.If, 0, "AbilityDone"); exit.hasExitTime = false; }
            else { exit.hasExitTime = true; exit.exitTime = 0.95f; }
            exit.duration = 0.08f;
        }
    }
}
#endif
