using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonsController : MonoBehaviour
{
    [SerializeField] private WeatherController weatherController;

    private int currentDay = 0;

    public GameObject[] seasonObjects;
    public SeasonState currentSeason = SeasonState.Summer;

    public enum SeasonState { Summer, Winter }

    private void Start()
    {
        weatherController = GameObject.FindObjectOfType<WeatherController>();
        ActivateSeason(currentSeason);
    }

    public void OnDayPassed(int day)
    {
        currentDay = day;
        if (currentDay % 2 == 0)
        {
            SwitchSeasons();
        }
    }

    private void SwitchSeasons()
    {
        // Get the index of the current season object
        int currentIndex = (int)currentSeason;

        // Store the previous season
        SeasonState previousSeason = currentSeason;

        // Increment the index to get the next season object
        currentIndex++;

        // Wrap around to the beginning of the array if necessary
        if (currentIndex >= Enum.GetValues(typeof(SeasonState)).Length)
        {
            currentIndex = 0;
        }

        // Set the current season to the new index
        currentSeason = (SeasonState)currentIndex;

        // Activate the new season GameObject
        seasonObjects[(int)currentSeason].SetActive(true);

        // Start the fade Coroutine
        StartCoroutine(FadeSeasons(previousSeason, currentSeason));
    }

    private void ActivateSeason(SeasonState season)
    {
        // Deactivate all season objects except for the current one
        for (int i = 0; i < seasonObjects.Length; i++)
        {
            if (i == (int)season)
            {
                seasonObjects[i].SetActive(true);
            }
            else
            {
                seasonObjects[i].SetActive(false);
            }
        }
    }

    private IEnumerator FadeSeasons(SeasonState previousSeason, SeasonState nextSeason)
    {
        float transitionDuration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);

            SetSeasonAlpha(previousSeason, alpha);
            SetSeasonAlpha(nextSeason, 1f - alpha);

            yield return null;
        }

        // Deactivate the previous season GameObject
        seasonObjects[(int)previousSeason].SetActive(false);
    }

    private void SetSeasonAlpha(SeasonState season, float alpha)
    {
        GameObject seasonObject = seasonObjects[(int)season];
        Renderer[] renderers = seasonObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Color color = renderer.material.GetColor("_Color");
            color.a = alpha;
            renderer.material.SetColor("_Color", color);
        }
    }
}
