using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string trainerName;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    bool battleLost = false;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if(!battleLost)
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }

    public void BattleLost()
    {
        fov.gameObject.SetActive(false);
        battleLost = true;
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
        yield return DialogManager.Instance.ShowDialog(dialog);

        GameController.Instance.StartTrainerBattle(this);

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

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool) state;
        if(battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
