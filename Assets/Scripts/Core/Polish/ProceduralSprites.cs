// Project D | Codename: Zantetsuken — ProceduralSprites.cs (runtime UI art, no assets)
using System.Collections.Generic; using UnityEngine;
namespace ProjectD.Polish.Art
{
    public static class ProceduralSprites
    {
        static readonly Dictionary<string,Sprite> _cache=new();
        public static Sprite RoundedRect(int size=48,int radius=12)
        {
            string k=$"rr_{size}_{radius}"; if(_cache.TryGetValue(k,out var s)) return s;
            var tex=NewTex(size); var px=new Color32[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=RoundedDist(x,y,size,radius); float a=Mathf.Clamp01(1f-d); px[y*size+x]=new Color32(255,255,255,(byte)(a*255)); }
            tex.SetPixels32(px); tex.Apply();
            var sp=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f,0,SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius)); _cache[k]=sp; return sp;
        }
        public static Sprite Circle(int size=32){ string k=$"circle_{size}"; if(_cache.TryGetValue(k,out var s)) return s; var tex=NewTex(size); var px=new Color32[size*size]; float r=size/2f-0.5f; Vector2 c=new(r,r); for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=Vector2.Distance(new Vector2(x,y),c); float a=Mathf.Clamp01(r-d); px[y*size+x]=new Color32(255,255,255,(byte)(a*255)); } tex.SetPixels32(px); tex.Apply(); var sp=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f); _cache[k]=sp; return sp; }
        public static Sprite Ring(int size=48,int thickness=5){ string k=$"ring_{size}_{thickness}"; if(_cache.TryGetValue(k,out var s)) return s; var tex=NewTex(size); var px=new Color32[size*size]; float outer=size/2f-0.5f, inner=outer-thickness; Vector2 c=new(outer,outer); for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=Vector2.Distance(new Vector2(x,y),c); float a=Mathf.Clamp01(outer-d)*Mathf.Clamp01(d-inner+1f); px[y*size+x]=new Color32(255,255,255,(byte)(Mathf.Clamp01(a)*255)); } tex.SetPixels32(px); tex.Apply(); var sp=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f); _cache[k]=sp; return sp; }
        public static Sprite Glow(int size=128,float falloff=2.2f){ string k=$"glow_{size}_{falloff:0.0}"; if(_cache.TryGetValue(k,out var s)) return s; var tex=NewTex(size); var px=new Color32[size*size]; float r=size/2f; Vector2 c=new(r,r); for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=Vector2.Distance(new Vector2(x,y),c)/r; float a=Mathf.Clamp01(1f-Mathf.Pow(d,falloff)); px[y*size+x]=new Color32(255,255,255,(byte)(a*255)); } tex.SetPixels32(px); tex.Apply(); var sp=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f); _cache[k]=sp; return sp; }
        public static Sprite VerticalGradient(Color top,Color bottom,int height=64){ string k=$"vg_{top}_{bottom}_{height}"; if(_cache.TryGetValue(k,out var s)) return s; var tex=new Texture2D(4,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp}; var px=new Color32[4*height]; for(int y=0;y<height;y++){ Color col=Color.Lerp(bottom,top,y/(float)(height-1)); for(int x=0;x<4;x++) px[y*4+x]=col; } tex.SetPixels32(px); tex.Apply(); var sp=Sprite.Create(tex,new Rect(0,0,4,height),new Vector2(0.5f,0.5f),100f,0,SpriteMeshType.FullRect,new Vector4(0,height/2f,0,height/2f)); _cache[k]=sp; return sp; }
        static Texture2D NewTex(int size)=>new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        static float RoundedDist(int x,int y,int size,int radius){ float half=size/2f; float px=Mathf.Abs(x-half+0.5f), py=Mathf.Abs(y-half+0.5f); float ex=half-radius, ey=half-radius; float dx=Mathf.Max(px-ex,0f), dy=Mathf.Max(py-ey,0f); float oc=Mathf.Sqrt(dx*dx+dy*dy)-radius; float oe=Mathf.Max(px-half,py-half); return Mathf.Max(oc,oe)+0.5f; }
    }
    public static class UIStyle
    {
        public static readonly Color Accent=new(0.36f,0.62f,0.92f), Gold=new(0.95f,0.80f,0.32f), Ink=new(0.93f,0.95f,0.99f);
        public static void Panel(UnityEngine.UI.Image img){ if(!img) return; img.sprite=ProceduralSprites.RoundedRect(64,16); img.type=UnityEngine.UI.Image.Type.Sliced; img.color=new Color(0.12f,0.14f,0.19f,0.94f); }
        public static void Button(UnityEngine.UI.Image img,Color tint){ if(!img) return; img.sprite=ProceduralSprites.RoundedRect(48,12); img.type=UnityEngine.UI.Image.Type.Sliced; img.color=tint; }
        public static void Badge(UnityEngine.UI.Image img,Color c){ if(!img) return; img.sprite=ProceduralSprites.Circle(32); img.color=c; }
        public static void BarFill(UnityEngine.UI.Image img,Color c){ if(!img) return; img.sprite=ProceduralSprites.RoundedRect(24,10); img.type=UnityEngine.UI.Image.Type.Filled; img.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal; img.fillOrigin=(int)UnityEngine.UI.Image.OriginHorizontal.Left; img.color=c; }
    }
}
