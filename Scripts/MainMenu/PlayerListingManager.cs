using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Manticore
{
  /// <summary>
  /// Class to handle a single player listing in waiting lobby.
  /// </summary>
  public class PlayerListingManager : MonoBehaviour
  {
    private TextMeshProUGUI playerName;
    public Player Player { get; private set; }
    public void SetPlayerInfo(Player player)
    {
      playerName = GetComponent<TextMeshProUGUI>();
      Player = player;
      playerName.text = player.NickName;
      string roleText = player.CustomProperties.ContainsKey("Role") && player.CustomProperties["Role"] != null ? ((RoleType)player.CustomProperties["Role"]).ToString() : "Selecting";
      playerName.text += " [" + roleText + "]";

      if (PhotonNetwork.LocalPlayer == player)
      {
        playerName.text += " (You)";
      }
      else if (PhotonNetwork.CurrentRoom.MasterClientId == player.ActorNumber)
      {
        playerName.text += " (Host)";
      }
    }
  }
}
