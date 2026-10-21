// Project D | Codename: Zantetsuken — PracticeTool.cs (sandbox config + shareable blob)
using System; using System.Text;
namespace ProjectD.Practice
{
    [Serializable] public sealed class PracticeConfig
    {
        public bool noCooldowns, infiniteGil, infiniteHealth, lockLevel; public int startLevel=1, startGil=0;
        public bool spawnEnemyDummy=true, spawnAllyDummy; public float gameSpeed=1f; public bool disableTurrets, instantRespawn=true; public string spawnMonster="";
        public PracticeConfig Clone()=>(PracticeConfig)MemberwiseClone();
        public string ToShareCode()
        { var sb=new StringBuilder(); sb.Append(noCooldowns?1:0).Append('|').Append(infiniteGil?1:0).Append('|').Append(infiniteHealth?1:0).Append('|').Append(lockLevel?1:0).Append('|').Append(startLevel).Append('|').Append(startGil).Append('|').Append(spawnEnemyDummy?1:0).Append('|').Append(spawnAllyDummy?1:0).Append('|').Append(gameSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('|').Append(disableTurrets?1:0).Append('|').Append(instantRespawn?1:0); return sb.ToString(); }
        public static PracticeConfig FromShareCode(string code)
        { var c=new PracticeConfig(); if(string.IsNullOrEmpty(code)) return c; var p=code.Split('|'); try{ c.noCooldowns=p[0]=="1"; c.infiniteGil=p[1]=="1"; c.infiniteHealth=p[2]=="1"; c.lockLevel=p[3]=="1"; c.startLevel=int.Parse(p[4]); c.startGil=int.Parse(p[5]); c.spawnEnemyDummy=p[6]=="1"; c.spawnAllyDummy=p[7]=="1"; c.gameSpeed=float.Parse(p[8],System.Globalization.CultureInfo.InvariantCulture); c.disableTurrets=p[9]=="1"; c.instantRespawn=p[10]=="1"; } catch { return new PracticeConfig(); } return c; }
    }
}
