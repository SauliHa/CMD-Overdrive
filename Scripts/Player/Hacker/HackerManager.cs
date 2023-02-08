using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Manticore
{
    public abstract class HackerManager : PlayerManager
    {
        private ChatManager chatManager;
        protected HackerUIManager hackerUIManager;
        public TileManager currentTile = null;
        private bool actionToMoveMode;
        private bool hasSendAMessage = false;
        private bool ExtraAbility;
        public TileManager CurrentTile
        {
            get => currentTile; set
            {
                currentTile = value;
                photonView.Owner.SetCustomProp("CurrentTileID", value.photonView.ViewID);
            }
        }
        protected bool moveMode = true;
        public bool MoveMode
        {
            get => moveMode; set
            {
                moveMode = value;
                photonView.Owner.SetCustomProp("MoveMode", value);
            }
        }
        protected bool isSendingMessage = false;
        public bool IsSendingMessage
        {
            get => isSendingMessage; set
            {
                isSendingMessage = value;
                photonView.Owner.SetCustomProp("IsSendingMessage", value);
            }
        }
        protected bool startingTileSelected = false;
        public bool StartingTileSelected
        {
            get => startingTileSelected; set
            {
                startingTileSelected = value;
                photonView.Owner.SetCustomProp("StartingTileSelected", value);
            }
        }
        protected bool isCaptured;
        public bool IsCaptured
        {
            get => isCaptured; set
            {
                isCaptured = value;
                photonView.Owner.SetCustomProp("IsCaptured", value);
            }
        }
        private bool hasBeenseen = false;
        public bool HasBeenSeen
        {
            get => hasBeenseen; set
            {
                hasBeenseen = value;
                photonView.Owner.SetCustomProp("HasBeenSeen", value);
            }
        }
        private bool canSaveTeammate = false;
        public bool CanSaveTeammate
        {
            get => canSaveTeammate; set
            {
                canSaveTeammate = value;
                photonView.Owner.SetCustomProp("CanSaveTeammate", value);
            }
        }
        private bool hasHackedComputer = false;
        public bool HasHackedComputer
        {
            get => hasHackedComputer; set
            {
                hasHackedComputer = value;
                photonView.Owner.SetCustomProp("HasHackedComputer", value);
            }
        }

        private CameraManager cameraManager;
        private bool HasActionAvailable
        {
            get
            {
                // Current tile is special tile, excluding starting tiles when computer is not yet hacked and has action point
                if ((CurrentTile is ISpecialTile && !(CurrentTile is StartTileManager && !GeneralManager.CEOComputerHacked)) && ActionCount > 0) return true;
                ChromaManager chroma = GeneralManager.LocalPlayer as ChromaManager;
                if (chroma != null && chroma.Cooldown <= 0 && ActionCount > 0) return true;
                ShiftManager shift = GeneralManager.LocalPlayer as ShiftManager;
                if (shift != null && shift.Cooldown <= 0) return true;
                return false;
            }
        }
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            chatManager = UIManager.GetComponent<ChatManager>();
            hackerUIManager = UIManager.transform.Find("HackerUI").GetComponent<HackerUIManager>();
            cameraManager = GameObject.Find("Main Camera").GetComponent<CameraManager>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            GetComponent<SpriteRenderer>().sprite = Role.IconSprite;
            actionToMoveMode = GeneralManager.ActionToMoves;
            if (IsMineOrIsDev)
            {
                // Ensure hackerUIManager hacker reference is set, needs to be here when reconnecting
                hackerUIManager.SetHacker(this);
                // Hide icon to begin with, show after start tile select
                GraphicsUtils.ChangeAlpha(spriteRenderer, 0f);
            }
            else
            {
                // Set other hackers as deactive by default
                gameObject.SetActive(false);
            }
        }
        public virtual void Start()
        {
            hackerUIManager.ToggleReleaseButton(CanSaveTeammate);
            // Unity game started from other than through MainMenu -> StartTurn manually and set infinite moveCount
            if (GeneralManager.IsDev)
            {
                UpdateUI();
            }
        }

        private void Update()
        {
            if (IsMineOrIsDev && IsPlayerInTurn && Input.GetMouseButtonDown(0))
            {
                TileManager clickedTile = EventUtils.GetTileOverMouse();
                if (clickedTile != null)
                {
                    DoorManager matchingDoor = mapManager.FindDoor(CurrentTile, clickedTile);
                    // Starting tile move
                    if (!StartingTileSelected)
                    {
                        if (mapManager.CheckIfStartingTile(clickedTile))
                        {
                            MovePlayer(clickedTile);
                            ToggleMoveMode();
                        }
                    }
                    else if (matchingDoor != null && MoveMode && CurrentTile != clickedTile)
                    {
                        // Door is window, open prompt
                        if (matchingDoor.IsWindow)
                        {
                            OpenDoorIsWindowPrompt(matchingDoor);
                        }
                        // Door is locked, open prompt
                        else if (matchingDoor.IsLocked)
                        {
                            OpenDoorLockedPrompt(matchingDoor);
                        }
                        // Regular move
                        else
                        {
                            OpenMovePrompt(matchingDoor);
                        }
                    }
                }
            }
        }

        // NOTE: This is maybe a bit bad practice. Everything gets changed on every small change. And getting a bit out of hand.
        public override void UpdateUI()
        {
            base.UpdateUI();
            if (IsMineOrIsDev)
            {
                Player localPlayer = PhotonNetwork.LocalPlayer;
                if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
                {
                    // Starting tile selection
                    if (!StartingTileSelected)
                    {
                        mapManager.HighlightStartingTiles();
                        mapManager.DarkenDoors();
                        UIManager.SetTurnSubtitle("Select your starting tile");
                        // Disable actions during starting tile selection
                        hackerUIManager.HideActions();
                        hackerUIManager.DisableButtons();
                        UIManager.DisableButtons();
                        return;
                    }
                    mapManager.RemoveHighlighting();
                    hackerUIManager.ToggleReleaseButton(CanSaveTeammate);
                    hackerUIManager.ShowMoveActions();
                    UIManager.HideTurnSubtitle();
                    hackerUIManager.SetActionTitle(TileManager.GetTileActionTitle(CurrentTile));
                    hackerUIManager.ToggleMoveMode(MoveMode);
                    LockTileManager lockTile = CurrentTile as LockTileManager;
                    // Move mode active -> only enable move
                    if (MoveMode)
                    {
                        if (CurrentTile != null)
                        {
                            CurrentTile.HighlightAdjacentsTiles();
                        }
                        UIManager.DisableButtons(); // Disable chat
                        hackerUIManager.ToggleActionButtons(true, false, false);
                    }
                    // Regular tile => disable action button
                    else if (!(CurrentTile is ISpecialTile) || (CurrentTile is StartTileManager && !HasHackedComputer))
                    {
                        hackerUIManager.ToggleActionButtons(MoveCount > 0 || (ActionCount > 0 && actionToMoveMode), false, true);
                    }
                    else if (lockTile != null && !lockTile.IsLocked)
                    {
                        hackerUIManager.ToggleActionButtons(MoveCount > 0 || (ActionCount > 0 && actionToMoveMode), false, true);
                    }
                    else // Special tile => enable all
                    {
                        hackerUIManager.ToggleActionButtons(MoveCount > 0 || (ActionCount > 0 && actionToMoveMode), ActionCount > 0, true);
                    }

                }
                // Send message phase
                // Default phase, moving and actions
                // Someone elses turn
                else
                {
                    UIManager.HideTurnSubtitle();
                    CheckForMessagePhase();
                    // Don't remove all highlighting unless start tile is selected so map is not all dark to begin with
                    if (StartingTileSelected) mapManager.RemoveHighlighting();
                }
            }
        }

        public void CheckForMessagePhase()
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;
            //it's players turn, hide message buttons
            if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
            {
                hackerUIManager.ToggleMessageButtons(false, false);
                hackerUIManager.ToggleReleaseButton(CanSaveTeammate);
            }
            else if (IsSendingMessage && !hasSendAMessage)
            {
                UIManager.SetTurnSubtitle("Send message");
                hackerUIManager.ShowMessageActions();
                hackerUIManager.ToggleMessageButtons(true, true);
                hackerUIManager.ToggleReleaseButton(false);
            }
            else
            {
                hackerUIManager.HideActions();
            }
        }

        public void ToggleMoveMode()
        {
            MoveMode = !MoveMode;
            UpdateUI();
        }

        public void CenterCamera()
        {
            cameraManager.CenterCamera(gameObject);
        }

        //Move player's pawn to x and y coordinates of the tile while keeping z coordinate the same
        public virtual void MovePlayer(TileManager tile)
        {
            CurrentTile = tile;
            MoveCharacterIcon(tile);
            CheckHackersInSameTile();
            tile.UpdateDoorSprites();
            CheckForGuards();
            // mapManager.RemoveHighlighting();

            NetworkManager.photonView.RPC("OnHackerMoved", RpcTarget.AllBuffered, photonView.Owner, tile.photonView.ViewID);

            // Select starting tile
            if (!StartingTileSelected)
            {
                StartingTileSelected = true;
                cameraManager.startingRoomZoom();
                // Show icon
                GraphicsUtils.ChangeAlpha(spriteRenderer, 1);
                ClearMoveCount();
                EndTurn();
            }
            else
            {
                UseTurn(false);
            }
        }
        private void MoveCharacterIcon(TileManager newTile)
        {
            RoleType localPlayerRole = PhotonNetwork.LocalPlayer.GetRole();
            //Determine which role the player has and assign right gameobject as its "movetarget"
            GameObject moveTarget = newTile.transform.GetChild(1 + (int)localPlayerRole).gameObject;
            // Move character to the position reserved for their role
            transform.position = moveTarget.transform.position;
            cameraManager.CenterCamera(moveTarget.gameObject);
        }

        // This doesn't check for capture, only change guards visibility
        public void CheckForGuards()
        {
            foreach (GuardManager guard in GeneralManager.Guards)
            {
                DoorManager doorBetweenGuardAndHacker = CurrentTile.GetDoor(guard.CurrentTile);
                // Hacker is in the same tile or adjacent tile as guard
                if (guard.CurrentTile == CurrentTile || CurrentTile.AdjacentTiles.Find((it) => it == guard.CurrentTile))
                {
                    guard.gameObject.SetActive(true);
                }
                else
                {
                    guard.gameObject.SetActive(false);
                }
            }
        }

        public virtual void UseTurn(bool isAction)
        {
            if (isAction)
            {
                ActionCount--;
            }
            else if (MoveCount > 0)
            {
                MoveCount--;
            }
            else if (actionToMoveMode)
            {
                ActionCount--;
            }
            CheckForTurnEnding();
        }

        public void CheckForTurnEnding()
        {
            if (MoveCount > 0)
            {
                UpdateUI();
            }
            //actions can be turned into movement.
            else if (ActionCount > 0 && actionToMoveMode)
            {
                UpdateUI();
            }
            else if (ActionCount > 0 && CanSaveTeammate)//Has action points left and is in a tile with captured teammate
            {
                UpdateUI();
            }
            // Has move or actions left. If only actions are left, check if current tile is ActionTile with the exception of being StartingTile and CEO Door not being opened. In the latter case, skip action.
            else if (HasActionAvailable && !IsCaptured)
            {
                UpdateUI();
            }
            else
            {
                EndTurn();
                IsSendingMessage = true;
            }
        }

        private void OpenDoorLockedPrompt(DoorManager door)
        {
            if (IsCaptured)
            {
                ToggleMoveMode();
                string title2 = "Door is locked and you are restrained";
                string description2 = "The door that you tried to move through is locked. Do you want to try to open it while restrained? This has a " + GeneralManager.CapturedMoveChance + "% chance of success.";
                UIManager.OpenPrompt(title2, description2, () => { TryToOpenDoor(door); });
                hackerUIManager.ToggleActionButtons(false, false, false);
                return;
            }
            ToggleMoveMode();
            string title = "Door is locked";
            string description = "The door that you tried to move through is locked. Do you want to try to open it? This has a " + GeneralManager.DoorMoveChance + "% chance of success. Failure will cause an alarm.";
            UIManager.OpenPrompt(title, description, () => { TryToOpenDoor(door); });
            hackerUIManager.ToggleActionButtons(false, false, false);
        }
        private void OpenDoorIsWindowPrompt(DoorManager door)
        {
            if (IsCaptured)
            {
                ToggleMoveMode();
                string title2 = "Try to go through window while restrained?";
                string description2 = "Trying to go through window has a " + GeneralManager.CapturedMoveChance + "% chance of success since you are restrained. Do you still wish to continue?";
                UIManager.OpenPrompt(title2, description2, () => { TryToOpenDoor(door); });
                hackerUIManager.ToggleActionButtons(false, false, false);
                return;
            }
            ToggleMoveMode();
            string title = "Try to go through window?";
            string description = "Trying to go through window has a " + GeneralManager.WindowMoveChance + "% chance of success. Failure will cause an alarm.";
            UIManager.OpenPrompt(title, description, () => { TryToOpenDoor(door); });
            hackerUIManager.ToggleActionButtons(false, false, false);
        }

        public void OpenPassTurnPrompt()
        {
            string title = "Do you want to pass your turn?";
            string description = "Doing this will pass your turn, but you will still be able to send a message before ending your turn.";
            UIManager.OpenPrompt(title, description, () => { PassTurn(); });
        }

        public virtual void OpenMovePrompt(DoorManager door)
        {
            TileManager nextTile = door.GetOtherTile(CurrentTile);
            ToggleMoveMode();
            string title = "Movement confirmation";
            string description = "Would you like to move to " + nextTile.gameObject.name + "?";
            UIManager.OpenPrompt(title, description, () =>
            {
                MovePlayer(nextTile);
                if (door.HasMotionDetector)
                {
                    door.HasMotionDetector = false;
                    photonView.RPC("OnMotionDetectorTriggered", RpcTarget.AllBuffered, door.photonView.ViewID, PhotonNetwork.LocalPlayer, CurrentTile.photonView.ViewID);
                }
            });
            hackerUIManager.ToggleActionButtons(false, false, false);
        }

        public virtual void TryToOpenDoor(DoorManager door)
        {
            if (door.IsCEODoor && !mapManager.ArethreeOutOfFourLocksOpen)
            {
                UIManager.OpenPrompt("Alarm!", "You're unable to breach the security of CEO's office door, because all of the security locks haven't been opened yet.");
                UIManager.TriggerAlarm(CurrentTile.photonView.ViewID);
                UseTurn(false);
            }
            else
            {
                float successThreshold;
                if (isCaptured)
                {
                    successThreshold = GeneralManager.CapturedMoveChance;
                }
                else if (door.IsWindow)
                    successThreshold = GeneralManager.WindowMoveChance;
                else
                    successThreshold = GeneralManager.DoorMoveChance;
                int roll = Random.Range(1, 101);
                int roll2 = 100 - roll;
                float limit = 100 - successThreshold;
                if (roll > successThreshold)
                {
                    UseTurn(false);
                    if (!isCaptured)
                    {
                        UIManager.OpenAlarmPrompt(roll, successThreshold);
                        UIManager.TriggerAlarm(CurrentTile.photonView.ViewID);
                    }
                    else
                    {
                        UIManager.OpenPrompt("FAILURE\n\nYou got " + roll2 + "\n\nYou needed " + limit + "\n\n", "You couldn't make it to the other side. Corporation doesn't know you tried.");
                    }

                }
                else
                {
                    if (door.IsCEODoor && door.IsWindow) // if CEO's window was breached by hacker, set it as breached
                    {
                        door.HasBeenBreached = true;
                        photonView.RPC("OnCeoWindowBreached", RpcTarget.AllBuffered, door.photonView.ViewID);
                    }
                    if (!door.IsWindow)
                    {
                        door.IsLocked = false;
                        photonView.RPC("OnDoorOpened", RpcTarget.AllBuffered, door.photonView.ViewID, PhotonNetwork.LocalPlayer, isCaptured);
                    }
                    MovePlayer(door.GetOtherTile(CurrentTile));
                    if (isCaptured)
                    {
                        UIManager.OpenPrompt("SUCCESS\n\nYou got " + roll2 + "\n\nYou needed " + limit + "\n\n", "You made it to the other side. Corporation does know this. Move your marker.");
                    }
                    else
                    {
                        UIManager.OpenPrompt("SUCCESS\n\nYou got " + roll2 + "\n\nYou needed " + limit + "\n\n", "You made it to the other side without causing an alarm");
                    }
                    if (door.HasMotionDetector)
                    {
                        door.HasMotionDetector = false;
                        photonView.RPC("OnMotionDetectorTriggered", RpcTarget.AllBuffered, door.photonView.ViewID, PhotonNetwork.LocalPlayer, CurrentTile.photonView.ViewID);
                    }

                }
            }
        }

        private void PassTurn()
        {
            ClearMoveCount();
            ClearActionCount();
            IsSendingMessage = true;
            EndTurn();
        }

        public void CheckHackersInSameTile()
        {
            if (CurrentTile == null) return;
            bool capturedhackerIsOnSameTile = false;
            foreach (Player player in GeneralManager.Players)
            {

                HackerManager hacker = player.TagObject as HackerManager;
                ChromaManager chroma = hacker as ChromaManager;
                if (hacker != null && hacker.CurrentTile != null && player.ActorNumber != photonView.OwnerActorNr) // Only for others than yourself
                {
                    DoorManager doorBetweenHackers = CurrentTile.GetDoor(hacker.CurrentTile);
                    if (chroma != null && chroma.Invisible) //hacker is chroma and they're invisible, set invisible
                        chroma.gameObject.SetActive(false);
                    else if (doorBetweenHackers != null && doorBetweenHackers.IsWindow) //window between hackers, set visible
                        hacker.gameObject.SetActive(true);
                    else if (hacker.CurrentTile == CurrentTile) //in the same tile, set visible
                        hacker.gameObject.SetActive(true);
                    else //otherwise hide
                        hacker.gameObject.SetActive(false);
                    if (hacker.CurrentTile == CurrentTile && hacker.IsCaptured && !IsCaptured)
                    {

                        capturedhackerIsOnSameTile = true;
                    }
                    //Additional checks for edge's hologram
                    EdgeManager edge = hacker as EdgeManager;
                    if (edge != null && edge.HologramTile != null)
                    {
                        DoorManager doorBetweenHackerAndHologram = CurrentTile.GetDoor(edge.HologramTile);
                        if (doorBetweenHackerAndHologram != null && doorBetweenHackerAndHologram.IsWindow)
                            GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
                        else if (CurrentTile == edge.HologramTile)
                            GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
                        else
                            GraphicsUtils.ChangeAlpha(hologramSprite, 0f);

                    }

                }
            }
            CanSaveTeammate = capturedhackerIsOnSameTile;

        }
        public virtual void ReleaseTeammates()
        {
            foreach (Player player in GeneralManager.Players)
            {
                HackerManager hacker = player.TagObject as HackerManager;
                if (hacker != null && player.ActorNumber != photonView.OwnerActorNr) // Only for others than yourself
                {
                    if (hacker.CurrentTile == CurrentTile && hacker.IsCaptured)
                    {
                        photonView.RPC("OnHackerReleased", RpcTarget.AllBuffered, player, PhotonNetwork.LocalPlayer);
                    }
                }
            }
            CanSaveTeammate = false;
            UseTurn(true);
        }

        public override void StartTurn()
        {
            base.StartTurn();
            isSendingMessage = false;
            if (IsMineOrIsDev)
            {
                if (IsCaptured)
                {
                    MoveCount = 1;
                    ClearActionCount();
                }
                else
                {
                    MoveCount = GeneralManager.PlayerMovesInTurn;
                    ActionCount = GeneralManager.PlayerActionsInTurn;
                }
                CheckHackersInSameTile();
                if (GeneralManager.TurnCounter > 1)
                    CenterCamera();
            }
        }

        public void SendMessage(Player receiver, string message)
        {
            NetworkManager.photonView.RPC("OnMessageReceived", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, receiver, message);
            chatManager.InitializeMessage("To " + receiver.NickName, message, true);
            hasSendAMessage = true;
            CheckForMessagePhase();
        }
        public new void SendMessage(string message)
        {
            NetworkManager.photonView.RPC("OnMessageReceived", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, message);
            chatManager.InitializeMessage("To everyone:", message, true);
            hasSendAMessage = true;
            CheckForMessagePhase();
        }
        public override void SyncStateWithServer()
        {
            base.SyncStateWithServer();
            Player player = photonView.Owner;
            // NOTE: This is the only place where local variable sets should be done to lowercase field and not property, just to prevent unnecessary SetCustomProp calls
            isSendingMessage = (bool)player.GetCustomProp("IsSendingMessage");
            startingTileSelected = (bool)player.GetCustomProp("StartingTileSelected");
            isCaptured = (bool)player.GetCustomProp("IsCaptured");
            canSaveTeammate = (bool)player.GetCustomProp("CanSaveTeammate");
            hasHackedComputer = (bool)player.GetCustomProp("HasHackedComputer");
            moveMode = (bool)player.GetCustomProp("MoveMode");
            object currentTileID = player.GetCustomProp("CurrentTileID");
            if (currentTileID != null)
            {
                CurrentTile = PhotonNetwork.GetPhotonView((int)currentTileID).GetComponent<TileManager>();
                // Transform is not synced automatically
                MoveCharacterIcon(CurrentTile);
            }
            if (startingTileSelected)
            {
                // Make sure icon is visible after reconnect, set to hiding in Awake by default
                GraphicsUtils.ChangeAlpha(spriteRenderer, 1);
            }
        }
        public override void SyncServerWithState()
        {
            base.SyncServerWithState();
            Player player = photonView.Owner;
            player.SetCustomProp("IsSendingMessage", IsSendingMessage);
            player.SetCustomProp("StartingTileSelected", StartingTileSelected);
            player.SetCustomProp("IsCaptured", IsCaptured);
            player.SetCustomProp("CanSaveTeammate", CanSaveTeammate);
            player.SetCustomProp("HasHackedComputer", HasHackedComputer);
            player.SetCustomProp("MoveMode", MoveMode);
            if (CurrentTile != null)
            {
                player.SetCustomProp("CurrentTileID", CurrentTile.photonView.ViewID);
            }
        }

        public void MessageSent()
        {
            hasSendAMessage = true;
        }

        public void NewRound()
        {
            isSendingMessage = true;
            hasSendAMessage = false;
        }
    }
}

