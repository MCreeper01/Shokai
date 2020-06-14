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
        [Min(0)] private int sellPrice; //maybe
        [Min(1)] private int maxStack = 1;

        public int SellPrice => sellPrice;
        public int MaxStack => maxStack;
    }
}

