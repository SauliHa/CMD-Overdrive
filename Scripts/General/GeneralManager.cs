using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Manticore
{
    /// <summary>
    /// General manager to handle any tasks that are needed throughout the app. For example better scene management.
    /// This should be included in DontDestroyOnLoad so it will be preserved throughout the app.
    /// </summary>
    public class GeneralManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string mainSceneName;
        [SerializeField] private RoleType testDevAs = RoleType.Edge;

        //probability for the alarms, meaning X% change of SUCCESS!
        private NetworkManager networkManager;
        private List<Player> players = new List<Player>();
        private List<GuardManager> guards = new List<GuardManager>();
        private AudioManager audioManager;
        private int playerInTurnIndex = 0;
        private int turnCounter = 0;

        private bool ceoComputerHacked = false;
        private bool hasPlayedAGame = false;
        private bool didHackersWin = false;

        public static bool IsDev
        {
            get => PhotonNetwork.CurrentRoom == null;
        }

        public static GeneralManager Instance { get; private set; }

        public static NetworkManager NetworkManager
        {
            get => Instance.networkManager;
        }

        public static AudioManager AudioManager
        {
            get => Instance.audioManager;
        }

        public static string MainSceneName
        {
            get => Instance.mainSceneName;
        }

        public static bool HasPlayedAGame
        {
            get => Instance.hasPlayedAGame;
        }

        public static bool DidHackersWin
        {
            get => Instance.didHackersWin;
        }

        public static int TurnCounter
        {
            get => Instance.turnCounter;
        }

        public static int PlayerMovesInTurn = 1;
        public static int PlayerActionsInTurn = 1;

        public static bool ActionToMoves = true;
        public static int CorporationMovesInTurn = 1;
        public static int CorporationActionsInTurn = 1;

        public static int WindowMoveChance = 50;
        public static int DoorMoveChance = 50;
        public static int CEOLockHackChance = 50;
        public static int FalseAlarmChance = 50;
        public static int CEOComputerHackChance = 50;
        public static int CapturedMoveChance = 20; // for moving through locked doors and windows as a captured player.

        public static bool CEOComputerHacked
        {
            get => Instance.ceoComputerHacked;
        }

        public static int MaxMotionDetectorsAmount = 3;
        public static int MaxLockedDoorsAmount = 3;
        public static int ShiftAbilityCooldown = 3;
        public static int ChromaAbilityCooldown = 5;
        public static int EdgeAbilityCooldown = 3;

        public static List<GuardManager> Guards
        {
            get => Instance.guards;
        }

        public static List<Player> Players
        {
            get => Instance.players;
        }

        public static List<Player> OtherPlayers
        {
            get => Instance.players.FindAll((player) => player != PhotonNetwork.LocalPlayer);
        }

        public static List<Player> OtherHackers
        {
            get => Instance.players.FindAll((player) =>
            {
                RoleType role = player.GetRole();
                return player != PhotonNetwork.LocalPlayer && role != RoleType.Corporation;
            });
        }

        public static int PlayerInTurnIndex
        {
            get => Instance.playerInTurnIndex;
        }

        public static Player PlayerInTurn
        {
            get => Instance.players.Count > Instance.playerInTurnIndex
                ? Instance.players[Instance.playerInTurnIndex]
                : null;
        }

        public static Color32 BASE_RED = new Color32(255, 20, 42, 255); // GREEN
        public static Color32 BASE_GREEN = new Color32(0, 173, 16, 255); // GREEN
        public static int START_AMOUNT_OF_GUARDS = 2;

        public static PlayerManager LocalPlayer
        {
            get
            {
                PlayerManager localPlayer = PhotonNetwork.LocalPlayer.TagObject as PlayerManager;
                if (localPlayer == null) localPlayer = PhotonNetwork.LocalPlayer.GetPlayerScript();
                return localPlayer;
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (Instance == null)
            {
                audioManager = GetComponent<AudioManager>();
                networkManager = GetComponent<NetworkManager>();
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
                if (IsDev && SceneManager.GetActiveScene().name == "GameScene")
                {
                    InitializeGame();
                }

                return;
            }

            // This triggers, if loading same scene again, where DDOL is launched.
            // Destroy in this case, so there is no many DDOLs generated
            Destroy(this.gameObject);
        }

        // This gets called for all clients when main game loads -> good place to initialize game
        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
        {
            if (newScene.name == mainSceneName)
            {
                if (IsDev || !PhotonNetwork.CurrentRoom.GetIsPlaying())
                {
                    InitializeGame();
                }
                // If room is already playing, this is called from reconnect, which means we don't want to initialize game again, we need to script references tho
                else if (
                    !hasPlayedAGame) // Make sure game has not ended, if this is called again on game end for some reason
                {
                    playerInTurnIndex = PhotonNetwork.CurrentRoom.GetPlayerInTurnIndex();
                    guards.Clear();
                    LoadSettings();
                    InitializePlayersList();
                    StartCoroutine(SetPlayerScripts(() =>
                    {
                        // Sync player state and Update UI after script setting is done
                        GeneralManager.LocalPlayer.SyncStateWithServer();
                        GeneralManager.LocalPlayer.UIManager.ActivateUI();

                        GeneralManager.LocalPlayer.UpdateUI();
                    }));

                }

            }
        }

        // Detect turn change by room custom props update
        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            if (changedProps.ContainsKey("PlayerInTurnIndex"))
            {
                int nextPlayerIndex = (int) changedProps["PlayerInTurnIndex"];
                if (nextPlayerIndex != playerInTurnIndex || players.Count == 1)
                {
                    ChangeTurn(nextPlayerIndex);
                }
            }
        }

        public void InitializeGame()
        {
            if (IsDev)
            {
                PhotonNetwork.LocalPlayer.SetRole(testDevAs);
                players.Add(PhotonNetwork.LocalPlayer);
            }
            else
            {
                InitializePlayersList();
            }

            RoleType localPlayerRole = PhotonNetwork.LocalPlayer.GetRole();
            bool isCorporate = localPlayerRole == RoleType.Corporation;

            // Set playing to be true, no need to be called from all clients since this changes custom props (network wide)
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetIsPlaying(true);
            }

            if (isCorporate)
            {
                InstantiateCorporation();
            }
            else
            {
                InstantiateHacker();
            }

            // Start first player turn
            StartCoroutine(StartFirstTurn());
        }

        public void InitializePlayersList()
        {
            players.Clear();
            foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
            {
                Player player = playerInfo.Value;
                players.Add(playerInfo.Value);
            }

            players.Sort((a, b) =>
            {
                // This assumes that Role has been set correctly to not be outside of RoleType enum indexes
                RoleType aRole = a.GetRole();
                RoleType bRole = b.GetRole();
                if (aRole == RoleType.Corporation || bRole == RoleType.Corporation)
                {
                    return bRole - aRole;
                }

                return a.ActorNumber - b.ActorNumber;
            });
        }

        // Check if PlayerManager scripts can be found and set them to matching players. Run until scripts are found.
        public IEnumerator SetPlayerScripts(Action onFinished)
        {
            bool done = false;
            while (!done)
            {
                done = true;
                foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
                {
                    Player player = playerInfo.Value;
                    PlayerManager
                        playerManager =
                            player.GetPlayerScript(); // If this returns null, photonview has not been registered yet
                    if (playerManager != null)
                    {
                        player.TagObject = playerManager;
                    }
                    else
                    {
                        // If any script is not found, run again
                        done = false;
                    }
                }

                yield return new WaitForSeconds(0.25f); // Wait for a quarter second before trying again
            }

            onFinished.Invoke();
        }

        public IEnumerator StartFirstTurn()
        {
            yield return new WaitForEndOfFrame();
            ChangeTurn(0);
        }

        public void ChangeTurn(int nextPlayerIndex)
        {
            // Last player in turn -> go back to first
            if (nextPlayerIndex == 0 || nextPlayerIndex >= players.Count)
            {
                playerInTurnIndex = 0;
                IncrementTurnCounter();
            }
            else
            {
                playerInTurnIndex = nextPlayerIndex;
            }

            Player playerInTurn = players[playerInTurnIndex];
            PlayerManager localPlayerManager = GeneralManager.LocalPlayer as PlayerManager;
            if (playerInTurn.ActorNumber == localPlayerManager.photonView.OwnerActorNr || IsDev)
            {
                localPlayerManager.StartTurn();
            }

            // Update UI for all players regardless if is player in turn
            localPlayerManager.UpdateUI();
        }

        public void ChangeTurn()
        {
            ChangeTurn(playerInTurnIndex + 1);
        }

        public void IncrementTurnCounter()
        {
            turnCounter++;
        }

        // Instantiate player obj and set it's script to LocalPlayer's TagObject to make it available everywhere through Photon player
        private HackerManager InstantiateHacker()
        {
            // TODO: Instantiate Corporation player here
            RoleType localPlayerRole = PhotonNetwork.LocalPlayer.GetRole();
            if (IsDev)
            {
                HackerManager obj = Instantiate(Resources.Load<HackerManager>(localPlayerRole.ToString()),
                    new Vector3(0f, 0f, -1f), Quaternion.identity);
                PhotonNetwork.LocalPlayer.TagObject = obj;
                return obj;
            }
            else
            {
                GameObject hackerObj = PhotonNetwork.Instantiate(localPlayerRole.ToString(), new Vector3(0f, 0f, -1f),
                    Quaternion.identity);
                HackerManager hackerScript = hackerObj.GetComponent<HackerManager>();
                // hackerScript.NetworkManager.SetPlayerInfo(PhotonNetwork.LocalPlayer, hackerScript.photonView.ViewID);
                hackerScript.photonView.RPC("SetPlayerInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer,
                    hackerScript.photonView.ViewID);
                PhotonNetwork.LocalPlayer.SetPlayerManager(hackerScript);
                return hackerScript;
            }
        }

        private CorporationManager InstantiateCorporation()
        {
            if (IsDev)
            {
                CorporationManager obj = Instantiate(Resources.Load<CorporationManager>("Corporation"),
                    new Vector3(0f, 0f, -1f), Quaternion.identity);
                // Should the amount of guards come dynamically?
                for (int i = 0; i < START_AMOUNT_OF_GUARDS; i++)
                {
                    GuardManager guard = InstantiateGuard();
                }

                PhotonNetwork.LocalPlayer.TagObject = obj;
                return obj;
            }
            else
            {
                GameObject obj =
                    PhotonNetwork.Instantiate("Corporation", new Vector3(0f, 0f, -1f), Quaternion.identity);
                CorporationManager corporationManager = obj.GetComponent<CorporationManager>();
                for (int i = 0; i < START_AMOUNT_OF_GUARDS; i++)
                {
                    GuardManager guard = InstantiateGuard();
                    corporationManager.photonView.RPC("OnGuardInstantiated", RpcTarget.Others, guard.photonView.ViewID);
                }

                // Call first manually for
                // corporationManager.NetworkManager.SetPlayerInfo(PhotonNetwork.LocalPlayer, corporationManager.photonView.ViewID);
                corporationManager.photonView.RPC("SetPlayerInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer,
                    corporationManager.photonView.ViewID);
                PhotonNetwork.LocalPlayer.SetPlayerManager(corporationManager);
                return corporationManager;
            }
        }

        public GuardManager InstantiateGuard()
        {
            if (IsDev)
            {
                GuardManager guard = Instantiate(Resources.Load<GuardManager>("Guard"), new Vector3(0f, 0f, -1f),
                    Quaternion.identity);
                return guard;
            }
            else
            {
                GameObject guard = PhotonNetwork.Instantiate("Guard", new Vector3(0f, 0f, -1f), Quaternion.identity);
                GuardManager guardManager = guard.GetComponent<GuardManager>();
                return guardManager;
            }
        }

        // Make a separate method to be able to call this from PlayerNetworkManager in RPC
        public void AddGuard(GuardManager guard)
        {
            guards.Add(guard);
        }

        private static IEnumerator LoadSceneAsync(string sceneName, Action afterSceneLoad)
        {
            // Start loading the scene
            PhotonNetwork.LoadLevel(SceneUtils.GetSceneIndexByName(sceneName));
            // Wait until the level finish loading
            while (PhotonNetwork.LevelLoadingProgress >= 100f)
                yield return null;
            // Wait a frame so every Awake and Start method is called
            yield return new WaitForEndOfFrame();

            // Scene loaded
            afterSceneLoad.Invoke();
        }

        public static void LoadScene(string sceneName, Action afterSceneLoad)
        {
            Instance.StartCoroutine(LoadSceneAsync(sceneName, afterSceneLoad));
        }

        public static void LoadScene(string sceneName)
        {
            PhotonNetwork.LoadLevel(SceneUtils.GetSceneIndexByName(sceneName));
        }

        public static void OpenQrReader()
        {
            Instance.StartCoroutine(LoadSceneAsync("ReadQR", () =>
            {
                QRCodeReader qrCodeReader = GameObject.Find("Camera Manager")?.GetComponent<QRCodeReader>();
                if (qrCodeReader)
                {
                    qrCodeReader.OpenCamera();
                }
            }));
        }

        public void SetCEOComputerAsHacked()
        {
            ceoComputerHacked = true;
        }

        public void RemovePlayerFromGame(Player player)
        {
            players.Remove(player);
            playerInTurnIndex--;
            //Room room = PhotonNetwork.CurrentRoom;
            //room.SetCustomProp("PlayerInTurnIndex", playerInTurnIndex);
        }

        public void EndGame(bool didHackersWin = true)
        {
            hasPlayedAGame = true;
            this.didHackersWin = didHackersWin;
            PhotonNetwork.LeaveRoom();
        }

        // If later comes other data that we want to show in GameOver, we can clear it here
        public void ClearPreviousGame()
        {
            turnCounter = 0;
            players = new List<Player>();
            guards = new List<GuardManager>();
            ceoComputerHacked = false;
            playerInTurnIndex = 0;
            hasPlayedAGame = false;
        }

        // Load settings from custom props, needed for reconnecting and set on GameSettingsManager InformSettings-method
        public void LoadSettings()
        {

            Room room = PhotonNetwork.CurrentRoom;
            PlayerMovesInTurn = (int) room.GetCustomProp("HackerMoves");
            PlayerActionsInTurn = (int) room.GetCustomProp("HackerActions");
            ActionToMoves = (bool) room.GetCustomProp("ActionToMoves");
            ChromaAbilityCooldown = (int) room.GetCustomProp("ChromaCD");
            ShiftAbilityCooldown = (int) room.GetCustomProp("ShiftCD");

            CorporationMovesInTurn = (int) room.GetCustomProp("CorporationMoves");
            CorporationActionsInTurn = (int) room.GetCustomProp("CorporationActions");
            START_AMOUNT_OF_GUARDS = (int) room.GetCustomProp("AmountOfGuards");
            MaxLockedDoorsAmount = (int) room.GetCustomProp("LockedDoors");
            MaxMotionDetectorsAmount = (int) room.GetCustomProp("MotionDetectors");

            WindowMoveChance = (int) room.GetCustomProp("WindowMoveChance");
            DoorMoveChance = (int) room.GetCustomProp("DoorMoveChance");
            CEOLockHackChance = (int) room.GetCustomProp("LockHackChance");
            FalseAlarmChance = (int) room.GetCustomProp("FalseAlarmChance");
            CEOComputerHackChance = (int) room.GetCustomProp("CEOComputerHackChance");
        }
    }
}