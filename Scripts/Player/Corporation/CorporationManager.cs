using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Manticore
{
    public class CorporationManager : PlayerManager
    {
        private bool selectingMotionDetectorDoor = false;
        private bool selectingDoorToLock = false;
        private bool selectingDoorToRemove = false;
        private CorporationUIManager corporationUIManager;
        private int lastCameraRoom = -1;
        private int movingGuardIndex = -1;
        private bool popUpOpen;
        private CameraManager cameraManager;
        private bool firstTurn = true;
        private bool alarmTriggeredLastTurn = false;
        public bool AlarmTriggeredLastTurn
        {
            get => alarmTriggeredLastTurn; set
            {
                alarmTriggeredLastTurn = value;
                photonView.Owner.SetCustomProp("AlarmTriggeredLastTurn", value);
            }
        }
        public bool IsMovingGuard { get => movingGuardIndex >= 0; }
        public GuardManager MovingGuard { get => IsMovingGuard ? GeneralManager.Guards[movingGuardIndex] : null; }
        public bool SelectingMotionDetectorDoor
        {
            get => selectingMotionDetectorDoor; private set
            {
                selectingMotionDetectorDoor = value;
                photonView.Owner.SetCustomProp("SelectingMotionDetectorDoor", value);
            }
        }
        public bool SelectingDoorToLock
        {
            get => selectingDoorToLock; private set
            {
                selectingDoorToLock = value;
                photonView.Owner.SetCustomProp("SelectingDoorToLock", value);
            }
        }
        // This shouldn't be in server state, because WaitForDoorSelect Coroutine can't really be stored in state, so select action can't be called after click
        public bool SelectingDoorToRemove
        {
            get => selectingDoorToRemove; private set
            {
                selectingDoorToRemove = value;
            }
        }
        public int LastCameraRoom
        {
            get => lastCameraRoom; private set
            {
                lastCameraRoom = value;
                photonView.Owner.SetCustomProp("LastCameraRoom", value);
            }
        }
        public int MovingGuardIndex
        {
            get => movingGuardIndex; private set
            {
                movingGuardIndex = value;
                photonView.Owner.SetCustomProp("MovingGuardIndex", value);
            }
        }
        public bool FirstTurn
        {
            get => firstTurn; private set
            {
                firstTurn = value;
                photonView.Owner.SetCustomProp("FirstTurn", value);
            }
        }
        protected override void Awake()
        {
            base.Awake();
            corporationUIManager = UIManager.transform.Find("CorporationUI").GetComponent<CorporationUIManager>();
            popUpOpen = false;
            cameraManager = GameObject.Find("Main Camera").GetComponent<CameraManager>();
            corporationUIManager.UpdateLockAndMotionDetectorCounts(mapManager.LockedDoorsForCorporation.Count, GeneralManager.MaxLockedDoorsAmount, mapManager.DoorsWithMotionDetector.Count, GeneralManager.MaxMotionDetectorsAmount);
        }


        private void Update()
        {
            if (IsMineOrIsDev && IsPlayerInTurn && Input.GetMouseButtonDown(0))
            {
                //  === Moving guards ===
                TileManager clickedTile = EventUtils.GetTileOverMouse();
                if (IsMovingGuard && clickedTile != null && !popUpOpen)
                {
                    GuardManager movingGuard = GeneralManager.Guards[MovingGuardIndex];

                    if (movingGuard.NextAllowedTiles.Find((tile) => tile == clickedTile) != null)
                    {
                        OpenMoveGuardPrompt(clickedTile);
                    }
                }
                // === Settings motion detector or locking ===
                DoorManager clickedDoor = EventUtils.GetDoorOverMouse();
                // Don't allow setting motion detector or lock that already has one
                if (clickedDoor == null) return;
                if (SelectingDoorToLock && !clickedDoor.HasMotionDetector && !clickedDoor.IsWindow && !popUpOpen)
                {
                    popUpOpen = true;
                    UIManager.OpenPrompt("Locking a door", "Would you like to lock the door between " + clickedDoor.GetLinkedTileName(0) + " and " + clickedDoor.GetLinkedTileName(1) + "?", () =>
                    {
                        popUpOpen = false;
                        LockDoor(clickedDoor);
                    }
                    , () =>
                     {
                         popUpOpen = false;
                     });

                }
                else if (SelectingMotionDetectorDoor && !clickedDoor.HasMotionDetector && !clickedDoor.IsLocked && !popUpOpen)
                {
                    popUpOpen = true;
                    UIManager.OpenPrompt("Setting a motion detector", "Would you like to set motion detector to the door between " + clickedDoor.GetLinkedTileName(0) + " and " + clickedDoor.GetLinkedTileName(1) + "?", () =>
                    {
                        popUpOpen = false;
                        PutMotionDetector(clickedDoor);
                    }
                    , () =>
                     {
                         popUpOpen = false;
                     });
                }
            }
        }
        public void MoveGuards()
        {
            MovingGuardIndex = 0;
            ToggleMoveGuardMode();
        }

        public void ShowUnlockedDoor(int doorID)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorID).GetComponent<DoorManager>();
            door.IsLockedForCorporation = false;
        }

        public void MoveGuard(TileManager tile)
        {
            GuardManager movingGuard = GeneralManager.Guards[MovingGuardIndex];
            movingGuard.Move(tile);
            // Update door locked color states in adjacent doors when moving to new tile
            foreach (TileManager adjacentTile in tile.AdjacentTiles)
            {
                DoorManager door = tile.GetDoor(adjacentTile);
                door.IsLockedForCorporation = door.IsLocked; // Update corporation lock state to match
                door.CorporationHasNoticedBreach = door.HasBeenBreached;
                door.SetDoorSprite();
                corporationUIManager.UpdateLockAndMotionDetectorCounts(mapManager.LockedDoorsForCorporation.Count, GeneralManager.MaxLockedDoorsAmount, mapManager.DoorsWithMotionDetector.Count, GeneralManager.MaxMotionDetectorsAmount);

            }
            NetworkManager.photonView.RPC("OnGuardMoved", RpcTarget.AllBuffered, movingGuard.photonView.ViewID, tile.photonView.ViewID);
            MovingGuardIndex++;
            if (tile.HasUncheckedAlarm)
            {
                tile.HasUncheckedAlarm = false;
                GuardManager.CanMoveBackNextTurn = true;
            }
            if (tile is LockTileManager)
                CheckLockTile(tile);
            if (MovingGuardIndex >= GeneralManager.Guards.Count)
            {
                MovingGuardIndex = -1;
                if (MoveCount > 0)
                    MoveCount--;
                else //if player is allowed to use action for moving, reduce actions instead
                    ActionCount--;
                GuardManager.CanMoveBackNextTurn = false;
                CheckForRevealedPlayers();
                CheckForTurnEnding();
            }
            ToggleMoveGuardMode();
        }
        private void CheckLockTile(TileManager tile)
        {
            LockTileManager lockTile = tile as LockTileManager;
            if (!lockTile.IsLocked && !lockTile.HasCorporationCheckedLock)
            {
                lockTile.HasCorporationCheckedLock = true;
                NetworkManager.photonView.RPC("OnGuardCheckedLock", RpcTarget.AllBuffered, tile.photonView.ViewID);
                UIManager.OpenPrompt("Lock has been opened!", "Guard notices that the lock in " + lockTile.gameObject.name + " has been opened by hackers!");
                UIManager.InitializeLogMessage("Lock in " + lockTile.gameObject.name + " has been opened by hackers.");
            }
        }

        public void CheckForRevealedPlayers()
        {
            foreach (Player player in GeneralManager.Players)
            {
                CheckForRevealedPlayer(player);
            }
        }

        public void CheckForRevealedPlayer(Player player)
        {
            foreach (GuardManager guard in GeneralManager.Guards)
            {
                HackerManager hacker = player.TagObject as HackerManager;
                ChromaManager chroma = hacker as ChromaManager;
                if (chroma != null)
                {
                    //If hacker is chroma and they are invisible, ignore this player              
                    if (chroma.Invisible)
                    {
                        return;
                    }

                }
                // Player is hacker
                if (hacker != null && !hacker.IsCaptured && guard.CurrentTile != null)
                {
                    EdgeManager edge = hacker as EdgeManager;
                    DoorManager doorBetweenGuardAndPlayer = guard.CurrentTile.GetDoor(hacker.CurrentTile);
                    // Hacker and guard on the same tile
                    if (hacker.CurrentTile == guard.CurrentTile)
                    {
                        hacker.gameObject.SetActive(true);
                        NetworkManager.photonView.RPC("OnHackerCaptured", RpcTarget.AllBuffered, player);
                        if (edge != null && edge.HologramTile != null) // If edge was captured and they have hologram active, destroy it
                            NetworkManager.photonView.RPC("OnEdgeHologramDestroyed", RpcTarget.AllBuffered, player, false);
                    }
                    // There is a window between guard and player
                    else if (doorBetweenGuardAndPlayer != null && doorBetweenGuardAndPlayer.IsWindow && !hacker.HasBeenSeen)
                    {
                        hacker.gameObject.SetActive(true);
                        NetworkManager.photonView.RPC("OnHackerSeenThroughWindow", RpcTarget.AllBuffered, player);
                    }
                    //Additional checks for edge's hologram
                    if (edge != null && edge.HologramTile != null)
                    {
                        DoorManager doorBetweenGuardAndHologram = guard.CurrentTile.GetDoor(edge.HologramTile);
                        if (doorBetweenGuardAndHologram != null && doorBetweenGuardAndHologram.IsWindow && !edge.HologramHasBeenSeen)
                        {
                            GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
                            NetworkManager.photonView.RPC("OnEdgeHologramSpotted", RpcTarget.AllBuffered, player, true);
                        }
                        else if (guard.CurrentTile == edge.HologramTile)
                        {
                            NetworkManager.photonView.RPC("OnEdgeHologramDestroyed", RpcTarget.AllBuffered, player, false);
                            UIManager.OpenPrompt("Hologram destroyed!", "Guard finds hacker " + player.NickName + " in " + guard.CurrentTile.gameObject.name + ", but they vanish as soon as guard touches them. It was a hologram!");
                            UIManager.InitializeLogMessage("Guard finds hacker " + player.NickName + " in " + guard.CurrentTile.gameObject.name + ", but they vanish as soon as guard touches them. It was a hologram!");
                        }
                    }
                }

            }
        }
        private void CheckForHackersSeenByCamera()
        {
            foreach (Player player in GeneralManager.Players)
            {
                CheckForHackerSeenByCamera(player);
            }
        }
        public void CheckForHackerSeenByCamera(Player player)
        {
            HackerManager hacker = player.TagObject as HackerManager;
            ChromaManager chroma = hacker as ChromaManager;
            if (chroma != null)
            {
                //If hacker is chroma and they are invisible, ignore this player
                if (chroma.Invisible)
                    return;
            }
            //Check if there are any hackers in tiles with active security camera. Ignore captured hackers
            if (hacker != null && !hacker.IsCaptured && hacker.CurrentTile.HasActiveCamera && !hacker.HasBeenSeen)
            {
                GuardManager.CanMoveBackNextTurn = true;
                NetworkManager.photonView.RPC("OnHackerSeenOnCamera", RpcTarget.AllBuffered, player, hacker.CurrentTile.photonView.ViewID);
            }
            //Additional checks for edge's hologram
            EdgeManager edge = hacker as EdgeManager;
            if (edge != null && edge.HologramTile != null && edge.HologramTile.HasActiveCamera && !edge.HologramHasBeenSeen)
            {
                GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
                NetworkManager.photonView.RPC("OnEdgeHologramSpotted", RpcTarget.AllBuffered, player, false);
            }
        }
        public void OpenHackerSeenPrompt(Player seenHacker)
        {

            UIManager.OpenPrompt("Hacker seen!", "You can see hacker " + seenHacker.NickName + " through a window. Tell them to place their character on the map.");
        }
        public void OpenHackerCapturedPrompt(Player capturedHacker)
        {
            UIManager.OpenPrompt("Hacker captured!", "You have captured hacker " + capturedHacker.NickName + ". Tell them to place their character on the map.");
        }

        private void ToggleMoveGuardMode()
        {
            if (IsMovingGuard && !popUpOpen)
            {
                GuardManager movingGuard = GeneralManager.Guards[MovingGuardIndex];
                //Focus camera on guard being moved (except on first turn)
                if (GeneralManager.TurnCounter > 1 && !movingGuard.FirstMove)
                {
                    cameraManager.CenterCamera(movingGuard.gameObject);
                }
                movingGuard.HighlightAllowedMoves();
            }
            else
            {
                mapManager.RemoveHighlighting();
            }
            UpdateUI();
        }
        public void OpenMoveGuardPrompt(TileManager nextTile)
        {
            popUpOpen = true;
            ToggleMoveGuardMode();
            string title = "Movement confirmation";
            string description = string.Format("Would you like guard {0} to move to " + nextTile.gameObject.name + "?", MovingGuardIndex + 1);
            UIManager.OpenPrompt(title, description, () =>
            {
                popUpOpen = false;
                MoveGuard(nextTile);
            }
            ,
            () =>
            {
                popUpOpen = false;
                ToggleMoveGuardMode();
            },
            true
            );
        }

        public override void StartTurn()
        {
            base.StartTurn();
            if (IsMineOrIsDev)
            {
                TileManager room = RandomCameraRoom();
                UIManager.OpenPrompt("Move the camera!", "Move the surveillance camera to " + room.gameObject.name + ".");
                NetworkManager.photonView.RPC("UpdateCameraRooms", RpcTarget.AllBuffered, room.photonView.ViewID);
                CheckForHackersSeenByCamera();
                CheckForRevealedPlayers();
                if (FirstTurn)
                {
                    MoveCount = 1;
                    FirstTurn = false;
                }
                else
                {
                    MoveCount = GeneralManager.CorporationMovesInTurn;
                }
                ActionCount = GeneralManager.CorporationActionsInTurn;
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            if (IsMineOrIsDev)
            {
                Player localPlayer = PhotonNetwork.LocalPlayer;
                if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
                {
                    corporationUIManager.ShowActions();
                    corporationUIManager.ToggleMotionDetectorMode(SelectingMotionDetectorDoor);
                    corporationUIManager.ToggleLockDoorMode(SelectingDoorToLock);
                    //Setting motion detector
                    if (SelectingMotionDetectorDoor)
                    {
                        mapManager.HighlightDoors(mapManager.OpenDoorsAndWindows, false);
                        mapManager.DarkenTiles();
                        UIManager.SetTurnSubtitle("Select door");
                        UIManager.DisableButtons();
                        corporationUIManager.ToggleButtons(false, true, false, false);
                    }
                    // Locking a door
                    else if (SelectingDoorToLock)
                    {
                        mapManager.HighlightDoors(mapManager.DoorsToBeLocked, false);
                        mapManager.DarkenTiles();
                        UIManager.SetTurnSubtitle("Select door");
                        UIManager.DisableButtons();
                        corporationUIManager.ToggleButtons(false, false, true, false);
                    }
                    else if (SelectingDoorToRemove)
                    {
                        UIManager.DisableButtons();
                        corporationUIManager.ToggleButtons(false, false, false, false);
                    }
                    else if (IsMovingGuard) //moving a guard
                    {
                        UIManager.SetTurnSubtitle("Move a guard");
                        UIManager.DisableButtons();
                        corporationUIManager.DisableButtons();
                        MovingGuard.HighlightAllowedMoves();
                    }
                    else //Normal state
                    {
                        mapManager.RemoveHighlighting();
                        UIManager.HideTurnSubtitle();
                        bool hasActionsLeft = ActionCount > 0;
                        bool hasMovesLeft = MoveCount > 0;
                        corporationUIManager.ToggleButtons((hasMovesLeft || (AlarmTriggeredLastTurn && hasActionsLeft && GeneralManager.CorporationMovesInTurn == 1)), hasActionsLeft, hasActionsLeft, hasActionsLeft);
                    }
                }
                // Someone elses turn
                else
                {
                    corporationUIManager.HideActions();
                    UIManager.HideTurnSubtitle();
                }
            }
        }
        private void CheckForTurnEnding()
        {
            if (IsPlayerInTurn)
            {
                UpdateUI();
            }
            else
            {
                AlarmTriggeredLastTurn = false;
                EndTurn();
            }
        }

        public void ToggleMotionDetectorMode()
        {
            SelectingMotionDetectorDoor = !SelectingMotionDetectorDoor;
            UpdateUI();
        }
        public void ToggleDoorLockMode()
        {
            SelectingDoorToLock = !SelectingDoorToLock;
            UpdateUI();
        }
        // public void MoveGuards()
        // {
        //   UIManager.OpenPrompt("Move guards", "Move guards on the table and click ok to mark it done.", () =>
        //   {
        //     MoveCount--;
        //     CheckForTurnEnding();
        //   }, () => { }, false);
        // }
        private void LockDoor(DoorManager door)
        {
            ToggleDoorLockMode();
            Action LockSelectedDoor = () =>
            {
                door.IsLocked = true;
                door.IsLockedForCorporation = true;
                NetworkManager.photonView.RPC("OnDoorLocked", RpcTarget.AllBuffered, door.photonView.ViewID);
                UIManager.OpenPrompt("Door locked!", "Door has been locked. Place a locked door marker on the table.");
                ActionCount--;
                SelectingDoorToRemove = false;
                CheckForTurnEnding();
            };
            // If max amount of motion detectors have been reached, remove a existing detector first
            if (mapManager.LockedDoorsForCorporation.Count >= GeneralManager.MaxLockedDoorsAmount)
            {
                UIManager.OpenPrompt("Maximum amount of doors locked", "You have reached maximum amount of locked doors. Select one from the existing to be removed");
                StartCoroutine(WaitForDoorSelect(mapManager.LockedDoorsForCorporation, (DoorManager selectedDoor) =>
                {
                    selectedDoor.IsLocked = false; // Should change IsLockedForCorporation as well.
                    selectedDoor.IsLockedForCorporation = false;
                    NetworkManager.photonView.RPC("OnDoorOpened", RpcTarget.Others, selectedDoor.photonView.ViewID);
                    LockSelectedDoor();
                }));
            }
            else
            {
                LockSelectedDoor();
            }
            corporationUIManager.UpdateLockAndMotionDetectorCounts(mapManager.LockedDoorsForCorporation.Count, GeneralManager.MaxLockedDoorsAmount, mapManager.DoorsWithMotionDetector.Count, GeneralManager.MaxMotionDetectorsAmount);
        }
        private void PutMotionDetector(DoorManager door)
        {
            ToggleMotionDetectorMode();
            Action PutSelectedMotionDetector = () =>
            {
                door.HasMotionDetector = true;
                NetworkManager.photonView.RPC("OnMotionDetectorInitialized", RpcTarget.AllBuffered, door.photonView.ViewID);
                ActionCount--;
                SelectingDoorToRemove = false;
                CheckForTurnEnding();
            };
            // If max amount of motion detectors have been reached, remove a existing detector first
            if (mapManager.DoorsWithMotionDetector.Count >= GeneralManager.MaxMotionDetectorsAmount)
            {
                UIManager.OpenPrompt("Maximum amount of motion detectors put", "You have reached maximum amount of motion detectors. Select one from the existing to be removed");
                StartCoroutine(WaitForDoorSelect(mapManager.DoorsWithMotionDetector, (DoorManager selectedDoor) =>
                {
                    selectedDoor.HasMotionDetector = false;
                    NetworkManager.photonView.RPC("OnMotionDetectorRemoved", RpcTarget.AllBuffered, selectedDoor.photonView.ViewID);
                    PutSelectedMotionDetector();
                }, true));
            }
            else
            {
                PutSelectedMotionDetector();
            }
            corporationUIManager.UpdateLockAndMotionDetectorCounts(mapManager.LockedDoorsForCorporation.Count, GeneralManager.MaxLockedDoorsAmount, mapManager.DoorsWithMotionDetector.Count, GeneralManager.MaxMotionDetectorsAmount);
        }
        private IEnumerator WaitForDoorSelect(List<DoorManager> allowedDoors, Action<DoorManager> callback, bool isSelectingMotionDetector = false)
        {
            SelectingDoorToRemove = true;
            corporationUIManager.ToggleButtons(false, false, false, false);
            mapManager.HighlightDoors(allowedDoors);
            mapManager.DarkenTiles();
            DoorManager selectedDoor = null;
            bool done = false;
            while (!done)
            {
                if (Input.GetMouseButtonDown(0) && !popUpOpen)
                {
                    DoorManager clickedDoor = EventUtils.GetDoorOverMouse();
                    if (allowedDoors.FindIndex((tile) => tile == clickedDoor) >= 0)
                    {
                        selectedDoor = clickedDoor;
                        popUpOpen = true;
                        string promptTitle = isSelectingMotionDetector ? "Removing motion detector" : "Removing lock";
                        string promptDescription = isSelectingMotionDetector ?
                          "Would you like remove the motion detector between " + clickedDoor.GetLinkedTileName(0) + " and " + clickedDoor.GetLinkedTileName(1) + "?" :
                          "Would you like remove the lock between " + clickedDoor.GetLinkedTileName(0) + " and " + clickedDoor.GetLinkedTileName(1) + "?";
                        UIManager.OpenPrompt(promptTitle, promptDescription, () =>
                        {
                            mapManager.RemoveHighlighting();
                            done = true;
                            popUpOpen = false;
                        }, () =>
                        {
                            popUpOpen = false;
                        });
                    }
                }
                yield return null;
            }
            callback(selectedDoor);
        }
        public void SkipAction()
        {
            ActionCount = 0;
            CheckForTurnEnding();
        }

        private TileManager RandomCameraRoom()
        {
            while (true)
            {
                int rnd = Random.Range(0, mapManager.CameraTiles.Count);
                if (rnd != LastCameraRoom)
                {
                    LastCameraRoom = rnd;
                    break;
                }
            }
            TileManager chosenCameraTile = mapManager.CameraTiles[LastCameraRoom];
            return chosenCameraTile;
        }
        public override void SyncStateWithServer()
        {
            base.SyncStateWithServer();
            Player player = photonView.Owner;
            SelectingMotionDetectorDoor = (bool)player.GetCustomProp("SelectingMotionDetectorDoor");
            SelectingDoorToLock = (bool)player.GetCustomProp("SelectingDoorToLock");
            LastCameraRoom = (int)player.GetCustomProp("LastCameraRoom");
            MovingGuardIndex = (int)player.GetCustomProp("MovingGuardIndex");
            AlarmTriggeredLastTurn = (bool)player.GetCustomProp("AlarmTriggeredLastTurn");
            FirstTurn = (bool)player.GetCustomProp("FirstTurn");
        }
        public override void SyncServerWithState()
        {
            base.SyncServerWithState();
            Player player = photonView.Owner;
            player.SetCustomProp("SelectingMotionDetectorDoor", SelectingMotionDetectorDoor);
            player.SetCustomProp("SelectingDoorToLock", SelectingDoorToLock);
            player.SetCustomProp("LastCameraRoom", LastCameraRoom);
            player.SetCustomProp("MovingGuardIndex", MovingGuardIndex);
            player.SetCustomProp("AlarmTriggeredLastTurn", AlarmTriggeredLastTurn);
            player.SetCustomProp("FirstTurn", FirstTurn);
        }
    }
}
