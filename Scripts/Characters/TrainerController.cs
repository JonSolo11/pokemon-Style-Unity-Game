using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(1f);
        exclamation.SetActive(false);

        //walk towards player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //show dialogue
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => 
        {
            Debug.Log("Starting battle");
        }));
    }

    public void SetFovRotation(Facingdirection dir)
    {
        float angle = 0;
        if(dir == Facingdirection.Right)
        {
            angle = 90f;
        }
        else if(dir == Facingdirection.Up)
        {
            angle = 180f;
        }
        else if(dir == Facingdirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
