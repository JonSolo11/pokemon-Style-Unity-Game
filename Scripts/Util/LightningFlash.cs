using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightningFlash : MonoBehaviour
{
    [System.Serializable]
    public class LightningSettings
    {
        [Range(0.01f, 2f)]
        public float minFlashDuration = 0.05f;
        [Range(0.01f, 2f)]
        public float maxFlashDuration = 0.2f;
        [Range(0.5f, 10f)]
        public float minTimeBetweenFlashes = 5f;
        [Range(0.5f, 10f)]
        public float maxTimeBetweenFlashes = 10f;
        public int minFlashesPerEvent = 2;
        public int maxFlashesPerEvent = 3;
    }

    public LightningSettings lightningSettings;

    private Image panelImage;
    private float timer;
    private float flashTimer;
    private float currentFlashDuration;
    private int flashesRemaining;

    private DayNightController dayNightController;

    void Start()
    {
        panelImage = GetComponent<Image>();
        timer = Random.Range(lightningSettings.minTimeBetweenFlashes, lightningSettings.maxTimeBetweenFlashes);
        dayNightController = FindObjectOfType<DayNightController>();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (flashesRemaining > 0)
            {
                flashTimer += Time.deltaTime;
                float currentIntensity = dayNightController.GetIntensityPercentage(dayNightController.currentTimeOfDay);
                float alpha = Mathf.Lerp(1 * currentIntensity, 0, flashTimer / currentFlashDuration);
                panelImage.color = new Color(.9f, .9f, .9f, alpha);

                if (flashTimer >= currentFlashDuration)
                {
                    flashesRemaining--;
                    flashTimer = 0;
                    currentFlashDuration = Random.Range(lightningSettings.minFlashDuration, lightningSettings.maxFlashDuration);
                }
            }
            else
            {
                timer = Random.Range(lightningSettings.minTimeBetweenFlashes, lightningSettings.maxTimeBetweenFlashes);
                flashesRemaining = Random.Range(lightningSettings.minFlashesPerEvent, lightningSettings.maxFlashesPerEvent + 1);
                currentFlashDuration = Random.Range(lightningSettings.minFlashDuration, lightningSettings.maxFlashDuration);
            }
        }
    }
}
