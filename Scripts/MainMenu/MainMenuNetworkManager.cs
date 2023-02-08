using System;
using System.Collections;
using System.Collections.Generic;
using Manticore;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class MainMenuNetworkManager : MonoBehaviourPun
{
  [SerializeField]
  private RoleUI roleUI;
  [SerializeField]
  private GameSettingsManager gameSettingsManager;

  [PunRPC]
  private void DisableRoleButton(int roleNumber, Player player)
  {
    roleUI.DisableButtonAndSetText(roleNumber, player);
  }

  [PunRPC]
  private void EnableRoleButton(int roleNumber)
  {
    roleUI.EnableButtonAndCleartext(roleNumber);
  }
  
  [PunRPC]
  private void SetHackerMoves(int moves)
  {
    GeneralManager.PlayerMovesInTurn = moves;
  }
  
  [PunRPC]
  private void SetHackerActions(int actions)
  {
    GeneralManager.PlayerActionsInTurn = actions;
  }
  
  [PunRPC]
  private void SetCorporationMoves(int moves)
  {
    GeneralManager.CorporationMovesInTurn = moves;
  }
  
  [PunRPC]
  private void SetCorporationActions(int actions)
  {
    GeneralManager.CorporationActionsInTurn = actions;
  }
  
  [PunRPC]
  private void SetShiftCD(int cd)
  {
    GeneralManager.ShiftAbilityCooldown = cd;
  }

  [PunRPC]
  private void SetChromaCD(int cd)
  {
    GeneralManager.ChromaAbilityCooldown = cd;
  }

  [PunRPC]
  private void SetWindowMoveChance(int chance)
  {
    GeneralManager.WindowMoveChance = chance;
  }

  [PunRPC]
  private void SetDoorMoveChance(int chance)
  {
    GeneralManager.DoorMoveChance = chance;
  }

  [PunRPC]
  private void SetLockHackChance(int chance)
  {
    GeneralManager.CEOLockHackChance = chance;
  }

  [PunRPC]
  private void SetFalseAlarmChance(int chance)
  {
    GeneralManager.FalseAlarmChance = chance;
  }

  [PunRPC]
  private void SetCEOComputerHackChance(int chance)
  {
    GeneralManager.CEOComputerHackChance = chance;

  }
  
  [PunRPC]
  private void SetGuardCount(int count)
  {
    GeneralManager.START_AMOUNT_OF_GUARDS = count;
  }
  
  [PunRPC]
  private void SetLockedDoors(int count)
  {
    GeneralManager.MaxLockedDoorsAmount = count;
  }
  
  [PunRPC]
  private void SetMotionDetectors(int count)
  {
    GeneralManager.MaxMotionDetectorsAmount = count;
  }
  
  [PunRPC]
  private void ActionToMoves(bool actionToMoves)
  {
    GeneralManager.ActionToMoves = actionToMoves;
  }
  
}
