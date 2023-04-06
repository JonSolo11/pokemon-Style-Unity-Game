using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MoveFetcher : MonoBehaviour
{

    private MoveFetcher moveFetcher;

    void Start()
    {
        FetchAllMoves();
    }

    private void FetchAllMoves()
    {
        // Adjust the range to match the number of moves you want to fetch
        for (int moveId = 1; moveId <= 165; moveId++)
        {
            string assetPath = $"Assets/Game/Resources/Moves/Move_{moveId}.asset";
            StartCoroutine(FetchAndSaveMoveData(moveId, assetPath));
        }
    }

    private IEnumerator FetchAndSaveMoveData(int moveId, string assetPath)
    {
        // Fetch move data from the API
        string url = $"https://pokeapi.co/api/v2/move/{moveId}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch move data for move ID {moveId}.");
                yield break;
            }

            // Parse the move data from the JSON response
            JSONNode moveData = JSON.Parse(webRequest.downloadHandler.text);
            MoveBase move = ParseMoveData(moveData);

            // Set the asset path to include the move name
            string moveName = move.Name.Replace(" ", "-");
            assetPath = $"Assets/Game/Resources/Moves/{moveName}.asset";

            // Save the move as an asset
    #if UNITY_EDITOR
            SaveMoveAsset(move, assetPath);
    #endif
        }
    }

    private MoveBase ParseMoveData(JSONNode moveData)
    {
        MoveBase move = ScriptableObject.CreateInstance<MoveBase>();

        // Parse move name and format it as camelCase
        string moveName = moveData["name"].Value.Replace("-", " ");
        moveName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(moveName.ToLower());
        Debug.Log("Move Name: " + moveName);

        // Parse move description
        string description = Regex.Replace(GetEnglishFlavorText(moveData["flavor_text_entries"].AsArray), @"(?<![0-9])-|-(?![0-9])", " ");
        Debug.Log("Description: " + description);

        // Parse move power, accuracy, pp, and type
        int power = moveData["power"].AsInt;
        int accuracy = moveData["accuracy"].AsInt;
        int pp = moveData["pp"].AsInt;
        PokemonType type = (PokemonType)System.Enum.Parse(typeof(PokemonType), moveData["type"]["name"], true);

        // Parse move category
        string categoryName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(moveData["damage_class"]["name"].Value.ToLower());
        MoveCategory category = (MoveCategory)Enum.Parse(typeof(MoveCategory), categoryName);

        Debug.Log("Move Category: " + category);

        // Parse stat changes
        List<StatBoost> statBoosts = new List<StatBoost>();
        JSONArray statChanges = moveData["stat_changes"].AsArray;
        for (int i = 0; i < statChanges.Count; i++)
        {
            JSONNode statChangeData = statChanges[i];
            Stat stat = (Stat)Enum.Parse(typeof(Stat), ConvertStatName(statChangeData["stat"]["name"]));



            int boost = statChangeData["change"].AsInt;
            statBoosts.Add(new StatBoost(stat, boost));
        }

        // Parse ailment
        ConditionID ailment = ConditionID.none;
        if (!moveData["meta"]["ailment"].IsNull)
        {
            string ailmentName = moveData["meta"]["ailment"]["name"].Value;
            if (ConditionsDB.ailmentNameToID.ContainsKey(ailmentName))
            {
                ailment = ConditionsDB.ailmentNameToID[ailmentName];
            }
        }

        // Parse ailment chance
        int ailmentChance = moveData["meta"]["ailment_chance"].AsInt;

        // Parse move target
        MoveTarget target = GetMoveTarget(moveData["target"]["name"].Value);

        // Create a MoveEffects object for stat changes
        MoveEffects effects = new MoveEffects(statBoosts, ailment, ConditionID.none);

        // Create a SecondaryEffects object for ailment
        List<StatBoost> emptyStatBoosts = new List<StatBoost>();
        SecondaryEffects secondaryEffect = new SecondaryEffects(emptyStatBoosts, ailment, ConditionID.none, ailmentChance, MoveTarget.Foe);
        List<SecondaryEffects> secondaries = new List<SecondaryEffects> { secondaryEffect };

        // Initialize move with parsed data
        move.Init(moveName, description, power, accuracy, pp, type, category, effects, secondaries, target);

        return move;
    }

    private string GetEnglishMoveName(JSONArray namesArray)
    {
        foreach (JSONNode nameEntry in namesArray)
        {
            if (nameEntry["language"]["name"] == "en")
            {
                return nameEntry["name"].Value;
            }
        }
        return "";
    }

    private string GetEnglishFlavorText(JSONArray flavorTextEntries)
    {
        foreach (JSONNode entry in flavorTextEntries)
        {
            if (entry["language"]["name"] == "en")
            {
                return entry["flavor_text"].Value.Replace('\n', ' ');
            }
        }
        return "";
    }

    public static MoveTarget GetMoveTarget(string targetValue)
    {
        switch (targetValue.ToLower())
        {
            case "user":
                return MoveTarget.Self;
            default:
                return MoveTarget.Foe;
        }
    }

    private void SaveMoveAsset(MoveBase move, string assetPath)
    {
        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(assetPath));

        // Save the move as an asset with the specified asset path
        #if UNITY_EDITOR
        AssetDatabase.CreateAsset(move, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        #endif
    }
    private static string ConvertStatName(string apiStatName)
    {
        switch (apiStatName)
        {
            case "special-attack":
                return "SpAttack";
            case "special-defense":
                return "SpDefense";
            default:
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(apiStatName);
        }
    }
}
