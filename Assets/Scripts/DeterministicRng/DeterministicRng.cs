// Project D | Codename: Zantetsuken — DeterministicRng.cs (seeded, streamable RNG)
namespace ProjectD.P6.DeterministicRng
{
    public sealed class DeterministicRng
    {
        uint _x,_y,_z,_w;
        public DeterministicRng(uint seed){ Reseed(seed); }
        public void Reseed(uint seed){ _x=seed==0?0x9E3779B9:seed; _y=_x*1812433253+1; _z=_y*1812433253+1; _w=_z*1812433253+1; }
        public uint NextUInt(){ uint t=_x^(_x<<11); _x=_y; _y=_z; _z=_w; _w=_w^(_w>>19)^t^(t>>8); return _w; }
        public int Range(int min,int max){ if(max<=min) return min; return min+(int)(NextUInt()%(uint)(max-min)); }
        public float NextFloat()=>(NextUInt()&0xFFFFFF)/(float)0x1000000;
        public bool Chance(float p)=>NextFloat()<p;
        public DeterministicRng Substream(string name){ uint h=2166136261; foreach(char c in name){ h^=c; h*=16777619; } return new DeterministicRng(_w^h); }
    }
}
