// Project D | Codename: Zantetsuken — NarrativeGraph.cs (branching story by world/player state)
using System; using System.Collections.Generic;
namespace ProjectD.P13.Narrative
{
    public sealed class StoryNode { public string id, title, body; public readonly List<StoryChoice> choices=new(); public bool visited; }
    public sealed class StoryChoice { public string label, targetNodeId; public string requiresFlag; public Func<IReadOnlyDictionary<string,int>,bool> condition; }
    public sealed class NarrativeGraph
    {
        readonly Dictionary<string,StoryNode> _nodes=new(); public string currentNodeId; readonly Dictionary<string,int> _state=new(); readonly HashSet<string> _flags=new();
        public void AddNode(StoryNode n){ if(n!=null&&!string.IsNullOrEmpty(n.id)) _nodes[n.id]=n; }
        public StoryNode Current=>currentNodeId!=null&&_nodes.TryGetValue(currentNodeId,out var n)?n:null;
        public void SetState(string key,int value)=>_state[key]=value;
        public void SetFlag(string flag)=>_flags.Add(flag);
        public void Start(string nodeId){ currentNodeId=nodeId; if(Current!=null) Current.visited=true; }
        public List<StoryChoice> AvailableChoices()
        {
            var outl=new List<StoryChoice>(); var n=Current; if(n==null) return outl;
            foreach(var c in n.choices){ if(!string.IsNullOrEmpty(c.requiresFlag)&&!_flags.Contains(c.requiresFlag)) continue; if(c.condition!=null&&!c.condition(_state)) continue; outl.Add(c); }
            return outl;
        }
        public bool Choose(string targetNodeId)
        {
            foreach(var c in AvailableChoices()) if(c.targetNodeId==targetNodeId){ currentNodeId=targetNodeId; if(Current!=null) Current.visited=true; return true; }
            return false;
        }
        public int VisitedCount(){ int n=0; foreach(var x in _nodes.Values) if(x.visited) n++; return n; }
    }
}
