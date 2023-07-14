using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]

public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] public int hpAmount;
    [SerializeField] public bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] public int ppAmount;
    [SerializeField] public bool restoreMaxPP;

    [Header("Status")]
    [SerializeField] public ConditionID status;
    [SerializeField] public bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] public bool revive;
    [SerializeField] public bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        //revive
        if(revive || maxRevive)
        {
            if(pokemon.HP > 0)
            {
                return false;
            }
            if(revive)
                pokemon.IncreaseHP(pokemon.MaxHP / 2);
            else if (maxRevive)
                pokemon.IncreaseHP(pokemon.MaxHP);

            pokemon.CureStatus();

            return true;
        }

        //other items can't be used on fainted pokemon
        if(pokemon.HP == 0)
            return false;

        //potions
        if(restoreMaxHP || hpAmount > 0)
        {
            if(pokemon.HP == pokemon.MaxHP)
            {
                return false;
            }

            if (restoreMaxHP == true)
            {
                pokemon.IncreaseHP(pokemon.MaxHP);
            }
            else
            {
                pokemon.IncreaseHP(hpAmount);
            }
        }

        //status recovery
        if(recoverAllStatus || status != ConditionID.none)
        {
            if(pokemon.Status == null && pokemon.VolatileStatus == null)
                return false;
            if(recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if(pokemon.Status.Id == status)
                    pokemon.CureStatus();
                else if(pokemon.VolatileStatus.Id == status)
                    pokemon.CureStatus();
                else return false;
            }

        }

        //pp restoration
        if(restoreMaxPP)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
