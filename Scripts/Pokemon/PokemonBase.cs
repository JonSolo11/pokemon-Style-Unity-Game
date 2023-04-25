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
    [SerializeField] public int expYield;
    [SerializeField] public GrowthRate growthRate;

    [SerializeField] public int catchRate = 255;

    [SerializeField] public List<LearnableMove> LearnableMove;
    [SerializeField] public List<MoveBase> LearnableByItems;

    public static int MaxNumOfMoves {get; set;} = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.Medium)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.Mediumslow)
        {
            return (6 * (level * level * level) / 5) - (15 * (level * level)) + (100 * level) - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Erratic)
        {
            if (level <= 50)
            {
                return (level * level * level * (100 - level)) / 50;
            }
            else if (level <= 68)
            {
                return (level * level * level * (150 - level)) / 100;
            }
            else if (level <= 98)
            {
                return (level * level * level * ((1911 - 10 * level) / 3)) / 500;
            }
            else
            {
                return (level * level * level * (160 - level)) / 100;
            }
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            if (level <= 15)
            {
                return (level * level * level * ((level + 1) / 3 + 24)) / 50;
            }
            else if (level <= 36)
            {
                return (level * level * level * (level + 14)) / 50;
            }
            else
            {
                return (level * level * level * (level / 2 + 32)) / 50;
            }
        }
        else
        {
            return -1;
        }
    }
    

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

    public List<MoveBase> learnableByItems
    {
        get {return LearnableByItems;}
    }

    public PokemonBase()
    {
        LearnableMove = new List<LearnableMove>();
        LearnableByItems = new List<MoveBase>();
    }

    public int CatchRate{
        get {return catchRate;}
    }

    public int ExpYield{
        get {return expYield;}
    }

    public GrowthRate GrowthRate{
        get {return growthRate;}
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

public enum GrowthRate
{
    Fast, Medium, Mediumslow, Slow, Erratic, Fluctuating
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
        //                   nor|Fir|wat|ele|grs|ice|fig|poi|grd|fly|psy|bug|roc|gst|drg|drk|ste|fai
        /*Nor*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 0f, 1f, 1f,.5f, 1f},
        /*Fir*/ new float[] { 1f,.5f,.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f,.5f, 1f,.5f, 1f, 2f, 1f},
        /*Wat*/ new float[] { 1f, 2f,.5f, 0f,.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f,.5f, 1f, 1f, 1f},
        /*Ele*/ new float[] { 1f, 1f, 2f,.5f,.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f,.5f, 1f, 1f, 1f},
        /*Grs*/ new float[] { 1f,.5f, 2f, 1f,.5f, 1f, 1f,.5f, 2f,.5f, 1f,.5f, 2f, 1f,.5f, 1f,.5f, 1f},
        /*Ice*/ new float[] { 1f,.5f,.5f, 1f, 2f,.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 1f},
        /*Fig*/ new float[] { 2f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 1f,.5f,.5f,.5f, 2f, 0f, 1f, 2f, 2f,.5f},
        /*Poi*/ new float[] { 1f, 1f, 1f, 1f, 2f, 1f, 1f,.5f,.5f, 1f, 1f, 1f,.5f,.5f, 1f, 1f, 0f, 2f},
        /*Grd*/ new float[] { 1f, 2f, 1f, 2f,.5f, 1f, 1f, 2f, 1f, 0f, 1f,.5f, 2f, 1f, 1f, 1f, 2f, 1f},
        /*Fly*/ new float[] { 1f, 1f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f,.5f, 1f, 1f, 1f,.5f, 1f},
        /*Psy*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f,.5f, 1f, 1f, 1f, 1f, 0f,.5f, 1f},
        /*Bug*/ new float[] { 1f,.5f, 1f, 1f, 2f, 1f,.5f,.5f, 1f,.5f, 2f, 1f, 1f,.5f, 1f, 2f,.5f,.5f},
        /*Roc*/ new float[] { 1f, 2f, 1f, 1f, 1f, 2f,.5f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f,.5f, 1f},
        /*Gst*/ new float[] { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f,.5f, 1f, 1f},
        /*Drg*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 0f},
        /*Drk*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 1f, 1f, 2f, 1f, 1f, 2f, 1f,.5f, 1f,.5f},
        /*Ste*/ new float[] { 1f,.5f,.5f,.5f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f,.5f, 2f},
        /*Fai*/ new float[] { 1f,.5f, 1f, 1f, 1f, 1f, 2f,.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f,.5f, 1f},
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
