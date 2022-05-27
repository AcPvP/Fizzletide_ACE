using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity;
using ACE.Common;
using ACE.Server.WorldObjects;
using ACE.Server.Managers;
using ACE.Database;
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
        public bool EndSequence { get; set; } = false;
        public bool Live { get; set; } = false;
        public int CurrentCountdownPhase { get; set; } = 0;
        public int CountdownSeconds { get; set; } = 0;
        public int CreateTeamsAt { get; set; } = 0;
        public int TeamSize { get; set; } = 0;
        public double? ArenaCountdownTimer { get; set; } = null;
        public double? TimeLimit { get; set; } = null;
        public double? TimeLimitAlert { get; set; } = null;
        public double? WinBuffer { get; set; } = null;
        public string ArenaType { get; set; } = null;
        public Position Team1Position { get; set; } = null;
        public Position Team2Position { get; set; } = null;
        public uint[] Landcells { get; set; } = { };

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public List<TeamPlayer> GetAllTeamPlayers() { return this.Team1.Concat(this.Team2).ToList(); }
        public List<Player> GetAllPlayers() { return this.GetAllTeamPlayers().Select(tPlayer => tPlayer.player).ToList(); }
        public List<TeamPlayer> GetLiveTeamPlayers() { return this.GetAllTeamPlayers().Where(tPlayer => tPlayer.isDead == false).ToList(); }
        public List<Player> GetLivePlayers() { return this.GetLiveTeamPlayers().Select(tPlayer => tPlayer.player).ToList(); }
        public bool ContainsPlayer(Player player) { return this.GetAllPlayers().Contains(player); }
        public void MessagePlayers(string msg) { this.GetAllTeamPlayers().ForEach(tPlayer => tPlayer.player.SendMessage(msg)); }
        public void ResetTeamPlayer(TeamPlayer tPlayer) { tPlayer.player.IsInArena = false; }
        public void RemovePlayer(Player player) { PlayerQueue.Remove(player); }

        public void Tick()
        {
            if (this.CountDownPhase)
                this.Countdown();

            if (this.WinBuffer != null && Time.GetUnixTime() > this.WinBuffer)
                this.ResetArena();

            this.CheckTimeLimit();
            this.CheckTeamLoss(Team1);
            this.CheckTeamLoss(Team2);

            if(this.Live)
                this.CheckPussyPlayers();
        }

        public void CheckPussyPlayers()
        {
            var liveTPlayers = this.GetLiveTeamPlayers();
            liveTPlayers.ForEach(tPlayer => {
                var stillInLandblock = this.Landcells.Contains(tPlayer.player.Location.Cell);
                if (!stillInLandblock)
                {
                    tPlayer.player.SendMessage("You pussy.");
                    this.GetOppositeTeam(tPlayer).ForEach(otPlayer =>
                    {
                        DatabaseManager.PKKills.CreateKill(tPlayer.player.Guid.Full, otPlayer.player.Guid.Full, true, this.ArenaType);
                    });
                    tPlayer.isDead = true;
                }
            });
        }

        public List<TeamPlayer> GetOppositeTeam(TeamPlayer tPlayer)
        {
            if (Team1.Contains(tPlayer))
                return Team2;

            return Team1;
        }

        public void CheckTimeLimit()
        {
            if (this.EndSequence)
                return;

            var shouldAlert = Time.GetUnixTime() > this.TimeLimitAlert;
            if (shouldAlert)
            {
                MessagePlayers($"You're running out of time!");
                this.TimeLimitAlert = null;
            }
                

            var shouldEnd = Time.GetUnixTime() > this.TimeLimit;
            if (shouldEnd)
            {
                MessagePlayers($"You're out of time!");
                this.ResetArena();
            }
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
            if (deadTeam && !this.EndSequence)
                this.StartEndSequence();
        }

        public void Init()
        {
            if (!this.Occupied)
            {
                log.Info("ARENAS: Generating Teams.");
                this.GenerateTeams();
                this.CountDownPhase = true;
                this.CountdownSeconds = (int)PropertyManager.GetDouble("arenas_countdown").Item;
                this.Occupied = true;
            }
        }

        public void Start()
        {
            this.TeleportTeams();
            this.TimeLimit = Time.GetFutureUnixTime((int)PropertyManager.GetDouble("arenas_time_limit").Item);
            this.TimeLimitAlert = Time.GetFutureUnixTime((int)PropertyManager.GetDouble("arenas_time_limit_alert").Item);
            this.Live = true;
        }

        public void Countdown()
        {
            var activePks = this.GetAllPlayers().Where(player => player.PKTimerActive).ToList();
            if(activePks.Count > 0)
            {
                var nonActivePks = this.GetAllPlayers().Where(player => !player.PKTimerActive).ToList();
                this.PlayerQueue.InsertRange(0, nonActivePks);
                this.ResetArena(true);
                MessagePlayers($"Queue has been reset because one of you bums got PK tagged during the countdown phase.");
                return;
            }
            
            var flagCountdown = ArenaCountdownTimer == null ? true : Time.GetUnixTime() > ArenaCountdownTimer;
            if (flagCountdown)
            {
                if (this.CurrentCountdownPhase == 0)
                    MessagePlayers($"You have been chosen for the arenas!! You will be teleported in {this.CountdownSeconds} seconds.");
                else if (this.CurrentCountdownPhase == 10)
                {
                    MessagePlayers($"Go fuck shit up!");
                    this.CountDownPhase = false;
                    this.Start();
                }
                else
                    MessagePlayers($"You will be teleported in {10 - this.CurrentCountdownPhase} seconds.");
                this.CurrentCountdownPhase += 1;
                this.ArenaCountdownTimer = Time.GetFutureUnixTime(1);
            }
        }

        public void StartEndSequence()
        {
            this.Live = false;
            this.EndSequence = true;
            this.WinBuffer = Time.GetFutureUnixTime((int)PropertyManager.GetDouble("arenas_win_buffer").Item);
            GetLivePlayers().ForEach(player => player.SendMessage($"Congrats! You have won! You will be teleported back to your lifestone in {(int)PropertyManager.GetDouble("arenas_win_buffer").Item} seconds."));
        }

        public void ResetArena(bool isPkTagged = false)
        {
            log.Info("ARENAS: Reset team players");
            if(!isPkTagged)
                this.GetLivePlayers().ForEach(player => player.ThreadSafeTeleportOnDeath());
            this.GetAllTeamPlayers().ForEach(tPlayer => this.ResetTeamPlayer(tPlayer));
            this.Team1 = new List<TeamPlayer>();
            this.Team2 = new List<TeamPlayer>();
            this.Occupied = false;
            this.CountDownPhase = false;
            this.CurrentCountdownPhase = 0;
            this.ArenaCountdownTimer = null;
            this.EndSequence = false;
        }

        public string QueuePlayer(Player player)
        {
            var arenasBlockedList = PropertyManager.GetString("arenas_blocked_list").Item ?? string.Empty;
            var levelReq = (int)PropertyManager.GetDouble("arenas_level_requirement").Item;
            var arenasBlockedListArray = arenasBlockedList.Split(",").ToList();
            if (arenasBlockedListArray.Contains(player.Guid.Full.ToString()))
                return "Nah.";

            if(player.IsNPK)
                return "PK Only you dolt.";

            if (player.Level < levelReq)
                return "You aren't high enough.";

            if (this.PlayerQueue.Contains(player))
                return $"You are already in the {this.ArenaType} queue.";

            var arena = ArenasManager.WhichArenaIsPlayerIn(player);
            if(arena != null)
                return $"You are already in the {arena.ArenaType} arena.";

            this.PlayerQueue.Add(player);

            return $"Queued up for {this.ArenaType} arenas. There are { this.PlayerQueue.Count } players in {this.ArenaType} queue.";
        }

        public void GenerateTeams()
        {
            var availablePlayers = this.ShufflePlayers(this.PlayerQueue).Take(this.CreateTeamsAt).ToList();
            availablePlayers = AddPlayerToTeam(this.Team1, availablePlayers);
            availablePlayers = AddPlayerToTeam(this.Team2, availablePlayers);
        }

        public void TeleportTeams()
        {
            log.Info("ARENAS: Teleporting Teams.");
            this.Team1.ForEach(tPlayer => tPlayer.player.Teleport(this.Team1Position));
            this.Team2.ForEach(tPlayer => tPlayer.player.Teleport(this.Team2Position));
        }

        public List<Player> AddPlayerToTeam(List<TeamPlayer> team, List<Player> totalAvailablePlayers)
        {
            var availablePlayers = totalAvailablePlayers.Take(this.TeamSize).ToList();
            availablePlayers.ForEach(player => {
                player.IsInArena = true;
                PlayerQueue.Remove(player);
                totalAvailablePlayers.Remove(player);
                var teamPlayer = new TeamPlayer();
                teamPlayer.player = player;
                team.Add(teamPlayer);
            });
            return totalAvailablePlayers;
        }

        public List<Player> ShufflePlayers(List<Player> availablePlayers)
        {
            Random rnd = new Random();
            return availablePlayers.OrderBy(x => rnd.Next()).ToList();
        }
    }
}
