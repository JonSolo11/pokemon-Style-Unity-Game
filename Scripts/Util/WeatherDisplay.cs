using UnityEngine;
using UnityEngine.UI;

public class WeatherDisplay : MonoBehaviour
{
    [SerializeField] private WeatherController weatherController;
    [SerializeField] private Image currentWeatherImage;
    [SerializeField] private Image tomorrowWeatherImage;
    [SerializeField] private Sprite rainSprite;
    [SerializeField] private Sprite snowSprite;
    [SerializeField] private Sprite clearSprite;
    [SerializeField] private Text weatherTextBox;
    [SerializeField] private Text tomorrowWeatherTextBox;

    private void Update()
    {
        UpdateCurrentWeather();
        UpdateWeatherTextBox();
    }

    private void UpdateCurrentWeather()
    {
        WeatherController.Season season = weatherController.currentSeason;
        bool isPrecipitating = weatherController.GetIsPrecipitating();

        if (!isPrecipitating)
        {
            currentWeatherImage.sprite = clearSprite;
        }
        else if (isPrecipitating && season == WeatherController.Season.Summer)
        {
            currentWeatherImage.sprite = rainSprite;
        }
        else if (isPrecipitating && season == WeatherController.Season.Winter)
        {
            currentWeatherImage.sprite = snowSprite;
        }
    }

    private void UpdateWeatherTextBox()
    {
        WeatherController.Season season = weatherController.currentSeason;
        bool isPrecipitating = weatherController.GetIsPrecipitating();

        string currentWeatherText;
        if (isPrecipitating)
        {
            if (weatherController.currentSeason == WeatherController.Season.Summer)
            {
                currentWeatherText = "Current Weather: Rain";
            }
            else
            {
                currentWeatherText = "Current Weather: Snow";
            }
        }
        else
        {
            currentWeatherText = "Current Weather: Clear";
        }

        weatherTextBox.text = currentWeatherText;
    }
}
