using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Manticore
{
    public class ShiftManager : HackerManager
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
        private bool usedAbilityLastTurn = false;
        public bool UsedAbilityLastTurn
        {
            get => usedAbilityLastTurn; set
            {
                usedAbilityLastTurn = value;
                photonView.Owner.SetCustomProp("UsedAbilityLastTurn", value);
            }
        }

        //private bool alreadyMovedThisTurn = false;
        /*public bool AlreadyMovedThisTurn
        {
          get => alreadyMovedThisTurn; set
          {
            alreadyMovedThisTurn = value;
            photonView.Owner.SetCustomProp("AlreadyMovedThisTurn", value);
          }
        }*/

        //At the start of each round, reduce cooldown and reset variables
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
                    if (Cooldown == 0)
                        UIManager.SetCoolDownText("Ability ready to use");
                    else
                        UIManager.SetCoolDownText("Cooldown: " + Cooldown + " round(s)");
                }
            }
            else
            {
                UIManager.SetCoolDownText("Ability ready to use");
            }
            //AlreadyMovedThisTurn = false;
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            Player localPlayer = PhotonNetwork.LocalPlayer;
            if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
            {
                if (startingTileSelected && !isCaptured && Cooldown == 0 && !MoveMode)
                {
                    hackerUIManager.ToggleShiftAbilityButton(true);
                }
                else
                {
                    hackerUIManager.ToggleShiftAbilityButton(false);
                }
            }
            else
            {
                hackerUIManager.ToggleShiftAbilityButton(false);
            }
        }

        public void ActivateExtraMove()
        {
            MoveCount++;
            Cooldown = GeneralManager.ShiftAbilityCooldown;
            UIManager.SetCoolDownText("Cooldown: " + cooldown + " round(s)");
            IsSendingMessage = false;
            UpdateUI();
        }
        /*
        public override void UseTurn(bool isAction)
        {
          base.UseTurn(isAction);
          if (!isAction)
            CheckIfSecondMove();

        }*/

        //This is called when moving to tiles or through doors
        //THIS WAS USED IN OLD SHIFT IMPLEMENTATION, NOT USED CURRENTLY
        /*
        private void CheckIfSecondMove()
        {
          if (MoveCount >= 1)//if moves left, don't do anything (for games with multiple move points)
            return;

          if (AlreadyMovedThisTurn) //Has already moved this turn, put the ability on cooldown
          {
            Cooldown = GeneralManager.ShiftAbilityCooldown;
            UsedAbilityLastTurn = true;
            UIManager.SetCoolDownText("Cooldown: " + cooldown + " round(s)");
            IsSendingMessage = false;
            UseTurn(true);
          }
          else if (MoveCount == 0 && ActionCount == 0) //If out of actions and movepoints, go to message phase ( case: use action and then move)
          {
            IsSendingMessage = false;
            CheckForTurnEnding();
          }
          else if (Cooldown == 0) //If ability is not on cooldown, allow second move
          {
            AlreadyMovedThisTurn = true;
            IsSendingMessage = false;
            UpdateUI();
          }

        }*/
        public override void SyncStateWithServer()
        {
            base.SyncStateWithServer();
            Cooldown = (int)photonView.Owner.GetCustomProp("ShiftCooldown");
            //AlreadyMovedThisTurn = (bool)photonView.Owner.GetCustomProp("AlreadyMovedThisTurn");
            UsedAbilityLastTurn = (bool)photonView.Owner.GetCustomProp("UsedAbilityLastTurn");
        }

        public override void SyncServerWithState()
        {
            base.SyncServerWithState();
            photonView.Owner.SetCustomProp("ShiftCooldown", Cooldown);
            //photonView.Owner.SetCustomProp("AlreadyMovedThisTurn", AlreadyMovedThisTurn);
            photonView.Owner.SetCustomProp("UsedAbilityLastTurn", UsedAbilityLastTurn);
        }
    }
}

