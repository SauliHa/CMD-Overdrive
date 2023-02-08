using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Manticore
{
    public class EdgeManager : HackerManager
    {

        private int cooldown = 0;
        public int Cooldown
        {
            get => cooldown; set
            {
                cooldown = value;
                photonView.Owner.SetCustomProp("Cooldown", value);
            }
        }
        private bool cooldownStartedLastTurn = false;
        public bool CooldownStartedLastTurn
        {
            get => cooldownStartedLastTurn; set
            {
                cooldownStartedLastTurn = value;
                photonView.Owner.SetCustomProp("CooldownStartedLastTurn", value);
            }
        }
        private TileManager hologramTile;
        public TileManager HologramTile
        {
            get => hologramTile; set
            {
                hologramTile = value;
                if (value != null)
                    photonView.Owner.SetCustomProp("HologramTileID", value.photonView.ViewID);
                else
                    photonView.Owner.SetCustomProp("HologramTileID", null);
            }
        }

        private bool hologramHasBeenSeen = false;
        public bool HologramHasBeenSeen { get => hologramHasBeenSeen; set => hologramHasBeenSeen = value; }


        /* OLD EDGE CODE FOR EXTRA MOVE WHEN RELEASING TEAMMATES
        public override void ReleaseTeammates()
        {
          base.ReleaseTeammates();
          MoveCount++;
          ActionCount++;
          UIManager.OpenPrompt("Extra turn!", "Releasing a fellow hacker has filled you with adrenaline and granted you a extra move and action!");
          IsSendingMessage = false;
          UpdateUI();
        }*/

        protected override void Awake()
        {
            base.Awake();
            HologramTile = null;
        }
        public override void StartTurn()
        {
            base.StartTurn();
            if (Cooldown > 0)
            {
                if (CooldownStartedLastTurn) //Don't reduce cooldown if ability was used last turn
                    CooldownStartedLastTurn = false;
                else
                {
                    Cooldown--;
                }
                if (Cooldown == 0)
                {
                    UIManager.SetCoolDownText("Ability ready to use");
                    hackerUIManager.ToggleEdgeAbilityButton(true);
                }
                else
                {
                    UIManager.SetCoolDownText("Cooldown: " + Cooldown + " round(s)");
                    hackerUIManager.ToggleEdgeAbilityButton(false);
                }

            }
            else
            {
                UIManager.SetCoolDownText("Ability ready to use");
                hackerUIManager.ToggleEdgeAbilityButton(true);
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            Player localPlayer = PhotonNetwork.LocalPlayer;
            if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
            {
                Debug.Log("sending message: " + IsSendingMessage);
                //If: startingtile not selected, or captured, or messagephase, or moving, or ability in cooldown, or no actions left => then disable button
                if (!startingTileSelected || isCaptured || MoveMode || Cooldown > 0 || ActionCount <= 0)
                {
                    hackerUIManager.ToggleEdgeAbilityButton(false);
                }
                else //otherwise enable it
                {
                    hackerUIManager.ToggleEdgeAbilityButton(true);
                }
            }
            else
            {
                hackerUIManager.ToggleEdgeAbilityButton(false);
            }

        }
        
       // public override void Edge

        public void DeployHologram()
        {
            HologramTile = CurrentTile;
            GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
            SetHologramIconPosition();
            UIManager.OpenPrompt("Hologram created!", "You placed a hologram of yourself to " + HologramTile.gameObject.name + ". It will continue to be there until you destroy it or guard walks into the room.");
            photonView.RPC("OnEdgeHologramDeployed", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
        }

        public void DestroyHologram()
        {
            NetworkManager.photonView.RPC("OnEdgeHologramDestroyed", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, true);
        }

        public void SetCooldown()
        {
            Cooldown = GeneralManager.EdgeAbilityCooldown;
            UIManager.SetCoolDownText("Cooldown: " + Cooldown + " round(s)");
            hackerUIManager.ToggleEdgeAbilityButton(false);
            CooldownStartedLastTurn = true;
        }
        public void HideHologram()
        {
            GraphicsUtils.ChangeAlpha(hologramSprite, 0f);
            hackerUIManager.EdgeHologramDestroyed();
        }
        public void SetHologramIconPosition()
        {
            GameObject hologramMoveTarget = HologramTile.transform.GetChild(2).gameObject;
            edgeHologram.transform.position = hologramMoveTarget.transform.position;
        }

        public override void SyncStateWithServer()
        {
            base.SyncStateWithServer();
            Cooldown = (int)photonView.Owner.GetCustomProp("EdgeCooldown");
            object hologramTileID = photonView.Owner.GetCustomProp("HologramTile");
            if (hologramTileID != null)
            {
                HologramTile = PhotonNetwork.GetPhotonView((int)hologramTileID).GetComponent<TileManager>();
                SetHologramIconPosition();
            }
        }

        public override void SyncServerWithState()
        {
            base.SyncServerWithState();
            photonView.Owner.SetCustomProp("EdgeCooldown", Cooldown);
            photonView.Owner.SetCustomProp("CooldownStartedLastTurn", CooldownStartedLastTurn);
            if (HologramTile != null)
                photonView.Owner.SetCustomProp("HologramTileID", HologramTile.photonView.ViewID);
        }

    }
}
