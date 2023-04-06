using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit {
        get {return isPlayerUnit;}
    }
    public BattleHud Hud {
        get {return hud;}
    }

    public Pokemon Pokemon {get; set;}

    Image image;
    Color originalColor;
    Vector3 originalPos;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-522f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(522f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1.5f);
    }

    public void PlayAttackAnimation(BattleUnit targetUnit)
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
        sequence.AppendCallback(() => targetUnit.PlayHitAnimation());
    }

    public void PlayHitAnimation()
    {
        Color originalColor = image.color;

        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        if (!isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 15f, 0.05f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 15f, 0.05f));

        sequence.Append(image.DOColor(originalColor, 0.1f));
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.05f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y-150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
    public void PlaySwitchAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.DOLocalMoveX(-522f, .75f);
        }
        else
        {
            image.transform.DOLocalMoveX(522f, .75f);
        }
    }
}
