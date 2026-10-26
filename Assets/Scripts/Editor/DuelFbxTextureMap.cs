// Project D  |  Codename: Zantetsuken
// DuelFbxTextureMap.cs  (Editor-only)
//
// Some model rips write material -> texture links into the FBX in a way Unity's importer ignores (every material shows up
// with no texture, so the model is white). This reads a binary FBX file directly, finds which texture file each material
// refers to, and lets the import tool apply them. It only reads; it never changes the FBX.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public sealed class DuelFbxTextureMap
    {
        /// <summary>Material name -> (slot such as "DiffuseColor" / "NormalMap" -> texture file name).</summary>
        public readonly Dictionary<string, Dictionary<string, string>> ByMaterial = new Dictionary<string, Dictionary<string, string>>();
        public int Materials, Textures, Videos, Links;
        public string Note = "";
        public readonly List<string> SampleTextureFiles = new List<string>();

        private static readonly Dictionary<string, DuelFbxTextureMap> Cache = new Dictionary<string, DuelFbxTextureMap>();

        private sealed class Node
        {
            public string Name;
            public readonly List<object> Props = new List<object>();
            public readonly List<Node> Children = new List<Node>();
        }

        public static DuelFbxTextureMap Load(string assetPath)
        {
            string full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
            if (!File.Exists(full)) return null;
            string key = assetPath + "|" + File.GetLastWriteTimeUtc(full).Ticks;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var map = new DuelFbxTextureMap();
            try { map.Parse(File.ReadAllBytes(full)); }
            catch (Exception e) { map.Note = "could not parse: " + e.Message; }
            Cache[key] = map;
            return map;
        }

        // ------------------------------------------------------------------ parsing (binary FBX 7.x)

        private byte[] _d;
        private int _pos;
        private bool _wide;

        private void Parse(byte[] data)
        {
            _d = data;
            string head = Encoding.ASCII.GetString(data, 0, Math.Min(20, data.Length));
            if (!head.StartsWith("Kaydara FBX Binary")) { Note = "ASCII or unknown FBX format (not read)"; return; }

            uint version = BitConverter.ToUInt32(data, 23);
            _wide = version >= 7500;
            _pos = 27;

            var roots = new List<Node>();
            int header = _wide ? 25 : 13;
            while (_pos + header <= data.Length)
            {
                var n = ReadNode();
                if (n == null) break;
                roots.Add(n);
            }

            Node objects = roots.Find(r => r.Name == "Objects");
            Node connections = roots.Find(r => r.Name == "Connections");
            if (objects == null) { Note = "no Objects section"; return; }

            var mats = new Dictionary<long, string>();
            var texFile = new Dictionary<long, string>();
            var vidFile = new Dictionary<long, string>();
            foreach (var c in objects.Children)
            {
                if (c.Props.Count < 2 || c.Props[0] == null) continue;
                long id = Convert.ToInt64(c.Props[0]);
                string name = Clean(c.Props[1] as string);
                if (c.Name == "Material") mats[id] = name;
                else if (c.Name == "Texture") texFile[id] = FileOf(c);
                else if (c.Name == "Video") vidFile[id] = FileOf(c);
            }
            Materials = mats.Count; Textures = texFile.Count; Videos = vidFile.Count;

            var videoOfTexture = new Dictionary<long, long>();
            var links = new List<KeyValuePair<long, KeyValuePair<long, string>>>();   // texture id -> (material id, slot)
            if (connections != null)
            {
                foreach (var c in connections.Children)
                {
                    if (c.Name != "C" || c.Props.Count < 3 || c.Props[1] == null || c.Props[2] == null) continue;
                    string type = c.Props[0] as string;
                    long child = Convert.ToInt64(c.Props[1]);
                    long parent = Convert.ToInt64(c.Props[2]);
                    string slot = c.Props.Count > 3 ? (c.Props[3] as string ?? "") : "";
                    if (vidFile.ContainsKey(child) && texFile.ContainsKey(parent)) videoOfTexture[parent] = child;
                    if (texFile.ContainsKey(child) && mats.ContainsKey(parent) && (type == "OP" || type == "OO"))
                        links.Add(new KeyValuePair<long, KeyValuePair<long, string>>(child, new KeyValuePair<long, string>(parent, slot)));
                }
            }

            foreach (var l in links)
            {
                string file = texFile[l.Key];
                if (string.IsNullOrEmpty(file) && videoOfTexture.TryGetValue(l.Key, out long vid)) file = vidFile[vid];
                if (string.IsNullOrEmpty(file)) continue;
                string matName = mats[l.Value.Key];
                if (!ByMaterial.TryGetValue(matName, out var slots)) ByMaterial[matName] = slots = new Dictionary<string, string>();
                slots[string.IsNullOrEmpty(l.Value.Value) ? "DiffuseColor" : l.Value.Value] = file;
                Links++;
                if (SampleTextureFiles.Count < 8 && !SampleTextureFiles.Contains(file)) SampleTextureFiles.Add(file);
            }
        }

        private static string Clean(string s)
        {
            if (s == null) return "";
            int z = s.IndexOf('\0');
            return z >= 0 ? s.Substring(0, z) : s;
        }

        private static string FileOf(Node n)
        {
            string best = "";
            foreach (var c in n.Children)
            {
                if ((c.Name == "RelativeFilename" || c.Name == "FileName") && c.Props.Count > 0 && c.Props[0] is string s && s.Length > 0)
                {
                    string file = Path.GetFileName(s.Replace('\\', '/'));
                    if (file.Length > 0 && (best.Length == 0 || c.Name == "RelativeFilename")) best = file;
                }
            }
            return best;
        }

        private Node ReadNode()
        {
            long end = ReadSize();
            long nprops = ReadSize();
            long plen = ReadSize();
            int nameLen = _d[_pos++];
            if (end == 0) return null;
            var node = new Node { Name = Encoding.UTF8.GetString(_d, _pos, nameLen) };
            _pos += nameLen;
            long propsEnd = _pos + plen;
            for (long i = 0; i < nprops; i++) node.Props.Add(ReadProp());
            _pos = (int)propsEnd;
            while (_pos < end)
            {
                var child = ReadNode();
                if (child == null) break;
                node.Children.Add(child);
            }
            _pos = (int)end;
            return node;
        }

        private long ReadSize()
        {
            if (_wide) { long v = (long)BitConverter.ToUInt64(_d, _pos); _pos += 8; return v; }
            long w = BitConverter.ToUInt32(_d, _pos); _pos += 4; return w;
        }

        private object ReadProp()
        {
            char t = (char)_d[_pos++];
            switch (t)
            {
                case 'Y': { var v = BitConverter.ToInt16(_d, _pos); _pos += 2; return (long)v; }
                case 'C': return (long)_d[_pos++];
                case 'I': { var v = BitConverter.ToInt32(_d, _pos); _pos += 4; return (long)v; }
                case 'F': _pos += 4; return null;
                case 'D': _pos += 8; return null;
                case 'L': { var v = BitConverter.ToInt64(_d, _pos); _pos += 8; return v; }
                case 'f': case 'd': case 'l': case 'i': case 'b':
                    { uint clen = BitConverter.ToUInt32(_d, _pos + 8); _pos += 12 + (int)clen; return null; }
                case 'S':
                    { int len = (int)BitConverter.ToUInt32(_d, _pos); string s = Encoding.UTF8.GetString(_d, _pos + 4, len); _pos += 4 + len; return s; }
                case 'R':
                    { int len = (int)BitConverter.ToUInt32(_d, _pos); _pos += 4 + len; return null; }
                default: throw new InvalidDataException("unknown FBX property type '" + t + "'");
            }
        }
    }
}
#endif
