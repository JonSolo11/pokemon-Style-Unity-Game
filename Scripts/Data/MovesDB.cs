using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovesDB
{
    public static Dictionary<string, MoveBase> moves;

    public static IEnumerator Init(MoveFetcher moveFetcher)
    {
        moves = new Dictionary<string, MoveBase>();

        // Fetch moves until there are 165
        while(moves.Count != 165){

            var moveArray = Resources.LoadAll<MoveBase>("");

            foreach (var move in moveArray)
            {
                if (moves.ContainsKey(move.Name))
                {
                    continue;
                }

                moves[move.Name] = move;
            }

            yield return moveFetcher.Init();
        }
        
    }
    public static MoveBase GetMoveByName(string name)
    {
        string formattedName = name.Replace(" ", "-");
        if (!moves.ContainsKey(name))
        {
            Debug.Log($"{name} not found");
            return null;
        }
        return moves[name];
    }

}

