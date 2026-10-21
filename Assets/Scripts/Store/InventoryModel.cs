// Project D | Codename: Zantetsuken — InventoryModel.cs (6-slot inventory, engine-agnostic)
using System.Collections.Generic;
namespace ProjectD.Store
{
    public sealed class InventoryModel
    {
        public const int SlotCount=6, BootsSlotIndex=5, GeneralSlots=5;
        public struct Slot { public string ItemId; public int Stack; public bool IsEmpty => string.IsNullOrEmpty(ItemId); }
        private readonly Slot[] _slots=new Slot[SlotCount];
        private readonly IItemLookup _lookup;
        public InventoryModel(IItemLookup lookup){ _lookup=lookup; }
        public Slot GetSlot(int i)=>_slots[i];
        public IReadOnlyList<Slot> Slots=>_slots;
        public bool HasBoots=>!_slots[BootsSlotIndex].IsEmpty;
        public bool IsFull { get { for(int i=0;i<GeneralSlots;i++) if(_slots[i].IsEmpty) return false; return true; } }
        public bool CanPlace(ItemView item)
        {
            if(item.IsBoots) return _slots[BootsSlotIndex].IsEmpty;
            if(item.MaxStack>1 && FindStackable(item.ItemId)>=0) return true;
            return FirstEmptyGeneral()>=0;
        }
        public int Place(ItemView item)
        {
            if(item.IsBoots){ if(!_slots[BootsSlotIndex].IsEmpty) return -1; _slots[BootsSlotIndex]=new Slot{ItemId=item.ItemId,Stack=1}; return BootsSlotIndex; }
            if(item.MaxStack>1){ int s=FindStackable(item.ItemId); if(s>=0 && _slots[s].Stack<item.MaxStack){ var sl=_slots[s]; sl.Stack++; _slots[s]=sl; return s; } }
            int e=FirstEmptyGeneral(); if(e<0) return -1; _slots[e]=new Slot{ItemId=item.ItemId,Stack=1}; return e;
        }
        public bool RemoveOne(int i){ if(i<0||i>=SlotCount||_slots[i].IsEmpty) return false; var sl=_slots[i]; sl.Stack--; _slots[i]=sl.Stack<=0?default:sl; return true; }
        public bool Contains(string id){ for(int i=0;i<SlotCount;i++) if(_slots[i].ItemId==id) return true; return false; }
        public Dictionary<StatType,float> AggregateStats()
        {
            var t=new Dictionary<StatType,float>();
            for(int i=0;i<SlotCount;i++){ if(_slots[i].IsEmpty) continue; var it=_lookup.Get(_slots[i].ItemId);
                if(it.Stats==null) continue; foreach(var s in it.Stats){ t.TryGetValue(s.stat,out float c); t[s.stat]=c+s.value*_slots[i].Stack; } }
            return t;
        }
        private int FirstEmptyGeneral(){ for(int i=0;i<GeneralSlots;i++) if(_slots[i].IsEmpty) return i; return -1; }
        private int FindStackable(string id){ for(int i=0;i<GeneralSlots;i++) if(_slots[i].ItemId==id) return i; return -1; }
    }
    public struct StatModView { public StatType stat; public float value; }
    public struct ItemView { public string ItemId; public bool IsBoots; public int MaxStack; public int TotalCost; public string ExclusiveToChampionId; public ItemCategory Category; public StatModView[] Stats; }
    public interface IItemLookup { ItemView Get(string itemId); bool Exists(string itemId); }
}
