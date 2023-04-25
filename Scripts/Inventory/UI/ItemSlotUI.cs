using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;
    [SerializeField] Image itemIcon;

    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public Text NameText => nameText;
    public Text CountText => countText;

    public float Height => rectTransform.rect.height;
    public float Width => rectTransform.rect.width;

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"x {itemSlot.Count}";
        itemIcon.sprite = itemSlot.Item.Icon;
    }
}
