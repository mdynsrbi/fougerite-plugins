using Fougerite;
using Fougerite.Events;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace HighPingKicker
{
    public class HighPingKicker : Fougerite.Module
    {
        public override string Name { get { return "HighPingKicker"; } }
        public override string Author { get { return "Yasin"; } }
        public override string Description { get { return "Kicks players with high ping"; } }
        public override Version Version { get { return new Version("1.0"); } }

        public int maxping = 750;
        public int kicktimer = 120000;

        public IniParser config;
        public StreamWriter file;
        public List<string> Players = new List<string>();

        private Dictionary<ulong, Vector3> lastloc = new Dictionary<ulong, Vector3> { };

        public override void Initialize()
        {
            Hooks.OnPlayerConnected += OnPlayerConnected;
            Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            Hooks.OnServerInit += OnServerInit;
            LoadConfig();
        }

        public override void DeInitialize()
        {
            Hooks.OnPlayerConnected -= OnPlayerConnected;
            Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
            Hooks.OnServerInit -= OnServerInit;
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            Players.Add(player.SteamID);
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (Players.Contains(player.SteamID))
                Players.Remove(player.SteamID);
        }

        public void OnServerInit()
        {
            Timer timer = new Timer();
            timer.Interval = kicktimer;
            timer.AutoReset = true;
            timer.Elapsed += delegate (object x, ElapsedEventArgs y)
            {
                foreach (string pl in Players)
                {
                    Fougerite.Player player = Server.GetServer().FindPlayer(pl);
                    if (player.IsOnline)
                    {
                        if (player.Ping >= maxping)
                        {
                            player.Notice("Your ping: " + player.Ping);
                            player.Message("You were kicked from server due to high ping");
                            player.Disconnect();
                        }
                    }
                }
            };
            timer.Start();
        }

        private void LoadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Config.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Config.ini")).Dispose();
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));

                config.AddSetting("Configuration", "MaxPing", "750");
                config.AddSetting("Configuration", "KickTimer", "120000");
                config.Save();
                Logger.Log("[HighPingKicker]: New configuration file generated");
                LoadConfig();
            }
            else
            {
                config = new IniParser(Path.Combine(ModuleFolder, "Config.ini"));
                try
                {
                    maxping = Convert.ToInt32(config.GetSetting("Configuration", "MaxPing"));
                    kicktimer = Convert.ToInt32(config.GetSetting("Configuration", "KickTimer"));
                    Logger.Log("[HighPingKicker]: Configuration file loaded");
                }
                catch (Exception ex)
                {
                    Logger.LogError("[HighPingKicker]: Detected a problem in the configuration");
                    Logger.Log("[ERROR]: " + ex.Message);
                    File.Delete(Path.Combine(ModuleFolder, "Config.ini"));
                    Logger.LogError("[HighPingKicker]: Deleted the old configuration file");
                    LoadConfig();
                }
            }
        }
    }
}
