// Project D | Codename: Zantetsuken — NavGrid.cs (A* on a walkable grid)
using System; using System.Collections.Generic;
namespace ProjectD.P6.Pathfinding
{
    public struct Cell { public int x, y; public Cell(int x,int y){ this.x=x; this.y=y; } }
    public sealed class NavGrid
    {
        public readonly int W, H; readonly bool[,] _walk;
        public NavGrid(int w,int h){ W=w; H=h; _walk=new bool[w,h]; for(int x=0;x<w;x++) for(int y=0;y<h;y++) _walk[x,y]=true; }
        public void SetBlocked(int x,int y,bool b){ if(In(x,y)) _walk[x,y]=!b; }
        public bool Walkable(int x,int y)=>In(x,y)&&_walk[x,y];
        bool In(int x,int y)=>x>=0&&y>=0&&x<W&&y<H;
        public List<Cell> FindPath(Cell start,Cell goal)
        {
            if(!Walkable(start.x,start.y)||!Walkable(goal.x,goal.y)) return null;
            var open=new List<Cell>{start}; var came=new Dictionary<long,Cell>(); var g=new Dictionary<long,int>{[K(start)]=0}; var f=new Dictionary<long,int>{[K(start)]=Heur(start,goal)};
            while(open.Count>0)
            {
                int bi=0; for(int i=1;i<open.Count;i++) if(f.GetValueOrDefault(K(open[i]),int.MaxValue)<f.GetValueOrDefault(K(open[bi]),int.MaxValue)) bi=i;
                var cur=open[bi]; if(cur.x==goal.x&&cur.y==goal.y) return Reconstruct(came,cur); open.RemoveAt(bi);
                foreach(var n in Neighbors(cur)){ int tg=g[K(cur)]+1; if(tg<g.GetValueOrDefault(K(n),int.MaxValue)){ came[K(n)]=cur; g[K(n)]=tg; f[K(n)]=tg+Heur(n,goal); if(!open.Exists(o=>o.x==n.x&&o.y==n.y)) open.Add(n); } }
            }
            return null;
        }
        IEnumerable<Cell> Neighbors(Cell c){ var d=new[]{(1,0),(-1,0),(0,1),(0,-1)}; foreach(var (dx,dy) in d){ int nx=c.x+dx,ny=c.y+dy; if(Walkable(nx,ny)) yield return new Cell(nx,ny); } }
        static int Heur(Cell a,Cell b)=>Math.Abs(a.x-b.x)+Math.Abs(a.y-b.y);
        static long K(Cell c)=>((long)c.x<<32)|(uint)c.y;
        static List<Cell> Reconstruct(Dictionary<long,Cell> came,Cell cur){ var p=new List<Cell>{cur}; while(came.TryGetValue(K(cur),out var pr)){ cur=pr; p.Add(cur); } p.Reverse(); return p; }
    }
}
