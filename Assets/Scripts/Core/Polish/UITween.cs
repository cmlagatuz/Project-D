// Project D | Codename: Zantetsuken — UITween.cs (tween helper + easings)
using System; using System.Collections.Generic; using UnityEngine; using UnityEngine.UI;
namespace ProjectD.Polish.Anim
{
    public enum Ease { Linear, OutQuad, InQuad, OutBack, OutElastic, InOutSine }
    public static class Easing
    {
        public static float Apply(Ease e,float t){ t=Mathf.Clamp01(t); switch(e){ case Ease.OutQuad: return 1f-(1f-t)*(1f-t); case Ease.InQuad: return t*t; case Ease.OutBack:{ float c1=1.70158f,c3=c1+1f; return 1f+c3*Mathf.Pow(t-1f,3f)+c1*Mathf.Pow(t-1f,2f); } case Ease.OutElastic:{ if(t==0||t==1) return t; float c4=(2f*Mathf.PI)/3f; return Mathf.Pow(2f,-10f*t)*Mathf.Sin((t*10f-0.75f)*c4)+1f; } case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI*t)-1f)/2f; default: return t; } }
    }
    public sealed class TweenRunner : MonoBehaviour
    {
        sealed class Tween { public float t,dur; public Ease ease; public Action<float> step; public Action done; }
        readonly List<Tween> _tw=new(); static TweenRunner _i;
        public static TweenRunner Instance{ get { if(_i==null){ var go=new GameObject("~TweenRunner"){hideFlags=HideFlags.HideAndDontSave}; _i=go.AddComponent<TweenRunner>(); } return _i; } }
        public void Run(float dur,Ease ease,Action<float> step,Action done=null){ if(dur<=0f){ step?.Invoke(1f); done?.Invoke(); return; } _tw.Add(new Tween{t=0f,dur=dur,ease=ease,step=step,done=done}); }
        void Update(){ float dt=Time.unscaledDeltaTime; for(int i=_tw.Count-1;i>=0;i--){ var tw=_tw[i]; tw.t+=dt; tw.step?.Invoke(Easing.Apply(tw.ease,tw.t/tw.dur)); if(tw.t>=tw.dur){ tw.done?.Invoke(); _tw.RemoveAt(i); } } }
    }
    public static class Tweens
    {
        public static void Punch(Transform tr,float peak=1.15f,float dur=0.3f)=>TweenRunner.Instance.Run(dur,Ease.Linear,k=>{ if(tr){ float s=1f+(peak-1f)*Mathf.Sin(k*Mathf.PI); tr.localScale=Vector3.one*s; } },()=>{ if(tr) tr.localScale=Vector3.one; });
        public static void Float(float from,float to,float dur,Action<float> apply,Ease ease=Ease.OutQuad,Action done=null)=>TweenRunner.Instance.Run(dur,ease,k=>apply(Mathf.LerpUnclamped(from,to,k)),done);
        public static void Scale(Transform tr,Vector3 from,Vector3 to,float dur,Ease ease=Ease.OutQuad)=>TweenRunner.Instance.Run(dur,ease,k=>{ if(tr) tr.localScale=Vector3.LerpUnclamped(from,to,k); });
    }
}
