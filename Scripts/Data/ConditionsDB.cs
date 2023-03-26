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

   public static Dictionary<ConditionID, Condition> Conditions {get; set;} = new Dictionary<ConditionID, Condition>()
   {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "is poisoned!",
                OnAfterTurn = (Pokemon pokemon) => 
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by poison...");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "is burned!",
                OnAfterTurn = (Pokemon pokemon) => 
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by burn...");
                }
            }
            
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
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
                Name = "Freeze",
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
                Name = "Sleep",
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
                Name = "Confusion",
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

                    pokemon.UpdateHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} Hurt Itself In Confusion");
                    return false;
                }
            }
            
        }
   };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
