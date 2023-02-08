using System.Collections;
using System.Collections.Generic;
using Manticore;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleUI : MonoBehaviour
{
  [SerializeField]
  private GeneralManager generalManager;
  [SerializeField]
  private Button corporateButton;
  [SerializeField]
  private TextMeshProUGUI corporatePlayerName;
  [SerializeField]
  private Button edgeButton;
  [SerializeField]
  private TextMeshProUGUI edgePlayerName;
  [SerializeField]
  private Button shiftButton;
  [SerializeField]
  private TextMeshProUGUI shiftPlayerName;
  [SerializeField]
  private Button bugzButton;
  [SerializeField]
  private TextMeshProUGUI bugzPlayerName;
  [SerializeField]
  private Button chromaButton;
  [SerializeField]
  private TextMeshProUGUI chromaPlayerName;

  public void DisableButtonAndSetText(int roleNumber, Player player)
  {
    TextMeshProUGUI text;
    switch (roleNumber)
    {
      case 0:
        corporateButton.interactable = false;
        text = corporatePlayerName;
        break;
      case 1:
        edgeButton.interactable = false;
        text = edgePlayerName;
        break;
      case 2:
        shiftButton.interactable = false;
        text = shiftPlayerName;
        break;
      case 3:
        bugzButton.interactable = false;
        text = bugzPlayerName;
        break;
      case 4:
        chromaButton.interactable = false;
        text = chromaPlayerName;
        break;

      default:
        text = edgePlayerName;
        break;
    }
    text.color = Color.red;
    text.text = "Chosen by " + player.NickName;

  }

  public void EnableButtonAndCleartext(int roleNumber)
  {
    TextMeshProUGUI text;
    switch (roleNumber)
    {
      case 0:
        corporateButton.interactable = true;
        text = corporatePlayerName;
        break;
      case 1:
        edgeButton.interactable = true;
        text = edgePlayerName;
        break;
      case 2:
        shiftButton.interactable = true;
        text = shiftPlayerName;
        break;
      case 3:
        bugzButton.interactable = true;
        text = bugzPlayerName;
        break;
      case 4:
        chromaButton.interactable = true;
        text = chromaPlayerName;
        break;
      default:
        return;
    }
    text.color = new Color(0.07843137f, 0.7637205f, 1, 1);
    text.text = "Not selected by another player";
  }

  public void UpdateSelectedRoles()
  {
    int i = 0;
    Player[] players = PhotonNetwork.PlayerListOthers;
    while (players.Length > i)
    {
      if (players[i].CustomProperties.ContainsKey("Role") && players[i].CustomProperties["Role"] != null)
      {
        int player = (int)players[i].GetRole();
        DisableButtonAndSetText(player, players[i]);
      }
      i++;
    }

  }
}
