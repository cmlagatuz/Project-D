// Project D  |  Codename: Zantetsuken
// PrototypePawn.cs  (Sprint 1 movement + Sprint 5 networking integration)
//
// The controllable prototype champion. As of Sprint 5, casts and movement route
// through an IAuthoritativeServer via NetworkedCastController + ClientMoveReconciler
// (client prediction, server validation, reconciliation on ack/reject/correction).
//
// If no server is wired (pure single-scene sandbox), it falls back to a local
// AbilitySystem so the pawn still works — but the DEFAULT and intended path is
// networked. In production, swap LocalLoopbackServer for the real transport; this
// pawn does not change because it only depends on IAuthoritativeServer.

using UnityEngine;
using ProjectD.Core;
using ProjectD.Data;
using ProjectD.Gameplay.Combat;
using ProjectD.Gameplay.Champions;
using ProjectD.Gameplay.InputSystem;
using ProjectD.Gameplay.CameraRig;
using ProjectD.Networking;

namespace ProjectD.Gameplay.Champions
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PrototypePawn : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ChampionDefinition champion;

        [Header("Input")]
        [SerializeField] private TouchJoystick moveStick;
        [SerializeField] private AbilityButton[] abilityButtons;

        [Header("Camera")]
        [SerializeField] private ArenaCameraRig cameraRig;

        [Header("Networking (Sprint 5)")]
        [Tooltip("Authoritative server. In the greybox this is a LocalLoopbackServer; " +
                 "in production it is the real transport. If null, falls back to local-only.")]
        [SerializeField] private MonoBehaviour serverBehaviour; // must implement IAuthoritativeServer

        [Header("Movement")]
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = 20f;

        private CharacterController _cc;
        private readonly StatBlock _stats = new();

        // Networked path (preferred).
        private IAuthoritativeServer _server;
        private NetworkedCastController _casts;
        private ClientMoveReconciler _moveNet;

        // Local fallback path.
        private readonly AbilitySystem _localAbilities = new();
        private bool _networked;

        private float _verticalVelocity;
        private Vector3 _lastFacing = Vector3.forward;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            InitStats();
            SetupNetworkingOrFallback();
            HookButtons();
        }

        private void OnDestroy()
        {
            _casts?.Dispose();
            _moveNet?.Dispose();
        }

        private void InitStats()
        {
            if (champion == null) { Debug.LogWarning("[Project D] Pawn has no ChampionDefinition."); return; }
            _stats.BaseMaxHealth = champion.baseHealth;
            _stats.BaseAttackDamage = champion.baseAttackDamage;
            _stats.BaseMagicPower = champion.baseMagicPower;
            _stats.BaseArmor = champion.baseArmor;
            _stats.BaseMagicResist = champion.baseMagicResist;
            _stats.BaseMoveSpeed = champion.baseMoveSpeed;
            _stats.BaseAttackSpeed = champion.baseAttacksPerSecond;
        }

        private void SetupNetworkingOrFallback()
        {
            _server = serverBehaviour as IAuthoritativeServer;
            if (_server != null && champion != null)
            {
                _networked = true;
                _casts = new NetworkedCastController(_server, champion, _stats);
                _casts.OnCastFeedback += HandleCastFeedback;
                _moveNet = new ClientMoveReconciler(_server, transform.position);
                Debug.Log("[Project D] Pawn running NETWORKED (server-authoritative) path.");
            }
            else
            {
                _networked = false;
                _localAbilities.CurrentResource = champion != null ? champion.baseResource : 300f;
                _localAbilities.LearnOrRankUp(AbilitySlot.Q);
                _localAbilities.LearnOrRankUp(AbilitySlot.W);
                _localAbilities.LearnOrRankUp(AbilitySlot.E);
                _localAbilities.LearnOrRankUp(AbilitySlot.Ultimate);
                Debug.Log("[Project D] Pawn running LOCAL fallback path (no server assigned).");
            }
        }

        private void HookButtons()
        {
            if (abilityButtons == null) return;
            foreach (var b in abilityButtons)
                if (b != null) b.OnCastRequested += HandleCastRequest;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_networked) _casts.Tick(dt);
            else _localAbilities.ServerTime += dt;

            TickMovement(dt);
            TickResourceLocalFallback(dt);
            TickCooldownUI();
        }

        private void TickMovement(float dt)
        {
            Vector2 stick = moveStick != null ? moveStick.Direction : Vector2.zero;
            Vector3 planar = new Vector3(stick.x, 0f, stick.y);
            Vector3 velocity = planar * _stats.MoveSpeed;

            if (planar.sqrMagnitude > 0.001f)
            {
                _lastFacing = planar.normalized;
                Quaternion look = Quaternion.LookRotation(_lastFacing, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * dt);
            }

            _verticalVelocity = _cc.isGrounded ? -1f : _verticalVelocity - gravity * dt;
            velocity.y = _verticalVelocity;

            // Move locally (prediction), then send/reconcile through the server.
            _cc.Move(velocity * dt);

            if (_networked && planar.sqrMagnitude > 0.001f)
                _moveNet.PredictMove(transform.position);
        }

        private void TickResourceLocalFallback(float dt)
        {
            if (_networked || champion == null) return;
            _localAbilities.CurrentResource = Mathf.Min(
                champion.baseResource, _localAbilities.CurrentResource + 8f * dt);
        }

        private void TickCooldownUI()
        {
            if (abilityButtons == null) return;
            foreach (var b in abilityButtons)
            {
                if (b == null) continue;
                float fill;
                AbilityDefinition ab = ResolveAbility(b.Slot);
                float baseCd = ab != null ? ab.baseCooldown : 8f;
                fill = _networked
                    ? _casts.CooldownFill(b.Slot, baseCd)
                    : _localAbilities.CooldownRemaining01(b.Slot, baseCd, _stats);
                b.SetCooldownFill(fill);
            }
        }

        private void HandleCastRequest(AbilitySlot slot, Vector2 aim, bool dragged)
        {
            Vector3 dir = dragged ? new Vector3(aim.x, 0f, aim.y).normalized : _lastFacing;

            if (_networked)
            {
                bool predicted = _casts.RequestCast(slot, transform.position, dir);
                if (predicted && slot == AbilitySlot.Ultimate && cameraRig != null)
                    cameraRig.Shake(0.3f);
                return;
            }

            // Local fallback.
            AbilityDefinition ability = ResolveAbility(slot);
            if (ability == null) return;
            var result = _localAbilities.TryCast(ability, _stats);
            if (result != CastResult.Success) { Debug.Log($"[Project D] Local cast {slot} blocked: {result}"); return; }
            float dmg = _localAbilities.ResolveDamage(ability, _stats);
            Debug.Log($"[Project D] (local) {champion.displayName} casts {ability.displayName} dmg={dmg:F0}");
            if (slot == AbilitySlot.Ultimate && cameraRig != null) cameraRig.Shake(0.3f);
        }

        private void HandleCastFeedback(NetworkedCastController.CastFeedback fb)
        {
            if (fb.Accepted)
                Debug.Log($"[Project D] Server CONFIRMED {fb.Slot} — predictedDamage={fb.PredictedDamage:F0}");
            else
                Debug.Log($"[Project D] Server REJECTED {fb.Slot}: {fb.Reason} (predicted state rolled back)");
        }

        private AbilityDefinition ResolveAbility(AbilitySlot slot) => champion == null ? null : slot switch
        {
            AbilitySlot.Q => champion.abilityQ,
            AbilitySlot.W => champion.abilityW,
            AbilitySlot.E => champion.abilityE,
            AbilitySlot.Ultimate => champion.ultimate,
            _ => null
        };

        public void SetChampion(ChampionDefinition def)
        {
            champion = def;
            InitStats();
        }
    }
}
