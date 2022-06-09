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


        //private static readonly List<Arena> arenas = new List<Arena>();
        public static readonly Arena onesArena = new Arena();
        public static readonly Arena threesArena = new Arena();
        public static readonly Arena fivesArena = new Arena();

        public static void PlayerDeath(Player player, Arena arena) { arena.PlayerDeath(player); }
        public static void ResetArena(Arena arena) { arena.ResetArena(true); }
        public static void RemovePlayer(Player player) { onesArena.RemovePlayer(player); threesArena.RemovePlayer(player); fivesArena.RemovePlayer(player); }
        

        public static void Initialize()
        {
            //0x33DA0025 [113.822762 103.755890 60.005001] -0.912438 0.000000 0.000000 -0.409214
            onesArena.Team1Position = new Position(0x33DA0025, (float)113.822762, (float)103.755890, (float)60.005001, (float)0.000000, (float)0.000000, (float)-0.409214, (float)-0.912438);
            //0x33DA0025 [104.038803 112.469658 60.005001] 0.343646 0.000000 0.000000 -0.939099
            onesArena.Team2Position = new Position(0x33DA0025, (float)104.038803, (float)112.469658, (float)60.005001, (float)0.000000, (float)0.000000, (float)-0.939099, (float)0.343646);
            onesArena.Landcells = new uint[] { 0x33DA0025 };
            onesArena.ArenaType = "1v1";
            onesArena.CreateTeamsAt = 2;
            onesArena.TeamSize = 1;
            //            arenas.Add(arena);

            //0xF4EF002B[141.692657 68.447868 0.005000] - 0.989551 0.000000 0.000000 - 0.144184
            threesArena.Team1Position = new Position(0xF4EF002B, (float)141.692657, (float)68.447868, (float)0.005000, (float)0.000000, (float)0.000000, (float)-0.144184, (float)-0.989551);
            //0xF4EF002D[128.417969 113.538658 0.005000] 0.132965 0.000000 0.000000 - 0.991121
            threesArena.Team2Position = new Position(0xF4EF002D, (float)128.417969, (float)113.538658, (float)0.005000, (float)0.000000, (float)0.000000, (float)-0.991121, (float)0.132965);
            threesArena.Landcells = new uint[] { 0xF4EF002D, 0xF4EF002B, 0xF4EF002C, 0xF4EF0024, 0xF4EF0025, 0xF4EF002E, 0xF4EF0035, 0xF4EF0034, 0xF4EF0033, 0xF4EF0026, 0xF4EF003D, 0xF4EF0023 };
            threesArena.ArenaType = "3v3";
            threesArena.CreateTeamsAt = 6;
            threesArena.TeamSize = 3;

            //0xF6F20024[112.634331 80.769066 0.005000] - 0.179465 0.000000 0.000000 0.983764
            fivesArena.Team1Position = new Position(0xF6F20024, (float)112.634331, (float)80.769066, (float)0.005000, (float)0.000000, (float)0.000000, (float)0.983764, (float)-0.179465);
            //0xF6F2002A [125.101768 29.044254 0.005000] -0.996179 0.000000 0.000000 -0.087335
            fivesArena.Team2Position = new Position(0xF6F2002A, (float)125.101768, (float)29.044254, (float)0.005000, (float)0.000000, (float)0.000000, (float)-0.087335, (float)-0.996179);
            fivesArena.Landcells = new uint[] { 0xF6F20022, 0xF6F20023, 0xF6F20024, 0xF6F2002A, 0xF6F20029, 0xF6F2002B, 0xF6F2002C, 0xF6F20034, 0xF6F20033, 0xF6F20025, 0xF6F2001D, 0xF6F20015, 0xF6F20014, 0xF6F2001C, 0xF6F2001B, 0xF6F2001A, 0xF6F20013, 0xF6F20012 };
            fivesArena.ArenaType = "5v5";
            fivesArena.CreateTeamsAt = 10;
            fivesArena.TeamSize = 5;
        }

        public static Arena GetArena(string str)
        {
            switch (str)
            {
                case "ones":
                    return ArenasManager.onesArena;
                case "threes":
                    return ArenasManager.threesArena;
                case "fives":
                    return ArenasManager.fivesArena;
                default:
                    return null;
            }
        }

        public static Arena WhichArenaIsPlayerIn(Player player)
        {
            if (onesArena.ContainsPlayer(player))
                return onesArena;
            if (threesArena.ContainsPlayer(player))
                return threesArena;
            if (fivesArena.ContainsPlayer(player))
                return fivesArena;

            return null;
        }

        public static void Tick()
        {
            //log.Info("ArenasManager.Tick()...");
            TickArena(onesArena);
            TickArena(threesArena);
            TickArena(fivesArena);
        }

        public static void TickArena(Arena arena)
        {
            if (!arena.Occupied && arena.PlayerQueue.Count >= arena.CreateTeamsAt)
                arena.Init();

            //if (!arena.Occupied || arena.PlayerQueue.Count <= 1)
            //    return;

            if (arena.Occupied)
                arena.Tick();
        }
    }
}
