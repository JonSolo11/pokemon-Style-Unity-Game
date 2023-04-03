using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Update TimeOfDay to use int for startSeconds, endSeconds, and transitionDuration
[System.Serializable]
public class TimeOfDay
{
    public string name;
    public int startSeconds;
    public int endSeconds;
    public Color color;
    public float intensity;
    public int transitionDuration = 10;
}

public class DayNightController : MonoBehaviour
{
    public float currentTimeOfDay = 0f;
    public Gradient nightDayColor;
    [SerializeField]
    public List<TimeOfDay> timeOfDayList = new List<TimeOfDay>
    {
        new TimeOfDay { name = "Dawn", startSeconds = 0, endSeconds = 30, color = new Color(0.6f, 0.6f, 0.6f), intensity = 0.5f, transitionDuration = 10 },
        new TimeOfDay { name = "Day", startSeconds = 30, endSeconds = 90, color = Color.white, intensity = 1f, transitionDuration = 10 },
        new TimeOfDay { name = "Dusk", startSeconds = 90, endSeconds = 120, color = new Color(0.6f, 0.6f, 0.6f), intensity = 0.5f, transitionDuration = 10 },
        new TimeOfDay { name = "Night", startSeconds = 120, endSeconds = 150, color = new Color(0.2f, 0.2f, 0.2f), intensity = 0.2f, transitionDuration = 10 }
    };
    public int dayLength = 150;
    private float timePassed = 0f;
    public int currentDay= 0;
    public delegate void DayPassed(int day);
    public event DayPassed OnDayPassed;


    private UnityEngine.Rendering.Universal.Light2D lightSource;
    private SeasonsController seasonsController;

    void AdjustTimeOfDayEntries()
    {
        int totalDayLength = dayLength;
        int currentTimeOfDayLength = 0;

        // Calculate the total time of day lengths
        for (int i = 0; i < timeOfDayList.Count; i++)
        {
            currentTimeOfDayLength += timeOfDayList[i].endSeconds - timeOfDayList[i].startSeconds;
        }

        // Calculate the difference between the total day length and the sum of the time of day lengths
        int difference = totalDayLength - currentTimeOfDayLength;

        // If there is a difference, add it to the end of the "Day" time of day and update the start and end times for the following time of day entries
        if (difference != 0f)
        {
            for (int i = 0; i < timeOfDayList.Count; i++)
            {
                if (timeOfDayList[i].name == "Day")
                {
                    timeOfDayList[i].endSeconds += difference;
                }
                else if (i > 1)
                {
                    timeOfDayList[i].startSeconds += difference;
                    timeOfDayList[i].endSeconds += difference;
                }
            }
        }
    }

    void Start()
    {
        lightSource = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        seasonsController = GameObject.FindObjectOfType<SeasonsController>();

    }

    void Update()
    {
        UpdateLight();
        timePassed += Time.deltaTime;
        if (timePassed >= dayLength)
        {
            timePassed = 0f;
            currentDay++;
            Debug.Log("Day passed");
            OnDayPassed?.Invoke(currentDay);
            seasonsController.OnDayPassed(currentDay); // Call method on SeasonsController script here
        }
    }

    public float GetIntensityPercentage(float currentTimeOfDay)
    {
        for (int i = 0; i < timeOfDayList.Count; i++)
        {
            var current = timeOfDayList[i];
            if (currentTimeOfDay >= current.startSeconds && currentTimeOfDay <= current.endSeconds)
            {
                int nextIndex = (i + 1) % timeOfDayList.Count;
                var next = timeOfDayList[nextIndex];

                if (currentTimeOfDay >= current.endSeconds - current.transitionDuration)
                {
                    float t = (currentTimeOfDay - (current.endSeconds - current.transitionDuration)) / current.transitionDuration;
                    return Mathf.Lerp(current.intensity, next.intensity, t);
                }
                else
                {
                    return current.intensity;
                }
            }
        }

        return timeOfDayList[0].intensity;
    }

    void UpdateLight()
    {
        int totalDayLength = dayLength;
        timePassed += Time.deltaTime;
        currentTimeOfDay = timePassed % totalDayLength;

        // Update light source color and intensity based on current time of day
        float intensityPercentage = GetIntensityPercentage(currentTimeOfDay);
        lightSource.color = GetInterpolatedColor(currentTimeOfDay);
        lightSource.intensity = intensityPercentage;
    }

    Color GetInterpolatedColor(float currentTimeOfDay)
    {
        for (int i = 0; i < timeOfDayList.Count; i++)
        {
            var current = timeOfDayList[i];
            if (currentTimeOfDay >= current.startSeconds && currentTimeOfDay <= current.endSeconds)
            {
                int nextIndex = (i + 1) % timeOfDayList.Count;
                var next = timeOfDayList[nextIndex];

                if (currentTimeOfDay >= current.endSeconds - current.transitionDuration)
                {
                    float t = (currentTimeOfDay - (current.endSeconds - current.transitionDuration)) / current.transitionDuration;
                    return Color.Lerp(current.color, next.color, t);
                }
                else
                {
                    return current.color;
                }
            }
        }

        return timeOfDayList[0].color;
    }
}
