using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] int xDir;
    [SerializeField] int yDir; 

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public bool TryJump(Character character, Vector2 moveDir)
    {
        if(moveDir.x == xDir && moveDir.y == yDir)
        {

            StartCoroutine(Jump(character));
            return true;
        }

        return false;
    }

    IEnumerator Jump(Character character)
    {
        GameController.Instance.PauseGame(true); //pauses game during animation
        character.Animator.IsJumping = true;

        var jumpDest = character.transform.position + new Vector3(xDir,yDir)* 2; // jumps 2 tiles
        yield return character.transform.DOJump(jumpDest, 1f, 1, .53f).WaitForCompletion();

        character.Animator.IsJumping = false;
        GameController.Instance.PauseGame(false);
    }
}
