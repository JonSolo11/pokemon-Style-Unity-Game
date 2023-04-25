using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{
    public static Dictionary<string, PokemonBase> pokemons;

    public static IEnumerator Init(GetPokemon pokemonFetcher)
    {
        pokemons = new Dictionary<string, PokemonBase>();

        var pokemonArray = Resources.LoadAll<PokemonBase>("");

            foreach(var pokemon in pokemonArray)
            {
                if(pokemons.ContainsKey(pokemon.Name))
                {
                    continue;
                }

                pokemons[pokemon.Name] = pokemon;
            }
            
            yield return pokemonFetcher.Init();
    }

    public static PokemonBase GetPokemonByName(string name)
    {
        if(!pokemons.ContainsKey(name)) 
        {
            Debug.LogError($"{name} not found");
            return null;
        }
        return pokemons[name];
    }
}
