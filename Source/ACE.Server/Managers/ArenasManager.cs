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
                arena.Start();

            //if (!arena.Occupied || arena.PlayerQueue.Count <= 1)
            //    return;

            if (arena.Occupied)
                arena.Tick();

        }
    }
}
