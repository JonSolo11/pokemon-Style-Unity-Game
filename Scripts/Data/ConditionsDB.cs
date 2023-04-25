using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<string, ConditionID> ailmentNameToID = new Dictionary<string, ConditionID>()
    {
        { "poison", ConditionID.psn },
        { "burn", ConditionID.brn },
        { "paralysis", ConditionID.par },
        { "freeze", ConditionID.frz },
        { "sleep", ConditionID.slp },
        { "confusion", ConditionID.confusion }
    };


   public static Dictionary<ConditionID, Condition> Conditions {get; set;} = new Dictionary<ConditionID, Condition>()
   {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "poison",
                StartMessage = "is poisoned!",
                OnAfterTurn = (Pokemon pokemon) => 
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by poison...");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "burn",
                StartMessage = "is burned!",
                OnAfterTurn = (Pokemon pokemon) => 
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by burn...");
                }
            }
            
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "paralyzed",
                StartMessage = "is paralyzed!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5)==1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed and can't move");
                        return false;
                    }
                    return true;
                }
            }
            
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "freeze",
                StartMessage = "is Frozen Solid!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5)==1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} thawed and is no longer Frozen");
                        return true;
                    }
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is Frozen Solid!");
                    return false;
                }
            }
            
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "sleep",
                StartMessage = "Fell Asleep",
                OnStart = (Pokemon pokemon) =>
                {
                    //sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will sleep for {pokemon.StatusTime} turns.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} Woke Up!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is Fast Asleep");
                    return false;
                }
            }
            
        },

        //volatile status conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "confusion",
                StartMessage = "is Confused",
                OnStart = (Pokemon pokemon) =>
                {
                    //confused for 1-4 turns
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} turns.");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is Confused");
                    
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} Snapped Out of Confusion!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;

                    if(Random.Range(1,3)==1)
                        return true;

                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} Hurt Itself In Confusion");
                    return false;
                }
            }
            
        }
   };

   public static float GetStatusBonus(Condition condition)
   {
        if(condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;
        return 1F;
            
   }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
