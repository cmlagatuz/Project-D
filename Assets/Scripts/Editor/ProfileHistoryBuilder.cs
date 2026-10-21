// Project D | Codename: Zantetsuken — ProfileHistoryBuilder.cs (Editor) Build Profile Match History
#if UNITY_EDITOR
using UnityEditor; using UnityEditor.SceneManagement; using UnityEngine; using UnityEngine.UI;
using ProjectD.HistoryUI;
namespace ProjectD.EditorTools
{
    public static class ProfileHistoryBuilder
    {
        static readonly Color Bg=new(0.05f,0.06f,0.10f), Panel=new(0.11f,0.13f,0.18f,0.98f), Ink=new(0.93f,0.95f,0.99f), Sub=new(0.68f,0.73f,0.83f), Gold=new(0.95f,0.80f,0.32f);
        static Font _f;
        [MenuItem("Tools/Project D/Build Profile Match History", priority=113)]
        public static void Build()
        {
            _f=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")??Resources.GetBuiltinResource<Font>("Arial.ttf");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camGo=new GameObject("Main Camera"); camGo.tag="MainCamera"; var cam=camGo.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=Bg; camGo.AddComponent<AudioListener>();
            if(Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()==null){ var es=new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>(); }
            var canvasGo=new GameObject("Canvas"); var canvas=canvasGo.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var sc=canvasGo.AddComponent<CanvasScaler>(); sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution=new Vector2(1920,1080); sc.matchWidthOrHeight=0.5f; canvasGo.AddComponent<GraphicRaycaster>();
            var root=(RectTransform)canvasGo.transform;
            L(root,"Title","PROFILE — MATCH HISTORY",new Vector2(0.5f,1),new Vector2(0,-40),new Vector2(800,44),30,Ink,TextAnchor.MiddleCenter,true);
            L(root,"WM","PROJECT D - CONFIDENTIAL",new Vector2(1,1),new Vector2(-20,-16),new Vector2(420,24),14,new Color(1,1,1,0.3f),TextAnchor.MiddleRight,false);
            var head=P(root,"Head",new Vector2(0.5f,1),new Vector2(0,-110),new Vector2(1200,64));
            var rec=L(head,"Rec","0W - 0L",new Vector2(0,0.5f),new Vector2(24,0),new Vector2(300,30),22,Ink,TextAnchor.MiddleLeft,true);
            var wr=L(head,"WR","0% WR",new Vector2(0.5f,0.5f),new Vector2(-100,0),new Vector2(200,28),18,Gold,TextAnchor.MiddleCenter,true);
            var streak=L(head,"Streak","",new Vector2(0.5f,0.5f),new Vector2(120,0),new Vector2(260,26),16,Sub,TextAnchor.MiddleLeft,true);
            var mp=L(head,"MP","",new Vector2(1,0.5f),new Vector2(-24,0),new Vector2(360,26),16,Sub,TextAnchor.MiddleRight,false);
            var scroll=P(root,"List",new Vector2(0.5f,0.5f),new Vector2(0,-40),new Vector2(1200,640));
            var vp=P(scroll,"Viewport",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(1180,620)); vp.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var content=new GameObject("Content",typeof(RectTransform)); content.transform.SetParent(vp,false); var crt=(RectTransform)content.transform; crt.anchorMin=new Vector2(0,1); crt.anchorMax=new Vector2(1,1); crt.pivot=new Vector2(0.5f,1); crt.sizeDelta=new Vector2(0,620);
            var vlg=content.AddComponent<VerticalLayoutGroup>(); vlg.spacing=6; vlg.padding=new RectOffset(14,14,10,10); vlg.childForceExpandHeight=false; vlg.childControlHeight=false;
            content.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var srect=scroll.gameObject.AddComponent<ScrollRect>(); srect.viewport=vp; srect.content=crt; srect.horizontal=false;
            var empty=L(scroll,"Empty","No matches yet.",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(400,30),20,Sub,TextAnchor.MiddleCenter,false); empty.gameObject.SetActive(false);
            var rowPrefab=BuildRow();
            var view=canvasGo.AddComponent<ProfileHistoryView>();
            var so=new SerializedObject(view);
            so.FindProperty("recordText").objectReferenceValue=rec; so.FindProperty("winRateText").objectReferenceValue=wr; so.FindProperty("streakText").objectReferenceValue=streak; so.FindProperty("mostPlayedText").objectReferenceValue=mp;
            so.FindProperty("rowParent").objectReferenceValue=crt; so.FindProperty("rowPrefab").objectReferenceValue=rowPrefab; so.FindProperty("emptyText").objectReferenceValue=empty;
            so.ApplyModifiedPropertiesWithoutUndo();
            var driver=canvasGo.AddComponent<ProfileHistoryDemoDriver>(); var dso=new SerializedObject(driver); dso.FindProperty("view").objectReferenceValue=view; dso.ApplyModifiedPropertiesWithoutUndo();
            if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/ProfileMatchHistory.unity"); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Project D","Profile Match History built.\nPress Play: record header (W-L, win-rate, streak, most-played) + recent-match rows (win/loss accent, KDA, CS, mode, MVP crown). Click a row to open that match's post-game analytics.","OK");
        }
        static Button BuildRow(){ var go=new GameObject("HistRow",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement)); go.GetComponent<Image>().color=new Color(0.14f,0.19f,0.30f,0.95f); go.GetComponent<LayoutElement>().preferredHeight=54;
            var crown=new GameObject("Crown",typeof(RectTransform),typeof(Image)); crown.transform.SetParent(go.transform,false); var cr=(RectTransform)crown.transform; cr.anchorMin=new Vector2(1,1); cr.anchorMax=new Vector2(1,1); cr.pivot=new Vector2(1,1); cr.anchoredPosition=new Vector2(-8,-6); cr.sizeDelta=new Vector2(18,18); crown.GetComponent<Image>().color=new Color(0.95f,0.8f,0.35f); crown.GetComponent<Image>().enabled=false;
            L((RectTransform)go.transform,"Outcome","WIN",new Vector2(0,0.5f),new Vector2(16,0),new Vector2(90,26),18,new Color(0.4f,0.65f,1f),TextAnchor.MiddleLeft,true);
            L((RectTransform)go.transform,"Champ","champ",new Vector2(0,0.5f),new Vector2(120,0),new Vector2(220,24),16,Ink,TextAnchor.MiddleLeft,true);
            L((RectTransform)go.transform,"Kda","0/0/0",new Vector2(0,0.5f),new Vector2(360,0),new Vector2(160,24),15,Sub,TextAnchor.MiddleLeft,false);
            L((RectTransform)go.transform,"Cs","0 cs",new Vector2(0,0.5f),new Vector2(540,0),new Vector2(120,24),14,Sub,TextAnchor.MiddleLeft,false);
            L((RectTransform)go.transform,"Mode","mode",new Vector2(1,0.5f),new Vector2(-8,0),new Vector2(360,24),14,Sub,TextAnchor.MiddleRight,false);
            go.SetActive(false); return go.GetComponent<Button>(); }
        static RectTransform P(RectTransform parent,string name,Vector2 a,Vector2 pos,Vector2 size){ var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=a; rt.anchorMax=a; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=pos; rt.sizeDelta=size; go.GetComponent<Image>().color=Panel; return rt; }
        static Text L(RectTransform parent,string name,string text,Vector2 a,Vector2 pos,Vector2 size,int fs,Color c,TextAnchor al,bool b){ var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false); var rt=(RectTransform)go.transform; rt.anchorMin=a; rt.anchorMax=a; rt.pivot=new Vector2(a.x,a.y); rt.anchoredPosition=pos; rt.sizeDelta=size; var t=go.GetComponent<Text>(); t.text=text; t.color=c; t.fontSize=fs; t.alignment=al; t.fontStyle=b?FontStyle.Bold:FontStyle.Normal; t.font=_f; return t; }
    }
}
#endif
