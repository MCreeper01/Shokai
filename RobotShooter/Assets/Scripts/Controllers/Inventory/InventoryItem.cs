using UnityEngine;

namespace Shokai.Items
{    
    public abstract class InventoryItem : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private new string name = "New Item Name";
        [SerializeField] private Sprite icon = null;        

        public string Name => name;
        public Sprite Icon => icon;
                [Header("Item Data")]
        
        [SerializeField] [Min(0)] private int cost = 0;
        [SerializeField] [Min(0)] private int sellPrice; //maybe
        [SerializeField] [Min(1)] private int charges = 1;
        [SerializeField] [Min(1)] private int maxStack = 1;

        public int Cost => cost;
        public int SellPrice => sellPrice;
        public int Charges => charges;
        public int MaxStack => maxStack;
    }
}

