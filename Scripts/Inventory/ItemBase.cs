using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] string category;
    [SerializeField] public Sprite icon;

    public virtual string Name => name;
    public string Description => description;
    public string Category => category;
    public Sprite Icon => icon;

    public void SetItemValues(string newName, string newDescription, string newCategory, Sprite newIcon)
    {
        name = newName;
        description = newDescription;
        category = newCategory;
        icon = newIcon;
    }


    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }

    public virtual bool IsReusable => false;
    public virtual bool CanUseInBattle => true;
    public virtual bool CanUseOutsideBattle => true;

}
