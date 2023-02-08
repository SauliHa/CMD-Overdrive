using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Manticore
{
    public class ChromaManager : HackerManager
    {
        SpriteRenderer spriteRenderer;
        [SerializeField]
        private Sprite chromaNormal, chromaInvisible;
        private int cooldown = 0;
        public int Cooldown
        {
            get => cooldown; set
            {
                cooldown = value;
                photonView.Owner.SetCustomProp("Cooldown", value);
            }
        }
        private bool usedAbilityLastTurn = false;
        public bool UsedAbilityLastTurn
        {
            get => usedAbilityLastTurn; set
            {
                usedAbilityLastTurn = value;
                photonView.Owner.SetCustomProp("UsedAbilityLastTurn", value);
            }
        }
        [SerializeField]
        private bool invisible = false;
        public bool Invisible
        {
            get => invisible; set
            {
                invisible = value;
                photonView.Owner.SetCustomProp("Invisible", value);
            }
        }
        private bool activatedAbilityThisTurn = false;
        public bool ActivatedAbilityThisTurn
        {
            get => activatedAbilityThisTurn; set
            {
                activatedAbilityThisTurn = value;
                photonView.Owner.SetCustomProp("ActivatedAbilityThisTurn", value);
            }
        }
        protected override void Awake()
        {
            base.Awake();
            spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }

        public override void StartTurn()
        {
            base.StartTurn();
            if (Cooldown > 0)
            {
                if (UsedAbilityLastTurn) //Don't reduce cooldown if ability was used last turn
                    UsedAbilityLastTurn = false;
                else
                {
                    Cooldown--;
                }
                if (Cooldown == 0)
                {
                    UIManager.SetCoolDownText("Ability ready to use");
                    hackerUIManager.ToggleChromaAbilityButton(true);
                }
                else
                {
                    UIManager.SetCoolDownText("Cooldown: " + Cooldown + " round(s)");
                    hackerUIManager.ToggleChromaAbilityButton(false);
                }

            }
            else
            {
                UIManager.SetCoolDownText("Ability ready to use");
                hackerUIManager.ToggleChromaAbilityButton(true);
            }
        }
        public override void EndTurn()
        {
            if (ActivatedAbilityThisTurn)
            {
                ActivatedAbilityThisTurn = false;
            }
            else if (Invisible)
            {
                UIManager.OpenPrompt("Your shadow mode has ended!", "Your invisibility has run out and you can now be seen again by guards and cameras!");
                Invisible = false;
                spriteRenderer.sprite = chromaNormal;
                NetworkManager.photonView.RPC("OnChromaAbilityEnded", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
            }
            base.EndTurn();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            Player localPlayer = PhotonNetwork.LocalPlayer;
            if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
            {
                //If: startingtile not selected, or captured, or messagephase, or moving, or ability in cooldown, or no actions left => then disable button
                if (!startingTileSelected || isCaptured || moveMode || Cooldown > 0 || ActionCount <= 0)
                {
                    hackerUIManager.ToggleChromaAbilityButton(false);
                }
                else //otherwise enable it
                {
                    hackerUIManager.ToggleChromaAbilityButton(true);
                }
            }
            else
            {
                hackerUIManager.ToggleChromaAbilityButton(false);
            }

        }

        public void ActivateShadowMode()
        {
            if (ActionCount > 0)
            {
                ActionCount--;
                Cooldown = GeneralManager.ChromaAbilityCooldown;
                UIManager.SetCoolDownText("Cooldown: " + cooldown + " round(s)");
                Invisible = true;
                hackerUIManager.ToggleChromaAbilityButton(false);
                ActivatedAbilityThisTurn = true;
                UsedAbilityLastTurn = true;
                spriteRenderer.sprite = chromaInvisible;
                NetworkManager.photonView.RPC("OnChromaAbilityTriggered", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
                CheckForTurnEnding();
            }
            else
            {
                Debug.LogError("Chroma action can't be used if there is no actions left. This shouldn't be called in that case");
            }
        }
        public override void SyncStateWithServer()
        {
            base.SyncStateWithServer();
            Invisible = (bool)photonView.Owner.GetCustomProp("Invisible");
            Cooldown = (int)photonView.Owner.GetCustomProp("ChromaCooldown");
        }

        public override void SyncServerWithState()
        {
            base.SyncServerWithState();
            photonView.Owner.SetCustomProp("Invisible", Invisible);
            photonView.Owner.SetCustomProp("ChromaCooldown", Cooldown);
            photonView.Owner.SetCustomProp("UsedAbilityLastTurn", UsedAbilityLastTurn);
            photonView.Owner.SetCustomProp("ActivatedAbilityThisTurn", ActivatedAbilityThisTurn);
        }
    }
}
