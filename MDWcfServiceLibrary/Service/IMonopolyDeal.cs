﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MDWcfServiceLibrary
{
    [ServiceContract/*(
    SessionMode = SessionMode.Required)*/] //Duplex mode specifing callback Contract
    interface IMonopolyDeal
    {
        /// <summary>
        /// Lobby service methods
        /// </summary>
        /// <returns></returns>
        [OperationContract]//
        GuidBox connectToLobby(string name);

        [OperationContract]
        GameLobbyStatus getGameLobbyStatus(GuidBox gameLobbyGuidP);

        [OperationContract]
        List<GameLobby> getListOfAllGameLobbys();

        [OperationContract]
        bool joinExistingGameLobby(GuidBox gameLobbyGuidP, GuidBox clientGuidP);

        [OperationContract]
        GuidBox joinNewGameLobby(GuidBox clientGuidP);

        [OperationContract]
        bool exitGameLobby(GuidBox clientGuidP);

        [OperationContract]
        bool setLobbyClientReady(GuidBox gameLobbyGuidP, GuidBox clientGuidP, bool readyP);

        /// <summary>
        /// Gets Current State of a Monopoly Deal game.
        /// </summary>
        /// <param name="playerGuid">Guid of player requesting state.</param>
        /// <param name="gameGuid">Guid of game to get current state of.</param>
        /// <returns>Returns the current PlayFieldModel </returns>
        [OperationContract]
        PlayFieldModel pollStateMonopolyDeal(GuidBox playerGuid, GuidBox gameGuid);

        //Make a moves
        [OperationContract]//Returns false if move is not valid for player at current time
        bool draw2AtStartOfTurnMD(GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to play card from hand to bank, returns true if move valid and played and card should be removed from players hand, false if move not valid and card stays in players hand
        Boolean playCardFromHandToBankMD(int playedCardID, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to play action card on their turn
        Boolean playWildRentActionCardOnTurnMD(PlayerModel player, Card playedCard, PlayerModel playerTargeted, PropertyCardSet setOfPropertiesToRentOn, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to play action card on their turn
        Boolean playStandardRentActionCardOnTurnMD(PlayerModel player, Card playedCard, PropertyCardSet setOfPropertiesToRentOn, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to play just say no card against action, returns true if card valid and action canceled
        Boolean playJustSayNoMD(int playedCard, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]//Player calls this method to play property to set
        Boolean playPropertyCardMD(PlayerModel player, Card playedCard, PropertyCardSet setToPlayPropertyTo, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to play property to new set
        Boolean playPropertyCardNewSetMD(int playedCardID, bool isOrientedUp, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]//Player calls this method to move a property between sets, return true if card is valid for set and is added to the set
        Boolean movePropertyCardMD(PlayerModel player, Card propertyCard, PropertyCardSet oldSet, PropertyCardSet setToPlayPropertyTo, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to give cards for payment, returns true if cards sufficent for payment, false if not
        Boolean payCardsMD(GuidBox playerPaying, List<int> cardsToPayWith, GuidBox gameLobbyGuid, GuidBox serverGuid, GuidBox playfieldModelInstanceGuid, GuidBox turnActionGuid);

        [OperationContract]//Player calls this method to end their turn, returns true to notify turn over completed with 7 or less cards, returns false if error ,requiring cards to be discarded will be in next state
        Boolean endTurnMD(GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]//discard  cards at end of turn
        Boolean discardMD(int cardsToDiscardIDs, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]
        Boolean playActionCardPassGoMD(int passGoCardID, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]
        Boolean playActionCardItsMyBirthdayMD(int myBirthdayCardID, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]
        Boolean playActionCardDebtCollectorMD(int debtCollectorCardID, GuidBox targetedPlayerGuid, GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]
        Boolean draw5AtStartOfTurnMD(GuidBox playerGuid, GuidBox gameLobbyGuid, GuidBox playfieldModelInstanceGuid);

        [OperationContract]
        Boolean hasGameStartedMD(GuidBox playerGuid, GuidBox gameLobbyGuid);
    }
}