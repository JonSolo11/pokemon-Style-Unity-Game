using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PokemonWrapper
{
    public PokemonData[] pokemon;
}

[System.Serializable]
public class PokemonData
{
    public string name;
    public EvolutionData[] evolutions;
}

[System.Serializable]
public class EvolutionData
{
    public string pokemonName;
    public int level;
    public string item;
}

public class EvolutionsDB : MonoBehaviour
{
    [SerializeField] private TextAsset jsonData;

    private static Dictionary<string, PokemonBase> pokemonDictionary = new Dictionary<string, PokemonBase>();

    public static void Init()
    {
        LoadPokemonBases();
        LoadEvolutions();
    }

    private static void LoadPokemonBases()
    {
        PokemonBase[] pokemonBases = Resources.LoadAll<PokemonBase>("Pokemon");
        foreach (PokemonBase pokemonBase in pokemonBases)
        {
            if (!pokemonDictionary.ContainsKey(pokemonBase.name))
            {
                pokemonDictionary[pokemonBase.name] = pokemonBase;
            }
            else
            {
                Debug.LogError($"Duplicate PokemonBase name detected: {pokemonBase.name}");
            }
        }
    }

    private static void LoadEvolutions()
{
    // Get reference to JSON data asset
    TextAsset jsonDataAsset = Resources.Load<TextAsset>("Evolutions");

    // Log if jsonDataAsset is null
    if (jsonDataAsset == null) Debug.LogError("jsonDataAsset is null");

    // Deserialize JSON data to PokemonWrapper
    PokemonWrapper pokemonWrapper = JsonUtility.FromJson<PokemonWrapper>(jsonDataAsset.text);

    // Log if pokemonWrapper is null
    if (pokemonWrapper == null) Debug.LogError("pokemonWrapper is null");

    foreach (var pokemonData in pokemonWrapper.pokemon)
    {
        // Log each pokemon's name being processed
        Debug.Log("Processing: " + pokemonData.name);

        // Find the corresponding PokemonBase
        if (pokemonDictionary.TryGetValue(pokemonData.name, out PokemonBase basePokemon))
        {
            // Log if basePokemon is null
            if (basePokemon == null) Debug.LogError("basePokemon is null for " + pokemonData.name);

            foreach (var evolutionData in pokemonData.evolutions)
            {
                // Log each evolution's name being processed
                Debug.Log("Processing evolution: " + evolutionData.pokemonName);

                // Find the PokemonBase that the current Pokemon evolves into
                if (pokemonDictionary.TryGetValue(evolutionData.pokemonName, out PokemonBase evolvesInto))
                {
                    // If the PokemonBase already has Evolutions, skip adding more
                    if (basePokemon.Evolutions.Count == pokemonData.evolutions.Length) continue;
                    // Log if evolvesInto is null
                    if (evolvesInto == null) Debug.LogError("evolvesInto is null for " + evolutionData.pokemonName);

                    // Try to load the item
                    ItemBase item = null;
                    if (!string.IsNullOrEmpty(evolutionData.item))
                    {
                        // Convert item name to lowercase and replace spaces with hyphens
                        string itemName = evolutionData.item.ToLower().Replace(" ", "-");
                        Debug.Log($"Attempting to load item: {itemName}"); // Debug line
                        item = Resources.Load<ItemBase>($"Items/Evolution/{itemName}");
                        if (item == null)
                        {
                            Debug.LogError($"Failed to load item: {itemName}");
                        }
                        else
                        {
                            Debug.Log($"Loaded item: {item.name}");
                        }
                    }

                    // Create a new Evolution and add it to basePokemon
                    Evolution newEvolution = new Evolution(evolvesInto, evolutionData.level, (EvolutionItem)item);
                    basePokemon.Evolutions.Add(newEvolution);
                }
            }
        }
        else
        {
            // Log if a PokemonData doesn't exist in the dictionary
            Debug.LogError("Pokemon not found in dictionary: " + pokemonData.name);
        }
    }
}


}
