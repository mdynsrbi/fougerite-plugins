using System;
using System.Linq;
using Fougerite;
using Fougerite.Events;

namespace FriendlyFire
{
    public class FriendlyFire : Fougerite.Module
    {
        public override string Name { get { return "FriendlyFire"; } }
        public override string Author { get { return "Yasin"; } }
        public override string Description { get { return "Friendly fire system based on Rust++"; } }
        public override Version Version { get { return new Version("1.0"); } }

        public DataStore ds = DataStore.GetInstance();

        public override void Initialize()
        {
            Hooks.OnCommand += OnCommand;
            Hooks.OnPlayerHurt += OnPlayerHurt;
        }

        public override void DeInitialize()
        {
            Hooks.OnCommand -= OnCommand;
            Hooks.OnPlayerHurt -= OnPlayerHurt;
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd.ToLower() == "friendlyfire" || cmd.ToLower() == "ff")
            {
                if (args.Length == 0)
                    player.MessageFrom("Friends", "Usage: /friendlyfire(ff) [on/off]");
                else
                {
                    string c = args[0];
                    var d = args.ToList();
                    d.Remove(args[0]);
                    string s = string.Join(" ", d.ToArray());
                    switch (c)
                    {
                        case "on":
                            if (ds.ContainsKey("FriendlyFire", player.UID))
                            {
                                player.MessageFrom("Friends", "Friendly fire already enabled");
                                return;
                            }
                            else
                            {
                                player.MessageFrom("Friends", "Friendly fire enabled");
                                ds.Add("FriendlyFire", player.UID, "true");
                            }
                            break;
                        case "off":
                            if (!ds.ContainsKey("FriendlyFire", player.UID))
                            {
                                player.MessageFrom("Friends", "Friendly fire already disabled");
                                return;
                            }
                            else
                            {
                                player.MessageFrom("Friends", "Friendly fire disabled");
                                ds.Remove("FriendlyFire", player.UID);
                            }
                            break;
                    }
                }
            }
        }

        public void OnPlayerHurt(HurtEvent he)
        {
            if (he.AttackerIsPlayer && !he.VictimIsSleeper)
            {
                Fougerite.Player attacker = (Fougerite.Player)he.Attacker;
                Fougerite.Player victim = (Fougerite.Player)he.Victim;
                var list = Server.GetServer().GetRustPPAPI().FriendsOf(attacker.UID);
                if (list != null)
                {
                    if (list.isFriendWith(victim.UID))
                    {
                        if (!ds.ContainsKey("FriendlyFire", attacker.UID))
                        {
                            attacker.InventoryNotice("This is your friend!");
                            he.DamageAmount = 0;
                        }
                    }
                }
            }
        }
    }
}
