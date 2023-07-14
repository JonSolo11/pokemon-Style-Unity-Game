using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] string category;
    [SerializeField] public Sprite icon;
    [SerializeField] float price;
    [SerializeField] public bool isSellable = true;

    public virtual string Name => name;
    public string Description => description;
    public string Category => category;
    public Sprite Icon => icon;

    public float Price => price;

    public void SetItemValues(string newName, string newDescription, string newCategory, Sprite newIcon, float newPrice, bool newIsSellable = true)
    {
        name = newName;
        description = newDescription;
        category = newCategory;
        icon = newIcon;
        price = newPrice;
        isSellable = newIsSellable;
    }


    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }

    public virtual bool IsReusable => false;
    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;

}
