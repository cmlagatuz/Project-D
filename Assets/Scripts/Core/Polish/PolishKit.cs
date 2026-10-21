// Project D | Codename: Zantetsuken — PolishKit.cs (procedural art + tween helpers + styling)
using System.Collections.Generic; using UnityEngine; using UnityEngine.UI;
namespace ProjectD.Polish
{
    public static class Sprites
    {
        static readonly Dictionary<string,Sprite> _c=new();
        public static Sprite Rounded(int size=48,int r=12){ string k=$"rr{size}_{r}"; if(_c.TryGetValue(k,out var s)) return s; var t=Tex(size); var px=new Color32[size*size]; for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=RD(x,y,size,r); px[y*size+x]=new Color32(255,255,255,(byte)(Mathf.Clamp01(1f-d)*255)); } t.SetPixels32(px); t.Apply(); var sp=Sprite.Create(t,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f,0,SpriteMeshType.FullRect,new Vector4(r,r,r,r)); _c[k]=sp; return sp; }
        public static Sprite Circle(int size=32){ string k=$"ci{size}"; if(_c.TryGetValue(k,out var s)) return s; var t=Tex(size); var px=new Color32[size*size]; float rad=size/2f-0.5f; Vector2 c=new(rad,rad); for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=Vector2.Distance(new Vector2(x,y),c); px[y*size+x]=new Color32(255,255,255,(byte)(Mathf.Clamp01(rad-d)*255)); } t.SetPixels32(px); t.Apply(); var sp=Sprite.Create(t,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f); _c[k]=sp; return sp; }
        public static Sprite Glow(int size=128,float fo=2.2f){ string k=$"gl{size}"; if(_c.TryGetValue(k,out var s)) return s; var t=Tex(size); var px=new Color32[size*size]; float rad=size/2f; Vector2 c=new(rad,rad); for(int y=0;y<size;y++) for(int x=0;x<size;x++){ float d=Vector2.Distance(new Vector2(x,y),c)/rad; px[y*size+x]=new Color32(255,255,255,(byte)(Mathf.Clamp01(1f-Mathf.Pow(d,fo))*255)); } t.SetPixels32(px); t.Apply(); var sp=Sprite.Create(t,new Rect(0,0,size,size),new Vector2(0.5f,0.5f),100f); _c[k]=sp; return sp; }
        static Texture2D Tex(int s)=>new Texture2D(s,s,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        static float RD(int x,int y,int size,int r){ float h=size/2f; float px=Mathf.Abs(x-h+0.5f),py=Mathf.Abs(y-h+0.5f); float ex=h-r,ey=h-r; float dx=Mathf.Max(px-ex,0f),dy=Mathf.Max(py-ey,0f); return Mathf.Max(Mathf.Sqrt(dx*dx+dy*dy)-r,Mathf.Max(px-h,py-h))+0.5f; }
    }
    public static class Style
    {
        public static readonly Color Ink=new(0.93f,0.95f,0.99f), Sub=new(0.68f,0.73f,0.83f), Gold=new(0.95f,0.80f,0.32f), Accent=new(0.36f,0.62f,0.92f);
        public static void Panel(Image i){ if(!i) return; i.sprite=Sprites.Rounded(64,16); i.type=Image.Type.Sliced; }
        public static void Button(Image i,Color c){ if(!i) return; i.sprite=Sprites.Rounded(48,12); i.type=Image.Type.Sliced; i.color=c; }
        public static void Badge(Image i,Color c){ if(!i) return; i.sprite=Sprites.Circle(32); i.color=c; }
    }
    public enum Ease { Linear, OutQuad, OutBack, OutElastic }
    public sealed class TweenRunner : MonoBehaviour
    {
        sealed class T { public float t,dur; public Ease e; public System.Action<float> step; public System.Action done; }
        readonly List<T> _t=new(); static TweenRunner _i;
        public static TweenRunner I{ get { if(_i==null){ var g=new GameObject("~Tween"){hideFlags=HideFlags.HideAndDontSave}; _i=g.AddComponent<TweenRunner>(); } return _i; } }
        public void Run(float d,Ease e,System.Action<float> step,System.Action done=null){ if(d<=0){ step?.Invoke(1f); done?.Invoke(); return; } _t.Add(new T{dur=d,e=e,step=step,done=done}); }
        void Update(){ float dt=Time.unscaledDeltaTime; for(int i=_t.Count-1;i>=0;i--){ var w=_t[i]; w.t+=dt; w.step?.Invoke(Ap(w.e,Mathf.Clamp01(w.t/w.dur))); if(w.t>=w.dur){ w.done?.Invoke(); _t.RemoveAt(i); } } }
        static float Ap(Ease e,float t)=>e switch{ Ease.OutQuad=>1f-(1f-t)*(1f-t), Ease.OutBack=>1f+2.7f*Mathf.Pow(t-1,3)+1.7f*Mathf.Pow(t-1,2), Ease.OutElastic=>t==0||t==1?t:Mathf.Pow(2,-10*t)*Mathf.Sin((t*10-0.75f)*(2*Mathf.PI/3))+1, _=>t };
    }
    public static class Tw
    {
        public static void Fill(Image img,float to,float dur=0.6f){ if(!img) return; float from=img.fillAmount; TweenRunner.I.Run(dur,Ease.OutQuad,k=>{ if(img) img.fillAmount=Mathf.Lerp(from,to,k); }); }
        public static void Count(Text label,int from,int to,float dur=0.7f,string suffix=""){ if(!label) return; TweenRunner.I.Run(dur,Ease.OutQuad,k=>{ if(label) label.text=Mathf.RoundToInt(Mathf.Lerp(from,to,k))+suffix; }); }
        public static void Pop(Transform tr,float dur=0.3f,float peak=1.12f){ if(!tr) return; TweenRunner.I.Run(dur,Ease.OutBack,k=>{ if(tr) tr.localScale=Vector3.one*Mathf.Lerp(0.6f,peak,k); },()=>{ if(tr) tr.localScale=Vector3.one; }); }
        public static void FadeIn(CanvasGroup g,float dur=0.3f){ if(!g) return; TweenRunner.I.Run(dur,Ease.OutQuad,k=>{ if(g) g.alpha=k; }); }
        public static void SlideIn(RectTransform rt,Vector2 from,Vector2 to,float dur=0.35f){ if(!rt) return; TweenRunner.I.Run(dur,Ease.OutBack,k=>{ if(rt) rt.anchoredPosition=Vector2.LerpUnclamped(from,to,k); }); }
    }
    // Attach: fades + slides a panel in on enable, and staggers child rows.
    public sealed class PanelIntro : MonoBehaviour
    {
        public float delay=0f; CanvasGroup _g;
        void OnEnable(){ _g=GetComponent<CanvasGroup>(); if(_g==null) _g=gameObject.AddComponent<CanvasGroup>(); _g.alpha=0f; TweenRunner.I.Run(delay,Ease.Linear,_=>{},()=>Tw.FadeIn(_g,0.35f)); }
    }
}
