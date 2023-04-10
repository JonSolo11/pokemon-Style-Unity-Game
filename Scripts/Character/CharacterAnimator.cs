using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]List<Sprite> walkDownSprites;
    [SerializeField]List<Sprite> walkUpSprites;
    [SerializeField]List<Sprite> walkLeftSprites;
    [SerializeField]List<Sprite> walkRightSprites;
    [SerializeField]Facingdirection defaultDirection = Facingdirection.Down;

    //Parameters
    public float MoveX{get; set;}
    public float MoveY{get; set;}
    public bool IsMoving{get; set;}

    //States

    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;

    SpriteAnimator currentAnim;
    bool wasMoving;

    //References

    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim= new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim= new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim= new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim= new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
            currentAnim = walkRightAnim;
        else if (MoveX == -1)
            currentAnim = walkLeftAnim;
        else if (MoveY == 1)
            currentAnim = walkUpAnim;
        else if (MoveY == -1)
            currentAnim = walkDownAnim;
        
        if(currentAnim != prevAnim || IsMoving != wasMoving)
            currentAnim.Start();


        if(IsMoving)
            currentAnim.HandleUpdate();
        else
            spriteRenderer.sprite = currentAnim.Frames[0];
        
        wasMoving = IsMoving;
    }

    public void SetFacingDirection(Facingdirection dir)
    {
        if(dir == Facingdirection.Right)
            MoveX = 1;
        else if(dir == Facingdirection.Left)
            MoveX = -1;
        else if(dir == Facingdirection.Up)
            MoveY = -1;
        else if(dir == Facingdirection.Down)
            MoveY = -1;

    }

    public Facingdirection DefaultDirection{
        get => defaultDirection;
    }
}

public enum Facingdirection {Up, Down, Left, Right}