using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Items/Create new TM - HM")]

public class TMItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] string itemName = null;
    [SerializeField] bool isHM;

    public void SetMove(MoveBase move)
    {
        this.move = move;
    }
    public void SetItemName(string itemName)
    {
        this.itemName = itemName;
    }

    public void SetIsHM(bool isHM)
    {
        this.isHM = isHM;
    }
    public void SetIsSellable()
    {
        this.isSellable = false;
    }


    public override string Name => $"{itemName}: {move.Name}";

    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public string ItemName => itemName;
    public bool IsHM => isHM;

     public override bool Use(Pokemon pokemon)
    {
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }
}
