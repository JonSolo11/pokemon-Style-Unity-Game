using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState {ItemSelection, PartySelection, MoveToForget, Busy}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Text categoryText;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory= 0;

    MoveBase moveToLearn;

    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;

    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        //

        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        // clear existing items
        foreach(Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        foreach(var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }
    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;
        categoryText.text = Inventory.itemCategories[selectedCategory];

        if(state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if(Input.GetKeyDown(KeyCode.S))
                ++selectedItem;
            else if(Input.GetKeyDown(KeyCode.W))
                --selectedItem;
            else if(Input.GetKeyDown(KeyCode.D))
                ++selectedCategory;
            else if(Input.GetKeyDown(KeyCode.A))
                --selectedCategory;

            if(selectedCategory > Inventory.itemCategories.Count - 1)
                selectedCategory = 0;
            else if(selectedCategory < 0)
                selectedCategory = Inventory.itemCategories.Count - 1;

            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.itemCategories[selectedCategory];
                UpdateItemList();
            }

            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(ItemSelected());
            }

            else if(Input.GetKeyDown(KeyCode.Z))
            {
                onBack?.Invoke();
            }
        }

        else if(state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected,onBackPartyScreen);
        }
        else if(state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
      
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if(GameController.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }

        if(GameController.Instance.State == GameState.Battle)
        {
            //in battle
            if(!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"this item can't be used in battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // outside battle
            if(!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"this item can't be used outside of battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
    
        if(selectedCategory == (int)ItemCategory.POKEBALLS)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();

            if(item is TMItem)
                partyScreen.ShowIfTMIsUsable(item as TMItem);
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTMItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var pokemon = partyScreen.SelectedMember;

        //Handles Evolution Items
        if(item is EvolutionItem)
        {
            var evolution = pokemon.CheckForEvolution(item);
            if(evolution != null)
            {
                yield return EvolutionManager.i.Evolve(pokemon, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect...");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, pokemon, selectedCategory);
        if(usedItem != null)
        {
            if(usedItem is RecoveryItem)
                yield return DialogManager.Instance.ShowDialogText($"{usedItem.Name} used on {pokemon.Base.Name}");
            onItemUsed?.Invoke(usedItem);
        }
        else 
        {
            if(selectedCategory == (int)ItemCategory.ITEMS)
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect...");
        }

        ClosePartyScreen();

    }

    IEnumerator HandleTMItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TMItem;

        if(tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} already knows {tmItem.Move.Name}!");
            yield break;
        }

        if(!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} can't learn {tmItem.Move.Name}!");
            yield break;
        }

        if(pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} learned {tmItem.Move.Name}!");
        }
        else
            {
                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {tmItem.Move.Name}");
                yield return DialogManager.Instance.ShowDialogText($"But it can't learn more than {PokemonBase.MaxNumOfMoves} moves...");
                yield return DialogManager.Instance.ShowDialogText($"Delete an older move to make room for {tmItem.Move.Name}?");
                ChooseMoveToForget(pokemon, tmItem.Move);
                yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
            }
    }

    void ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;

    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0 ,slots.Count - 1);

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

        if(slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            //itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
            DialogManager.Instance.ShowDialogText($"{item.Description}");
        }
        

        HandleScrolling();
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

    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        //itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.ClearMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        DialogManager.Instance.CloseDialog();
        var pokemon = partyScreen.SelectedMember;
        moveSelectionUI.gameObject.SetActive(false);
        if(moveIndex == PokemonBase.MaxNumOfMoves)
        {
            //dont learn new move
            yield return StartCoroutine(DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} did not learn {moveToLearn.Name}"));
        }
        else
        {
            //forget a move and learn new move
            var selectedMove = pokemon.Moves[moveIndex].Base;

            yield return StartCoroutine(DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));
            
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
