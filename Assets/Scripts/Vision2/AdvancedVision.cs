// Project D | Codename: Zantetsuken — AdvancedVision.cs (brush/reveal/true-sight grid)
using System.Collections.Generic;
namespace ProjectD.P8.Vision
{
    public enum CellType { Open, Brush, Wall }
    public sealed class VisionGrid
    {
        public readonly int W,H; readonly CellType[,] _cells;
        public VisionGrid(int w,int h){ W=w; H=h; _cells=new CellType[w,h]; }
        public void Set(int x,int y,CellType t){ if(In(x,y)) _cells[x,y]=t; }
        public CellType At(int x,int y)=>In(x,y)?_cells[x,y]:CellType.Wall;
        bool In(int x,int y)=>x>=0&&y>=0&&x<W&&y<H;
        /// <summary>Can a viewer see a target? Walls block; brush hides a target unless the viewer is also in/next to the brush.</summary>
        public bool CanSee(int vx,int vy,int tx,int ty,int range)
        {
            int dx=vx-tx, dy=vy-ty; if(dx*dx+dy*dy>range*range) return false;
            if(At(tx,ty)==CellType.Wall) return false;
            if(At(tx,ty)==CellType.Brush)
            {
                // Only visible if the viewer is inside the same brush or adjacent to the target.
                bool viewerInBrush=At(vx,vy)==CellType.Brush;
                bool adjacent=System.Math.Abs(dx)<=1&&System.Math.Abs(dy)<=1;
                if(!viewerInBrush&&!adjacent) return false;
            }
            return !LineBlocked(vx,vy,tx,ty);
        }
        /// <summary>True-sight (from a control ward/ability) ignores brush concealment.</summary>
        public bool TrueSight(int vx,int vy,int tx,int ty,int range){ int dx=vx-tx,dy=vy-ty; if(dx*dx+dy*dy>range*range) return false; if(At(tx,ty)==CellType.Wall) return false; return !LineBlocked(vx,vy,tx,ty); }
        bool LineBlocked(int x0,int y0,int x1,int y1)
        {
            int dx=System.Math.Abs(x1-x0), dy=System.Math.Abs(y1-y0); int sx=x0<x1?1:-1, sy=y0<y1?1:-1; int err=dx-dy;
            while(true){ if(!(x0==x1&&y0==y1)&&At(x0,y0)==CellType.Wall) return true; if(x0==x1&&y0==y1) break; int e2=2*err; if(e2>-dy){ err-=dy; x0+=sx; } if(e2<dx){ err+=dx; y0+=sy; } }
            return false;
        }
    }
}
