using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDB
{
    public static Dictionary<string, ItemBase> items;

    public static void Init()
    {
        items = new Dictionary<string, ItemBase>();

        var itemArray = Resources.LoadAll<ItemBase>("Items");
        foreach (var item in itemArray)
        {
            if (items.ContainsKey(item.Name))
            {
                continue;
            }

            items[item.Name] = item;
        }
    }
    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.Log($"{name} not found");
            return null;
        }
        return items[name];
    }

}

