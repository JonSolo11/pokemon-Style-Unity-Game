using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Text categoryText;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedItem = 0;
    int selectedCategory= 0;

    private bool categoryChanged = false;

    List<SellableItems> sellableItemsList;

    Action<ItemBase> onItemSelected;
    Action onBack;
    List<ItemSlotUI> slotUIList;

    Dictionary<string, List<ItemBase>> itemsByCategory;
    const int itemsInViewport = 8;

    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<SellableItems> sellableItemsList, Action<ItemBase> onItemSelected, Action onBack)
    {
        this.sellableItemsList = sellableItemsList;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        selectedCategory = 0;
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSel = selectedItem;
        int prevCategory = selectedCategory;

        if(Input.GetKeyDown(KeyCode.W))
            --selectedItem;
        else if(Input.GetKeyDown(KeyCode.S))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ++selectedCategory;
            selectedItem = 0;  // Reset selected item to 0
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            --selectedCategory;
            selectedItem = 0;  // Reset selected item to 0
        }

        if (selectedCategory >= sellableItemsList.Count) 
            selectedCategory = 0;
        else if (selectedCategory < 0) 
            selectedCategory = sellableItemsList.Count - 1;

        selectedCategory = Mathf.Clamp(selectedCategory, 0, sellableItemsList.Count - 1);

        categoryText.text = sellableItemsList[selectedCategory].Category; // Update category text

        SellableItems currentCategory = sellableItemsList[selectedCategory];
            
        // Clamp the selected item between 0 and the count of items in the selected category - 1
        selectedItem = Mathf.Clamp(selectedItem, 0, currentCategory.Items.Count - 1);

        if(prevCategory != selectedCategory)
        {
            UpdateItemList();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ItemBase selectedItemBase = sellableItemsList[selectedCategory].Items[selectedItem];
            onItemSelected?.Invoke(selectedItemBase);
        }
        else if(Input.GetKeyDown(KeyCode.Z)) 
        onBack?.Invoke();

        
        // Always update item selection
        UpdateItemSelection();
    }

    void UpdateItemList()
    {
        // clear existing items
        foreach(Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        // Get availableItems for current category
        List<ItemBase> availableItems = sellableItemsList[selectedCategory].Items;

        foreach(var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);
            slotUIList.Add(slotUIObj);
        }

        // Always reset the scrolling to the top when the category changes
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, 0);

        UpdateItemSelection();
    }

    void UpdateItemSelection()
    {
        // Get availableItems for current category
        List<ItemBase> availableItems = sellableItemsList[selectedCategory].Items;

        if(availableItems.Count > 0)
        {
            selectedItem = Mathf.Clamp(selectedItem, 0 ,availableItems.Count - 1);

            for(int i = 0; i < slotUIList.Count; i++)
            {
                if(i == selectedItem)
                {
                    slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
                }
                else
                {
                    slotUIList[i].NameText.color = Color.black;
                }
            }

            var item = availableItems[selectedItem];  // Reference the item from the sorted list
            //itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
            DialogManager.Instance.ShowDialogText($"{item.Description}");
            
            HandleScrolling();
        }
    }


    void HandleScrolling()
    {

        if(slotUIList.Count <= itemsInViewport) return;
        
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport/2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport/2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}