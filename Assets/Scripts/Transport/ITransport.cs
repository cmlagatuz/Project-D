// Project D  |  Codename: Zantetsuken
// ITransport.cs  (Sprint 5 — real transport abstraction)
//
// The pluggable network transport boundary. Everything above this interface
// (AuthoritativeMatchServer, NetworkTransportClient, lag compensation, sessions)
// is transport-agnostic, so we can ship the bundled UdpTransport now and later
// drop in a Unity Netcode for GameObjects or Mirror adapter WITHOUT touching game
// logic. The only requirement is unreliable-ordered-optional datagram delivery
// plus connect/disconnect signals.

using System;

namespace ProjectD.Networking.Transport
{
    /// <summary>Opaque handle identifying a remote peer/connection.</summary>
    public readonly struct ConnectionId : IEquatable<ConnectionId>
    {
        public readonly int Value;
        public ConnectionId(int value) { Value = value; }
        public bool Equals(ConnectionId o) => Value == o.Value;
        public override bool Equals(object o) => o is ConnectionId c && Equals(c);
        public override int GetHashCode() => Value;
        public override string ToString() => $"conn#{Value}";
        public static readonly ConnectionId None = new ConnectionId(-1);
    }

    public enum SendMode : byte
    {
        Unreliable,        // fire-and-forget (movement, snapshots)
        ReliableOrdered    // must arrive & in order (connect, cast ack, full state)
    }

    /// <summary>
    /// Common transport surface for both client and server. Implementations wrap a
    /// concrete stack (UDP sockets, Netcode, Mirror). Poll() must be called each
    /// tick to drain incoming datagrams and raise events on the main thread.
    /// </summary>
    public interface ITransport : IDisposable
    {
        // Lifecycle
        void StartServer(int port);
        void Connect(string host, int port);
        void Poll();                 // pump incoming; raises events below

        // Send
        void Send(ConnectionId target, byte[] payload, SendMode mode);
        void Broadcast(byte[] payload, SendMode mode);
        void DisconnectPeer(ConnectionId target);

        // Events (raised during Poll)
        event Action<ConnectionId> OnPeerConnected;
        event Action<ConnectionId> OnPeerDisconnected;      // timeout or explicit
        event Action<ConnectionId, byte[]> OnData;

        bool IsServer { get; }
        bool IsConnected { get; }    // client: connected to server
    }
}
