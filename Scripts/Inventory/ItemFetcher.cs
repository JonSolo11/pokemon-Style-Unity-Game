using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using SimpleJSON;
using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

public class ItemFetcher : MonoBehaviour
{
    // List of items to retrieve
    private List<string> itemList = new List<string> 
    {
        "poke-ball","great-ball","ultra-ball","master-ball","repel","super-repel","max-repel","escape-rope","old-rod","good-rod","super-rod","pp-up",
        "ether","max-ether","elixir","max-elixir","carbos","calcium","protein","iron","zinc","hp-up","rare-candy","super-potion","potion","full-restore","max-potion",
        "hyper-potion","lemonade","soda-pop","fresh-water","antidote","paralyze-heal","awakening","burn-heal","ice-heal","full-heal","revive","max-revive","sun-stone",
        "moon-stone","fire-stone","thunder-stone","water-stone","leaf-stone","nugget","exp-share"
    };

    private List<string> hmList = new List<string>
    {
        "cut","fly","surf","strength","flash"
    };

    public IEnumerator Init()
    {
        // Load all Pokemon from the Resources folder
        PokemonBase[] allPokemon = Resources.LoadAll<PokemonBase>("Pokemon");

        // Load all TM and HM items from the Resources folder
        TMItem[] tmAndHmItems = Resources.LoadAll<TMItem>("Items/TM&HM");

        // If there are already 55 TMs in the folder, return
        if (tmAndHmItems.Length >= 55)
        {
            Debug.Log("55 tm items found. skipping API call");
            yield break;
        }

        // Get a list of all the MoveBase names from the TM and HM items
        List<string> tmAndHmMoveNames = tmAndHmItems
            .Select(item => item.Move.Name.ToLower().Replace(" ", "-"))
            .ToList();

        // Get a list of all the moves learnable by items from all Pokémon
        List<MoveBase> learnableByItems = allPokemon
            .SelectMany(pokemon => pokemon.LearnableByItems)
            .Distinct()
            .ToList();

        // Loop through the moves learnable by items
        foreach (MoveBase learnableByItem in learnableByItems)
        {
            Debug.Log($"{learnableByItem.Name}");
            string tmOrHmName = learnableByItem.Name.ToLower().Replace(" ", "-");

            // Check if the TM or HM move name is not in the list of existing TM and HM move names
            if (!tmAndHmMoveNames.Contains(tmOrHmName))
            {
                // Call CreateFromList method to create TM or HM with the given move
                bool isHM = hmList.Contains(tmOrHmName);
                yield return StartCoroutine(CreateTMItem(tmOrHmName, learnableByItem, isHM));

                // Add the new TM or HM move name to the list
                tmAndHmMoveNames.Add(tmOrHmName);
            }
        }
        RenameItemsBasedOnIsHM();

        ItemBase[] allItems = Resources.LoadAll<ItemBase>("Items");
        foreach (string itemName in itemList)
        {
            // Check if the item already exists in the folder
            string folderPath = GetFolderPathByCategory(itemName);
            ItemBase existingItem = allItems.FirstOrDefault(item => item.name == itemName);

            // If the item doesn't exist, call CreateFromList method to retrieve data for each item
            if (existingItem == null)
            {
                yield return StartCoroutine(CreateFromAPI(itemName));
            }
        }
    }

    public static IEnumerator CreateFromAPI(string itemName, MoveBase move = null)
    {
        // Send API request to retrieve item data
        string url = $"https://pokeapi.co/api/v2/item/{itemName}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                yield break;
            }

            // Parse JSON response
            JSONNode response = JSON.Parse(request.downloadHandler.text);

            // Extract the name and description
            string name = response["name"].Value;
            string description = response["effect_entries"][0]["short_effect"].Value;
            string categoryName = response["category"]["name"].Value;

          
            // Create new ItemBase instance and set properties from JSON data
            ItemBase newItem;
            if (categoryName == "healing" || categoryName == "status-cures" || categoryName == "revival" || categoryName == "pp-recovery")
            {
                RecoveryItem recoveryItem = ScriptableObject.CreateInstance<RecoveryItem>();
                recoveryItem.SetItemValues(name, description, categoryName, null);

                // Set the appropriate properties for the RecoveryItem based on the item name or other attributes

                    switch (itemName)
                {
                    case "potion":
                        recoveryItem.hpAmount = 20;
                        break;
                    case "super-potion":
                        recoveryItem.hpAmount = 50;
                        break;
                    case "hyper-potion":
                        recoveryItem.hpAmount = 200;
                        break;
                    case "max-potion":
                        recoveryItem.restoreMaxHP = true; // Set to maximum possible HP, since it fully restores HP
                        break;
                    case "full-restore":
                        recoveryItem.restoreMaxHP = true; // Set to maximum possible HP, since it fully restores HP
                        recoveryItem.recoverAllStatus = true; // Also heals all status conditions
                        break;
                    case "lemonade":
                        recoveryItem.hpAmount = 80;
                        break;
                    case "soda-pop":
                        recoveryItem.hpAmount = 60;
                        break;
                    case "fresh-water":
                        recoveryItem.hpAmount = 50;
                        break;
                    case "antidote":
                        recoveryItem.status = ConditionID.psn; // Heals poison
                        break;
                    case "paralyze-heal":
                        recoveryItem.status = ConditionID.par; // Heals paralysis
                        break;
                    case "awakening":
                        recoveryItem.status = ConditionID.slp; // Heals sleep
                        break;
                    case "burn-heal":
                        recoveryItem.status = ConditionID.brn; // Heals burn
                        break;
                    case "ice-heal":
                        recoveryItem.status = ConditionID.frz; // Heals freeze
                        break;
                    case "full-heal":
                        recoveryItem.recoverAllStatus = true; // Heals all status conditions
                        break;
                    case "revive":
                        recoveryItem.revive = true;
                        break;
                    case "max-revive":
                        recoveryItem.revive = true;
                        break;
                    case "ether":
                        recoveryItem.ppAmount = 10;
                        break;
                    case "max-ether":
                        recoveryItem.restoreMaxPP = true;
                        break;
                    case "elixir":
                        recoveryItem.restoreMaxPP = true;
                        break;
                    case "max-elixir":
                        recoveryItem.restoreMaxPP = true;
                        break;
                    default:
                        // Handle items that are not applicable (e.g., no hpAmount or statusHeal value)
                        break;
                }

                newItem = recoveryItem;
            }
            else if (categoryName == "standard-balls" || categoryName == "special-balls")
            {
                PokeballItem pokeballItem = ScriptableObject.CreateInstance<PokeballItem>();
                pokeballItem.SetItemValues(name, description, categoryName, null);

                // Get the catch rate modifier based on the Pokeball name
                float catchRateModifier;
                switch (name)
                {
                    case "poke-ball":
                        catchRateModifier = 1.0f;
                        break;
                    case "great-ball":
                        catchRateModifier = 1.5f;
                        break;
                    case "ultra-ball":
                        catchRateModifier = 2.0f;
                        break;
                    case "master-ball":
                        catchRateModifier = 255.0f;
                        break;
                    // Add more cases for other Pokeballs here
                    default:
                        catchRateModifier = 1.0f;
                        break;
                }

                // Assign the catch rate to the PokeballItem
                pokeballItem.catchRateModifier = catchRateModifier;

                newItem = pokeballItem;
            }
            else
            {
                newItem = ScriptableObject.CreateInstance<ItemBase>();
            }
            // Send API request to retrieve sprite
            string spriteUrl = response["sprites"]["default"];
            Debug.Log(spriteUrl);
            string spritePath = null;

            yield return DownloadSprite(spriteUrl, path => {
                spritePath = path;
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.SaveAndReimport();
            });

            newItem.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            // Save created ItemBase object as asset
            string folderPath = ItemFetcher.GetFolderPathByCategory(response["category"]["name"].Value);
            string path = $"Assets/Game/Resources/Items/{folderPath}/{response["name"].Value}.asset";
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();
        }
    }

    private static IEnumerator DownloadSprite(string url, Action<string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to download image: {www.error}");
            callback(null);
        }
        else
        {
            byte[] imageData = www.downloadHandler.data;

            string fileName = Path.GetFileNameWithoutExtension(url);
            string folderPath = "Assets/Art/Items/";
            string path = $"{folderPath}{fileName}.png";

            File.WriteAllBytes(path, imageData);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.mipmapEnabled = false;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.SaveAndReimport();

            callback(path);
        }
    }

    private ItemBase CreateAndSaveItem(ItemJSON itemJSON)
    {
        string formattedName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemJSON.name);

        // Create new ItemBase instance and set its properties from JSON data
        ItemBase newItem = ScriptableObject.CreateInstance<ItemBase>();
        newItem.SetItemValues(formattedName, itemJSON.effect_entries[0].short_effect, itemJSON.category.name, null);

        // Save created ItemBase object as asset
        string folderPath = GetFolderPathByCategory(itemJSON.category.name);
        string path = $"{folderPath}/{newItem.name}.asset";
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        // Load asset from file and return it
        ItemBase savedItem = AssetDatabase.LoadAssetAtPath<ItemBase>(path);
        return savedItem;
    }
    IEnumerator CreateTMItem(string name, MoveBase move, bool isHM)
    {
        Debug.Log($"{name} is missing. Calling api.");
        // Load all the sprites from the specified path
        UnityEngine.Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Items/3DS - Pokemon Sun Moon - Item Icons.png");

        // Access the TM icon by its index (replace 'iconIndex' with the correct index)
        int iconIndex = 310; // Replace this value with the actual index of the TM icon
        Sprite iconSprite = allSprites[iconIndex] as Sprite;

        // Create the new TM Item
        TMItem newItem = ScriptableObject.CreateInstance<TMItem>();
        name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        name = name.Replace("-", " ");
        newItem.SetItemValues(name, $"Teaches {name} to a Pokemon", "all-machines", iconSprite);
        newItem.SetMove(move);
        newItem.SetIsHM(isHM);

        // Save created ItemBase object as asset
         string path = $"Assets/Game/Resources/Items/TM&HM/{name}.asset";
        
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        // Load asset from file and return it
        ItemBase savedItem = AssetDatabase.LoadAssetAtPath<ItemBase>(path);
        yield return savedItem;
    }

    public void RenameItemsBasedOnIsHM()
    {
        // Load all ItemBase assets in the TM&HM folder
        ItemBase[] allTMs = Resources.LoadAll<ItemBase>("Items/TM&HM");

        // Sort items based on their name
        Array.Sort(allTMs, (x, y) => string.Compare(x.Name, y.Name));

        int tmCounter = 0;
        int hmCounter = 0;

        // Loop through all items in the folder
        foreach (TMItem tm in allTMs)
        {
            string oldPath = AssetDatabase.GetAssetPath(tm);
            string newPath;

            // Check if the item is a TM or HM
            if (tm.IsHM)
            {
                // Increment the HM counter
                hmCounter++;

                // Generate the new path for the HM item
                newPath = $"Assets/Game/Resources/Items/TM&HM/HM{hmCounter.ToString("00")}.asset";

                // Check if the item's name is already in the correct format
                if (!oldPath.EndsWith(newPath.Substring(newPath.LastIndexOf("/"))))
                {
                    // Rename the asset file
                    AssetDatabase.MoveAsset(oldPath, newPath);
                }
            }
            else
            {
                // Increment the TM counter
                tmCounter++;

                // Generate the new path for the TM item
                newPath = $"Assets/Game/Resources/Items/TM&HM/TM{tmCounter.ToString("00")}.asset";

                // Check if the item's name is already in the correct format
                if (!oldPath.EndsWith(newPath.Substring(newPath.LastIndexOf("/"))))
                {
                    // Rename the asset file
                    AssetDatabase.MoveAsset(oldPath, newPath);
                }
            }
        }

        // Refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static string GetFolderPathByCategory(string categoryName)
    {
        switch (categoryName)
        {
            case "standard-balls":
            case "special-balls":
                return "Pokeballs";
            case "healing":
            case "status-cures":
            case "revival":
            case "pp-recovery":
                return "Recovery";
            case "all-machines":
                return "TM&HM";
            default:
                return "Other";
        }
    }


    [System.Serializable]
    public class ItemJSON
    {
        public int id;
        public string name;
        public SpritesJSON sprites;
        public CategoryJSON category;
        public List<EffectEntryJSON> effect_entries;

        [System.Serializable]
        public class SpritesJSON
        {
            public string _default;
        }

        [System.Serializable]
        public class CategoryJSON
        {
            public string name;
        }

        [System.Serializable]
        public class EffectEntryJSON
        {
            public string short_effect;
        }
    }
}

