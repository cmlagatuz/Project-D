// Project D  |  Codename: Zantetsuken
// ReconnectController.cs  (Sprint 5 — client reconnect state machine)
//
// Drives the client's reconnect experience: on a dropped connection it moves to
// Reconnecting and retries with exponential backoff (capped), invoking a supplied
// connect callback that presents the saved session token. On success the server
// reattaches the same entity and streams a FullState to resync. Also maintains a
// smoothed RTT estimate the client reports to the server for lag compensation.
//
// Pure-ish: the only Unity dependency is none — it takes a callback and is driven
// by Tick(dt), so it is unit-testable.

using System;

namespace ProjectD.Networking.Reconnect
{
    public enum ClientLinkState { Connecting, Connected, Reconnecting, Failed }

    public sealed class ReconnectController
    {
        private readonly Action<bool> _connect;   // (isReconnect)
        private float _retryTimer;
        private int _attempt;

        public ClientLinkState State { get; private set; } = ClientLinkState.Connecting;
        public int MaxAttempts { get; set; } = 6;
        public float BaseBackoffSeconds { get; set; } = 1f;
        public float MaxBackoffSeconds { get; set; } = 12f;
        public float EstimatedRtt { get; private set; } = 0.09f;

        public event Action<ClientLinkState> OnStateChanged;

        public ReconnectController(Action<bool> connectCallback)
        {
            _connect = connectCallback ?? throw new ArgumentNullException(nameof(connectCallback));
        }

        public void NotifyConnected()
        {
            _attempt = 0;
            _retryTimer = 0f;
            SetState(ClientLinkState.Connected);
        }

        public void NotifyDisconnected()
        {
            if (State == ClientLinkState.Failed) return;
            _attempt = 0;
            _retryTimer = NextBackoff();
            SetState(ClientLinkState.Reconnecting);
        }

        public void Tick(float dt)
        {
            if (State != ClientLinkState.Reconnecting) return;

            _retryTimer -= dt;
            if (_retryTimer > 0f) return;

            if (_attempt >= MaxAttempts)
            {
                SetState(ClientLinkState.Failed);
                return;
            }

            _attempt++;
            _retryTimer = NextBackoff();
            _connect(true); // attempt reconnect with saved token
        }

        /// <summary>Feed a new RTT sample (seconds); keeps an EMA for stability.</summary>
        public void ReportRttSample(float rttSeconds)
        {
            rttSeconds = Math.Min(Math.Max(0f, rttSeconds), 1f);
            EstimatedRtt = EstimatedRtt <= 0f ? rttSeconds : EstimatedRtt * 0.85f + rttSeconds * 0.15f;
        }

        public string Describe() => State switch
        {
            ClientLinkState.Connecting => "Connecting…",
            ClientLinkState.Connected => "Connected",
            ClientLinkState.Reconnecting => $"Reconnecting… (attempt {_attempt}/{MaxAttempts})",
            ClientLinkState.Failed => "Disconnected — could not reconnect",
            _ => State.ToString()
        };

        private float NextBackoff()
        {
            float b = BaseBackoffSeconds * (float)Math.Pow(2, Math.Max(0, _attempt));
            return Math.Min(b, MaxBackoffSeconds);
        }

        private void SetState(ClientLinkState s)
        {
            if (s == State) return;
            State = s;
            OnStateChanged?.Invoke(s);
        }
    }
}
