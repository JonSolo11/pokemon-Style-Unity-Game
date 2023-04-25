using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected; 
    public event Action onBack; 
    GameController gameController;

    List<Text> menuItems;

    int selectedItem = 0;

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
        gameController = GetComponent<GameController>();
    }
    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }
    public void CloseMenu()
    {
        menu.SetActive(false);

    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;
        if(Input.GetKeyDown(KeyCode.S))
            ++selectedItem;
        else if(Input.GetKeyDown(KeyCode.W))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count -1);

        if (prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            onMenuSelected?.Invoke(selectedItem);

        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for(int i = 0; i < menuItems.Count; i++)
        {
            if(i == selectedItem)
            {
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                menuItems[i].color = Color.black;
            }
        }
    }
}
