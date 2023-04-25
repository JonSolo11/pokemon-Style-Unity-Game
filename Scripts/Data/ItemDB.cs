// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ItemDB
// {
//     public static Dictionary<string, ItemBase> items;

//     public static IEnumerator Init(ItemFetcher itemFetcher)
//     {
//         items = new Dictionary<string, ItemBase>();

//         // Fetch items until the desired number of items is reached
//         while (items.Count != desiredItemCount)
//         {
//             var itemArray = Resources.LoadAll<ItemBase>("Items");

//             foreach (var item in itemArray)
//             {
//                 if (items.ContainsKey(item.Name))
//                 {
//                     continue;
//                 }

//                 items[item.Name] = item;
//             }

//             yield return itemFetcher.Init();
//         }

//     }
//     public static ItemBase GetItemByName(string name)
//     {
//         string formattedName = name.Replace(" ", "-");
//         if (!items.ContainsKey(name))
//         {
//             Debug.Log($"{name} not found");
//             return null;
//         }
//         return items[name];
//     }

// }
