using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using ACE.Common;
using ACE.Database;
using ACE.Database.Models.Shard;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Network.Enum;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.WorldObjects;

using Biota = ACE.Entity.Models.Biota;

using ACE.Server.Entity.Arenas;
namespace ACE.Server.Managers
{
    public static class ArenasManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static readonly List<Arena> arenas = new List<Arena>();

        public static void Initialize()
        {
            var arena = new Entity.Arenas.Arena();
            //0x33DA0025 [113.822762 103.755890 60.005001] -0.912438 0.000000 0.000000 -0.409214
            arena.Team1Position = new Position(0x33DA0025, (float)113.822762, (float)103.755890, (float)60.005001, (float)0.000000, (float)0.000000, (float)-0.409214, (float)-0.912438);
            //0x33DA0025 [104.038803 112.469658 60.005001] 0.343646 0.000000 0.000000 -0.939099
            arena.Team2Position = new Position(0x33DA0025, (float)104.038803, (float)112.469658, (float)60.005001, (float)0.000000, (float)0.000000, (float)-0.939099, (float)0.343646);

            arenas.Add(arena);
        }

        // Since there is only 1 arena for now, we are going to have a method to access that main arena
        public static Arena GetArena()
        {
            return arenas.First();
        }

        public static void PlayerDeath(Player player)
        {
            var arena = GetArena();
            arena.PlayerDeath(player);
        }

        public static void Tick()
        {
            var arena = GetArena();
            //log.Info("ArenasManager.Tick()...");
            if (!arena.Occupied && arena.PlayerQueue.Count >= 2)
                arena.Init();

            //if (!arena.Occupied || arena.PlayerQueue.Count <= 1)
            //    return;

            if (arena.Occupied)
                arena.Tick();

        }

        public static void ResetArena()
        {
            var arena = GetArena();
            arena.ResetArena(true);
        }
    }
}
