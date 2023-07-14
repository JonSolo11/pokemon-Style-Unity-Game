using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{

    [SerializeField] ChoiceText choiceTextPrefab;

    List<ChoiceText> choiceTexts;

    bool choiceSelected = false;
    int currentChoice;
    public IEnumerator ShowChoices(List<string> choices, Action<int> OnChoiceSelected)
    {

        choiceSelected = false;
        currentChoice = 0;

        gameObject.SetActive(true);
        
        //Delete existing choices
        foreach(Transform child in transform)
            Destroy(child.gameObject);
        
        choiceTexts = new List<ChoiceText>();
        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
            choiceTexts.Add(choiceTextObj);
        }

        yield return new WaitUntil(() => choiceSelected == true);

        OnChoiceSelected?.Invoke(currentChoice);

        gameObject.SetActive(false);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
            ++currentChoice;
        else if (Input.GetKeyDown(KeyCode.W))
            --currentChoice;

        currentChoice = Mathf.Clamp(currentChoice, 0, choiceTexts.Count - 1);

        for (int i = 0; i < choiceTexts.Count;i++)
        {
            choiceTexts[i].SetSelected(i == currentChoice);
        }

        if(Input.GetKeyDown(KeyCode.Space))
            choiceSelected = true;
    }
}
