// Project D | Codename: Zantetsuken — MapVariant.cs (swappable map configs + validation)
using System.Collections.Generic;
namespace ProjectD.P8.MapVariants
{
    public sealed class MapVariant
    {
        public string id, name; public int laneCount=3; public int riverPits=2;
        public readonly List<string> buffCamps=new(); public readonly List<string> objectives=new();
        public bool symmetric=true;
    }
    public static class MapValidator
    {
        public static bool Validate(MapVariant m, out List<string> errors)
        {
            errors=new List<string>();
            if(string.IsNullOrEmpty(m.id)) errors.Add("Map id is empty.");
            if(m.laneCount<1||m.laneCount>3) errors.Add($"Lane count {m.laneCount} must be 1-3.");
            if(m.riverPits<1||m.riverPits>2) errors.Add($"River pits {m.riverPits} must be 1-2.");
            if(m.objectives.Count==0) errors.Add("No objectives defined.");
            if(m.symmetric && m.buffCamps.Count%2!=0) errors.Add("Symmetric map needs an even number of buff camps.");
            return errors.Count==0;
        }
    }
}
