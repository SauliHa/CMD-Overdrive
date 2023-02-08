using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Malee.List;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
    public class HackerUIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject moveActionsContainer, messageActionsContainer, messageList, playerList, selectMessagePanel, playerSelectPanel, releaseActionsContainer;
        [SerializeField]
        private TextMeshProUGUI messagePrefab, receiverListingPrefab;
        [SerializeField]
        private Button moveButton, actionButton, messageButton, skipMessageButton, passTurnButton, chromaAbilityButton, shiftAbilityButton, edgeAbilityButton;
        [SerializeField]
        private GeneralUIManager generalUIManager;
        [SerializeField]
        private GameObject tileSelection;
        private TileSelectionManager tileSelectionManager;
        private Player selectedMessageReceiver;
        private MapManager mapManager;
        // Reference comes from HackerManager Awake, ensures there is no nulls in this
        private HackerManager hacker;
        private CameraManager cameraManager;
        public bool lockCamera = false;
        private static string[] STATIC_MESSAGES = new string[]{
          "Don't move!",
          "I'm going to the CEOs room to hack the computer",
          "I'm coming to save you",
          "Rescue me, please!",
          "I am already being rescued",
          "Please don't cause an alarm",
          "How many locks have you opened?",
          "Which lock have you opened?",
          "Which room are you in?",
          "I have the data from CEO's computer"
    };

        private void Start()
        {
            mapManager = GameObject.Find("Map").GetComponent<MapManager>();
            CloseMessagePanel();
            InitializeMessageList();
            InitializeMessageReceiverList();
            tileSelectionManager = tileSelection.GetComponent<TileSelectionManager>();
            RoleType localPlayerRole = PhotonNetwork.LocalPlayer.GetRole();
            string playerRole = localPlayerRole.ToString();
            if (playerRole == "Chroma")
                chromaAbilityButton.gameObject.SetActive(true);
            else
                chromaAbilityButton.gameObject.SetActive(false);
            if (playerRole == "Shift")
                shiftAbilityButton.gameObject.SetActive(true);
            else
                shiftAbilityButton.gameObject.SetActive(false);
            if (playerRole == "Edge")
                edgeAbilityButton.gameObject.SetActive(true);
            else
                edgeAbilityButton.gameObject.SetActive(false);
            edgeAbilityButtonText = edgeAbilityButton.GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (lockCamera)
            {
                hacker.CenterCamera();
            }
        }

        public void ShowMoveActions()
        {
            moveActionsContainer.SetActive(true);
            messageActionsContainer.SetActive(false);
        }
        public void ShowMessageActions()
        {
            moveActionsContainer.SetActive(false);
            messageActionsContainer.SetActive(true);
        }
        public void HideActions()
        {
            messageActionsContainer.SetActive(false);
            moveActionsContainer.SetActive(false);
            releaseActionsContainer.SetActive(false);
        }
        public void ToggleActionButtons(bool moveEnabled, bool actionEnabled, bool passEnabled)
        {
            moveButton.interactable = moveEnabled;
            actionButton.interactable = actionEnabled;
            passTurnButton.interactable = passEnabled;

        }
        public void ToggleMessageButtons(bool messageEnabled, bool skipEnabled)
        {
            messageButton.interactable = messageEnabled;
            skipMessageButton.interactable = skipEnabled;
        }
        public void DisableButtons()
        {
            ToggleActionButtons(false, false, false);
            ToggleMessageButtons(false, false);
            ToggleReleaseButton(false);
        }

        public void OnMoveClick()
        {
            if (hacker == null) return;
            hacker.ToggleMoveMode();
        }

        public void OnActionClick()
        {
            ISpecialTile tile = hacker.CurrentTile as ISpecialTile;
            if (tile != null)
            {
                tile.OpenActionPrompt();
            }
            else
            {
                Debug.LogError("This tile does not have special actions available. Button should probably be disabled and not clickable");
            }
            ToggleActionButtons(false, false, false);
        }

        public void OnPassTurnClick()
        {
            hacker.OpenPassTurnPrompt();
            ToggleActionButtons(false, false, false);
        }
        public void OnReleaseButtonClick()
        {
            generalUIManager.OpenPrompt("Save teammates?", "Do you want to save captured teammates on this room?", () =>
            {
                hacker.ReleaseTeammates();
            });
            DisableButtons();
        }
        public void ToggleReleaseButton(bool canSaveHackers)
        {
            releaseActionsContainer.SetActive(canSaveHackers);
        }
        private void InitializeMessageReceiverList()
        {
            // Delete existing players (e.g. example players)
            foreach (Transform child in playerList.transform)
            {
                Destroy(child.gameObject);
            }
            if (GeneralManager.OtherHackers.Count == 0)
            {
                TextMeshProUGUI noOtherPlayersMessage = Instantiate<TextMeshProUGUI>(receiverListingPrefab, playerList.transform);
                noOtherPlayersMessage.text = "No other hackers in game";
            }
            // Create listing of actual players in game
            foreach (Player player in GeneralManager.OtherHackers)
            {
                TextMeshProUGUI newReceiverListing = Instantiate<TextMeshProUGUI>(receiverListingPrefab, playerList.transform);
                ReceiverOptionManager receiverManager = newReceiverListing.GetComponent<ReceiverOptionManager>();
                receiverManager.SetPlayer(player);
            }
            if (hacker != null && hacker.CurrentTile != null && hacker.CurrentTile.IsSignalRoom)
            {
                TextMeshProUGUI newReceiverListing = Instantiate<TextMeshProUGUI>(receiverListingPrefab, playerList.transform);
                ReceiverOptionManager receiverManager = newReceiverListing.GetComponent<ReceiverOptionManager>();
                receiverManager.SetPlayer();
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerList.GetComponent<RectTransform>());
        }
        private void InitializeMessageList()
        {
            // Delete existing messages (e.g. example message)
            foreach (Transform child in messageList.transform)
            {
                Destroy(child.gameObject);
            }
            // Special messages that require further actions before sending to other player
            // == Send location ==
            InitializeMessage("I am in the room...", () =>
            {
                SendMessageRPC("I am in the " + hacker.CurrentTile.name);
            });
            InitializeMessage("I am moving towards room..", () =>
            {
                StartCoroutine(WaitForTileSelect(mapManager.Tiles, (tile) =>
              {
                  SendMessageRPC("I am moving towards " + tile.name);
              }));
            });
            InitializeMessage("Please move towards room ...", () =>
            {
                StartCoroutine(WaitForTileSelect(mapManager.Tiles, (tile) =>
              {
                  SendMessageRPC("Please move towards " + tile.name);
              }));
            });
            // == Send opened lock ==
            //Not used currently after merging this and "I have opened this many locks..." into "I've opened locks..."
            /*
            InitializeMessage("I opened lock...", () =>
            {
                StartCoroutine(WaitForTileSelect(mapManager.LockTiles.Cast<TileManager>().ToList(), (tile) =>
          {
              SendMessageRPC("I opened lock " + tile.name);
          }));
            });*/
            InitializeMessage("I've opened these locks...", () =>
            {
                int locksOpened = generalUIManager.PersonalDoorsOpened.Count;
                if (locksOpened == 0)
                {
                    SendMessageRPC("I haven't opened any locks");
                }
                else if (locksOpened == 1)
                {
                    SendMessageRPC("I have opened lock in " + generalUIManager.PersonalDoorsOpened[0]);
                }
                else if (locksOpened == 2)
                {
                    SendMessageRPC("I have opened locks in " + generalUIManager.PersonalDoorsOpened[0] + " and " + generalUIManager.PersonalDoorsOpened[1]);
                }
                else if (locksOpened == 3)
                {
                    SendMessageRPC("I have opened locks in " + generalUIManager.PersonalDoorsOpened[0] + ", " + generalUIManager.PersonalDoorsOpened[1] + " and " + generalUIManager.PersonalDoorsOpened[2]);
                }
            });
            InitializeMessage("I'm going to make a false alarm to room...", () =>
            {
                StartCoroutine(WaitForTileSelect(mapManager.Tiles, (tile) =>
          {
              SendMessageRPC("I'm going to make a false alarm to " + tile.name);
          }));
            });
            InitializeMessage("I have/haven't hacked CEO computer...", () =>
            {
                if (hacker.HasHackedComputer)
                    SendMessageRPC("I have hacked the CEO computer.");
                else
                    SendMessageRPC("I have not hacked the CEO computer yet.");
            });
            // All static messages
            foreach (string message in STATIC_MESSAGES)
            {
                InitializeMessage(message);
            }
            BugzManager bugz = hacker as BugzManager;
            if (bugz != null && bugz.AdjacentDoorsWithMotionDetectors != null)
            {
                InitializeMessage("I've detected these motion detectors...", () =>
                {
                    if (bugz.AdjacentDoorsWithMotionDetectors.Count <= 0)
                    {
                        SendMessageRPC("I don't detect any motion detectors in my current room");
                    }
                    else
                    {
                        string message = "I know these doors have motion detector:";
                        for (int i = 0; i < bugz.AdjacentDoorsWithMotionDetectors.Count; i++)
                        {
                            message = message + " between " + bugz.AdjacentDoorsWithMotionDetectors[i].GetLinkedTileName(0) + " and " + bugz.AdjacentDoorsWithMotionDetectors[i].GetLinkedTileName(1);
                            if (i > bugz.AdjacentDoorsWithMotionDetectors.Count - 1) //if not the last item on list add (,) between it and next one
                                message = message + ",";
                        }
                        SendMessageRPC(message);
                    }
                });



            }
        }
        private TextMeshProUGUI InitializeMessage(string message)
        {
            TextMeshProUGUI newMessage = Instantiate<TextMeshProUGUI>(messagePrefab, messageList.transform);
            newMessage.text = message;
            return newMessage;
        }
        private TextMeshProUGUI InitializeMessage(string message, Action action)
        {
            TextMeshProUGUI newMessage = InitializeMessage(message);
            MessageOptionManager messageManager = newMessage.GetComponent<MessageOptionManager>();
            messageManager.Action = action;
            return newMessage;
        }

        public void SendMessageRPC(string message)
        {
            if (selectedMessageReceiver != null)
            {
                generalUIManager.OpenPrompt("Message confirmation", "Do you want to send message \"" + message + "\" to " + selectedMessageReceiver.NickName + "?",
                () =>
                {
                    hacker.SendMessage(selectedMessageReceiver, message);
                });
            }
            else
            {
                generalUIManager.OpenPrompt("Message confirmation", "Do you want to send message \"" + message + "\" to all other hackers?",
                () =>
                {
                    hacker.SendMessage(message);
                });
            }
            CloseMessagePanel();
        }

        public void SkipMessage()
        {
            hacker.MessageSent();
            hacker.CheckForMessagePhase();
        }
        // Use with a coroutine to wait for a player to select tile before continuing 
        /* // not in use anymore
        private IEnumerator WaitForTileSelect(List<TileManager> allowedTiles, Action<TileManager> callback)
        {
          mapManager.HighlightTiles(allowedTiles);
          generalUIManager.SetTurnSubtitle("Select tile");
          HideActions();
          CloseMessagePanel();
          // messageList.SetActive(false); // Don't show other messages when selecting tile 
          TileManager selectedTile = null;
          bool done = false;
          while (!done)
          {
            if (Input.GetMouseButtonDown(0))
            {
              TileManager clickedTile = EventUtils.GetTileOverMouse();
              if (allowedTiles.FindIndex((tile) => tile == clickedTile) >= 0)
              {
                selectedTile = clickedTile;
                done = true;
              }
            }
            yield return null;
          }
          if (selectedTile != null)
          {
            callback(selectedTile);
          }
        }
        */
        private IEnumerator WaitForTileSelect(List<TileManager> allowedTiles, Action<TileManager> callback)
        {
            HideActions();
            CloseMessagePanel();
            // messageList.SetActive(false); // Don't show other messages when selecting tile 
            TileManager selectedTile = null;
            bool done = false;
            generalUIManager.ToggleSelectMode(() =>
            {
                selectMessagePanel.SetActive(true);
                done = true;
            });
            int roomNumber = 0;
            tileSelection.SetActive(true);
            while (!done)
            {
                hacker.CenterCamera();
                roomNumber = tileSelectionManager.GetTile();
                if (roomNumber > 0)
                {
                    TileManager clickedTile = GameObject.Find("Room " + roomNumber).GetComponent<TileManager>();
                    if (allowedTiles.FindIndex((tile) => tile == clickedTile) >= 0)
                    {
                        tileSelection.SetActive(false);
                        generalUIManager.ToggleCancelButtonVisibility(false);
                        generalUIManager.ToggleChatButtonVisibility(true);
                        selectedTile = clickedTile;
                        done = true;
                    }
                }
                yield return null;
            }
            if (selectedTile != null)
            {
                callback(selectedTile);
            }
        }
        public IEnumerator WaitForTileSelect(Action<int> callback)
        {
            var allowedTiles = mapManager.Tiles;
            HideActions();
            CloseMessagePanel();
            // messageList.SetActive(false); // Don't show other messages when selecting tile 
            TileManager selectedTile = null;
            bool done = false;
            generalUIManager.ToggleSelectMode(() =>
            {
                selectMessagePanel.SetActive(true);
                done = true;
            });
            int roomNumber = 0;
            tileSelection.SetActive(true);
            while (!done)
            {
                hacker.CenterCamera();
                roomNumber = tileSelectionManager.GetTile();
                if (roomNumber > 0)
                {
                    TileManager clickedTile = GameObject.Find("Room " + roomNumber).GetComponent<TileManager>();
                    if (allowedTiles.FindIndex((tile) => tile == clickedTile) >= 0)
                    {
                        tileSelection.SetActive(false);
                        generalUIManager.ToggleCancelButtonVisibility(false);
                        generalUIManager.ToggleChatButtonVisibility(true);
                        selectedTile = clickedTile;
                        done = true;
                    }
                }
                yield return null;
            }
            if (selectedTile != null)
            {
                callback(roomNumber);
            }
        }

        public void SelectMessageReceiver(Player player)
        {
            selectedMessageReceiver = player;
            playerSelectPanel.SetActive(false);
            selectMessagePanel.SetActive(true);
            messageList.SetActive(true); // If player closed during tile select, this needs to be activated again as well
        }
        public void SelectMessageReceiver()
        {
            selectedMessageReceiver = null;
            playerSelectPanel.SetActive(false);
            selectMessagePanel.SetActive(true);
            messageList.SetActive(true); // If player closed during tile select, this needs to be activated again as well
        }
        public void OpenSendMessagePanel()
        {
            InitializeMessageReceiverList();
            playerSelectPanel.SetActive(true);
            lockCamera = true;
            // DarkenMessages();
        }
        // To remove old hover highlightings when opening chat box
        public void DarkenMessages()
        {
            foreach (Transform playerListing in playerList.transform)
            {
                ReceiverOptionManager listing = playerListing.gameObject.GetComponent<ReceiverOptionManager>();
                listing.DarkenText();
            }
            foreach (Transform messageListing in messageList.transform)
            {
                MessageOptionManager listing = messageListing.gameObject.GetComponent<MessageOptionManager>();
                listing.DarkenText();
            }
        }
        public void CloseMessagePanel()
        {
            playerSelectPanel.SetActive(false);
            selectMessagePanel.SetActive(false);
            lockCamera = false;
        }

        public void SetActionTitle(string text)
        {
            actionButton.GetComponentInChildren<Text>().text = text;
        }

        public void ToggleMoveMode(bool isMoveModeActive)
        {
            Text moveButtonTextEl = moveButton.GetComponentInChildren<Text>();
            moveButtonTextEl.text = isMoveModeActive ? "CANCEL" : "MOVE";
        }

        public void ActivateChromaAbility()
        {
            ToggleActionButtons(false, false, false);
            ToggleChromaAbilityButton(false);
            ChromaManager chromaManager = GeneralManager.LocalPlayer as ChromaManager;
            if (chromaManager == null)
                return;

            generalUIManager.OpenPrompt("Activate shadow mode?",
            "This will cause you to be invisible from guards and security cameras until the end of your next turn. Ability will be on cooldown for " + GeneralManager.ChromaAbilityCooldown + " turns."
            , () =>
            {
                chromaManager.ActivateShadowMode();
            });
        }
        public void ToggleChromaAbilityButton(bool active)
        {
            chromaAbilityButton.gameObject.SetActive(active);
        }

        public void ActivateShiftAbility()
        {
            ToggleActionButtons(false, false, false);
            ToggleShiftAbilityButton(false);
            ShiftManager shiftManager = GeneralManager.LocalPlayer as ShiftManager;
            if (shiftManager == null)
                return;

            generalUIManager.OpenPrompt("Activate extra move?",
          "This will give you one extra move this turn. Ability will be on cooldown for " + GeneralManager.ShiftAbilityCooldown + " turns."
          , () =>
          {
              shiftManager.ActivateExtraMove();
          });
        }
        public void ToggleShiftAbilityButton(bool active)
        {
            shiftAbilityButton.gameObject.SetActive(active);
        }

        private Text edgeAbilityButtonText;
        public void ActivateEdgeAbility()
        {
            ToggleActionButtons(false, false, false);
            //ToggleEdgeAbilityButton(false);
            EdgeManager edgeManager = GeneralManager.LocalPlayer as EdgeManager;
            if (edgeManager == null)
                return;
            if (edgeManager.HologramTile == null)
            {
                generalUIManager.OpenPrompt("Deploy a hologram of yourself?",
                "This allow you to place hologram of yourself in your current position. It will stay there unless guard enters the room or you destroy it."
                , () =>
                {
                    edgeAbilityButtonText.text = "DESTROY HOLOGRAM";
                    edgeManager.DeployHologram();
                });
            }
            else
            {
                generalUIManager.OpenPrompt("Destroy the hologram?",
                "This will remove the hologram from the map and allow you to place another one in " + GeneralManager.EdgeAbilityCooldown + " turns."
                , () =>
                {
                    edgeAbilityButtonText.text = "DEPLOY HOLOGRAM";
                    edgeManager.DestroyHologram();
                });
            }
        }

        public void EdgeHologramDestroyed()
        {
            edgeAbilityButtonText.text = "DEPLOY HOLOGRAM";
        }

        public void ToggleEdgeAbilityButton(bool active)
        {
            edgeAbilityButton.gameObject.SetActive(active);
        }

        public void openMessageList()
        {
            selectMessagePanel.SetActive(true);
        }

        public TileManager GetCurrentTile()
        {
            return hacker.CurrentTile;
        }

        // This needs to be called if hacker can't be found yet on Start, needs to be called when reconnecting
        public void SetHacker(HackerManager manager)
        {
            hacker = manager;
        }
        public void NewRound()
        {
            hacker.NewRound();
        }
    }
}
