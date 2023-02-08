using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manticore
{
    /// <summary>
    /// Handles receiving Photon RPCs that players get.
    /// </summary>
    [RequireComponent(typeof(PhotonView), typeof(PlayerManager))]
    public class PlayerNetworkManager : MonoBehaviourPun
    {
        [PunRPC]
        private void OnHackerMoved(Player player, int tileViewID)
        {
            // Change player's Currenttile to update make it position correct on this client as well
            TileManager newTile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<TileManager>();
            HackerManager hacker = player.TagObject as HackerManager;
            hacker.CurrentTile = newTile;
            hacker.HasBeenSeen = false;
            // Set player activity based on whether the new tile is same that this client's player current tile.
            PlayerManager localPlayer = GeneralManager.LocalPlayer as PlayerManager;
            if (localPlayer.IsHacker)
            {
                HackerManager localHacker = GeneralManager.LocalPlayer as HackerManager;
                localHacker.CheckHackersInSameTile();
            }
            else
            {
                if (!hacker.IsCaptured)
                {
                    hacker.gameObject.SetActive(false);
                }
                CorporationManager cm = GeneralManager.LocalPlayer as CorporationManager;
                cm.CheckForRevealedPlayer(player);
                cm.CheckForHackerSeenByCamera(player);
            }
        }
        [PunRPC]
        private void OnGuardMoved(int guardViewID, int tileViewID)
        {
            // Change guard's Currenttile to update make it position correct on this client as well
            TileManager newTile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<TileManager>();
            GuardManager guard = PhotonNetwork.GetPhotonView(guardViewID).GetComponent<GuardManager>();
            guard.Move(newTile);
        }


        [PunRPC]
        public void SetPlayerInfo(Player player, int viewID)
        {
            PhotonView view = PhotonNetwork.GetPhotonView(viewID);
            PlayerManager script = view.gameObject.GetComponent<PlayerManager>();
            player.TagObject = script;
            // NOTE: This shouldn't be necessary because this is called before starting tile selections
            // Check if all players have been linked a script -> linking ready -> Can check for players in same tile
            // if (GeneralManager.Players.Find((it) => it.TagObject == null) == null)
            // {
            //   HackerManager 
            //   ShowPlayersInSameTile();
            // }
        }
        [PunRPC]
        private void OnMessageReceived(Player sender, Player receiver, string message)
        {
            PlayerManager localPlayerManager = GeneralManager.LocalPlayer;
            if (PhotonNetwork.LocalPlayer == receiver && localPlayerManager.IsHacker)
            {
                // Show message
                HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
                hacker.UIManager.InitializeChatMessage(sender.GetNameWithRole(), message);
            }
            else if (!localPlayerManager.IsHacker)
            {
                // Note corporate of receiving message here.
            }
        }

        [PunRPC]
        private void OnMessageReceived(Player sender, string message)
        {
            PlayerManager localPlayerManager = GeneralManager.LocalPlayer;
            if (localPlayerManager.IsHacker && PhotonNetwork.LocalPlayer != sender)
            {
                // Show message
                HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
                hacker.UIManager.InitializeChatMessage(sender.GetNameWithRole(), message);
            }
            else if (!localPlayerManager.IsHacker)
            {
                // Note corporate of receiving message here.
            }
        }
        [PunRPC]
        private void OnLockOpened(int tileViewID)
        {
            LockTileManager tile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<LockTileManager>();
            tile.IsLocked = false;
        }
        [PunRPC]
        //This is not used currently
        private void OnAlarmTriggeredWithoutID()
        {
            GuardManager.CanMoveBackNextTurn = true;
            if (!GeneralManager.LocalPlayer.IsHacker)
            {
                CorporationManager cm = GeneralManager.LocalPlayer as CorporationManager;
                cm.AlarmTriggeredLastTurn = true;
                GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
                uiManager.InitializeLogMessage("Alarm was triggered");
                uiManager.OpenPrompt("Alarm triggered!", "Alarm has been triggered by the current player. Tell them to place alarm marker on the table");
            }
        }
        [PunRPC]
        private void OnAlarmTriggered(int tileViewID)
        {
            TileManager tile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<TileManager>();
            GuardManager.CanMoveBackNextTurn = true;
            tile.HasUncheckedAlarm = true;
            if (!GeneralManager.LocalPlayer.IsHacker)
            {
                CorporationManager cm = GeneralManager.LocalPlayer as CorporationManager;
                cm.AlarmTriggeredLastTurn = true;
                GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
                uiManager.InitializeLogMessage("Alarm was triggered on the room " + tile.gameObject.name);
                uiManager.OpenPrompt("Alarm triggered!", "Alarm has been triggered on the room " + tile.gameObject.name + ". Tell them to place alarm marker on the table");
            }
        }

        [PunRPC]
        private void OnCapturedBreachedLockedDoor(Player hacker, int doorViewID)
        {
            if (!GeneralManager.LocalPlayer.IsHacker)
            {
                CorporationManager cm = GeneralManager.LocalPlayer as CorporationManager;
                GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
                uiManager.InitializeLogMessage("Captured hacker made it thought a locked door");
                uiManager.OpenPrompt("Locked door was opened", "Captured hacker made it thought a locked door");
            }
        }

        [PunRPC]
        private void OnMotionDetectorInitialized(int doorViewID)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.HasMotionDetector = true;
        }
        [PunRPC]
        private void OnMotionDetectorRemoved(int doorViewID)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.HasMotionDetector = false;
        }
        [PunRPC]
        private void OnMotionDetectorTriggered(int doorViewID, Player spottedHacker, int destinationTileViewId)
        {
            HackerManager hackerManager = spottedHacker.TagObject as HackerManager;
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.HasMotionDetector = false;
            TileManager destinationTile = PhotonNetwork.GetPhotonView(destinationTileViewId).GetComponent<TileManager>();
            TileManager sourceTile = door.GetOtherTile(destinationTile);
            if (!GeneralManager.LocalPlayer.IsHacker)
            {
                GuardManager.CanMoveBackNextTurn = true;
                GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
                uiManager.OpenPrompt("Motion detector triggered!", "Hacker " + spottedHacker.NickName + " triggered a motion detector while moving from " + sourceTile.gameObject.name + " to " + destinationTile.gameObject.name + ".");
                uiManager.InitializeLogMessage("Hacker " + spottedHacker.NickName + " triggered a motion detector while moving from " + sourceTile.gameObject.name + " to " + destinationTile.gameObject.name + ".");
                hackerManager.gameObject.SetActive(true);
            }
        }
        [PunRPC]
        private void OnDoorLocked(int doorViewID)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.IsLocked = true;
        }
        [PunRPC]
        private void OnDoorOpened(int doorViewID, Player hacker, bool iscaptured)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.IsLocked = false;
            if (!GeneralManager.LocalPlayer.IsHacker && iscaptured)
            {
                CorporationManager cm = GeneralManager.LocalPlayer as CorporationManager;
                GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
                cm.ShowUnlockedDoor(doorViewID);
                uiManager.InitializeLogMessage("Captured hacker " + hacker.NickName + " made it thought a locked door");
                uiManager.OpenPrompt("Locked door was opened", "Captured hacker " + hacker.NickName + " made it thought a locked door");
            }
        }


        [PunRPC]
        private void OnCeoWindowBreached(int doorViewID)
        {
            DoorManager door = PhotonNetwork.GetPhotonView(doorViewID).GetComponent<DoorManager>();
            door.HasBeenBreached = true;
        }
        [PunRPC]
        private void OnGuardInstantiated(int guardViewID)
        {
            GuardManager guard = PhotonNetwork.GetPhotonView(guardViewID).GetComponent<GuardManager>();
        }
        [PunRPC]
        private void OnCEOComputerHacked()
        {
            GeneralManager gm = GameObject.Find("DDOL").GetComponent<GeneralManager>();
            GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
            if (!GeneralManager.CEOComputerHacked)
            {
                if (!GeneralManager.LocalPlayer.IsHacker && GeneralManager.START_AMOUNT_OF_GUARDS < 3)
                {
                    gm.InstantiateGuard();
                    uiManager.OpenPrompt("CEO computer hacked!",
                        "CEO's computer has been hacked! Reinforcements have been called");
                }
                else
                {
                    uiManager.OpenPrompt("CEO computer hacked!", "CEO's computer has been hacked by another player! Make sure the data gets outside the building");
                }
            }
            gm.SetCEOComputerAsHacked();
        }

        [PunRPC]
        private void OnHackerReleased(Player releaseTarget, Player releasedBy)
        {
            HackerManager releasedHacker = releaseTarget.TagObject as HackerManager;
            releasedHacker.IsCaptured = false;
            if (PhotonNetwork.LocalPlayer == releaseTarget)
            {
                releasedHacker.UIManager.OpenPrompt("You have been saved!", "You have been saved by " + releasedBy.NickName + ". You can now do actions again. You can take your player of the table.");
            }
            else if (PhotonNetwork.LocalPlayer == releasedBy)
            {
                releaseTarget.SetCustomProp("IsCaptured", false);
                HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
                hacker.UIManager.OpenPrompt("Release success!", "You have successfully released " + releaseTarget.NickName);
            }
            else if (GeneralManager.LocalPlayer.IsHacker)
            {
                HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
                hacker.UIManager.OpenPrompt("Hacker released!", releaseTarget.NickName + " has been released by " + releasedBy.NickName);
            }
        }

        [PunRPC]
        private void OnHackerCaptured(Player capturedHacker)
        {
            HackerManager hacker = capturedHacker.TagObject as HackerManager;
            hacker.IsCaptured = true;

            //Go through each player in game to see if there are any hackers left who aren't captured
            bool areThereRemainingHackers = false;
            foreach (Player player in GeneralManager.Players)
            {
                HackerManager pm = player.TagObject as HackerManager;
                if (pm != null && !pm.IsCaptured)
                    areThereRemainingHackers = true;
            }
            if (!areThereRemainingHackers)
                GeneralManager.Instance.EndGame(false);

            PlayerManager localPlayer = PhotonNetwork.LocalPlayer.TagObject as PlayerManager;
            if (!localPlayer.IsHacker)
            {
                (localPlayer as CorporationManager).OpenHackerCapturedPrompt(capturedHacker);
                GuardManager.CanMoveBackNextTurn = true;
            }
            else if (capturedHacker == PhotonNetwork.LocalPlayer)
            {
                capturedHacker.SetCustomProp("IsCaptured", true);
                hacker.UIManager.OpenPrompt("You have been captured!", "Guards have captured you! Place your character on the map. " +
                                                                       "You are unable to perform actions until you are saved by someone else but you can still move on the board if a guard is not in the same room." +
                                                                       "The corporation will see your movements at all times.");
            }
            else
            {
                hacker.UIManager.OpenPrompt("Hacker captured!", capturedHacker.NickName + " has been captured!");
            }
        }
        [PunRPC]
        private void OnHackerSeenThroughWindow(Player seenHacker)
        {
            HackerManager hacker = seenHacker.TagObject as HackerManager;
            TileManager hackerTile = hacker.CurrentTile;
            PlayerManager localPlayer = PhotonNetwork.LocalPlayer.TagObject as PlayerManager;
            hacker.HasBeenSeen = true;
            if (!localPlayer.IsHacker)
            {
                (localPlayer as CorporationManager).OpenHackerSeenPrompt(seenHacker);
                GuardManager.CanMoveBackNextTurn = true;
                ChatManager chatManager = localPlayer.UIManager.GetComponent<ChatManager>();
                chatManager.InitializeMessage("Hacker " + seenHacker.NickName + " was spotted in " + hackerTile.gameObject.name);
            }
            else if (seenHacker == PhotonNetwork.LocalPlayer)
            {
                hacker.UIManager.OpenPrompt("You have been seen!", "Guards have seen you through a window! Place your character on the map until guard can't see you anymore.");
            }
        }
        [PunRPC]
        private void OnHackerEscapedBuilding(Player escapedHacker)
        {
            GeneralManager.Instance.EndGame();
            //Following code is for old implementation. Since only hacker with data can escape and hacker escaping now ends the game, it is unnesessary
            /*
            HackerManager hacker = escapedHacker.TagObject as HackerManager;
            GeneralManager.Instance.RemovePlayerFromGame(escapedHacker);
            if (GeneralManager.Players.Count <= 1)
            {
                GeneralManager.Instance.EndGame();
            }
            else if (escapedHacker == PhotonNetwork.LocalPlayer)
            {
                hacker.EndTurn();
                hacker.UIManager.OpenPrompt("You have escaped!", "You have escaped the corporation building! You're now spectator for rest of the game.");
            }
            else
            {
                hacker.gameObject.SetActive(false);
                hacker.UIManager.OpenPrompt("Hacker has escaped!", escapedHacker.NickName + " has escaped the building!");
            }*/
        }
        [PunRPC]
        private void UpdateCameraRooms(int activeCameraRoomViewID)
        {
            TileManager activeCameraRoom = PhotonNetwork.GetPhotonView(activeCameraRoomViewID).GetComponent<TileManager>();
            MapManager mapManager = activeCameraRoom.GetComponentInParent<MapManager>();
            foreach (TileManager cameraRoom in mapManager.CameraTiles)
            {
                if (cameraRoom == activeCameraRoom)
                {
                    cameraRoom.HasActiveCamera = true;
                }
                else
                {
                    cameraRoom.HasActiveCamera = false;
                }
            }
        }

        [PunRPC]
        private void OnHackerSeenOnCamera(Player spottedHacker, int tileViewID)
        {
            TileManager tile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<TileManager>();
            GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
            HackerManager hackerManager = spottedHacker.TagObject as HackerManager;
            hackerManager.HasBeenSeen = true;
            if (PhotonNetwork.LocalPlayer == spottedHacker)
            {
                uiManager.OpenPrompt("You have been seen!", "You have been seen by the active security camera in the room! Place your character on the table.");
            }
            else if (!GeneralManager.LocalPlayer.IsHacker)
            {
                uiManager.OpenPrompt("Hacker seen on camera!", "Hacker " + spottedHacker.NickName + " has been seen on security camera in " + tile.gameObject.name + ".");
                uiManager.InitializeLogMessage("Hacker " + spottedHacker.NickName + " has been seen on security camera in " + tile.gameObject.name);
                hackerManager.gameObject.SetActive(true);
            }
        }
        [PunRPC]
        private void OnChromaAbilityTriggered(Player player)
        {
            ChromaManager chroma = player.TagObject as ChromaManager;
            if (chroma == null) //Null check just in case, though only Chroma should be able to call this
                return;

            chroma.Invisible = true;
        }
        [PunRPC]
        private void OnChromaAbilityEnded(Player player)
        {
            ChromaManager chroma = player.TagObject as ChromaManager;
            if (chroma == null) //Null check just in case, though only Chroma should be able to call this
                return;

            chroma.Invisible = false;
        }

        [PunRPC]
        private void OnGuardCheckedLock(int tileViewID)
        {
            LockTileManager tile = PhotonNetwork.GetPhotonView(tileViewID).GetComponent<LockTileManager>();
            tile.HasCorporationCheckedLock = true;

        }
        [PunRPC]
        private void OnEdgeHologramDeployed(Player player)
        {
            EdgeManager edge = player.TagObject as EdgeManager;
            edge.HologramTile = edge.CurrentTile;
            edge.SetHologramIconPosition();
        }

        [PunRPC]
        private void OnEdgeHologramDestroyed(Player player, bool wasDestroyedByEdge)
        {
            GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
            EdgeManager edge = player.TagObject as EdgeManager;
            edge.HologramTile = null;
            edge.HologramHasBeenSeen = false;
            edge.HideHologram();
            if (PhotonNetwork.LocalPlayer == player)
            {
                edge.SetCooldown();
                if (wasDestroyedByEdge)
                    uiManager.OpenPrompt("Hologram destroyed!", "You've destroyed the hologram. You will need to wait " + GeneralManager.EdgeAbilityCooldown + " turns before you can place another.");
                else
                    uiManager.OpenPrompt("Hologram destroyed!", "Your hologram has been destroyed! Ability will be on cooldown for " + GeneralManager.EdgeAbilityCooldown + " turns.");
            }
        }

        [PunRPC]
        private void OnEdgeHologramSpotted(Player player, bool wasSeenByGuard)
        {
            PlayerManager localPlayer = PhotonNetwork.LocalPlayer.TagObject as PlayerManager;
            EdgeManager edge = player.TagObject as EdgeManager;
            edge.HologramHasBeenSeen = true;
            GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
            if (PhotonNetwork.LocalPlayer == player)
            {
                if (wasSeenByGuard)
                {
                    uiManager.OpenPrompt("Your hologram has been spotted!", "Your hologram has been spotted by a guard!");
                }
                else
                {
                    uiManager.OpenPrompt("Your hologram has been seen on camera!", "Your hologram has been spotted by a security camera!");
                }
            }
            else if (!localPlayer.IsHacker)
            {
                GuardManager.CanMoveBackNextTurn = true;
                if (wasSeenByGuard)
                {
                    (localPlayer as CorporationManager).OpenHackerSeenPrompt(player);
                    ChatManager chatManager = localPlayer.UIManager.GetComponent<ChatManager>();
                    chatManager.InitializeMessage("Hacker " + player.NickName + " was spotted in " + edge.HologramTile.gameObject.name);
                }
                else
                {
                    uiManager.OpenPrompt("Hacker seen on camera!", "Hacker " + player.NickName + " has been seen on security camera in " + edge.HologramTile.gameObject.name + ".");
                    uiManager.InitializeLogMessage("Hacker " + player.NickName + " has been seen on security camera in " + edge.HologramTile.gameObject.name);
                }

            }

        }


    }
}
