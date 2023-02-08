using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manticore
{
  /// <summary>
  /// Handles all general stuff for networking (Photon) events, main responsibility being on reconnecting to existing game. General Photon configurations are here as well.
  /// This should be included in DontDestroyOnLoad so it can handle everything throughout the app.
  ///
  /// Connecting (and reconnecting) goes in a bit of a chain through different callbacks/hooks. This cycle is the following:
  /// 1. Connect (or reconnect) through TryRecover: calls Reconnect first, if that fails -> default ConnectUsingSettings
  /// 2. OnConnectMaster callback, does following:
  ///   2.1 If LastRoom cache => try RejoinRoom:
  ///     2.1.1 If success => automatically joins last game and stops this chain, loads GameScene and continues from there
  ///     2.1.2 Else (fail) => OnJoinRoomFailed => calls JoinLobby
  ///   2.2 Else => calls JoinLobby
  /// 3. OnJoinedLobby callback, initializes MainMenuPanel default view
  ///
  /// On disconnect OnDisconnected callback is called.
  /// NetworkManager then loops TryRecover with 1 second delays until connection is found, then it goes to this same chain.
  ///
  /// For reconnecting to work, players' UserId needs to stay same than which it was when last connected to room.
  /// This is ensured by using deviceUniqueIdentifier as UserId in other than dev. This is set in Start
  /// </summary>
  public class NetworkManager : MonoBehaviourPunCallbacks
  {
    [SerializeField]
    private byte maxPlayersPerRoom = 5;
    public static byte MaxPlayersPerRoom { get; private set; }
    public void Awake()
    {
      MaxPlayersPerRoom = maxPlayersPerRoom;
      PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void Start()
    {
      string userID = GetAndSetUserID();
      bool isEditor = Application.installMode == ApplicationInstallMode.Editor;
      // If is prod build or running in editor, set user id by device id. Editor also so reconnect can be tested through editor without messing other dev builds of same device.
      if (!Debug.isDebugBuild || isEditor)
      {
        PhotonNetwork.AuthValues = new AuthenticationValues(userID);
      }
      TryRecover();
    }

    public void Update()
    {
      // if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
      // {
      //   PhotonNetwork.Disconnect();
      // }
    }

    public override void OnConnectedToMaster()
    {
      // If LastRoom exists in cache, try rejoin room
      if (PlayerPrefs.HasKey("LastRoom"))
      {
        string lastRoom = PlayerPrefs.GetString("LastRoom");
        PhotonNetwork.RejoinRoom(lastRoom);
      }
      else
      {
        PhotonNetwork.JoinLobby();
      }
    }
    public override void OnJoinedLobby()
    {
      if (SceneManager.GetActiveScene().name == "MainMenu")
      {
        ActivateMainMenu();
      }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
      Debug.Log("Disconnected: " + cause);
      if (Application.isPlaying)
      {
        if (SceneManager.GetActiveScene().name != "MainMenu") // First load back to MainMenu
        {
          GeneralManager.LoadScene("MainMenu", () =>
          {
            HandleDisconnect(cause);
          });
        }
        else
        {
          ActivateMainMenu();
          HandleDisconnect(cause);
        }
      }
    }
    private void HandleDisconnect(DisconnectCause cause)
    {
      if (CanRecoverFromDisconnect(cause)) // In these cases, fall back to normal connect
      {
        StartCoroutine(WaitAndTryRecover());
      }
      else
      {
        // NOTE: This shouldn't really ever be called
        Debug.LogError("Can't recover from disconnect, crashing app");
        Application.Quit();
      }
    }

    // Base from Photon example
    private bool CanRecoverFromDisconnect(DisconnectCause cause)
    {
      switch (cause)
      {
        // the list here may be non exhaustive and is subject to review
        case DisconnectCause.Exception:
        case DisconnectCause.ExceptionOnConnect:
        case DisconnectCause.ServerTimeout:
        case DisconnectCause.ClientTimeout:
        case DisconnectCause.DisconnectByServerLogic:
        case DisconnectCause.DisconnectByServerReasonUnknown:
        case DisconnectCause.DisconnectByClientLogic:
          return true;
      }
      return false;
    }
    private IEnumerator WaitAndTryRecover()
    {
      yield return new WaitForSecondsRealtime(1);
      TryRecover();
    }

    // From Photon example
    private void TryRecover()
    {
      if (!PhotonNetwork.Reconnect())
      {
        if (!PhotonNetwork.ConnectUsingSettings())
        {
          Debug.LogError("ConnectUsingSettings failed");
        }
      }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
      // If not in lobby, join it and wait for its callback
      if (!PhotonNetwork.InLobby)
      {
        PhotonNetwork.JoinLobby();
        return;
      }
      else if (SceneManager.GetActiveScene().name == "MainMenu")
      {
        ActivateMainMenu();
      }
    }
    public override void OnLeftRoom()
    {
      if (SceneManager.GetActiveScene().name == "MainMenu")
      {
        ActivateMainMenu();
      }
      else
      {
        SceneManager.LoadScene("MainMenu");
      }
    }
    public void ActivateMainMenu()
    {
      MainMenuManager menuManager = GameObject.Find("MenuManager").GetComponent<MainMenuManager>();
      menuManager.ActivateInitialMenuState();
    }

    public string GetAndSetUserID()
    {
      string userID = null;
      // If already exists in cache, get and return
      if (PlayerPrefs.HasKey("UserID"))
      {
        userID = PlayerPrefs.GetString("UserID");
        return userID;
      }
      // If WebGL build, form a new Guid as ID
      if (Application.platform == RuntimePlatform.WebGLPlayer)
      {
        userID = System.Guid.NewGuid().ToString();
      }
      // Otherwise assume deviceUniqueIdentifier is availabe
      else
      {
        userID = SystemInfo.deviceUniqueIdentifier;
      }
      PlayerPrefs.SetString("UserID", userID);
      return userID;
    }
  }
}
