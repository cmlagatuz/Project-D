// Project D  |  Codename: Zantetsuken
// NetPrimitives.cs  (Sprint 5 — real transport)
//
// Engine-agnostic value types and a compact binary reader/writer used by the wire
// protocol. Kept free of UnityEngine so the dedicated server and unit tests can
// use them headlessly. Positions travel as NetVec2 (planar X/Z); Y is derived
// from the ground on the client.

using System;
using System.Text;

namespace ProjectD.Networking.Transport
{
    public readonly struct NetVec2 : IEquatable<NetVec2>
    {
        public readonly float X, Z;
        public NetVec2(float x, float z) { X = x; Z = z; }

        public static NetVec2 Lerp(NetVec2 a, NetVec2 b, float t)
            => new NetVec2(a.X + (b.X - a.X) * t, a.Z + (b.Z - a.Z) * t);

        public float DistanceTo(NetVec2 o)
        {
            float dx = X - o.X, dz = Z - o.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public bool Equals(NetVec2 o) => X == o.X && Z == o.Z;
        public override string ToString() => $"({X:F2},{Z:F2})";
    }

    /// <summary>Message type ids for the wire protocol.</summary>
    public enum NetMsg : byte
    {
        // Client -> Server
        ConnectRequest = 1,   // new session or reconnect (carries token if reconnecting)
        MoveIntent = 2,
        CastIntent = 3,
        Ping = 4,
        ResyncRequest = 5,    // client asks for a full authoritative snapshot
        Heartbeat = 6,
        Disconnect = 7,

        // Server -> Client
        ConnectAccept = 20,   // carries assigned entityId + session token + tick
        CastResolved = 21,    // ack/reject
        Snapshot = 22,        // delta or keyframe world state
        FullState = 23,       // complete authoritative state (used on reconnect/resync)
        StateCorrection = 24, // movement reconciliation
        MatchEnded = 25,
        Kick = 26
    }

    /// <summary>Minimal, allocation-light binary writer over a byte buffer.</summary>
    public sealed class NetWriter
    {
        private byte[] _buf;
        private int _pos;
        public NetWriter(int capacity = 256) { _buf = new byte[capacity]; }

        public int Length => _pos;

        private void Ensure(int extra)
        {
            if (_pos + extra <= _buf.Length) return;
            int n = _buf.Length * 2;
            while (n < _pos + extra) n *= 2;
            Array.Resize(ref _buf, n);
        }

        public void Byte(byte v) { Ensure(1); _buf[_pos++] = v; }
        public void Bool(bool v) => Byte(v ? (byte)1 : (byte)0);

        public void Int(int v)
        {
            Ensure(4);
            _buf[_pos++] = (byte)(v & 0xFF);
            _buf[_pos++] = (byte)((v >> 8) & 0xFF);
            _buf[_pos++] = (byte)((v >> 16) & 0xFF);
            _buf[_pos++] = (byte)((v >> 24) & 0xFF);
        }

        public void UShort(ushort v) { Ensure(2); _buf[_pos++] = (byte)(v & 0xFF); _buf[_pos++] = (byte)((v >> 8) & 0xFF); }
        public void Float(float v) => Int(BitConverter.SingleToInt32Bits(v));
        public void Vec2(NetVec2 v) { Float(v.X); Float(v.Z); }

        public void Str(string s)
        {
            s ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(s);
            UShort((ushort)bytes.Length);
            Ensure(bytes.Length);
            Buffer.BlockCopy(bytes, 0, _buf, _pos, bytes.Length);
            _pos += bytes.Length;
        }

        public byte[] ToArray()
        {
            var outArr = new byte[_pos];
            Buffer.BlockCopy(_buf, 0, outArr, 0, _pos);
            return outArr;
        }
    }

    /// <summary>Reader counterpart to NetWriter.</summary>
    public sealed class NetReader
    {
        private readonly byte[] _buf;
        private int _pos;
        public NetReader(byte[] buffer, int offset = 0) { _buf = buffer; _pos = offset; }

        public bool HasMore => _pos < _buf.Length;

        public byte Byte() => _buf[_pos++];
        public bool Bool() => Byte() != 0;

        public int Int()
        {
            int v = _buf[_pos] | (_buf[_pos + 1] << 8) | (_buf[_pos + 2] << 16) | (_buf[_pos + 3] << 24);
            _pos += 4; return v;
        }

        public ushort UShort() { ushort v = (ushort)(_buf[_pos] | (_buf[_pos + 1] << 8)); _pos += 2; return v; }
        public float Float() => BitConverter.Int32BitsToSingle(Int());
        public NetVec2 Vec2() => new NetVec2(Float(), Float());

        public string Str()
        {
            int len = UShort();
            var s = Encoding.UTF8.GetString(_buf, _pos, len);
            _pos += len; return s;
        }
    }
}
