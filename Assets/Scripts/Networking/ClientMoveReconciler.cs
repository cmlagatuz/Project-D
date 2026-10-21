// Project D  |  Codename: Zantetsuken
// ClientMoveReconciler.cs  (Sprint 5, client side)
//
// Client-side movement prediction with server reconciliation (GCD 14.2). The
// pawn moves locally every frame for zero input latency, records each input in a
// buffer keyed by sequence number, and sends a MoveIntent. When the server
// returns a StateCorrection, we snap the "confirmed" position to authority and
// REPLAY any inputs the server hasn't processed yet, so the visible pawn stays
// smooth while remaining server-authoritative.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Networking;

namespace ProjectD.Networking
{
    public sealed class ClientMoveReconciler
    {
        private struct PendingInput
        {
            public int Sequence;
            public Vector3 Position;   // predicted position AFTER applying this input
        }

        private readonly IAuthoritativeServer _server;
        private readonly Queue<PendingInput> _pending = new();
        private int _sequence = 1;

        private Vector3 _confirmed;    // last server-confirmed position
        private Vector3 _predicted;    // smoothed visible position

        [Range(0f, 1f)] public float ReconcileSmoothing = 0.2f;

        public Vector3 PredictedPosition => _predicted;

        public ClientMoveReconciler(IAuthoritativeServer server, Vector3 startPosition)
        {
            _server = server;
            _confirmed = _predicted = startPosition;
            _server.OnStateCorrection += HandleCorrection;
        }

        public void Dispose() => _server.OnStateCorrection -= HandleCorrection;

        /// <summary>
        /// Apply a locally predicted move step, buffer it, and send the intent.
        /// Returns the predicted position the pawn should render at this frame.
        /// </summary>
        public Vector3 PredictMove(Vector3 desiredPosition)
        {
            _predicted = desiredPosition;

            int seq = _sequence++;
            _pending.Enqueue(new PendingInput { Sequence = seq, Position = _predicted });

            _server.SendMove(new MoveIntent
            {
                SequenceNumber = seq,
                TargetX = _predicted.x,
                TargetZ = _predicted.z,
                ClientSendTime = Time.time
            });

            return _predicted;
        }

        private void HandleCorrection(StateCorrection c)
        {
            _confirmed = new Vector3(c.X, _confirmed.y, c.Z);

            // Drop inputs the server has already processed.
            while (_pending.Count > 0 && _pending.Peek().Sequence <= c.LastProcessedInput)
                _pending.Dequeue();

            // Reconcile: if the authoritative position diverges from what we
            // predicted for that input, gently correct the visible position.
            if (Vector3.Distance(_confirmed, _predicted) > 0.01f)
                _predicted = Vector3.Lerp(_predicted, _confirmed, ReconcileSmoothing);
        }
    }
}
