using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  /// <summary>
  /// Top manager class for main menus. Handles toggling different panels open, when specific networking events occur.
  /// </summary>
  public class MainMenuManager : MonoBehaviourPunCallbacks
  {
    [SerializeField]
    private MainPanelManager mainPanel;
    [SerializeField]
    private GameObject notConnectedPanel, roleSelectionPanel, gameOverPanel, quitGamePanel;
    [SerializeField]
    private EnterNamePanelManager enterNamePanel;
    [SerializeField]
    private WaitingRoomPanelManager waitingRoomPanel;
    [SerializeField]
    private RoleUI roleUI;
    private Button roleButton;
    private void Start()
    {
      mainPanel.gameObject.SetActive(false);
      enterNamePanel.gameObject.SetActive(false);
      waitingRoomPanel.gameObject.SetActive(false);
      roleSelectionPanel.SetActive(false);
      mainPanel.gameObject.SetActive(false);
      notConnectedPanel.gameObject.SetActive(true);
    }

    // If not connected, activate not connected panel, otherwise go to main menu
    public void ActivateInitialMenuState()
    {
      mainPanel.gameObject.SetActive(false);
      enterNamePanel.gameObject.SetActive(false);
      waitingRoomPanel.gameObject.SetActive(false);
      roleSelectionPanel.SetActive(false);
      mainPanel.gameObject.SetActive(PhotonNetwork.IsConnected && !GeneralManager.HasPlayedAGame);
      mainPanel.ClearInputState(); // In case something is left to input from before
      notConnectedPanel.gameObject.SetActive(!PhotonNetwork.IsConnected);
      gameOverPanel.SetActive(GeneralManager.HasPlayedAGame);
    }


    public override void OnJoinedRoom()
    {
      notConnectedPanel.gameObject.SetActive(false);
      mainPanel.gameObject.SetActive(false);
      enterNamePanel.gameObject.SetActive(false);
      // Check if the nickname you joined with already exists and add actor number to name if so to separate every client
      // NOTE: This should be checked in EnterNamePanelManager when joining, but Photon doesn't provide a way to get players list
      // before joining a room so this should be done manually. This is a quick fix solution.
      if (PhotonUtils.GetOtherPlayerInCurrentRoom(PhotonNetwork.LocalPlayer.NickName) != null)
      {
        PhotonNetwork.LocalPlayer.NickName += "-" + PhotonNetwork.LocalPlayer.ActorNumber;
      }
      // Default role
      roleUI.UpdateSelectedRoles();
      waitingRoomPanel.ShowPanel();
      if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"))
      {
        waitingRoomPanel.ClearRoleSelection();
      }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
      waitingRoomPanel.UpdatePlayerListing();
    }
    public void SetCorporate()
    {
      PhotonNetwork.LocalPlayer.SetRole(RoleType.Corporation);
      ShowWaitingRoom(0);
    }

    public void SetEdge()
    {
      PhotonNetwork.LocalPlayer.SetRole(RoleType.Edge);
      ShowWaitingRoom(1);
    }

    public void SetShift()
    {
      PhotonNetwork.LocalPlayer.SetRole(RoleType.Shift);
      ShowWaitingRoom(2);
    }

    public void SetBugz()
    {
      PhotonNetwork.LocalPlayer.SetRole(RoleType.Bugz);
      ShowWaitingRoom(3);
    }

    public void SetChroma()
    {
      PhotonNetwork.LocalPlayer.SetRole(RoleType.Chroma);
      ShowWaitingRoom(4);
    }
    public void ShowWaitingRoom(int roleNumber)
    {
      photonView.RPC("DisableRoleButton", RpcTarget.Others, roleNumber, PhotonNetwork.LocalPlayer);
      roleSelectionPanel.SetActive(false);
    }

    public void OpenQuitGamePanel()
    {
      quitGamePanel.SetActive(true);
    }
    
    public void CloseQuitGamePanel()
    {
      quitGamePanel.SetActive(false);
    }
    
    public void QuitApplication()
    {
      Application.Quit();
    }
  }
}
