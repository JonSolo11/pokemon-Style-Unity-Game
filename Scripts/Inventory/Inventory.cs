using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ItemCategory {ITEMS, POKEBALLS, TMs }
public class Inventory : MonoBehaviour
{
     [SerializeField] List<ItemSlot> slots;
     [SerializeField] List<ItemSlot> pokeballSlots;
     [SerializeField] List<ItemSlot> tmSlots;

     List<List<ItemSlot>> allSlots;

     public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>> {slots, pokeballSlots, tmSlots};
    }
    public static List<string> itemCategories {get; set;} = new List<string>()
    {
        "ITEMS", "BALLS", "TMs"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem (int itemIndex, int categoryIndex)
    {
        var currSlots = GetSlotsByCategory(categoryIndex);
        return currSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        
        var item = GetItem(itemIndex, selectedCategory);
        
        bool itemUsed = item.Use(selectedPokemon);

        if(itemUsed)
        {
            if(!item.IsReusable)
                RemoveItem(item, selectedCategory);
            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item, int selectedCategory)
    {
        var currSlots = GetSlotsByCategory(selectedCategory);
        var itemSlot = currSlots.First(slot => slot.Item == item);
        itemSlot.Count --;
        if(itemSlot.Count == 0)
        {
            currSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]

public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;

    public int Count {

        get => count;
        set => count = value;
    }


}
