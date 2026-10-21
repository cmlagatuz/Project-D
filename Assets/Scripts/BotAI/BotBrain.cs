// Project D | Codename: Zantetsuken — BotBrain.cs (utility-scored bot decisions)
using System.Collections.Generic;
namespace ProjectD.P6.BotAI
{
    public enum BotAction { Farm, Fight, Retreat, Recall, PushLane, TakeObjective, Ward, Roam }
    public struct BotContext { public float healthPct, manaPct; public int nearbyEnemies, nearbyAllies; public bool minionsAvailable, objectiveUp; public float distToTurret, gold; public bool lowOnItems; }
    public static class BotBrain
    {
        public static BotAction Decide(in BotContext c)
        {
            var s=new Dictionary<BotAction,float>();
            s[BotAction.Retreat]=(c.healthPct<0.3f?0.9f:0f)+(c.nearbyEnemies>c.nearbyAllies?0.4f:0f);
            s[BotAction.Recall]=(c.healthPct<0.2f?0.5f:0f)+(c.lowOnItems&&c.gold>1200?0.6f:0f)+(c.manaPct<0.15f?0.3f:0f);
            s[BotAction.Fight]=(c.nearbyEnemies>0&&c.nearbyAllies>=c.nearbyEnemies?0.7f:0f)+(c.healthPct>0.6f?0.2f:0f);
            s[BotAction.TakeObjective]=(c.objectiveUp&&c.nearbyAllies>=c.nearbyEnemies?0.8f:0f);
            s[BotAction.Farm]=(c.minionsAvailable?0.5f:0f)+(c.nearbyEnemies==0?0.2f:0f);
            s[BotAction.PushLane]=(c.minionsAvailable&&c.nearbyEnemies==0?0.4f:0f);
            s[BotAction.Ward]=(c.nearbyEnemies==0?0.15f:0f); s[BotAction.Roam]=0.1f;
            BotAction best=BotAction.Farm; float bv=float.MinValue;
            foreach(var kv in s) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best;
        }
    }
}
