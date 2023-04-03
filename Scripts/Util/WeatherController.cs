using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [SerializeField] private SeasonsController seasonsController;
    [SerializeField] private DayNightController dayNightController;

    public enum Season
    {
        Summer,
        Winter
    }

    public Season currentSeason = Season.Summer;
    public float precipitationChance = 0.1f;
    public float temperature = 20f;
    public GameObject rainObject;
    public GameObject snowObject;
    public GameObject lightningObject;

    private float timePassed = 0f;
    private int precipitationDuration = 5;
    private bool isPrecipitating = false;

    public float baseRainIntensity = 1f;
    public float baseSnowIntensity = 1f;

    private int startPrecipitationIndex = -1;
    private int endPrecipitationIndex = -1;

    public bool GetIsPrecipitating()
    {
        return isPrecipitating;
    }

    void Start()
    {
        dayNightController.OnDayPassed += OnDayPassed;
        OnDayPassed(0);
        GenerateTodayForecast();
    }
    void OnDestroy()
    {
        dayNightController.OnDayPassed -= OnDayPassed;
    }

    void Update()
    {
        // Check for precipitation
        if (isPrecipitating == false && timePassed >= precipitationDuration)
        {
            int currentTimeOfDayIndex = GetCurrentTimeOfDayIndex(dayNightController.currentTimeOfDay);

            if (currentTimeOfDayIndex >= startPrecipitationIndex && currentTimeOfDayIndex <= endPrecipitationIndex)
            {
                StartPrecipitation();
            }
            UpdateWeather();
        }

        // Check for end of precipitation
        if (isPrecipitating)
        {
            int currentTimeOfDayIndex = GetCurrentTimeOfDayIndex(dayNightController.currentTimeOfDay);

            if (currentTimeOfDayIndex > endPrecipitationIndex)
            {
                StopPrecipitation();
            }
        }

        // Increment time passed
        timePassed += Time.deltaTime;
        UpdateWeather();
    }

    public void OnDayPassed(int day)
    {
        GenerateTodayForecast();

        int randomTimePeriodIndex = UnityEngine.Random.Range(0, dayNightController.timeOfDayList.Count);
        startPrecipitationIndex = randomTimePeriodIndex;
        endPrecipitationIndex = randomTimePeriodIndex;
    }

    private void GenerateTodayForecast()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= precipitationChance)
        {
            int startIndex = UnityEngine.Random.Range(0, dayNightController.timeOfDayList.Count);
            startPrecipitationIndex = startIndex;
            endPrecipitationIndex = startIndex;
        }
        else
        {
            startPrecipitationIndex = -1;
            endPrecipitationIndex = -1;
        }
    }

    private void UpdateWeather()
    {
        // Update the intensity of the rain and snow color based on the current time of day's intensity
        if (rainObject.activeInHierarchy)
        {
            ParticleSystem rainParticleSystem = rainObject.GetComponent<ParticleSystem>();
            var rainMain = rainParticleSystem.main;
            Color rainColor = rainMain.startColor.color;
            rainColor.a *= dayNightController.GetIntensityPercentage(dayNightController.currentTimeOfDay);
            rainMain.startColor = new ParticleSystem.MinMaxGradient(rainColor);

            Debug.Log("Rain color intensity: " + rainColor.a); // Debug statement
        }

        if (snowObject.activeInHierarchy)
        {
            ParticleSystem snowParticleSystem = snowObject.GetComponent<ParticleSystem>();
            var snowMain = snowParticleSystem.main;
            Color snowColor = snowMain.startColor.color;
            snowColor.a *= dayNightController.GetIntensityPercentage(dayNightController.currentTimeOfDay);
            snowMain.startColor = new ParticleSystem.MinMaxGradient(snowColor);

            Debug.Log("Snow color intensity: " + snowColor.a); // Debug statement
        }
    }

    private int GetCurrentTimeOfDayIndex(float currentTime)
    {
        for (int i = 0; i < dayNightController.timeOfDayList.Count; i++)
        {
            var current = dayNightController.timeOfDayList[i];
            if (currentTime >= current.startSeconds && currentTime <= current.endSeconds)
            {
                return i;
            }
        }
        return -1;
    }
    public void StartPrecipitation()
    {
        // Activate rain or snow object based on the current season
        if (seasonsController.currentSeason == SeasonsController.SeasonState.Summer)
        {
            rainObject.SetActive(true);
            snowObject.SetActive(false);

            // Check for thunderstorms
            bool isThunderstorm = Random.value < 0.5f;
            if (isThunderstorm)
            {
                lightningObject.SetActive(true);
            }
        }
        else if (seasonsController.currentSeason == SeasonsController.SeasonState.Winter)
        {
            rainObject.SetActive(false);
            snowObject.SetActive(true);
        }

        isPrecipitating = true;
    }


    public void StopPrecipitation()
    {
        // Deactivate rain and snow objects
        rainObject.SetActive(false);
        snowObject.SetActive(false);
        lightningObject.SetActive(false);

        isPrecipitating = false;
        timePassed = 0f;
    }
}
