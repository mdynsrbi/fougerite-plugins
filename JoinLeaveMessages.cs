using System;
using System.IO;
using Fougerite;
using Fougerite.Events;

namespace JoinLeaveMessages
{
    public class JoinLeaveMessages : Fougerite.Module
    {
        public override string Name { get { return "JoinLeaveMessages"; } }
        public override string Author { get { return "Yasin"; } }
        public override string Description { get { return "Sends a message when a player joins or leaves the server"; } }
        public override Version Version { get { return new Version("1.0"); } }

		public bool notice = true;
        public string sysname = "JoinLeaveMessages";
        public string noticetext = "Welcome to our Server!";
        public string joinmsg = "{player} has joined the server";
        public string leavemsg = "{player} has left the server";

        public IniParser config;
        public DataStore ds = DataStore.GetInstance();

        public override void Initialize()
        {
            Hooks.OnPlayerConnected += OnPlayerConnected;
            Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            Hooks.OnPlayerSpawned += OnPlayerSpawned;
            LoadConfig();
        }

        public override void DeInitialize()
        {
            Hooks.OnPlayerConnected -= OnPlayerConnected;
            Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
            Hooks.OnPlayerSpawned -= OnPlayerSpawned;
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            ds.Add("JoiningServer", player.UID, "joined");
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (ds.ContainsKey("JoiningServer", player.UID))
                ds.Remove("JoiningServer", player.UID);
            else
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                Server.GetServer().BroadcastFrom(sysname, leavemsg.Replace("{player}", player.Name));
            }
        }

        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (ds.ContainsKey("JoiningServer", player.UID))
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                Server.GetServer().BroadcastFrom(sysname, joinmsg.Replace("{player}", player.Name));
				if (notice)
					player.Notice(noticetext);
                ds.Remove("JoiningServer", player.UID);
            }
        }

        private void LoadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Config.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Config.ini")).Dispose();
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));

                config.AddSetting("Configuration", "Notice", "true");
                config.AddSetting("Configuration", "SysName", "JoinLeaveMessages");
                config.AddSetting("Configuration", "NoticeText", "Welcome to our Server!");
                config.AddSetting("Configuration", "JoinMessage", "{player} has joined the server");
                config.AddSetting("Configuration", "LeaveMessage", "{player} has left the server");
                config.Save();
                Logger.Log("[JoinLeaveMessages]: New configuration file generated!");
                LoadConfig();
            }
            else
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                try
                {
					notice = config.GetBoolSetting("Configuration", "Notice");
                    sysname = config.GetSetting("Configuration", "SysName");
                    noticetext = config.GetSetting("Configuration", "NoticeText");
                    joinmsg = config.GetSetting("Configuration", "JoinMessage");
                    leavemsg = config.GetSetting("Configuration", "LeaveMessage");
                    Logger.Log("[JoinLeaveMessages]: Configuration file loaded!");
                }
                catch (Exception ex)
                {
                    Logger.LogError("[JoinLeaveMessages]: Detected a problem in the configuration");
                    Logger.Log("[ERROR]: " + ex.Message);
                    File.Delete(Path.Combine(ModuleFolder, "Config.ini"));
                    Logger.LogError("[JoinLeaveMessages]: Deleted the old configuration file");
                    LoadConfig();
                }
            }
        }
    }
}

