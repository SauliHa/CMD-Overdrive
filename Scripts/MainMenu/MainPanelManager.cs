using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  /// <summary>
  /// Handles actions in Main panel. These are showing available games and creating a new game.
  public class MainPanelManager : MonoBehaviourPunCallbacks
  {
    [SerializeField]
    private InputField gameNameInput;
    [SerializeField]
    private GameListingManager gameListing;
    [SerializeField]
    private TextMeshProUGUI errorText;
    [SerializeField]
    private GameObject availableGamesContainer;
    [SerializeField]
    private EnterNamePanelManager enterNamePanel;
    private List<GameListingManager> listings = new List<GameListingManager>();
    private void Awake()
    {
      errorText.gameObject.SetActive(false);
      ClearGameListings();
      gameNameInput.interactable = true;
    }
    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Return) && !enterNamePanel.gameObject.activeInHierarchy)
      {
        CreateGame();
      }
    }

    // TODO: Update room list on roomlistproperties IsPlaying update
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
      foreach (RoomInfo roomInfo in roomList)
      {
        GameListingManager matchingListing = listings.Find((listing) => listing.RoomInfo.Name == roomInfo.Name);
        if (roomInfo.CustomProperties.ContainsKey("IsPlaying") || roomInfo.RemovedFromList)
        {
          if (matchingListing != null)
          {
            Destroy(matchingListing.gameObject);
            listings.Remove(matchingListing);
          }
        }
        else if (matchingListing == null)
        {
          GameListingManager listing = Instantiate(gameListing);
          listing.transform.SetParent(availableGamesContainer.transform, false);
          listings.Add(listing);
          listing.SetGameInfo(roomInfo, () =>
          {
            enterNamePanel.TogglePanel(roomInfo.Name);
          });
        }
      }
    }
    public void CreateGame()
    {
      // Check if photon is connected correctly
      if (PhotonNetwork.IsConnected)
      {
        string gameName = gameNameInput.text;
        if (gameName.Length == 0)
        {
          SetErrorText("You must put a name to create a room");
        }
        else if (gameName.Length > 20)
        {
          SetErrorText("The name of the room must not contain more than 20 letters");
        }
        else if (listings.FindIndex((listing) => listing.RoomInfo.Name == gameName) >= 0)
        {
          SetErrorText("Game with name '" + gameName + "' already exists");
        }
        else if (PhotonNetwork.CountOfRooms >= 4)
        {
          SetErrorText("Sorry, but the maximum amount of rooms already exist");
        }
        else
        {
          errorText.gameObject.SetActive(false);
          gameNameInput.interactable = false;
          enterNamePanel.TogglePanel(gameName, true);
        }
      }
      else
      {
        SetErrorText("You are not connected to the server");
      }
    }
    private void SetErrorText(string errorMessage)
    {
      errorText.gameObject.SetActive(true);
      errorText.text = errorMessage;
    }
    private void ClearGameListings()
    {
      foreach (Transform availableGame in availableGamesContainer.transform)
      {
        Destroy(availableGame.gameObject);
      }
    }

    public void ReturnToGameSelection()
    {
      gameNameInput.interactable = true;
      enterNamePanel.ReturnToGameSelection();
    }
    public void ClearInputState()
    {
      gameNameInput.text = "";
      gameNameInput.interactable = true;
    }
  }
}
