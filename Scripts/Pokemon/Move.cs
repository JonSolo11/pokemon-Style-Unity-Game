using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base {get; set;}

    public int PP{get; set;}

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MovesDB.GetMoveByName(saveData.name);
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData= new MoveSaveData()
        {
            name = Base.Name,
            pp = PP,
        };
        return saveData;
    }

    public void IncreasePP(int amt)
    {
        PP = Mathf.Clamp(PP + amt, 0, Base.PP);
    }
}

[Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
