// Project D  |  Codename: Zantetsuken
// TwoClientHarness.cs  (Sprint 5+ — multi-client test harness)
//
// Runtime driver that stands up ONE AuthoritativeMatchServer and TWO independent
// NetworkTransportClients in a single process (UDP loopback), then drives simple
// scripted movement/casts so you can watch two players replicate to each other
// with interest management + reconnect. It also exposes a manual "drop client B"
// button to exercise the reconnect/resync path live.
//
// This is the in-editor stand-in for a true multi-device test; for real devices
// run a headless server build and point each device's client at its address.

using System.Collections;
using UnityEngine;
using ProjectD.Data;
using ProjectD.Core;
using ProjectD.Networking;
using ProjectD.Networking.Server;

namespace ProjectD.Networking.TestHarness
{
    public sealed class TwoClientHarness : MonoBehaviour
    {
        [SerializeField] private ChampionDefinition champion;
        [SerializeField] private int port = 7787;
        [SerializeField] private bool driveScriptedMotion = true;

        private AuthoritativeMatchServer _server;
        private NetworkTransportClient _clientA;
        private NetworkTransportClient _clientB;

        private void Start()
        {
            // Server
            var serverGo = new GameObject("Harness_Server");
            serverGo.transform.SetParent(transform, false);
            _server = serverGo.AddComponent<AuthoritativeMatchServer>();
            _server.ConfigureForHeadless(champion, port, 30);
            _server.SpawnGreyboxNeutrals();

            // Two clients, both pointing at the loopback server, on different teams.
            _clientA = MakeClient("Harness_ClientA", team: 0);
            _clientB = MakeClient("Harness_ClientB", team: 1);

            if (driveScriptedMotion) StartCoroutine(DriveMotion());
            Debug.Log("[Project D][Harness] Server + 2 clients started. Watch the console for replication + hits.");
        }

        private NetworkTransportClient MakeClient(string name, int team)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var client = go.AddComponent<NetworkTransportClient>();
            client.ConfigureEndpoint("127.0.0.1", port, team, autoConnect: true);

            client.OnEntityMoved += (id, x, z) =>
            {
                if (id != client.LocalEntityId) // only log OTHERS to prove replication
                    Debug.Log($"[Project D][{name}] sees entity {id} at ({x:F1},{z:F1})");
            };
            client.OnHealthChanged += (id, hp, max) =>
                Debug.Log($"[Project D][{name}] entity {id} health {hp:F0}/{max:F0}");
            return client;
        }

        // Scripted: A walks right and casts Q at B; B walks left. Demonstrates
        // cross-client replication + server-authoritative, lag-compensated hits.
        private IEnumerator DriveMotion()
        {
            yield return new WaitForSeconds(1f); // let connect/handshake settle

            float t = 0f;
            while (true)
            {
                t += Time.deltaTime;
                // Move intents (client-predicted; server validates & reconciles).
                _clientA.SendMove(new MoveIntent { SequenceNumber = Mathf.RoundToInt(t * 30),
                    TargetX = -20 + Mathf.PingPong(t * 4f, 12f), TargetZ = 0, ClientSendTime = Time.time });
                _clientB.SendMove(new MoveIntent { SequenceNumber = Mathf.RoundToInt(t * 30),
                    TargetX = 20 - Mathf.PingPong(t * 4f, 12f), TargetZ = 0, ClientSendTime = Time.time });

                // Every ~2s, client A fires Q toward +X (where B is heading).
                if (Mathf.FloorToInt(t) % 2 == 0 && Mathf.Repeat(t, 1f) < Time.deltaTime)
                {
                    _clientA.SendCast(new CastIntent
                    {
                        SequenceNumber = Mathf.RoundToInt(t * 30),
                        AbilitySlot = (int)AbilitySlot.Q,
                        OriginX = _clientA.LastKnownX, OriginZ = _clientA.LastKnownZ,
                        AimX = 1, AimZ = 0, TargetEntityId = -1, ClientSendTime = Time.time
                    });
                }
                yield return null;
            }
        }

        // Hook this to a UI button to test reconnect live.
        public void ForceDropClientB()
        {
            Debug.Log("[Project D][Harness] Forcing client B disconnect to test reconnect…");
            _clientB.SimulateConnectionLoss();
        }
    }
}
