using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countTxt;
    [SerializeField] Text priceTxt;

    bool selected;
    int currCount;

    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountselected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;

        selected = false;
        currCount = 1;

        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected == true);

        onCountselected?.Invoke(currCount);
        gameObject.SetActive(false);
    }

    private void Update()
    {

        int prevCount = currCount;

        if(Input.GetKeyDown(KeyCode.W))
            ++currCount;
        else if(Input.GetKeyDown(KeyCode.S))
            --currCount;

        currCount = Mathf.Clamp(currCount, 1, maxCount);

        if(currCount != prevCount)
            SetValues();

        if(Input.GetKeyDown(KeyCode.Space))
            selected = true;
            
        AudioManager.i.PlaySfx(AudioId.UISelect);
    }

    void SetValues()
    {
        countTxt.text = "x " + currCount;
        priceTxt.text = "$" + pricePerUnit * currCount;
    }
}
