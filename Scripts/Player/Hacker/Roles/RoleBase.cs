using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

namespace Manticore

{
  /// <summary>
  /// base for roles
  /// Doesn't really do anything yet
  /// </summary>
  public static class RoleExtension
  {
    public static void SetRole(this Player player, RoleType role)
    {
      ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
      hash.Add("Role", role);
      player.SetCustomProperties(hash);
    }

    public static RoleType GetRole(this Player player)
    {
      return (RoleType)player.CustomProperties["Role"];
    }

    public static string GetNameWithRole(this Player player)
    {
      if (!player.CustomProperties.ContainsKey("Role")) return player.NickName;
      return string.Format("{0} ({1})", player.NickName, player.GetRole().ToString());
    }
  }

}
