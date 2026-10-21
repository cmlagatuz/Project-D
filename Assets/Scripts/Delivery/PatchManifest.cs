// Project D | Codename: Zantetsuken — PatchManifest.cs (download only changed files + resume)
using System; using System.Collections.Generic;
namespace ProjectD.P7.Delivery
{
    public struct FileEntry { public string path, hash; public long size; }
    public sealed class Manifest
    {
        public readonly Dictionary<string,FileEntry> Files=new();
        public void Add(string path,string hash,long size){ Files[path]=new FileEntry{path=path,hash=hash,size=size}; }
    }
    public struct PatchPlan { public List<FileEntry> ToDownload; public List<string> ToDelete; public long TotalBytes; }
    public static class PatchPlanner
    {
        /// <summary>Compute what to download/delete to move from 'local' to 'remote' (changed hashes only).</summary>
        public static PatchPlan Plan(Manifest local, Manifest remote)
        {
            var plan=new PatchPlan{ToDownload=new List<FileEntry>(), ToDelete=new List<string>()};
            foreach(var kv in remote.Files){ if(!local.Files.TryGetValue(kv.Key,out var lf)||lf.hash!=kv.Value.hash){ plan.ToDownload.Add(kv.Value); plan.TotalBytes+=kv.Value.size; } }
            foreach(var kv in local.Files) if(!remote.Files.ContainsKey(kv.Key)) plan.ToDelete.Add(kv.Key);
            return plan;
        }
        /// <summary>Resume: skip files already fully downloaded (present in 'completed' with matching hash).</summary>
        public static List<FileEntry> Remaining(PatchPlan plan, IReadOnlyDictionary<string,string> completedHashes)
        {
            var rem=new List<FileEntry>();
            foreach(var f in plan.ToDownload){ if(completedHashes.TryGetValue(f.path,out var h)&&h==f.hash) continue; rem.Add(f); }
            return rem;
        }
    }
}
