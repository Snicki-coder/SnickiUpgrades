using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace SnickiUpgrades
{
    internal class Teleporter : MonoBehaviourPun
    {
        public static Teleporter Instance;
        private PhotonView photonView;

        private void Awake()
        {
            if ((UnityEngine.Object)Teleporter.Instance != (UnityEngine.Object)null && (UnityEngine.Object)Teleporter.Instance != (UnityEngine.Object)this)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
            }
            else
            {
                Teleporter.Instance = this;
                this.photonView = this.AddComponent<PhotonView>();
            }
        }

        public void MoveToTruck(PlayerAvatar player)
        {
            SnickiUpgrades.Logger.LogDebug("Mocing to Truck");

            if (SemiFunc.IsMultiplayer())
                player.photonView.RPC("ForceTeleportRPC", RpcTarget.All, (object)player.photonView.ViewID, (object)LevelGenerator.Instance.LevelPathTruck.transform.position, (object)LevelGenerator.Instance.LevelPathTruck.transform.rotation);
            else
                ForceTeleportRPC(player.photonView.ViewID, LevelGenerator.Instance.LevelPathTruck.transform.position, LevelGenerator.Instance.LevelPathTruck.transform.rotation);
        }

        public void MoveToObject(PlayerAvatar player, GameObject gameObject)
        {
            if (SemiFunc.IsMultiplayer())
                player.photonView.RPC("ForceTeleportRPC", RpcTarget.All, (object)player.photonView.ViewID, gameObject.transform.position, gameObject.transform.rotation);
            else
                ForceTeleportRPC(player.photonView.ViewID, gameObject.transform.position, gameObject.transform.rotation);

        }

        [PunRPC]
        public void ForceTeleportRPC(int photonViewId, Vector3 position, Quaternion rotation)
        {
            ForceTeleportRPC_internal(SemiFunc.PlayerAvatarGetFromPhotonID(photonViewId), position, rotation);
        }

        private void ForceTeleportRPC_internal(
            PlayerAvatar player,
            Vector3 position,
            Quaternion rotation)
        {
            player.transform.position = position;
            player.transform.rotation = rotation;
            player.clientPosition = position;
            player.clientRotation = rotation;
            player.clientPositionCurrent = position;
            player.clientRotationCurrent = rotation;
            if (!player.isLocal || !(PlayerController.instance != null))
                return;
            PlayerController instance = PlayerController.instance;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.rb.velocity = Vector3.zero;
            instance.rb.angularVelocity = Vector3.zero;
            instance.rb.MovePosition(position);
            instance.rb.MoveRotation(rotation);
            instance.InputDisable(0.1f);
            instance.CollisionController?.ResetFalling();
        }
    }
}
