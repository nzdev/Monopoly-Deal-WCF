﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDWcfServiceLibrary
{
    internal class MonopolyDealGameGen
    {
        private List<MonopolyDeal> games = new List<MonopolyDeal>();
        public static readonly int MAX_PLAYERS_PER_GAME = 5;
        public static readonly int MIN_PLAYERS_PER_GAME = 2;

        public MonopolyDealGameGen(List<MonopolyDeal> monopolyDealGames)
        {
            games = monopolyDealGames;
        }

        public bool createGame(List<LobbyClient> clients, Guid guidForGame)
        {
            try
            {
                List<PlayerModel> playersMD = new List<PlayerModel>();
                foreach (LobbyClient lc in clients)
                {
                    PlayerModel p = new PlayerModel(lc.getName());
                    p.guid = lc.getGuid();
                    p.isReadyToStartGame = true;
                    playersMD.Add(p);
                }
                MonopolyDeal game = new MonopolyDeal(playersMD, guidForGame);
                games.Add(game);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}