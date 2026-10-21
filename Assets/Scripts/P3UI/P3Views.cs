// Project D  |  Codename: Zantetsuken
// P3Views.cs  (P3 UI — views bound to the P3 cores)
// Honor commend, Battle Pass, Guild hub, Party lobby, Mastery, News. Server-authoritative
// in a real build; these render whatever the server-owned models contain.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Honor; using ProjectD.BattlePass; using ProjectD.Guild;
using ProjectD.Party; using ProjectD.Mastery; using ProjectD.News;

namespace ProjectD.P3UI
{
    using Guild = ProjectD.Guild.Guild;
    using Party = ProjectD.Party.Party;
    public sealed class HonorCommendView : MonoBehaviour
    {
        [SerializeField] private RectTransform rowParent; [SerializeField] private Button rowPrefab; [SerializeField] private Text statusText; [SerializeField] private string localPlayerId="me";
        private readonly HonorSystem.MatchTally _tally=new(); private readonly Dictionary<string,int> _per=new(); private readonly Dictionary<string,Text> _counts=new();
        public void Bind(IEnumerable<(string id,string name)> teammates)
        {
            foreach(var (id,name) in teammates){ if(id==localPlayerId) continue; var b=Instantiate(rowPrefab,rowParent); b.gameObject.SetActive(true); var t=b.GetComponentsInChildren<Text>(); if(t.Length>0) t[0].text=name; if(t.Length>1){ t[1].text=""; _counts[id]=t[1]; } string cid=id; b.onClick.AddListener(()=>Commend(cid)); }
            Status("Commend a teammate (up to 2).");
        }
        void Commend(string id){ if(!HonorSystem.Commend(_tally,localPlayerId,id,_per)){ Status("Commend limit reached."); return; } _tally.Commends.TryGetValue(id,out int c); if(_counts.TryGetValue(id,out var t)) t.text=$"\u2665 {c}"; _per.TryGetValue(localPlayerId,out int used); Status(used>=HonorSystem.CommendationsPerMatchCap?"Thanks! Commends used.":$"Commended! ({used}/2)"); }
        void Status(string s){ if(statusText) statusText.text=s; }
    }

    public sealed class BattlePassView : MonoBehaviour
    {
        [SerializeField] private RectTransform tierParent; [SerializeField] private RectTransform tierCellPrefab; [SerializeField] private Image xpFill; [SerializeField] private Text tierText, xpText, premiumText; [SerializeField] private Button unlockPremiumButton, addXpButton;
        private BattlePassDefinition _def; private readonly BattlePassProgress _prog=new(); private readonly List<(int tier,Button free,Button premium)> _cells=new();
        public void Bind(BattlePassDefinition def){ _def=def; if(unlockPremiumButton) unlockPremiumButton.onClick.AddListener(()=>{ _prog.UnlockPremium(); Refresh(); }); if(addXpButton) addXpButton.onClick.AddListener(()=>{ _prog.AddXp(_def,1200); Refresh(); }); BuildTiers(); Refresh(); }
        void BuildTiers(){ if(tierParent==null||tierCellPrefab==null) return; int show=Mathf.Min(_def.maxTier,12); for(int t=1;t<=show;t++){ var cell=Instantiate(tierCellPrefab,tierParent); cell.gameObject.SetActive(true); var texts=cell.GetComponentsInChildren<Text>(); if(texts.Length>0) texts[0].text=t.ToString(); var b=cell.GetComponentsInChildren<Button>(); Button free=b.Length>0?b[0]:null, prem=b.Length>1?b[1]:null; int tier=t; if(free) free.onClick.AddListener(()=>ClaimTier(tier,RewardTrack.Free)); if(prem) prem.onClick.AddListener(()=>ClaimTier(tier,RewardTrack.Premium)); _cells.Add((t,free,prem)); } }
        void ClaimTier(int tier,RewardTrack track){ if(_prog.Claim(_def,tier,track)!=null) Refresh(); }
        void Refresh(){ if(tierText) tierText.text=$"Tier {_prog.tier}"; if(xpFill) xpFill.fillAmount=_prog.TierProgress(_def); if(xpText) xpText.text=$"{_prog.xpIntoTier} / {_def.xpPerTier} XP"; if(premiumText) premiumText.text=_prog.premiumUnlocked?"PREMIUM UNLOCKED":"FREE TRACK"; foreach(var (tier,free,premium) in _cells){ bool reached=tier<=_prog.tier; if(free){ free.interactable=reached&&!_prog.IsClaimed(tier,RewardTrack.Free); Tint(free,reached); } if(premium){ premium.interactable=reached&&_prog.premiumUnlocked&&!_prog.IsClaimed(tier,RewardTrack.Premium); Tint(premium,reached&&_prog.premiumUnlocked); } } }
        static void Tint(Button b,bool on){ var i=b.GetComponent<Image>(); if(i) i.color=on?new Color(0.30f,0.55f,0.40f):new Color(0.18f,0.20f,0.26f); }
    }

    public sealed class GuildHubView : MonoBehaviour
    {
        [SerializeField] private Text nameText, levelText, capacityText, xpText; [SerializeField] private RectTransform memberParent, requestParent; [SerializeField] private RectTransform memberRowPrefab, requestRowPrefab;
        private Guild _g; private string _viewer;
        public void Bind(Guild g,string viewerId){ _g=g; _viewer=viewerId; Refresh(); }
        void Refresh(){ if(nameText) nameText.text=$"[{_g.tag}] {_g.name}"; if(levelText) levelText.text=$"Lv {_g.level}"; if(capacityText) capacityText.text=$"{_g.MemberCount} / {_g.capacity}"; if(xpText) xpText.text=$"{_g.guildXp} / {Guild.XpForNext(_g.level)} XP"; Members(); Requests(); }
        void Members(){ if(memberParent==null) return; foreach(Transform c in memberParent) Destroy(c.gameObject); foreach(var m in _g.Members){ var r=Instantiate(memberRowPrefab,memberParent); r.gameObject.SetActive(true); var t=r.GetComponentsInChildren<Text>(); if(t.Length>0) t[0].text=m.displayName; if(t.Length>1) t[1].text=m.role.ToString(); if(t.Length>2) t[2].text=$"{m.contributedXp} XP"; } }
        void Requests(){ if(requestParent==null) return; foreach(Transform c in requestParent) Destroy(c.gameObject); foreach(var reqId in _g.JoinRequests){ var r=Instantiate(requestRowPrefab,requestParent); r.gameObject.SetActive(true); var t=r.GetComponentInChildren<Text>(); if(t) t.text=reqId; var b=r.GetComponentInChildren<Button>(); string id=reqId; if(b) b.onClick.AddListener(()=>{ if(_g.ApproveRequest(_viewer,id,id)) Refresh(); }); } }
    }

    public sealed class PartyLobbyView : MonoBehaviour
    {
        [SerializeField] private RectTransform memberParent; [SerializeField] private RectTransform memberRowPrefab; [SerializeField] private Text mmrText, statusText; [SerializeField] private Button startButton;
        private Party _p; public event System.Action OnStart;
        public void Bind(Party p,string viewerId){ _p=p; if(startButton) startButton.onClick.AddListener(()=>{ if(_p.AllReady()) OnStart?.Invoke(); }); Refresh(); }
        void Refresh(){ if(memberParent!=null){ foreach(Transform c in memberParent) Destroy(c.gameObject); foreach(var m in _p.Members){ var r=Instantiate(memberRowPrefab,memberParent); r.gameObject.SetActive(true); var t=r.GetComponentsInChildren<Text>(); if(t.Length>0) t[0].text=m.displayName+(m.playerId==_p.leaderId?"  (Leader)":""); if(t.Length>1) t[1].text=m.ready?"READY":"..."; var b=r.GetComponentInChildren<Button>(); string id=m.playerId; if(b) b.onClick.AddListener(()=>{ _p.SetReady(id,!GetReady(id)); Refresh(); }); } } if(mmrText) mmrText.text=$"Party MMR: {_p.AverageMmr()}"; if(statusText) statusText.text=_p.AllReady()?"All ready!":"Waiting for ready..."; if(startButton) startButton.interactable=_p.AllReady(); }
        bool GetReady(string id){ foreach(var m in _p.Members) if(m.playerId==id) return m.ready; return false; }
    }

    public sealed class MasteryView : MonoBehaviour
    {
        [SerializeField] private RectTransform rowParent; [SerializeField] private RectTransform rowPrefab;
        public void Bind(Dictionary<string,MasteryEntry> mastery){ if(rowParent==null) return; foreach(var kv in mastery){ var e=kv.Value; var r=Instantiate(rowPrefab,rowParent); r.gameObject.SetActive(true); var t=r.GetComponentsInChildren<Text>(); if(t.Length>0) t[0].text=e.championId; if(t.Length>1) t[1].text=$"Mastery {e.level}"; if(t.Length>2) t[2].text=e.level<ChampionMastery.MaxPointLevel?$"{ChampionMastery.PointsToNext(e)} pts to next":$"{e.tokens} token(s)"; } }
    }

    public sealed class NewsFeedView : MonoBehaviour
    {
        [SerializeField] private RectTransform rowParent; [SerializeField] private Button rowPrefab; [SerializeField] private Text unreadBadge;
        private NewsFeed _feed; private long _now;
        public void Bind(NewsFeed feed,long nowUnix){ _feed=feed; _now=nowUnix; Refresh(); }
        void Refresh(){ if(rowParent!=null){ foreach(Transform c in rowParent) Destroy(c.gameObject); foreach(var item in _feed.Active(_now)){ var b=Instantiate(rowPrefab,rowParent); b.gameObject.SetActive(true); var t=b.GetComponentsInChildren<Text>(); if(t.Length>0) t[0].text=(item.pinned?"\u25C6 ":"")+item.title; if(t.Length>1) t[1].text=item.category.ToString(); var imgs=b.GetComponentsInChildren<Image>(); if(imgs.Length>1) imgs[1].enabled=!_feed.IsRead(item.id); string id=item.id; b.onClick.AddListener(()=>{ _feed.MarkRead(id); Refresh(); }); } } if(unreadBadge) unreadBadge.text=_feed.UnreadCount(_now)>0?_feed.UnreadCount(_now).ToString():""; }
    }
}
