using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver}

public enum BattleAction { Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleEnd;

    BattleState state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
    MoveBase moveToLearn;

    int escapeAttempts;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if(!isTrainerBattle)
        {
            //wild pokemon 
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

            yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
            yield return new WaitForSeconds(1f);

        }
        else
        {
            //trainer battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogueBox.TypeDialogue($"{trainer.Name} wants to battle!");
            yield return new WaitForSeconds(1f);

            //send out trainer pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);

            yield return dialogueBox.TypeDialogue($"{trainer.Name} sent out {enemyPokemon.Base.Name}");
            yield return new WaitForSeconds(1f);

            //send out player pokemon
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogueBox.TypeDialogue($"Go {playerPokemon.Base.Name}!");
            yield return new WaitForSeconds(1f);
            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p=>p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleEnd(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogueBox.SetDialogue("Choose an action");
        dialogueBox.EnableActionSelector(true);

    }

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.calledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.SetMessageText("Choose Next Pokemon");
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
        
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogueBox.TypeDialogue($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to switch Pokemon?");

        state = BattleState.AboutToUse;
        dialogueBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogueBox.TypeDialogue($"Choose a move to forget?");

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        dialogueBox.EnableActionSelector(false);
        state = BattleState.RunningTurn;
        bool playerGoesFirst = true;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
            {

                //Check poke move priority. Assumed player goes first unless other conditions
                if(playerUnit.Pokemon.Speed < enemyUnit.Pokemon.Speed)
                {
                    playerGoesFirst = false;
                }
                else if(playerUnit.Pokemon.Speed < enemyUnit.Pokemon.Speed)
                {
                    Debug.Log("Speed tie");
                    var num = UnityEngine.Random.Range(1,3);
                    if(num % 2 != 0){
                        playerGoesFirst = false;
                    }
                }
            }

            var firstUnit = (playerGoesFirst)? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst)? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // first move
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if(state == BattleState.BattleOver) yield break;

            if(secondPokemon.HP > 0){
                // second move
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if(state == BattleState.BattleOver) yield break;
            }
        }
        else{
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if(playerAction == BattleAction.UseItem)
            {
                //item using handled from item screen
                dialogueBox.EnableActionSelector(false);
            }
            else if(playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var EnemyMove = enemyUnit.Pokemon.GetRandomMove();

            yield return RunMove(enemyUnit, playerUnit, EnemyMove);
            yield return RunAfterTurn(enemyUnit);
            if(state == BattleState.BattleOver) yield break;
        }
        if(state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove =sourceUnit.Pokemon.OnBeforeMove();

        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit);

        move.PP--;
        if(sourceUnit.IsPlayerUnit)
            yield return dialogueBox.TypeDialogue($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");
        else
            yield return dialogueBox.TypeDialogue($"Enemy {sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if(CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation(targetUnit);

            yield return new WaitForSeconds(1f);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit, targetUnit, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }
            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    if(UnityEngine.Random.Range(1,101) <= secondary.Chance)
                    {
                        Debug.Log(secondary.Chance);
                        yield return RunMoveEffects(secondary, sourceUnit, targetUnit, secondary.Target);
                    }
                }
            }

            if(targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue($"The attack missed!");
        }

    }

    IEnumerator RunMoveEffects(MoveEffects effects, BattleUnit source, BattleUnit target, MoveTarget moveTarget)
    {
        Debug.Log("reached Secondaries");
        //Stat changes
            if(effects.Boosts != null)
            
            {Debug.Log("reached Boosts");
                if(moveTarget == MoveTarget.Self)
                    source.Pokemon.ApplyBoost(effects.Boosts);
                else
                    target.Pokemon.ApplyBoost(effects.Boosts);
            }

        //Status Conditions
            if(effects.Status != ConditionID.none)
            
            {Debug.Log("reached Status Conditions");
                if(moveTarget == MoveTarget.Self)
                    source.Pokemon.SetStatus(effects.Status);
                else
                    target.Pokemon.SetStatus(effects.Status);
            }
        //volatile status

            if(effects.VolatileStatus != ConditionID.none)
            
            {Debug.Log("volatile status");
                if(moveTarget == MoveTarget.Self)
                    source.Pokemon.SetVolatileStatus(effects.VolatileStatus);
                else
                    target.Pokemon.SetVolatileStatus(effects.VolatileStatus);
            }       
            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {

        if(state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if(sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if(move.Base.AlwaysHits)
        {
            return true;
        }
        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] {1f, 4f/3f, 5f/3f, 2f, 7f/3f, 8f/3f, 3f};

        if(accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if(evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return  UnityEngine.Random.Range(1,101) <= moveAccuracy;

    }

    IEnumerator ShowStatusChanges(BattleUnit sourceUnit)
    {
        while (sourceUnit.Pokemon.StatusChanges.Count > 0)
        if(sourceUnit.IsPlayerUnit)
        {
            var message = sourceUnit.Pokemon.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
        else
        {
            var message = sourceUnit.Pokemon.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue("Enemy " + message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogueBox.TypeDialogue($"Enemy {faintedUnit.Pokemon.Base.Name} fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if(!faintedUnit.IsPlayerUnit)
        {
            //Exp gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;

            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} gained {expGain} experience!");
            yield return playerUnit.Hud.SetExpSmooth();
            
            //check level up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}!");

                //New move check
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();

                if(newMove != null)
                {
                    if( playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove.Base);
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}!");
                        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogueBox.TypeDialogue($"But it can't learn more than {PokemonBase.MaxNumOfMoves} moves...");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    public void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else    
                BattleOver(false);
        }
        else
        {
            if(!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if(nextPokemon != null)
                    StartCoroutine(AboutToUse(nextPokemon));
                else    
                    BattleOver(true);
            }
        }
        
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogueBox.TypeDialogue("Critical Hit!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogueBox.TypeDialogue("It's Super Effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogueBox.TypeDialogue("It's Not Very Effective...");
        }

    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if(state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };
            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };
            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if(state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if(state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) => 
            {
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //dont learn new move
                   StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    //forget a move and learn new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;

                   StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));
                    
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }


    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.D))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.S))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentAction -= 2;

        currentAction= Mathf.Clamp(currentAction, 0, 3);

        dialogueBox.UpdateActionSelection(currentAction);

         if (Input.GetKeyDown(KeyCode.Space)) 
         {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
         
            else if( currentAction ==1)
            {
                //Bag
                OpenBag();
            }
            else if( currentAction ==2)
            {
                //Party
                OpenPartyScreen();
            }
            else if( currentAction ==3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
         }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.D))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.S))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentMove -= 2;

        currentMove= Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count -1);
        
        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(playerUnit.Pokemon.Moves[currentMove].PP == 0) return;
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("Pokemon is too tired for battle!");
                return;
            }
            if(selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"{playerUnit.Pokemon.Base.Name} is already out!");
                return;
            }
            partyScreen.gameObject.SetActive(false);

            if(partyScreen.calledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                //remove if issues
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.calledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember,isTrainerAboutToUse));
            }
            partyScreen.calledFrom = null;
        };

        Action onBack = () =>
        {
            if(playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You must choose a Pokemon!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if(partyScreen.calledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();
            
            partyScreen.calledFrom = null;
        };
        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        {
            aboutToUseChoice = !aboutToUseChoice;

            dialogueBox.UpdateChoiceBox(aboutToUseChoice);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueBox.EnableChoiceBox(false);
            if(aboutToUseChoice == true)
            {
                OpenPartyScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if(playerUnit.Pokemon.HP > 0){
            yield return dialogueBox.TypeDialogue($"Good Work {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.Setup(newPokemon);

        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");
        yield return new WaitForSeconds(1f);

        if(isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(nextPokemon);
        yield return dialogueBox.TypeDialogue($"{trainer.Name} sent out {nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;

    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);

                if(usedItem is PokeballItem)
                {
                    yield return ThrowPokeball((PokeballItem)usedItem);
                }

                StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {

        if (isTrainerBattle)
        {
            yield return dialogueBox.TypeDialogue($"You can't steal a trainers pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }
        state = BattleState.Busy;

        yield return dialogueBox.TypeDialogue($"{player.Name} used {pokeballItem.Name}!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        //Animations

        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 2.3f, 0.43f).WaitForCompletion();
        yield return pokeball.transform.DOJump(pokeball.transform.position, .6f, 1, .45f).WaitForCompletion();
        yield return pokeball.transform.DOJump(pokeball.transform.position, .3f, 1, .43f).WaitForCompletion();
        yield return pokeball.transform.DOJump(pokeball.transform.position, .15f, 1, .43f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        for(int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0,0,10f), 0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            //poke caught
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} has been added to your party");

            Destroy(pokeball);
            BattleOver(true);
        }
        else{
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, .2f);
            yield return enemyUnit.PlayBreakoutAnimation();

            if(shakeCount < 2)
                yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Base.Name} broke free!");
            else
                yield return dialogueBox.TypeDialogue($"Almost had it!");
            
            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeballItem.CatchRateModifier * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHP);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;

        while (shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++ shakeCount;
        }

        return shakeCount;

    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if(isTrainerBattle)
        {
            yield return dialogueBox.TypeDialogue($"You can't run from a trainer battle!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(playerSpeed > enemySpeed)
        {
            yield return dialogueBox.TypeDialogue($"Got away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f% 256;

            if(UnityEngine.Random.Range(0, 255) < f)
            {
                yield return dialogueBox.TypeDialogue($"Got away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
