using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color psnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color frzColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHp((float) pokemon.HP / pokemon.MaxHP);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.frz, frzColor},
            {ConditionID.par, parColor},
        };
        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
        
    }

    void SetStatusText()
    {
        Debug.Log("Setting status text");
        if(_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
            Debug.Log($"Status: {_pokemon.Status.Id}, Color: {statusText.color}");
        }
    }

    public IEnumerator UpdateHP()
    {
        if(_pokemon.HpChanged){
            yield return hpBar.SetHPSmooth((float) _pokemon.HP / _pokemon.MaxHP);
            _pokemon.HpChanged = false;
        }
    }
}
