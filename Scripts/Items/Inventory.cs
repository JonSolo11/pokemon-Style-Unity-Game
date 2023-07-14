using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ItemCategory {ITEMS, POKEBALLS, TMs }
public class Inventory : MonoBehaviour, ISavable
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
                RemoveItem(item);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(category);

        var itemSlot = currSlots.FirstOrDefault(slot => slot.Item == item);

        if(itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count,
            });
        }

        OnUpdated?.Invoke();
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if(item is RecoveryItem || item is EvolutionItem)
            return ItemCategory.ITEMS;
        else if(item is PokeballItem)
            return ItemCategory.POKEBALLS;
        else
            return ItemCategory.TMs;
    }
    public void RemoveItem(ItemBase item)
    {
        int selectedCategory = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(selectedCategory);

        var itemSlot = currSlots.First(slot => slot.Item == item);
        itemSlot.Count --;
        if(itemSlot.Count == 0)
        {
            currSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int selectedCategory = (int)GetCategoryFromItem(item);
        var currSlots = GetSlotsByCategory(selectedCategory);

        return currSlots.Exists(slot => slot.Item == item);
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorysaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorysaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>> {slots, pokeballSlots, tmSlots};

        OnUpdated?.Invoke();
    }
}

[Serializable]

public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot(){}

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetItemByName(saveData.name);
        count = saveData.count;
    }
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.Name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item{
        get => item;
        set => item = value;    
    }

    public int Count {

        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorysaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
}
