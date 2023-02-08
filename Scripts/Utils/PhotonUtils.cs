using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Manticore
{
  /// <summary>
  /// Class to contain useful generic utility methods for handling Photon stuff
  /// </summary>
  public static class PhotonUtils
  {
    public static Player GetOtherPlayerInCurrentRoom(string playerName)
    {
      foreach (Player player in PhotonNetwork.PlayerListOthers)
      {
        if (player.NickName == playerName)
        {
          return player;
        }
      }
      return null;
    }

    public static void SetIsPlaying(this Room room, bool isPlaying)
    {
      Hashtable hash = new Hashtable();
      hash.Add("IsPlaying", isPlaying);
      room.SetCustomProperties(hash);
    }
    public static void SetPlayerInTurnIndex(this Room room, int playerInTurnIndex)
    {
      int nextPlayerIndex = playerInTurnIndex;
      if (nextPlayerIndex >= room.PlayerCount) nextPlayerIndex = 0;
      Hashtable hash = new Hashtable();
      hash.Add("PlayerInTurnIndex", nextPlayerIndex);
      room.SetCustomProperties(hash);
    }

    public static void SetCustomProp(this Player player, string key, object value)
    {
      if (GeneralManager.IsDev) return;
      Hashtable hash = new Hashtable();
      hash.Add(key, value);
      player.SetCustomProperties(hash);
    }
    public static object GetCustomProp(this Player player, string key)
    {
      if (GeneralManager.IsDev) return null;
      if (!player.CustomProperties.ContainsKey(key)) return null;
      return player.CustomProperties[key];
    }
    public static void SetCustomProp(this Room room, string key, object value)
    {
      Hashtable hash = new Hashtable();
      hash.Add(key, value);
      room.SetCustomProperties(hash);
    }
    public static object GetCustomProp(this Room room, string key)
    {
      if (!room.CustomProperties.ContainsKey(key)) return null;
      return room.CustomProperties[key];
    }

    public static void SetPlayerManager(this Player player, PlayerManager playerManager)
    {
      Hashtable hash = new Hashtable();
      hash.Add("PlayerViewID", playerManager.photonView.ViewID);
      player.SetCustomProperties(hash);

    }
    public static bool GetIsPlaying(this Room room)
    {
      if (!room.CustomProperties.ContainsKey("IsPlaying")) return false;
      return (bool)room.CustomProperties["IsPlaying"];
    }
    public static int GetPlayerInTurnIndex(this Room room)
    {
      if (!room.CustomProperties.ContainsKey("PlayerInTurnIndex")) return 0;
      return (int)room.CustomProperties["PlayerInTurnIndex"];
    }

    public static PlayerManager GetPlayerScript(this Player player)
    {
      if (!player.CustomProperties.ContainsKey("PlayerViewID")) return null;
      PlayerManager[] managers = GameObject.FindObjectsOfType<PlayerManager>(true);
      PlayerManager matchingObj = null;
      foreach (PlayerManager manager in managers)
      {
        if (manager.photonView.ViewID == (int)player.CustomProperties["PlayerViewID"])
        {
          matchingObj = manager;
        }
      }
      return matchingObj;
    }
  }
}
