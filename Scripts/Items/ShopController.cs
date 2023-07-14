using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ShopState{Menu, Buying, Selling, Busy}

public class ShopController : MonoBehaviour
{

    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;

    public event Action OnStart;
    public event Action OnFinish;
    ShopState state;
    public static ShopController i {get; private set;}

    PlayerController player;

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    public void Start()
    {
        inventory = Inventory.GetInventory();
        player = GetComponent<PlayerController>();
    }
    public IEnumerator StartTrading(Merchant merchant)
    {
        OnStart?.Invoke();
        yield return StartMenuState();
    }

    IEnumerator StartMenuState()
    {

        state = ShopState.Menu;
        
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Hi there! How may I help you?",
        waitForInput: false,
        choices: new List<string>() {"Buy", "Sell", "Leave"},
        onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
            //BUY
        {
            
        }
        else if(selectedChoice == 1)
        {
            //Sell
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if(selectedChoice == 2)
        {
            //LEAVE
            walletUI.Close();
            OnFinish.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if(state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
            walletUI.Show();
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        if(!item.isSellable)
        {
           yield return DialogManager.Instance.ShowDialogText("I'm sorry, but I can't buy that.");
           state = ShopState.Selling;
           yield break;
        }

        float sellingPrice = Mathf.Round(item.Price / 2);

         int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"I can give you {sellingPrice} for that. \n Sell item(s)?",
        waitForInput: false,
        choices: new List<string>() {"Yes", "No"},
        onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
        {
            //Yes
            inventory.RemoveItem(item);
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"Received {sellingPrice}");
        }

        state = ShopState.Selling;
    }
}
