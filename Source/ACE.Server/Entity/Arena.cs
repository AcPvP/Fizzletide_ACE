using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity;
using ACE.Common;
using ACE.Server.WorldObjects;
using log4net;

namespace ACE.Server.Entity.Arenas
{
    public class TeamPlayer
    {
        public bool isDead { get; set; } = false;
        public Player player { get; set; } = null;
    }

    public class Arena
    {
        //public string Description { get; set; }
        public List<Player> PlayerQueue { get; set; } = new List<Player>();
        public bool Occupied { get; set; } = false;
        public List<TeamPlayer> Team1 { get; set; } = new List<TeamPlayer>();
        public List<TeamPlayer> Team2 { get; set; } = new List<TeamPlayer>();

        public bool CountDownPhase { get; set; } = false;
        public int CurrentCountdownPhase { get; set; } = 0;
        public int CountdownSeconds { get; set; } = 10;
        public double? ArenaCountdownTimer { get; set; } = null;

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Tick()
        {

            // TODO - If a player is PK tagged, remove them from the queue
            // TODO - Reward players when they dominate
            // Player_Death:602
            // TODO - fix the ELO server to calculate only arena ELO
            if (this.CountDownPhase)
                this.Countdown();

            this.CheckTeamLoss(Team1);
            this.CheckTeamLoss(Team2);
            // TODO - winning team rewards (not necessary for 1's)
            // Don't need this for 1v1's

        }

        public void PlayerDeath(Player player)
        {
            log.Info($"ARENAS: {player.Name} has died!");
            var teamPlayerWhoDied = this.GetAllTeamPlayers().Where(_teamPlayer => _teamPlayer.player == player).First();
            player.IsInArena = false;
            teamPlayerWhoDied.isDead = true;
        }

        public void CheckTeamLoss(List<TeamPlayer> team)
        {
            var deadTeam = team.Where(tPlayer => tPlayer.player.IsInArena == false).ToList().Count == team.Count;
            if (deadTeam)
                this.ResetArena();
        }

        public void Start()
        {
            if (!this.Occupied)
            {
                log.Info("ARENAS: Generating Teams.");
                this.GenerateTeams();
                this.CountDownPhase = true;
                this.Occupied = true;

            }
        }

        public void Countdown()
        {
            var flagCountdown = ArenaCountdownTimer == null ? true : Time.GetUnixTime() > ArenaCountdownTimer;
            if (flagCountdown)
            {
                if (this.CurrentCountdownPhase == 0)
                    MessagePlayers($"You have been chosen for the arenas!! You will be teleported in {this.CountdownSeconds} seconds.");
                else if (this.CurrentCountdownPhase == 10)
                {
                    MessagePlayers($"Go fuck shit up!");
                    this.CountDownPhase = false;
                    this.TeleportTeams();
                }
                else
                    MessagePlayers($"You will be teleported in {10 - this.CurrentCountdownPhase} seconds.");
                this.CurrentCountdownPhase += 1;
                this.ArenaCountdownTimer = Time.GetFutureUnixTime(1);
            }
        }

        public void MessagePlayers(string msg)
        {
            this.GetAllTeamPlayers().ForEach(tPlayer => tPlayer.player.SendMessage(msg));
        }

        public void ResetTeamPlayer(TeamPlayer tPlayer)
        {
            tPlayer.player.IsInArena = false;
        }
        public void ResetArena()
        {
            log.Info("ARENAS: Reset team players");
            this.GetAllTeamPlayers().ForEach(tPlayer => this.ResetTeamPlayer(tPlayer));
            this.Team1 = new List<TeamPlayer>();
            this.Team2 = new List<TeamPlayer>();
            var livePlayers = this.GetAllTeamPlayers().Where(tPlayer => tPlayer.isDead == false);
            // TODO - Teleport live players back to their LS.
            this.Occupied = false;
            this.CountDownPhase = false;
            this.CurrentCountdownPhase = 0;
            this.ArenaCountdownTimer = null;
        }

        public string QueuePlayer(Player player)
        {
            if (this.PlayerQueue.Contains(player))
                return "You are already in the queue.";

            this.PlayerQueue.Add(player);

            return "Queued up";
        }

        public List<TeamPlayer> GetAllTeamPlayers()
        {
            return this.Team1.Concat(this.Team2).ToList();
        }

        public void GenerateTeams()
        {
            AddPlayerToTeam(this.Team1);
            AddPlayerToTeam(this.Team2);
        }

        public void TeleportTeams()
        {
            log.Info("ARENAS: Teleporting Teams.");
            //var currentPos = new Position(player.Location);
            //player.Teleport(session.Player.Location);
            //player.SetPosition(PositionType.TeleportedCharacter, currentPos);
            //0x016C021C[87.998772 - 53.007347 0.005000] 0.637975 0.000000 0.000000 - 0.770057
            //0x016C0239[100.235168 - 55.323395 0.005000] - 0.723000 0.000000 0.000000 - 0.690848
            //new Position(0x016C021C, 87.998772, 53.007347, 0.005000, 0.637975, 0.000000, 0.000000, 0.770057);
            //new Position(cell, positionData[0], positionData[1], positionData[2], positionData[3], positionData[4], positionData[5], positionData[6])
        }

        public void RemovePlayer(Player player)
        {
            PlayerQueue.Remove(player);
        }

        public Player AddPlayerToTeam(List<TeamPlayer> team)
        {
            var player = PlayerQueue.First();
            player.IsInArena = true;
            PlayerQueue.Remove(player);
            var teamPlayer = new TeamPlayer();
            teamPlayer.player = player;
            team.Add(teamPlayer);
            return player;
        }


    }
}
