using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EvolutionManager : MonoBehaviour
{
   [SerializeField] GameObject evolutionUI;
   [SerializeField] Image pokemonImage;
   [SerializeField] Image pokemonImageAlpha;

   [SerializeField] VideoPlayback video;

   public event Action OnStartEvolution;
   public event Action OnCompleteEvolution;
   

   public static EvolutionManager i {get; private set;}
   
   private void Awake()
   {
        i = this;
   }

   public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);
        video.ResetVideo();
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        pokemonImageAlpha.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.Instance.ShowDialogText($"What?! \n{pokemon.Base.Name} is evolving!", false, false);

        var oldPokemon = pokemon.Base;

        pokemon.Evolve(evolution);

        float bounceDuration = 0.3f;
        float bounceScale = 1.2f;

        yield return new WaitForSeconds(4.5f);

        float fadeDuration = 1f;
        float totalDuration = 8f;
        float whiteDuration = 1.5f;
        Color startColor = pokemonImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // Transparent color

        pokemonImage.DOFade(0f, fadeDuration)
            .From(startColor.a)
            .SetEase(Ease.Linear);

        yield return new WaitForSeconds(3.5f);

        float startTime = Time.time;

        // Bounce animation loop
        while (Time.time - startTime < totalDuration)
        {
            pokemonImage.sprite = pokemon.Base.FrontSprite;
            pokemonImageAlpha.sprite = pokemon.Base.FrontSprite;
            // Scale up
            pokemonImageAlpha.transform.DOScale(new Vector3(bounceScale, bounceScale, 1f), bounceDuration / 2f)
                .SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(bounceDuration / 2f);

            // Scale down
            pokemonImageAlpha.transform.DOScale(Vector3.one, bounceDuration / 2f)
                .SetEase(Ease.InQuad);

            yield return new WaitForSeconds(bounceDuration / 2f);

            pokemonImage.sprite = oldPokemon.FrontSprite;
            pokemonImageAlpha.sprite = oldPokemon.FrontSprite;

            // Scale up
            pokemonImageAlpha.transform.DOScale(new Vector3(bounceScale, bounceScale, 1f), bounceDuration / 2f)
                .SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(bounceDuration / 2f);

            // Scale down
            pokemonImageAlpha.transform.DOScale(Vector3.one, bounceDuration / 2f)
                .SetEase(Ease.InQuad);

            yield return new WaitForSeconds(bounceDuration / 2f);

            if(bounceDuration > .05f)
                bounceDuration *= .92f;
        }

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        pokemonImageAlpha.sprite = pokemon.Base.FrontSprite;

        yield return new WaitForSeconds(whiteDuration);

        // Set final evolved sprite and fade in color
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        pokemonImage.DOFade(1f, 5f)
            .SetEase(Ease.OutCubic);

        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} evolved into {pokemon.Base.Name}!",false,false, true);

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
    
}

