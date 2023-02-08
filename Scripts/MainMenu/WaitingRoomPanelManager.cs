using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Manticore
{
  /// <summary>
  /// Handles actions and events in waiting room panel. These are player joining a room and host starting the game.
  /// </summary>
  public class WaitingRoomPanelManager : MonoBehaviourPunCallbacks
  {
    [SerializeField]
    private TextMeshProUGUI waitingRoomTitle;
    [SerializeField]
    private PlayerListingManager playerListingPrefab;
    [SerializeField]
    private GameObject joinedPlayersContainer;
    [SerializeField]
    private GameObject roleSelection;
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private Button changeSettingsButton;
    [SerializeField]
    private GameSettingsManager gameSettings;
    [SerializeField]
    private RoleUI roleUi;

    private List<PlayerListingManager> listings = new List<PlayerListingManager>();
    private PlayerListingManager listing;
    public void UpdatePlayerListing()
    {
      ClearPlayerListings();
      bool haveAllPlayersSelectedRoles = true;
      foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
      {
        Player player = playerInfo.Value;
        AddPlayerListing(player);
        if (!player.CustomProperties.ContainsKey("Role") || player.CustomProperties["Role"] == null) haveAllPlayersSelectedRoles = false;
      }
      // Allow game start based on whether all players have selected roles
      startGameButton.interactable = haveAllPlayersSelectedRoles;
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
      UpdatePlayerListing();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
      UpdateHostInfo();
    }

    public void AddPlayerListing(Player player)
    {
      listing = Instantiate(playerListingPrefab);
      listing.transform.SetParent(joinedPlayersContainer.transform, false);
      listings.Add(listing);
      listing.SetPlayerInfo(player);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
      UpdatePlayerListing();
    }

    public void RemovePlayer(Player player)
    {
      PlayerListingManager removedListing = listings.Find((listing) => listing.Player == player);
      if (removedListing != null)
      {
        Destroy(removedListing.gameObject);
        listings.Remove(removedListing);
      }
    }
    public void StartGame()
    {
      if (PhotonNetwork.IsMasterClient)
      {
        gameSettings.InformSettings();
        GeneralManager.LoadScene(GeneralManager.MainSceneName);
      }
    }
    public void ShowPanel()
    {
      gameObject.SetActive(true);
      //roleSelection.SetActive(!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role"));
      roleSelection.SetActive(true);
      UpdateHostInfo();
    }

    public void UpdateHostInfo()
    {
      startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
      changeSettingsButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
      if (PhotonNetwork.IsMasterClient)
      {
        waitingRoomTitle.text = "Waiting for other players...";
      }
      else
      {
        waitingRoomTitle.text = "Waiting for host to start the game...";
      }
      UpdatePlayerListing();
    }

    public void ChangeRole()
    {
      ClearRoleSelection();
      roleSelection.SetActive(true);
    }

    public void ClearRoleSelection()
    {
      Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
      int localPlayerRole = (int)PhotonNetwork.LocalPlayer.GetRole();
      PhotonView ph = PhotonNetwork.GetPhotonView(1);
      ph.RPC("EnableRoleButton", RpcTarget.Others, localPlayerRole);
      hash["Role"] = null;
      PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void changeSettings()
    {
      gameSettings.gameObject.SetActive(true);
    }

    private void ClearPlayerListings()
    {
      foreach (Transform availableGame in joinedPlayersContainer.transform)
      {
        Destroy(availableGame.gameObject);
      }
    }
    public void LeaveGame()
    {
      // Clear LastRoom cache so OnConnectedToMaster will not try reconnect back to this
      ClearRoleSelection();
      PlayerPrefs.DeleteKey("LastRoom");
      PhotonNetwork.LeaveRoom(false);
    }
  }
}
