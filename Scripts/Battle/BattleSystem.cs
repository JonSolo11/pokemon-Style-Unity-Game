using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}

public enum BattleAction { Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
     

    public event Action<bool> OnBattleEnd;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        yield return new WaitForSeconds(1f);

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p=>p.OnBattleOver());
        OnBattleEnd(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogueBox.SetDialogue("Choose an action");
        dialogueBox.EnableActionSelector(true);

    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetMessageText("Choose Next Pokemon");
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
        
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
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
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
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
            yield return sourceUnit.Hud.UpdateHP();
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
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
                yield return new WaitForSeconds(0.5f);
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
                yield return StartCoroutine(CheckForBattleOver(targetUnit));
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
        yield return sourceUnit.Hud.UpdateHP();
        if(sourceUnit.Pokemon.HP <= 0)
        {
            yield return StartCoroutine(CheckForBattleOver(sourceUnit));
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
            yield return new WaitForSeconds(1f);

        }
        else
        {
            var message = sourceUnit.Pokemon.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue("Enemy " + message);
            yield return new WaitForSeconds(1f);

        }
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Base.Name} fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue($"Enemy {faintedUnit.Pokemon.Base.Name} fainted");
            faintedUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogueBox.TypeDialogue("Critical Hit!");
            yield return new WaitForSeconds(0.5f);
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogueBox.TypeDialogue("It's Super Effective!");
            yield return new WaitForSeconds(0.5f);
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogueBox.TypeDialogue("It's Not Very Effective...");
            yield return new WaitForSeconds(0.5f);
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
            }
            else if( currentAction ==2)
            {
                //Party
                prevState = state;
                OpenPartyScreen();
            }
            else if( currentAction ==3)
            {
                //Run
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
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.D))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.S))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentMember -= 2;

        currentMove= Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count -1);

        partyScreen.UpdateMemberSelection(currentMember);

         if(Input.GetKeyDown(KeyCode.Space))
         {
            var selectedMember = playerParty.Pokemons[currentMember];
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

            if(prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
         else if (Input.GetKeyDown(KeyCode.Z))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if(playerUnit.Pokemon.HP > 0){
            yield return dialogueBox.TypeDialogue($"Good Work {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlaySwitchAnimation();
            yield return new WaitForSeconds(1.3f);
        }

        playerUnit.Setup(newPokemon);

        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");
        yield return new WaitForSeconds(2f);

        state = BattleState.RunningTurn;
    }
}
