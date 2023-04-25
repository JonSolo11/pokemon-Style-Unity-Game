using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using SimpleJSON;
using System;
using System.IO;

public class GetPokemon : MonoBehaviour
{
    public string pokemonName;
    public string description;
    public Sprite frontSprite;
    public Sprite backSprite;
    public PokemonType type1;
    public PokemonType type2;
    public int maxHP;
    public int attack;
    public int defense;
    public int spAttack;
    public int spDefense;
    public int speed;

    public int expYield;

    public int catchRate;

    public GrowthRate growthRate;


    [SerializeField] public List<LearnableMove> LearnableMove = new List<LearnableMove>();

    public IEnumerator Init()
    {
        yield return StartCoroutine(FetchPokemonNames(pokemonNames =>
        {
            foreach (string pokemonName in pokemonNames)
            {
                string formattedName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemonName);
                if (PokemonDB.GetPokemonByName(formattedName) == null)
                {
                    StartCoroutine(CreateFromAPI(pokemonName));
                }
                else
                {
                    continue;
                }
            }
        }));
    }


    private IEnumerator FetchPokemonNames(Action<List<string>> callback)
    {
        string url = "https://pokeapi.co/api/v2/pokemon?limit=151";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                yield break;
            }

            JSONNode response = JSON.Parse(request.downloadHandler.text);
            List<string> pokemonNames = new List<string>();

            foreach (JSONNode result in response["results"])
            {
                pokemonNames.Add(result["name"].Value);
            }

            callback(pokemonNames);
        }
    }

    public static IEnumerator CreateFromAPI(string pokemonName)
    {
    // Send API request to retrieve Pokemon data
    string url = $"https://pokeapi.co/api/v2/pokemon/{pokemonName}";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            yield break;
        }

        // Parse JSON response
        JSONNode response = JSON.Parse(request.downloadHandler.text);

        // Create new PokemonBase instance and set properties from JSON data
        PokemonBase newPokemon = ScriptableObject.CreateInstance<PokemonBase>();
        newPokemon.pokemonName = ToTitleCase(response["name"].Value);
        newPokemon.type1 = (PokemonType)System.Enum.Parse(typeof(PokemonType), ToTitleCase(response["types"][0]["type"]["name"].Value));
        if (response["types"].Count > 1)
        {
            newPokemon.type2 = (PokemonType)System.Enum.Parse(typeof(PokemonType), ToTitleCase(response["types"][1]["type"]["name"].Value));
        }
        foreach (JSONNode statNode in response["stats"])
        {
            string apiStatName = statNode["stat"]["name"].Value;
            string convertedStatName = ConvertStatName(apiStatName);
            int baseStat = statNode["base_stat"].AsInt;

            switch (convertedStatName)
            {
                case "hp":
                    newPokemon.maxHP = baseStat;
                    break;
                case "attack":
                    newPokemon.attack = baseStat;
                    break;
                case "defense":
                    newPokemon.defense = baseStat;
                    break;
                case "spAttack":
                    newPokemon.spAttack = baseStat;
                    break;
                case "spDefense":
                    newPokemon.spDefense = baseStat;
                    break;
                case "speed":
                    newPokemon.speed = baseStat;
                    break;
            }
            newPokemon.expYield = response["base_experience"].AsInt;
        }

        // Send API request to retrieve Pokemon species data, including description
        string speciesUrl = response["species"]["url"];
        using (UnityWebRequest speciesRequest = UnityWebRequest.Get(speciesUrl))
        {
            yield return speciesRequest.SendWebRequest();

            if (speciesRequest.result == UnityWebRequest.Result.ConnectionError || 
                speciesRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(speciesRequest.error);
                yield break;
            }

            // Parse species JSON response
            JSONNode speciesResponse = JSON.Parse(speciesRequest.downloadHandler.text);

            // Set PokemonBase description property from species data
            foreach (JSONNode flavorTextEntry in speciesResponse["flavor_text_entries"])
            {
                if (flavorTextEntry["language"]["name"] == "en")
                {
                    newPokemon.description = flavorTextEntry["flavor_text"].Value.Replace("\n", " ");
                    break;
                }
            }
            newPokemon.catchRate = speciesResponse["capture_rate"].AsInt;
            newPokemon.growthRate = (GrowthRate)System.Enum.Parse(typeof(GrowthRate), ToTitleCase(speciesResponse["growth_rate"]["name"].Value.Replace("-", "")));
        }

        // Send API request to retrieve front sprite
        string frontSpriteUrl = response["sprites"]["front_default"];
        string backSpriteUrl = response["sprites"]["back_default"];
        string frontSpritePath = null;
        string backSpritePath = null;

        yield return DownloadSprite(frontSpriteUrl, path => {
            frontSpritePath = path;
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.SaveAndReimport();
        });

        yield return DownloadSprite(backSpriteUrl, path => {
            backSpritePath = path;
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.SaveAndReimport();
        });


        newPokemon.frontSprite = AssetDatabase.LoadAssetAtPath<Sprite>(frontSpritePath);
        newPokemon.backSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backSpritePath);


        foreach (JSONNode moveNode in response["moves"])
        {
            string moveName = ToTitleCase(moveNode["move"]["name"].Value);
            // Load move object using move name
            MoveBase moveBase = Resources.Load<MoveBase>($"Moves/{moveName}");
            if (moveBase != null)
            {
                if((moveNode["version_group_details"][0]["move_learn_method"]["name"] == "machine") && (moveNode["version_group_details"][0]["version_group"]["name"] == "red-blue") || ((moveNode["version_group_details"][1]["move_learn_method"]["name"] == "machine") && (moveNode["version_group_details"][1]["version_group"]["name"] == "red-blue")))
                {
                    newPokemon.LearnableByItems.Add(moveBase);
                }

                int level = moveNode["version_group_details"][0]["level_learned_at"].AsInt;
                if(level ==0)
                    continue;
                else
                    newPokemon.LearnableMove.Add(new LearnableMove(moveBase, level));
            }
        }

        // Save created PokemonBase object as asset
        string path = $"Assets/Game/Resources/Pokemon/{newPokemon.pokemonName}.asset";
        AssetDatabase.CreateAsset(newPokemon, path);
        AssetDatabase.SaveAssets();

        // Load asset from file and invoke callback with it
        PokemonBase savedPokemon = Resources.Load<PokemonBase>($"Pokemon/{pokemonName}");
        }
    }

    // Helper method to convert string to title case
    private static string ToTitleCase(string str)
    {
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        return textInfo.ToTitleCase(str);
    }

    private static string ConvertStatName(string apiStatName)
    {
        switch (apiStatName)
        {
            case "special-attack":
                return "spAttack";
            case "special-defense":
                return "spDefense";
            default:
                return apiStatName;
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
            string folderPath = url.Contains("back") ? "Assets/Art/Pokemons/Back/" : "Assets/Art/Pokemons/Front/";
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
}

