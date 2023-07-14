using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialog(dialog, 
            new List<string>() {"Yes", "No"},
            (choiceIndex) => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
        {
            yield return DialogManager.Instance.ShowDialogText("Okay, I'll take your Pokémon for a few seconds.");
            yield return Fader.i.FadeIn(1f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(1f);
            yield return DialogManager.Instance.ShowDialogText("We've restored your Pokémon to full health. We hope to see you again!");
        }
        else if(selectedChoice == 1)
        {
            yield return DialogManager.Instance.ShowDialogText("Oh, I see. Please come back when you need us!");
        }

    }
}
