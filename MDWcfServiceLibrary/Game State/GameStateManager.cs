﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MDWcfServiceLibrary
{
    /// <summary>
    /// GameStateManager modifies the state of the game
    /// </summary>
    ///
    internal class GameStateManager
    {
        GameModel gameModel;

        PlayFieldModel currentPlayFieldModel;
        //PlayFieldModel nextPlayFieldModel; //Careful of justSayNo chains
        List<Acknowledgement> acknowledgementsRecieved = new List<Acknowledgement>();

        public GameStateManager(GameModel gm)
        {
            gameModel = gm;
        }

        public PlayFieldModel getPlayFieldModelByGuid(Guid playFieldModelGuid)
        {
            foreach (PlayFieldModel pfm in gameModel.gameStates)
            {
                if (pfm.thisPlayFieldModelInstanceGuid.CompareTo(playFieldModelGuid) == 0)
                {
                    return pfm;
                }
            }
            return null;// no playfield found with specified guid
        }

        public PlayFieldModel getCurrentPlayFieldModel()
        {
            //Get CurrentPlayFieldModelState
            return gameModel.gameStates[(gameModel.gameStates.Count - 1)];
        }

        public void endTurn(PlayerModel player)
        {
            updateState(TurnActionTypes.EndTurn, getCurrentPlayFieldModel(), player.guid);
        }

        /*
        public bool doAction(Guid gameGuid, Guid playerGuid, Guid gameStateActionShouldBeAppliedOnGuid, TurnActionTypes actionType)
        {
            ///Returns false if action not carried out
            ///
            //Get CurrentPlayFieldModelState
            currentPlayFieldModel = getCurrentPlayFieldModel();
            if (checkIfActionIsForThisState(actionType, gameStateActionShouldBeAppliedOnGuid, playerGuid, gameGuid))
            {
                if (actionType.CompareTo(TurnActionTypes.drawTwoCardsAtStartOfTurn) == 0)
                {
                    drawTwoCardsAtTurnStart(getPlayerByGuid(playerGuid, currentPlayFieldModel));
                }
                if (actionType.CompareTo(TurnActionTypes.drawFiveCardsAtStartOfTurn) == 0)
                {
                    drawFiveCards(getPlayerByGuid(playerGuid, currentPlayFieldModel));
                }
                if (actionType.CompareTo(TurnActionTypes.EndTurn) == 0)
                {
                    endTurn(getPlayerByGuid(playerGuid, currentPlayFieldModel));
                }
                if (actionType.CompareTo(TurnActionTypes.PlayPropertyCard_New_Set) == 0)
                {
                    //playPropertyCardToNewSet(getPlayerByGuid(playerGuid, currentPlayFieldModel));
                }
                //turn action is for this playfieldmodel
            }
            return true;

        }
         * */

        public bool playPropertyCardToNewSet(Guid gameGuid, Guid playerGuid, Guid gameStateActionShouldBeAppliedOnGuid, TurnActionTypes actionType, int propertyCardID)
        {
            //Get CurrentPlayFieldModelState
            currentPlayFieldModel = getCurrentPlayFieldModel();
            PlayerModel player = getPlayerByGuid(playerGuid, currentPlayFieldModel);
            if (checkIfActionIsForThisState(actionType, gameStateActionShouldBeAppliedOnGuid, playerGuid, gameGuid))
            {
                if (actionType.CompareTo(TurnActionTypes.PlayPropertyCard_New_Set) == 0)
                {
                    foreach (Card c in player.hand.cardsInHand)
                    {
                        if (c.cardID == propertyCardID && c is PropertyCard)
                        {
                            Card card = removeCardFromHand(c, player);
                            if (card != null)
                            {
                                PropertyCard cP = c as PropertyCard;
                                PropertyCardSet ps = new PropertyCardSet(cP);
                                player.propertySets.addSet(ps);
                                updateState(TurnActionTypes.PlayPropertyCard_New_Set, currentPlayFieldModel, player.guid);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /*
        public bool checkIfMoveLegal(Guid guidOfPlayerMakingMove, TurnActionTypes typeOfActionAttempted)
        {
            throw new NotImplementedException();
        }
        */
        public void drawTwoCardsAtTurnStart(PlayerModel player)
        {
            //draws two cards to players hand Unsafe
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            //actionPerformed();
            updateState(TurnActionTypes.drawTwoCardsAtStartOfTurn, currentPlayFieldModel, player.guid);
        }

        public void drawFiveCards(PlayerModel player)
        {
            //draws five cards to players hand Unsafe
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
            //actionPerformed();
            updateState(TurnActionTypes.drawFiveCardsAtStartOfTurn, currentPlayFieldModel, player.guid);
        }

        public Card checkIfCardInHand(Card card, PlayerModel pm)
        {
            foreach (Card c in pm.hand.cardsInHand)
            {
                if (c.cardID == card.cardID)
                {
                    return c;
                }
            }
            return null;
        }

        private Card removeCardFromHand(Card card, PlayerModel pm)
        {
            Card cardInHand = checkIfCardInHand(card, pm);
            if (cardInHand != null && pm.hand.cardsInHand.Remove(cardInHand))
            {
                return cardInHand;
            }
            return null;
        }

        public bool bankCard(int playedCardID, Guid playerGuid, Guid serverGuid, Guid playfieldModelInstanceGuid)
        {
            Card cardInHandToBeBanked = gameModel.deck.getCardByID(playedCardID);
            //Get the reference to the players playerModel in the current PlayFieldModel
            PlayerModel playerWhoIsBankingCard = getPlayerModel(playerGuid, serverGuid, playfieldModelInstanceGuid);
            //Get the reference to the Card in the current PlayFieldModel

            Card card = removeCardFromHand(cardInHandToBeBanked, playerWhoIsBankingCard);
            if (card != null)
            {
                playerWhoIsBankingCard.bank.addCardToBank(card);
                //Change state on success
                updateState(TurnActionTypes.BankActionCard, getCurrentPlayFieldModel(), playerWhoIsBankingCard.guid);
                return true;
            }
            else
            {
                //Card not in players hand, can't be banked
                return false;
            }
        }

        public PlayerModel getPlayerModel(Guid playerGuid, Guid gameGuid, Guid currentPlayFieldModelGuid)
        {
            //Modify for multiple games on service
            foreach (PlayerModel p in currentPlayFieldModel.playerModels)
            {
                if (p.guid.CompareTo(playerGuid) == 0)
                {
                    return p;
                }
            }
            return null;
        }

        public bool isActionAllowedForPlayer(TurnActionTypes turnActionToDo, Guid playerGuid, PlayFieldModel currentState)
        {
            TurnActionTypes tAT = turnActionToDo;
            PlayerModel playerAttemptingAction = getPlayerModel(playerGuid, gameModel.gameModelGuid, currentState.thisPlayFieldModelInstanceGuid);
            if (playerAttemptingAction != null)
            {
                foreach (TurnActionTypes t in playerAttemptingAction.actionsCurrentlyAllowed)
                {
                    if (t.CompareTo(tAT) == 0)
                    {
                        //Action is in allowable list for player
                        return true;
                    }
                }
            }
            return false; //Action not allowable
        }

        public bool haveAllPlayersAcknowledgedCurrentState(Guid gameServiceInstanceGuid, Guid currentStateGuidP, List<PlayerModel> playersInGame)
        {
            //Current State Guid
            foreach (PlayerModel pm in playersInGame)
            {
                bool recievedFromThisPlayer = false;
                foreach (Acknowledgement a in acknowledgementsRecieved)
                {
                    if (a.equal(pm.guid, gameServiceInstanceGuid, currentStateGuidP))
                    {
                        recievedFromThisPlayer = true;
                        break;
                    }
                }
                if (!recievedFromThisPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkIfActionIsForThisState(TurnActionTypes ta, Guid stateGuid, Guid playerGuid, Guid gameGuid)
        {
            PlayerModel player = getPlayerModel(playerGuid, gameGuid, stateGuid);
            foreach (TurnActionTypes t in player.actionsCurrentlyAllowed)
            {
                if (t.CompareTo(ta) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public PlayerModel getPlayerByGuid(Guid player, PlayFieldModel pfm)
        {
            foreach (PlayerModel p in pfm.playerModels)
            {
                if (p.guid.CompareTo(player) == 0)
                {
                    return p;
                }
            }
            return null;
        }

        public PlayFieldModel copyPlayFieldModel(PlayFieldModel currentPlayFieldModel)
        {
            //not implemented
            return currentPlayFieldModel;
        }

        private void updateAllowableStates(PlayFieldModel state, List<TurnActionTypes> allowedForPlayersNotOnTurn, List<TurnActionTypes> allowedForPlayerOnTurn, Guid playerOnTurnGuid)
        {
            foreach (PlayerModel p in state.playerModels)
            {
                if (p.guid.CompareTo(playerOnTurnGuid) == 0)
                //if (p.isThisPlayersTurn)
                {
                    //its p's turn
                    p.actionsCurrentlyAllowed = allowedForPlayerOnTurn;
                }
                else
                {
                    p.actionsCurrentlyAllowed = allowedForPlayersNotOnTurn;
                }
            }
        }

        private int playerTurnCounter = 0;

        private void setNextPlayerOnTurn(PlayFieldModel pfm)
        {
            playerTurnCounter++;
            if (playerTurnCounter >= pfm.playerModels.Count)
            {
                playerTurnCounter = 0;
            }

            int nextPlayerID = playerTurnCounter;

            pfm.guidOfPlayerWhosTurnItIs = pfm.playerModels[nextPlayerID].guid;
            foreach (PlayerModel p in pfm.playerModels)
            {
                if (p.guid.CompareTo(pfm.guidOfPlayerWhosTurnItIs) == 0)
                {
                    if (p.isThisPlayersTurn)
                    {
                        throw new Exception("player already on turn");
                    }
                    p.isThisPlayersTurn = true;
                }
                else
                {
                    p.isThisPlayersTurn = false;
                }
            }
            pfm.guidOfPlayerWhosTurnItIs = pfm.playerModels.ElementAt(nextPlayerID).guid;
        }

        private void updateStateForPropertyPlayedToSet(TurnActionTypes actionToAttemptToPerform, PlayFieldModel currentState, Guid playerWhoPerformedActionGuid, PlayerModel playerWhoPerformedAction, PlayFieldModel newState)
        {
            throw new NotImplementedException();
        }

        private List<TurnActionTypes> setAllowableActionsOnTurn(List<TurnActionTypes> listToSet, PlayFieldModel newState)
        {
            if (newState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_0_Cards_Played) == 0 || newState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_1_Cards_Played) == 0 || newState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_2_Cards_Played) == 0)
            {
                listToSet.Add(TurnActionTypes.BankActionCard);
                listToSet.Add(TurnActionTypes.BankMoneyCard);
                listToSet.Add(TurnActionTypes.PlayCard);
                listToSet.Add(TurnActionTypes.PlayPropertyCard_New_Set);
                listToSet.Add(TurnActionTypes.SwitchAroundPlayedProperties);
                listToSet.Add(TurnActionTypes.EndTurn);
                return listToSet;
            }
            else if (newState.currentPhase.CompareTo(Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards) == 0)
            {
                listToSet.Add(TurnActionTypes.Discard_1_Card);
                return listToSet;
            }
            else if (newState.currentPhase.CompareTo(Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards) == 0)
            {
                listToSet.Add(TurnActionTypes.Discard_1_Card);
                return listToSet;
            }
            else if (newState.currentPhase.CompareTo(Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards) == 0)
            {
                listToSet.Add(TurnActionTypes.Discard_1_Card);
                return listToSet;
            }
            else if (newState.currentPhase.CompareTo(Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards) == 0)
            {
                listToSet.Add(TurnActionTypes.Discard_1_Card);
                return listToSet;
            }
            else if (newState.currentPhase.CompareTo(Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card) == 0)
            {
                listToSet.Add(TurnActionTypes.Discard_1_Card);
                return listToSet;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void updateState(TurnActionTypes actionToAttemptToPerform, PlayFieldModel currentState, Guid playerWhoPerformedAction)
        {
            PlayerModel player = getPlayerByGuid(playerWhoPerformedAction, currentState);

            PlayFieldModel newState = copyPlayFieldModel(currentState);

            //List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
            //List<TurnActionTypes> onTurn = new List<TurnActionTypes>();

            #region draw2state

            //draw 2 on turn start state
            if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Draw_2_Cards) == 0)
            {
                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.drawTwoCardsAtStartOfTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_0_Cards_Played;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }
            }

            #endregion draw2state

            #region draw5state

            //draw 5 on turn start state
            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Draw_5_Cards) == 0)
            {
                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.drawFiveCardsAtStartOfTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_0_Cards_Played;
                        //player has drawn their five cards as they started the turn with zero cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }
            }

            #endregion draw5state

            #region Turn_Started_Cards_Drawn_0_Cards_Played

            //draw 2 on turn start state
            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_0_Cards_Played) == 0)
            {
                #region bankActionCard

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.BankActionCard) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_1_Cards_Played;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion bankActionCard

                #region endTurn

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.EndTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        switch (player.hand.cardsInHand.Count)
                        {
                            case 8:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card;
                                    onTurn.Add(TurnActionTypes.Discard_1_Card);
                                    break;
                                }
                            case 9:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_2_Cards);
                                    break;
                                }
                            case 10:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_3_Cards);
                                    break;
                                }
                            case 11:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_4_Cards);
                                    break;
                                }
                            case 12:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_5_Cards);
                                    break;
                                }
                            default:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_7_Or_Less_Cards_In_Hand_Setup_NextPlayer;
                                    setNextPlayerOnTurn(newState);
                                    if (getPlayerByGuid(newState.guidOfPlayerWhosTurnItIs, newState).hand.cardsInHand.Count == 0)
                                    {
                                        //Player has 0 cards draws 5 on turn start instead of 2
                                        newState.currentPhase = Statephase.Turn_Started_Draw_5_Cards;
                                        onTurn.Add(TurnActionTypes.drawFiveCardsAtStartOfTurn);
                                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                                    }
                                    else
                                    {
                                        newState.currentPhase = Statephase.Turn_Started_Draw_2_Cards;
                                        onTurn.Add(TurnActionTypes.drawTwoCardsAtStartOfTurn);
                                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                                    }

                                    break;
                                }
                        }

                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion endTurn

                #region playPropertyToNewSet

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.PlayPropertyCard_New_Set) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();

                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        //updateStateForPropertyPlayedToSet(actionToAttemptToPerform, currentState, playerWhoPerformedAction, player, newState);
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_1_Cards_Played;
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion playPropertyToNewSet

                //Play Action
            }

            #endregion Turn_Started_Cards_Drawn_0_Cards_Played

            #region Turn_Started_Cards_Drawn_1_Cards_Played

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_1_Cards_Played) == 0)
            {
                #region bankActionCard

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.BankActionCard) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_2_Cards_Played;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion bankActionCard

                #region endTurn

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.EndTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        switch (player.hand.cardsInHand.Count)
                        {
                            case 8:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card;
                                    onTurn.Add(TurnActionTypes.Discard_1_Card);
                                    break;
                                }
                            case 9:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_2_Cards);
                                    break;
                                }
                            case 10:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_3_Cards);
                                    break;
                                }
                            case 11:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_4_Cards);
                                    break;
                                }
                            case 12:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_5_Cards);
                                    break;
                                }
                            default:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_7_Or_Less_Cards_In_Hand_Setup_NextPlayer;
                                    setNextPlayerOnTurn(newState);
                                    if (getPlayerByGuid(newState.guidOfPlayerWhosTurnItIs, newState).hand.cardsInHand.Count == 0)
                                    {
                                        //Player has 0 cards draws 5 on turn start instead of 2
                                        newState.currentPhase = Statephase.Turn_Started_Draw_5_Cards;
                                        onTurn.Add(TurnActionTypes.drawFiveCardsAtStartOfTurn);
                                    }
                                    else
                                    {
                                        newState.currentPhase = Statephase.Turn_Started_Draw_2_Cards;
                                        onTurn.Add(TurnActionTypes.drawTwoCardsAtStartOfTurn);
                                    }

                                    break;
                                }
                        }

                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion endTurn

                #region playPropertyToNewSet

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.PlayPropertyCard_New_Set) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();

                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        //updateStateForPropertyPlayedToSet(actionToAttemptToPerform, currentState, playerWhoPerformedAction, player, newState);
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_2_Cards_Played;
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion playPropertyToNewSet

                //Play Action
            }

            #endregion Turn_Started_Cards_Drawn_1_Cards_Played

            #region Turn_Started_Cards_Drawn_2_Cards_Played

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_2_Cards_Played) == 0)
            {
                //Actions that can be taken on this phase

                #region bankActionCard

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.BankActionCard) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_3_Cards_Played_Swap_Properties_Or_End_Turn_Only;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn.Add(TurnActionTypes.SwitchAroundPlayedProperties);
                        onTurn.Add(TurnActionTypes.EndTurn);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion bankActionCard

                #region endTurn

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.EndTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        switch (player.hand.cardsInHand.Count)
                        {
                            case 8:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card;
                                    onTurn.Add(TurnActionTypes.Discard_1_Card);
                                    break;
                                }
                            case 9:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_2_Cards);
                                    break;
                                }
                            case 10:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_3_Cards);
                                    break;
                                }
                            case 11:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_4_Cards);
                                    break;
                                }
                            case 12:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_5_Cards);
                                    break;
                                }
                            default:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_7_Or_Less_Cards_In_Hand_Setup_NextPlayer;
                                    setNextPlayerOnTurn(newState);
                                    if (getPlayerByGuid(newState.guidOfPlayerWhosTurnItIs, newState).hand.cardsInHand.Count == 0)
                                    {
                                        //Player has 0 cards draws 5 on turn start instead of 2
                                        newState.currentPhase = Statephase.Turn_Started_Draw_5_Cards;
                                        onTurn.Add(TurnActionTypes.drawFiveCardsAtStartOfTurn);
                                    }
                                    else
                                    {
                                        newState.currentPhase = Statephase.Turn_Started_Draw_2_Cards;
                                        onTurn.Add(TurnActionTypes.drawTwoCardsAtStartOfTurn);
                                    }

                                    break;
                                }
                        }

                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion endTurn

                #region playPropertyToNewSet

                else if (actionToAttemptToPerform.CompareTo(TurnActionTypes.PlayPropertyCard_New_Set) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn.Add(TurnActionTypes.SwitchAroundPlayedProperties);
                        onTurn.Add(TurnActionTypes.EndTurn);
                        //onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        //updateStateForPropertyPlayedToSet(actionToAttemptToPerform, currentState, playerWhoPerformedAction, player, newState);
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Started_Cards_Drawn_3_Cards_Played_Swap_Properties_Or_End_Turn_Only;
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion playPropertyToNewSet

                //Play Action
            }

            #endregion Turn_Started_Cards_Drawn_2_Cards_Played

            #region Turn_Started_Cards_Drawn_3_Cards_Played_Swap_Properties_Or_End_Turn_Only

            //draw 2 on turn start state
            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Started_Cards_Drawn_3_Cards_Played_Swap_Properties_Or_End_Turn_Only) == 0)
            {
                #region endTurn

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.EndTurn) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        switch (player.hand.cardsInHand.Count)
                        {
                            case 8:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card;
                                    onTurn.Add(TurnActionTypes.Discard_1_Card);
                                    break;
                                }
                            case 9:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_2_Cards);
                                    break;
                                }
                            case 10:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_3_Cards);
                                    break;
                                }
                            case 11:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_4_Cards);
                                    break;
                                }
                            case 12:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards;
                                    onTurn.Add(TurnActionTypes.Discard_5_Cards);
                                    break;
                                }
                            default:
                                {
                                    newState.currentPhase = Statephase.Turn_Ended_7_Or_Less_Cards_In_Hand_Setup_NextPlayer;
                                    setNextPlayerOnTurn(newState);
                                    if (getPlayerByGuid(newState.guidOfPlayerWhosTurnItIs, newState).hand.cardsInHand.Count == 0)
                                    {
                                        //Player has 0 cards draws 5 on turn start instead of 2
                                        newState.currentPhase = Statephase.Turn_Started_Draw_5_Cards;
                                        onTurn.Add(TurnActionTypes.drawFiveCardsAtStartOfTurn);
                                    }
                                    else
                                    {
                                        newState.currentPhase = Statephase.Turn_Started_Draw_2_Cards;
                                        onTurn.Add(TurnActionTypes.drawTwoCardsAtStartOfTurn);
                                    }

                                    break;
                                }
                        }

                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion endTurn

                //Play Action
            }

            #endregion Turn_Started_Cards_Drawn_3_Cards_Played_Swap_Properties_Or_End_Turn_Only

            #region Turn_Ended_12_Cards_In_Hand_Discard_5_Cards

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Ended_12_Cards_In_Hand_Discard_5_Cards) == 0)
            {
                #region discard1Card

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.Discard_1_Card) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion discard1Card
            }

            #endregion Turn_Ended_12_Cards_In_Hand_Discard_5_Cards

            #region Turn_Ended_11_Cards_In_Hand_Discard_4_Cards

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Ended_11_Cards_In_Hand_Discard_4_Cards) == 0)
            {
                #region discard1Card

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.Discard_1_Card) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion discard1Card
            }

            #endregion Turn_Ended_11_Cards_In_Hand_Discard_4_Cards

            #region Turn_Ended_10_Cards_In_Hand_Discard_3_Cards

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Ended_10_Cards_In_Hand_Discard_3_Cards) == 0)
            {
                #region discard1Card

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.Discard_1_Card) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion discard1Card
            }

            #endregion Turn_Ended_10_Cards_In_Hand_Discard_3_Cards

            #region Turn_Ended_9_Cards_In_Hand_Discard_2_Cards

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Ended_9_Cards_In_Hand_Discard_2_Cards) == 0)
            {
                #region discard1Card

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.Discard_1_Card) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card;

                        //player has drawn their two cards, Now can play up to three cards on their turn
                        List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                        List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                        onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion discard1Card
            }

            #endregion Turn_Ended_9_Cards_In_Hand_Discard_2_Cards

            #region Turn_Ended_8_Cards_In_Hand_Discard_1_Card

            else if (currentState.currentPhase.CompareTo(Statephase.Turn_Ended_8_Cards_In_Hand_Discard_1_Card) == 0)
            {
                #region discard1Card

                if (actionToAttemptToPerform.CompareTo(TurnActionTypes.Discard_1_Card) == 0)
                {
                    //Move was a valid move at current state
                    //Check if move is valid for player
                    List<TurnActionTypes> notOnTurn = new List<TurnActionTypes>();
                    List<TurnActionTypes> onTurn = new List<TurnActionTypes>();
                    if (isActionAllowedForPlayer(actionToAttemptToPerform, playerWhoPerformedAction, currentState))
                    {
                        //action is valid for player at this time

                        //Could perform the action here instead, for now just change the phase of the state
                        //Not an action so cant be just say no'd
                        //Change phase
                        newState.currentPhase = Statephase.Turn_Ended_7_Or_Less_Cards_In_Hand_Setup_NextPlayer;
                        setNextPlayerOnTurn(newState);
                        if (getPlayerByGuid(newState.guidOfPlayerWhosTurnItIs, newState).hand.cardsInHand.Count == 0)
                        {
                            //Player has 0 cards draws 5 on turn start instead of 2
                            newState.currentPhase = Statephase.Turn_Started_Draw_5_Cards;
                            onTurn.Add(TurnActionTypes.drawFiveCardsAtStartOfTurn);
                        }
                        else
                        {
                            newState.currentPhase = Statephase.Turn_Started_Draw_2_Cards;
                            onTurn.Add(TurnActionTypes.drawTwoCardsAtStartOfTurn);
                        }

                        //player has drawn their two cards, Now can play up to three cards on their turn

                        //onTurn = setAllowableActionsOnTurn(onTurn, newState);
                        updateAllowableStates(newState, notOnTurn, onTurn, newState.guidOfPlayerWhosTurnItIs);
                    }
                }

                #endregion discard1Card
            }

            #endregion Turn_Ended_8_Cards_In_Hand_Discard_1_Card
        }

        /// <summary>
        /// Discards one specified card at the end of the players turn
        /// </summary>
        /// <param name="cardsToDiscardID"></param>
        /// <param name="playerGuid"></param>
        /// <param name="serverGuid"></param>
        /// <param name="playfieldModelInstanceGuid"></param>
        /// <returns></returns>
        internal bool discard(int cardsToDiscardID, Guid playerGuid, Guid serverGuid, Guid playfieldModelInstanceGuid)
        {
            Card cardInHandToBeDiscarded = gameModel.deck.getCardByID(cardsToDiscardID);
            //Get the reference to the players playerModel in the current PlayFieldModel
            PlayerModel playerWhoIsDiscardingCard = getPlayerModel(playerGuid, serverGuid, playfieldModelInstanceGuid);
            //Get the reference to the Card in the current PlayFieldModel

            Card card = removeCardFromHand(cardInHandToBeDiscarded, playerWhoIsDiscardingCard);
            if (card != null)
            {
                getCurrentPlayFieldModel().drawPile.discardCard(card);
                //Change state on success
                updateState(TurnActionTypes.Discard_1_Card, getCurrentPlayFieldModel(), playerWhoIsDiscardingCard.guid);
                return true;
            }
            else
            {
                //Card not in players hand, can't be discarded
                return false;
            }
        }


        /// <summary>
        /// Plays a Pass Go Action Card on a players turn
        /// </summary>
        /// <param name="passGoCardID"></param>
        /// <param name="serverGuid"></param>
        /// <param name="playerGuid"></param>
        /// <param name="playfieldModelInstanceGuid"></param>
        /// <param name="turnActionTypes"></param>
        /// <returns></returns>
        internal bool playActionCardPassGo(int passGoCardID, Guid serverGuid, Guid playerGuid, Guid playfieldModelInstanceGuid, TurnActionTypes turnActionTypes)
        {
            Card cardInHandToBePlayed = gameModel.deck.getCardByID(passGoCardID);
            //Get the reference to the players playerModel in the current PlayFieldModel

            PlayerModel player = getPlayerModel(playerGuid, serverGuid, playfieldModelInstanceGuid);
            //Get the reference to the Card in the current PlayFieldModel
            if (cardInHandToBePlayed != null && cardInHandToBePlayed is ActionCard && ((ActionCard)cardInHandToBePlayed).actionType.CompareTo(ActionCardAction.PassGo) == 0)
            {
                Card card = removeCardFromHand(cardInHandToBePlayed, player);
                if (card != null)
                {
                    ActionCard actionCard = card as ActionCard;
                    player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
                    player.hand.addCardToHand(currentPlayFieldModel.drawPile.drawcard());
                    //Change state on success
                    updateState(TurnActionTypes.PlayActionCard, getCurrentPlayFieldModel(), player.guid);
                    return true;
                }
                return false;
            }
            else
            {
                //Card not in players hand, can't be discarded not an actioncard
                return false;
            }
        }
    }
}