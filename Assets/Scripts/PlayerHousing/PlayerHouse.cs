// Project D | Codename: Zantetsuken — PlayerHouse.cs (customizable personal space + rating)
using System; using System.Collections.Generic;
namespace ProjectD.P13.Housing
{
    public sealed class DecorItem { public string id, name; public int styleValue; public string theme; }
    public sealed class Room { public string id, name; public int capacity=10; public readonly List<string> placed=new(); }
    public sealed class PlayerHouse
    {
        readonly Dictionary<string,Room> _rooms=new(); readonly Dictionary<string,DecorItem> _catalog=new(); readonly HashSet<string> _owned=new();
        public void AddRoom(Room r){ if(r!=null&&!string.IsNullOrEmpty(r.id)) _rooms[r.id]=r; }
        public void RegisterDecor(DecorItem d){ if(d!=null&&!string.IsNullOrEmpty(d.id)) _catalog[d.id]=d; }
        public void GrantDecor(string id){ if(_catalog.ContainsKey(id)) _owned.Add(id); }
        public bool Place(string roomId,string decorId){ if(!_rooms.TryGetValue(roomId,out var room)||!_owned.Contains(decorId)) return false; if(room.placed.Count>=room.capacity) return false; if(room.placed.Contains(decorId)) return false; room.placed.Add(decorId); return true; }
        public bool Remove(string roomId,string decorId){ return _rooms.TryGetValue(roomId,out var room)&&room.placed.Remove(decorId); }
        /// <summary>House rating = sum of placed decor style, with a theme-cohesion bonus.</summary>
        public int Rating()
        {
            int total=0; var themeCount=new Dictionary<string,int>();
            foreach(var room in _rooms.Values) foreach(var id in room.placed){ if(_catalog.TryGetValue(id,out var d)){ total+=d.styleValue; if(!string.IsNullOrEmpty(d.theme)){ themeCount.TryGetValue(d.theme,out int c); themeCount[d.theme]=c+1; } } }
            foreach(var kv in themeCount) if(kv.Value>=3) total+=25;   // cohesion bonus per themed set of 3+
            return total;
        }
        public int PlacedCount(){ int n=0; foreach(var r in _rooms.Values) n+=r.placed.Count; return n; }
    }
}
