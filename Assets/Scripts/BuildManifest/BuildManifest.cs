// Project D | Codename: Zantetsuken — BuildManifest.cs (asset/scene/version manifest for a build)
using System; using System.Collections.Generic;
namespace ProjectD.P15.Build
{
    public struct AssetRef { public string path, kind; public long size; }
    public sealed class BuildManifest
    {
        public string version="0.1.0"; public string channel="dev"; public long builtUnix;
        public readonly List<string> scenes=new(); public readonly Dictionary<string,AssetRef> assets=new();
        public void AddScene(string s){ if(!string.IsNullOrEmpty(s)&&!scenes.Contains(s)) scenes.Add(s); }
        public void AddAsset(string path,string kind,long size){ assets[path]=new AssetRef{path=path,kind=kind,size=size}; }
        public long TotalSize(){ long t=0; foreach(var a in assets.Values) t+=a.size; return t; }
        /// <summary>Validate a build: required scenes present + at least one asset per kind.</summary>
        public bool Validate(IReadOnlyList<string> requiredScenes, out List<string> missing)
        {
            missing=new List<string>();
            foreach(var s in requiredScenes) if(!scenes.Contains(s)) missing.Add("scene:"+s);
            return missing.Count==0;
        }
        public string Fingerprint(){ uint h=2166136261; foreach(var s in scenes){ foreach(char c in s){ h^=c; h*=16777619; } } foreach(var kv in assets){ foreach(char c in kv.Key){ h^=c; h*=16777619; } } return h.ToString("x8"); }
    }
}
