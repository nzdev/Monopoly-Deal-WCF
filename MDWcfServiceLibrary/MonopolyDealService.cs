﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MDWcfServiceLibrary
{
    //MonopolyDealService implements IMonopolyDeal interface using Duplex mode
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    public class MonopolyDealService : IMonopolyDeal
    {
        // NOTE: The variables for storing callbacks and beer inventory are static.
        //       This is necessary since the service is using PerCall instancing.
        //       An instance of the service will be created each time a service method is invoked by a client.
        //       Consequently, the state must be persisted somewhere in between calls.
        private static List<IMonopolyDealCallback> _callbackList = new List<IMonopolyDealCallback>();
        private static int id = 2;
        //private static List<Player> wcfPlayers = new List<Player>();
        //private static List<Player> players = new List<Player>();
        //game contains state
        private static GameModel gameModel;
        private static List<PlayerModel> playerModels = new List<PlayerModel>();
        private static List<Guid> playerIdLookup = new List<Guid>();
        //private static WCFGame game;
        private static bool gameCreated = false;
        private static bool isStarted = false;
        private static int NUMBER_OF_DECKS = 1;
        private static Deck deck = new Deck(NUMBER_OF_DECKS);

        private static String serverLog = "";
        private int i = 0;

        private static MessageManager messageManager = new MessageManager();

        public string GetData(int value)
        {
            throw new NotImplementedException();
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            throw new NotImplementedException();
        }

        //Create new game
        private void createGame()
        {
            //Create Game if one does not exist
            if (!gameCreated)
            {
                gameModel = new GameModel(playerModels);
                gameCreated = true;
            }
        }

        private void createPlayer(string name)
        {
            // Subscribe the guest
            IMonopolyDealCallback guest = OperationContext.Current.GetCallbackChannel<IMonopolyDealCallback>();

            if (!_callbackList.Contains(guest))
            {
                _callbackList.Add(guest);
            }
            //Create new PlayerModel for client
            PlayerModel player = new PlayerModel(guest, name);
            //Add player to list
            playerModels.Add(player);
            playerIdLookup.Add(player.guid);
            //call back and tell player what number they are
            createPlayerCallback(player.ICallBack, player.name, playerModels.IndexOf(player) + 1, player.guid);
        }

        private void createPlayerCallback(IMonopolyDealCallback playerCallback, string name, int id, Guid guidP)
        {
            //CallBack one
            playerCallback.testOperationReturn2("Your Player Name:" + name + " ID:" + id);

            //Assign guid to player
            playerCallback.recieveGuid(guidP);

            //Tell all players
            addToClientsLogs("Welcome Player:" + name);
            /*
            //CallBack all
            _callbackList.ForEach(
                delegate(IMonopolyDealCallback callback)
                { callback.testOperationReturn2("Welcome Player:" + name); });
             * */
        }

        public void addToClientsLogs(String description)
        {
            //CallBack all
            _callbackList.ForEach(
                delegate(IMonopolyDealCallback callback)
                { callback.addToLog(description); });
        }

        public void connect(string name)
        {
            //Create a playermodel for client and add to list
            createPlayer(name);
        }

        public void testOperation(int id)
        {
        }

        private void testCallback(string name)
        {
        }

        private PlayerModel getPlayerModelByGuid(Guid g)
        {
            int id = playerIdLookup.IndexOf(g);
            return playerModels.ElementAt(id);
        }

        public void startGame(Guid guid)
        {
            getPlayerModelByGuid(guid).isReadyToStartGame = true;
            bool allReady = true;
            foreach (PlayerModel p in playerModels)
            {
                if (!p.isReadyToStartGame)
                {
                    allReady = false;
                    addToClientsLogs("Player " + p.name + " is not Ready");
                }
                else
                {
                    addToClientsLogs("Player " + p.name + " is Ready");
                }
            }
            if (allReady)
            {
                //Create game
                createGame();
                isStarted = true;
                addToClientsLogs("Game Started");
            }
        }

        public void chatToAll(string chat)
        {
            //CallBack all
            _callbackList.ForEach(
                delegate(IMonopolyDealCallback callback)
                { callback.recieveChat(chat); });
        }

        public void sendMessageToService(Message message)
        {
            throw new NotImplementedException();
        }

        public void pollState(Message message)
        {
            throw new NotImplementedException();
        }
    }
}