using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shokai.Items
{
    [Serializable]
    public class ItemContainer : IItemContainer
    {
        private ItemSlot[] itemSlots = new ItemSlot[0];

        public Action OnItemsUpdated = delegate { };

        public ItemContainer (int size) => itemSlots = new ItemSlot[size];

        public ItemSlot GetSlotByIndex(int index) => itemSlots[index];

        public ItemSlot AddItem(ItemSlot itemSlot)
        {
            if (itemSlot.item.Cost > GameManager.instance.player.cash) return itemSlot;
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i].item != null)
                {
                    if (itemSlots[i].item == itemSlot.item)
                    {
                        int slotRemainingSpace = itemSlots[i].item.MaxStack - itemSlots[i].quantity;

                        if (itemSlot.quantity <= slotRemainingSpace)
                        {
                            itemSlots[i].quantity += itemSlot.quantity;

                            itemSlot.quantity = 0;

                            OnItemsUpdated.Invoke();

                            GameManager.instance.player.cash -= itemSlot.item.Cost;

                            GameManager.instance.uiController.ChangeCash(GameManager.instance.player.cash);

                            AudioManager.instance.PlayOneShotSound("Buy", GameManager.instance.player.transform.position);

                            return itemSlot;
                        }
                        else if (itemSlots[i].quantity < itemSlots[i].item.MaxStack && itemSlots[i].quantity + itemSlot.quantity > itemSlots[i].item.MaxStack)
                        {
                            itemSlots[i].quantity = itemSlots[i].item.MaxStack;

                            itemSlot.quantity = 0;

                            OnItemsUpdated.Invoke();

                            GameManager.instance.player.cash -= itemSlot.item.Cost;

                            GameManager.instance.uiController.ChangeCash(GameManager.instance.player.cash);

                            AudioManager.instance.PlayOneShotSound("Buy", GameManager.instance.player.transform.position);

                            return itemSlot;
                        }
                        else if (slotRemainingSpace > 0)
                        {                           
                            itemSlots[i].quantity += slotRemainingSpace;

                            itemSlot.quantity -= slotRemainingSpace;
                        }                        
                    }
                }
            }

            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i].item == null)
                {
                    for (int y = 0; y < itemSlots.Length; y++)
                    {
                        if (itemSlots[y].item == itemSlot.item) return itemSlot;
                    }
                    if (itemSlot.quantity <= itemSlot.item.MaxStack)
                    {
                        itemSlots[i] = itemSlot;

                        itemSlot.quantity = 0;

                        OnItemsUpdated.Invoke();

                        GameManager.instance.player.cash -= itemSlot.item.Cost;

                        GameManager.instance.uiController.ChangeCash(GameManager.instance.player.cash);

                        AudioManager.instance.PlayOneShotSound("Buy", GameManager.instance.player.transform.position);

                        return itemSlot;
                    }
                    // else que no faig perque crec que es usless
                }
            }

            OnItemsUpdated.Invoke();

            return itemSlot;
        }

        public void Consume(int index)
        {
            itemSlots[index].quantity--;

            if (itemSlots[index].quantity <= 0) itemSlots[index].item = null;

            OnItemsUpdated.Invoke();
        }

        public void ResetAllSlots()
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i].item = null;
                itemSlots[i].quantity = 0;
            }

            OnItemsUpdated.Invoke();
        }

        public int GetSlotQuantityByIndex(int index)
        {
            return itemSlots[index].quantity;
        }

        public string GetItemNameByIndex(int index)
        {
            if (itemSlots[index].item != null) return itemSlots[index].item.Name;
            else return "";
        }

        public int GetTotalQuantity(InventoryItem item)
        {
            int totalCount = 0;

            foreach (ItemSlot itemSlot in itemSlots)
            {
                if (itemSlot.item == null) { continue; }
                if (itemSlot.item != item) { continue; }

                totalCount += itemSlot.quantity;
            }

            return totalCount;
        }

        public bool HasItem(InventoryItem item)
        {
            foreach (ItemSlot itemSlot in itemSlots)
            {
                if (itemSlot.item == null) { continue; }
                if (itemSlot.item != item) { continue; }

                return true;
            }

            return false;
        }

        public void RemoveAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > itemSlots.Length - 1) { return; }

            itemSlots[slotIndex] = new ItemSlot();

            OnItemsUpdated.Invoke();
        }

        public void RemoveItem(ItemSlot itemSlot)
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i].item != null)
                {
                    if (itemSlots[i].item == itemSlot.item)
                    {
                        if (itemSlots[i].quantity < itemSlot.quantity)
                        {
                            itemSlot.quantity -= itemSlots[i].quantity;

                            itemSlots[i] = new ItemSlot();
                        }
                        else
                        {
                            itemSlots[i].quantity -= itemSlot.quantity;

                            if (itemSlots[i].quantity == 0)
                            {
                                itemSlots[i] = new ItemSlot();

                                OnItemsUpdated.Invoke();

                                return;
                            }
                        }
                    }
                }
            }
        }

        public void Swap(int indexOne, int indexTwo)
        {
            ItemSlot firstSlot = itemSlots[indexOne];
            ItemSlot secondSlot = itemSlots[indexTwo];

            if (firstSlot == secondSlot) { return; }

            itemSlots[indexOne] = secondSlot;
            itemSlots[indexTwo] = firstSlot;

            OnItemsUpdated.Invoke();
        }
    }
}
