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
  /// Handles actions and events in enter name panel. These are joining game and toggling the panel with game information.
  /// </summary>
  public class EnterNamePanelManager : MonoBehaviour
  {
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private InputField nameInput;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private TextMeshProUGUI errorMessage;
    private string gameName;
    private bool isCreating;

    private void Start()
    {
      errorMessage.gameObject.SetActive(false);
    }
    public void Update()
    {
      if (Input.GetKeyDown(KeyCode.Return))
      {
        JoinOrCreateGame();
      }
    }
    public void JoinOrCreateGame()
    {
      // TODO: This should be checked to contain at least 1 letter
      string nickName = nameInput.text;
      errorMessage.color = new Color(1, 0.07843137f, 0.1637874f);
      if (nickName.Length == 0)
      {
        SetErrorText("You must enter a name");
        return;
      }
      else if (nickName.Length > 14)
      {
        SetErrorText("The name must not contain more than 14 letters");
        return;
      }
      PhotonNetwork.LocalPlayer.NickName = nickName;
      PlayerPrefs.SetString("LastRoom", gameName);
      if (isCreating)
      {
        PhotonNetwork.CreateRoom(gameName, new RoomOptions { MaxPlayers = NetworkManager.MaxPlayersPerRoom, PlayerTtl = 1200000, EmptyRoomTtl = 0, CleanupCacheOnLeave = false });
      }
      else
      {
        PhotonNetwork.JoinRoom(gameName);
        errorMessage.color = Color.cyan;
        SetErrorText("Trying to join room");
        StartCoroutine(FailedToJoin());
      }
    }
    private IEnumerator FailedToJoin()
    {
      yield return new WaitForSecondsRealtime(2);
      bool joined = PhotonNetwork.InRoom;
      if (!joined)
      {
        errorMessage.color = new Color(1, 0.07843137f, 0.1637874f);
        SetErrorText("Room is already full");
      }
      else
      {
        errorMessage.gameObject.SetActive(false);
      }
    }

    public void TogglePanel(string gameName, bool isCreating = false)
    {
      gameObject.SetActive(true);
      this.isCreating = isCreating;
      this.gameName = gameName;
      if (isCreating)
      {
        title.text = "Create game\n" + gameName;
        buttonText.text = "CREATE";
      }
      else
      {
        title.text = "Joining game\n" + gameName;
        buttonText.text = "JOIN";
      }
    }
    private void SetErrorText(string error)
    {
      errorMessage.gameObject.SetActive(true);
      errorMessage.text = error;
    }
    public void ReturnToGameSelection()
    {
      gameObject.SetActive(false);
    }
  }
}
