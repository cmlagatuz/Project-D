// Project D  |  Codename: Zantetsuken
// SessionManager.cs  (Sprint 5 — reconnect / resync)
//
// Server-side player-session registry that survives a dropped connection. Each
// player gets a secret session token at connect time. If their transport peer
// disconnects, the session is kept ALIVE for a grace window and their champion is
// flagged disconnected (AI-held or frozen per design) rather than removed. When
// the player returns, they present the token to RECLAIM the same entity, and the
// server replies with a full authoritative snapshot to RESYNC.
//
// Pure C# — deterministic and unit-testable. Token generation uses an injectable
// RNG so tests are reproducible; production uses a cryptographically strong source.

using System;
using System.Collections.Generic;
using ProjectD.Networking.Transport;

namespace ProjectD.Networking.Reconnect
{
    public enum SessionState { Active, Disconnected, Expired }

    public sealed class PlayerSession
    {
        public int EntityId;
        public int Team;
        public string Token;
        public ConnectionId Connection;
        public SessionState State;
        public float DisconnectedAtServerTime;
        public int LastAckedInput;         // for input resync
        public float LastSnapshotTime;
    }

    public sealed class SessionManager
    {
        private readonly Dictionary<string, PlayerSession> _byToken = new();
        private readonly Dictionary<int, PlayerSession> _byConnection = new();
        private readonly Dictionary<int, PlayerSession> _byEntity = new();
        private readonly Func<string> _tokenFactory;

        /// <summary>Grace window a disconnected player may reclaim their entity.</summary>
        public float ReconnectGraceSeconds { get; set; } = 90f;

        public SessionManager(Func<string> tokenFactory = null)
        {
            _tokenFactory = tokenFactory ?? DefaultTokenFactory;
        }

        public IReadOnlyCollection<PlayerSession> All => _byEntity.Values;

        /// <summary>Register a brand-new player and mint a session token.</summary>
        public PlayerSession CreateSession(ConnectionId conn, int entityId, int team)
        {
            var session = new PlayerSession
            {
                EntityId = entityId,
                Team = team,
                Token = _tokenFactory(),
                Connection = conn,
                State = SessionState.Active
            };
            _byToken[session.Token] = session;
            _byConnection[conn.Value] = session;
            _byEntity[entityId] = session;
            return session;
        }

        public PlayerSession GetByConnection(ConnectionId conn)
            => _byConnection.TryGetValue(conn.Value, out var s) ? s : null;

        public PlayerSession GetByEntity(int entityId)
            => _byEntity.TryGetValue(entityId, out var s) ? s : null;

        /// <summary>Transport reported this peer dropped. Flag the session, keep the entity.</summary>
        public PlayerSession MarkDisconnected(ConnectionId conn, float serverTime)
        {
            if (!_byConnection.TryGetValue(conn.Value, out var s)) return null;
            s.State = SessionState.Disconnected;
            s.DisconnectedAtServerTime = serverTime;
            _byConnection.Remove(conn.Value); // old connection is gone
            return s;
        }

        /// <summary>
        /// Attempt to reclaim a disconnected session with its token on a NEW peer.
        /// Returns the reattached session, or null if the token is unknown/expired.
        /// </summary>
        public PlayerSession TryReconnect(string token, ConnectionId newConn, float serverTime)
        {
            if (string.IsNullOrEmpty(token) || !_byToken.TryGetValue(token, out var s))
                return null;
            if (s.State == SessionState.Expired) return null;
            if (s.State == SessionState.Disconnected &&
                serverTime - s.DisconnectedAtServerTime > ReconnectGraceSeconds)
            {
                s.State = SessionState.Expired;
                return null;
            }

            s.State = SessionState.Active;
            s.Connection = newConn;
            _byConnection[newConn.Value] = s;
            return s;
        }

        /// <summary>Expire sessions whose grace window elapsed (call each tick).</summary>
        public List<PlayerSession> ExpireStale(float serverTime)
        {
            List<PlayerSession> expired = null;
            foreach (var s in _byEntity.Values)
            {
                if (s.State == SessionState.Disconnected &&
                    serverTime - s.DisconnectedAtServerTime > ReconnectGraceSeconds)
                {
                    s.State = SessionState.Expired;
                    (expired ??= new List<PlayerSession>()).Add(s);
                }
            }
            return expired ?? EmptyList;
        }

        private static readonly List<PlayerSession> EmptyList = new();

        private static string DefaultTokenFactory()
            => Guid.NewGuid().ToString("N");
    }
}
