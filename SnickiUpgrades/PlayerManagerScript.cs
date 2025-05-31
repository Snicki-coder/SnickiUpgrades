using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using REPOLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnickiUpgrades
{
    [HarmonyPatch(typeof(RunManager))]
    internal class PlayerManagerScript
    {
        private static System.Random rng = new System.Random();
        private static List<Player> _players = new List<Player>();

        private static bool playerRevived = false;

        [HarmonyPatch("ChangeLevel")]
        private static void RunManagerChangeLevelPostfix()
        {
            if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient())
                return;
            ApplyPatch();
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateLevel")]
        private static void RunManagerUpdateLevelPostfix()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
                return;
            ApplyPatch();
        }

        private static void ApplyPatch()
        {
            if (SemiFunc.RunIsShop() && SemiFunc.IsMasterClientOrSingleplayer())
            {
                SnickiUpgrades.Logger.LogDebug("Dies ist der Shop");
            }
            else if (SemiFunc.RunIsLevel())
            {
                SnickiUpgrades.Logger.LogDebug("Dies ist ein Level");
                playerRevived = false;
                _players = PhotonNetwork.PlayerList.ToList();
            }
            else
            {
                if (!SemiFunc.RunIsArena())
                    return;
                SnickiUpgrades.Logger.LogDebug("Dies ist die Arena");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerHealth), "Death")]
        private static void PlayerDied()
        {
            SnickiUpgrades.Logger.LogDebug("Player Died");

            var players = GameObject.FindObjectsOfType<PlayerAvatar>();

            List<PlayerAvatar> playersWithUpgrade = new List<PlayerAvatar>();
            foreach (var dic in Upgrades.GetUpgrade("Last Chance").PlayerDictionary)
            {
                if (dic.Value < 1)
                    continue;

                var steamPlayer = players.Where(x => x.steamID == dic.Key).ToList();
                if(steamPlayer.Count() > 0 && steamPlayer[0])
                    playersWithUpgrade.Add(steamPlayer[0]);
            }

            if (!players.Any(x => x.playerHealth.health > 0) && playerRevived == false)
            {
                PlayerAvatar revivedPlayer = players[rng.Next(0, players.Count() - 1)];
                RevivePlayer(revivedPlayer);

                SnickiUpgrades.LastChanceUpgradeRegister.RemoveLevel(revivedPlayer.steamID);
                playerRevived = true;
            }
        }

        private async static void RevivePlayer(PlayerAvatar player)
        {
            Component teleporter;
            if(!player.TryGetComponent(typeof(Teleporter), out teleporter))
                teleporter = player.gameObject.AddComponent<Teleporter>();

            await Task.Delay(1250);
            player.playerHealth.health = player.playerHealth.maxHealth;
            player.playerHealth.InvincibleSet(1.5f);
            player.Revive(true);
            SnickiUpgrades.Logger.LogInfo($"Revived player named => {player.playerName}");
            await Task.Delay(100);

            if(RoundDirector.instance.allExtractionPointsCompleted)
            {
                SnickiUpgrades.Logger.LogDebug("Teleporting to random Extract");
                var extractpoints = RoundDirector.instance.extractionPointList;

                var truckdoor = SnickiUpgrades.FindObjectOfType<TruckDoor>();

                GameObject selectedPoint = null;
                if (extractpoints.Count > 1)
                    extractpoints.Remove(truckdoor.extractionPointNearest.gameObject);

                selectedPoint = extractpoints[rng.Next(0, extractpoints.Count() - 1)];

                ((Teleporter)teleporter).MoveToObject(player, selectedPoint);
            }
            else
            {
                SnickiUpgrades.Logger.LogDebug("Teleporting to Truck");
                ((Teleporter)teleporter).MoveToTruck(player);
            }


        }
    }
}
