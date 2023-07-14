using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//teleports player without switching scenes

public class LocationPortal : MonoBehaviour,IPlayerTriggerable
{
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }
    IEnumerator Teleport()
    {
        Debug.Log("Fader: " + fader);
        Debug.Log("Player: " + player);
        Debug.Log("DestinationPortal: " + destinationPortal);
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint;

    public bool triggerRepeatedly => false;
}
