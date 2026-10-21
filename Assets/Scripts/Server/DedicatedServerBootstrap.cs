// Project D  |  Codename: Zantetsuken
// DedicatedServerBootstrap.cs  (Sprint 5+ — headless dedicated server entry point)
//
// The entry point for a headless server build. Detects batchmode/-server, reads
// port + tick overrides from the command line, forces a fixed server framerate
// (no rendering), spawns a match of AI/greybox minions to exercise interest
// management, and starts the AuthoritativeMatchServer.
//
// Build via: Tools > Project D > Build Headless Dedicated Server (see
// Editor/DedicatedServerBuild.cs). Run: ProjectD_Server -batchmode -nographics
// -server -port 7777 -tick 30
//
// Kept Unity-side (uses Application/Time) but delegates all fairness logic to the
// engine-agnostic server pieces.

using UnityEngine;
using ProjectD.Data;

namespace ProjectD.Networking.Server
{
    [DefaultExecutionOrder(-1000)]
    public sealed class DedicatedServerBootstrap : MonoBehaviour
    {
        [SerializeField] private ChampionDefinition defaultChampion;
        [SerializeField] private int defaultPort = 7777;
        [SerializeField] private int targetTickRate = 30;
        [SerializeField] private bool spawnGreyboxBots = true;

        private AuthoritativeMatchServer _server;

        public static bool IsHeadlessServer =>
            Application.isBatchMode || HasArg("-server");

        private void Awake()
        {
            if (!IsHeadlessServer)
            {
                // Not a server process; do nothing (client scenes ignore this).
                enabled = false;
                return;
            }

            int port = GetIntArg("-port", defaultPort);
            int tick = GetIntArg("-tick", targetTickRate);

            // Headless perf: no vsync, fixed server tick, minimal CPU spin.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(30, tick);
            Application.runInBackground = true;

            Debug.Log($"[Project D][Server] Starting dedicated server on :{port} @ {tick}Hz " +
                      $"(headless={Application.isBatchMode}).");

            var serverGo = new GameObject("AuthoritativeMatchServer");
            serverGo.transform.SetParent(transform, false);
            _server = serverGo.AddComponent<AuthoritativeMatchServer>();
            _server.ConfigureForHeadless(defaultChampion, port, tick);

            if (spawnGreyboxBots)
                _server.SpawnGreyboxNeutrals();

            Debug.Log("[Project D][Server] Ready. Waiting for players…");
        }

        // ---- command-line helpers ---- //
        private static string[] Args => System.Environment.GetCommandLineArgs();

        private static bool HasArg(string flag)
        {
            foreach (var a in Args) if (a == flag) return true;
            return false;
        }

        private static int GetIntArg(string flag, int fallback)
        {
            var args = Args;
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == flag && int.TryParse(args[i + 1], out int v)) return v;
            return fallback;
        }
    }
}
