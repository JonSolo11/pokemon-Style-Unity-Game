using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM - HM")]

public class TMItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public void SetMove(MoveBase move)
    {
        this.move = move;
    }

    public void SetIsHM(bool isHM)
    {
        this.isHM = isHM;
    }


    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon pokemon)
    {
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;

    public bool IsHM => isHM;
}
