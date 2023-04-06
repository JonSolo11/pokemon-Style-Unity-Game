using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] public string pokemonName;

    [TextArea]
    [SerializeField] public string description;

    [SerializeField] public Sprite frontSprite;
    [SerializeField] public Sprite backSprite;

    [SerializeField] public PokemonType type1;
    [SerializeField] public PokemonType type2;

    //Base Stats
    [SerializeField] public int maxHP;
    [SerializeField] public int attack;
    [SerializeField] public int defense;
    [SerializeField] public int spAttack;
    [SerializeField] public int spDefense;
    [SerializeField] public int speed;

    [SerializeField] public List<LearnableMove> LearnableMove;
    

    public string Name{
        get {return name;}
    }

    public string Description{
        get {return description;}
    }

    public Sprite FrontSprite{
        get {return frontSprite;}
    }
    public Sprite BackSprite{
        get {return backSprite;}
    }
    public PokemonType Type1{
        get {return type1;}
    }
    public PokemonType Type2{
        get {return type2;}
    }
    public int MaxHP{
        get {return maxHP;}
    }
    public int Attack{
        get {return attack;}
    }
    public int Defense{
        get {return defense;}
    }
    public int SpAttack{
        get {return spAttack;}
    }
    public int SpDefense{
        get {return spDefense;}
    }
    public int Speed{
        get {return speed;}
    }
    public List<LearnableMove> LearnableMoves{
        get {return LearnableMove;}
    }
    public PokemonBase()
    {
        LearnableMove = new List<LearnableMove>();
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] public MoveBase moveBase;
    [SerializeField] public int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }

    public LearnableMove(MoveBase moveBase, int level)
    {
        this.moveBase = moveBase;
        this.level = level;
    }
}


public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    //Not actual stats
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float [][] chart = 
    {
        //                   nor|Fir|wat|ele|grs|ice|fig|poi|grd|fly|psy|bug|roc|gst|drg|
        /*Nor*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 0f, 1f},
        /*Fir*/ new float[] { 1f,.5f,.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f,.5f, 1f,.5f},
        /*Wat*/ new float[] { 1f, 2f,.5f, 0f,.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f,.5f},
        /*Ele*/ new float[] { 1f, 1f, 2f,.5f,.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f,.5f},
        /*Grs*/ new float[] { 1f,.5f, 2f, 1f,.5f, 1f, 1f,.5f, 2f,.5f, 1f,.5f, 2f, 1f,.5f},
        /*Ice*/ new float[] { 1f,.5f,.5f, 1f, 2f,.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f},
        /*Fig*/ new float[] { 2f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 1f,.5f,.5f,.5f, 2f, 0f, 1f},
        /*Poi*/ new float[] { 1f, 1f, 1f, 1f, 2f, 1f, 1f,.5f,.5f, 1f, 1f, 1f,.5f,.5f, 1f},
        /*Grd*/ new float[] { 1f, 2f, 1f, 2f,.5f, 1f, 1f, 2f, 1f, 0f, 1f,.5f, 2f, 1f, 1f},
        /*Fly*/ new float[] { 1f, 1f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f,.5f, 1f, 1f},
        /*Psy*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f,.5f, 1f, 1f, 1f, 1f},
        /*Bug*/ new float[] { 1f,.5f, 1f, 1f, 2f, 1f,.5f,.5f, 1f,.5f, 2f, 1f, 1f,.5f, 1f},
        /*Roc*/ new float[] { 1f, 2f, 1f, 1f, 1f, 2f,.5f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f},
        /*Gst*/ new float[] { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f},
        /*Drg*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f},
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if(attackType == PokemonType.None || defenseType == PokemonType.None )
            return 1;
        int row = (int)attackType -1;
        int col = (int)defenseType -1;

        return chart[row][col];
    }
    
}
