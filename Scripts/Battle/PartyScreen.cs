using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    PokemonParty party;
    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];

    public BattleState? calledFrom {get; set;}

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = PokemonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = party.Pokemons;

        for(int i = 0; i < memberSlots.Length; i++)
        {
            if(i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {

        var prevSelection = selection;
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.D))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.A))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.S))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;
        else if (Input.GetKeyDown(KeyCode.W))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, pokemons.Count -1);

        if(selection != prevSelection)
            UpdateMemberSelection(selection);

         if(Input.GetKeyDown(KeyCode.Space))
         {
          onSelected?.Invoke(); 
         }
         else if (Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
        }
    }
    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if(i == selectedMember)
                memberSlots[i].SetSelected(true);
        
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void ShowIfTMIsUsable(TMItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i])? "ABLE" : "NOT ABLE";
            memberSlots[i].SetMessage(message);
        }
    }
    public void ClearMemberSlotMessage()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
