using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malee.List;
using Photon.Pun;

namespace Manticore
{
    public class DoorManager : GraphicsElement
    {
        [SerializeField]
        private bool isWindow = false;
        public bool IsWindow { get => isWindow; }
        // NOTE: These two are serialized for testing purposes. Later Corporation player will set these -> no need to set in Unity
        [SerializeField]
        private bool hasMotionDetector = false;
        [SerializeField]
        private bool isLocked = false;
        [SerializeField]
        private bool isCEODoor = false;
        private bool isLockedForCorporation = false;
        private bool hasBeenBreached = false;
        private bool corporationHasNoticedBreach = false;
        [SerializeField]
        private Sprite openDoorSprite, lockedDoorSprite, windowSprite, CEODoorSprite;
        private SpriteRenderer motionDetectorIcon;
        public bool HasMotionDetector
        {
            get => hasMotionDetector; set
            {
                hasMotionDetector = value;
                if (motionDetectorIcon == null) motionDetectorIcon = transform.Find("MotionDetectorIcon").GetComponent<SpriteRenderer>();
                // Show icons of motion detector changes only for corporation
                if (GeneralManager.LocalPlayer != null && !GeneralManager.LocalPlayer.IsHacker) SetMotionDetectorIcon(hasMotionDetector);
                else SetMotionDetectorIcon(false);
            }
        }
        // Use property setter to always change graphics of door when door is locked
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                isLocked = value;
                HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
                // If is corporation or hacker CurrentTile has this door
                if (hacker == null || hacker.CurrentTile != null && HasTile(hacker.CurrentTile))
                {
                    SetDoorSprite();
                }
                // Only set isLockedForCorporation as well if is setting to true or set is coming for Corporation
                //REMOVED: this is called in OnDoorOpened RPC, causing doors to appear open to corporation when they shouldn't
                /*if (value == true || (GeneralManager.LocalPlayer != null && !GeneralManager.LocalPlayer.IsHacker))
                {
                    isLockedForCorporation = value;
                }*/
            }
        }
        public bool IsLockedForCorporation
        {
            get => isLockedForCorporation;
            set
            {
                isLockedForCorporation = value;
                SetDoorSprite();
            }
        }
        public bool IsCEODoor { get => isCEODoor; }

        public bool HasBeenBreached { get => hasBeenBreached; set => hasBeenBreached = value; }
        public bool CorporationHasNoticedBreach { get => corporationHasNoticedBreach; set => corporationHasNoticedBreach = value; }


        // Door is link between two tiles => array of two
        private TileManager[] linkedTiles = new TileManager[2];

        protected override void Awake()
        {
            base.Awake();
            SetDoorSprite();
        }

        // For testing: triggers when changing values on Unity -> Set colors according to door state
        private void OnValidate()
        {
            SetDoorSprite();
        }
        // This is to avoid needing to put doors -> tiles reference manually. Tiles are linked to doors already, so this can be reversed by calling
        // this method from TileManager Awake-methods 
        public void SetTile(TileManager tile)
        {
            if (linkedTiles[0] == null)
            {
                linkedTiles[0] = tile;
            }
            else if (linkedTiles[1] == null)
            {
                linkedTiles[1] = tile;
            }
            else
            {
                Debug.LogError("Door " + this.name + " tiles have already been set, can't link tile: " + tile.name);
            }
        }

        // Get other side of door. Assumes that linkedTiles are set in this point and is called with other tile from this link.
        public TileManager GetOtherTile(TileManager tile)
        {
            if (!HasTile(tile)) return null;
            if (linkedTiles[0] != tile)
            {
                return linkedTiles[0];
            }
            else
            {
                return linkedTiles[1];
            }
        }
        public string GetLinkedTileName(int index)
        {
            return linkedTiles[index].gameObject.name;
        }
        public bool HasTile(TileManager tile)
        {
            return linkedTiles[0] == tile || linkedTiles[1] == tile;
        }
        // NOTE: This will probably be changed to SetSprite, when door and window graphics arrive
        public void SetDoorSprite()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (IsCeoDoorUnbreached())
            {
                spriteRenderer.sprite = CEODoorSprite;
            }
            else if (IsWindow)
            {
                spriteRenderer.sprite = windowSprite;
            }
            // Show as locked for Corporation by separate rule, so door can be shown locked for corporation even if hacker has opened it
            else if (IsLocked || (IsLockedForCorporation && GeneralManager.LocalPlayer != null && !GeneralManager.LocalPlayer.IsHacker))
            {
                spriteRenderer.sprite = lockedDoorSprite;
            }
            else
            {
                spriteRenderer.sprite = openDoorSprite;
            }
        }
        private bool IsCeoDoorUnbreached()
        {
            if (IsCEODoor)
            {
                if (IsWindow && (CorporationHasNoticedBreach || (HasBeenBreached && GeneralManager.LocalPlayer != null && GeneralManager.LocalPlayer.IsHacker)))
                {
                    return false;
                }
                else if (!IsLockedForCorporation || (!IsLocked && GeneralManager.LocalPlayer != null && GeneralManager.LocalPlayer.IsHacker))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void SetMotionDetectorIcon(bool visible)
        {
            if (motionDetectorIcon == null) motionDetectorIcon = transform.Find("MotionDetectorIcon").GetComponent<SpriteRenderer>();
            motionDetectorIcon.gameObject.SetActive(visible);
        }

        public void OpenCEODoorLock()
        {
            // No need for this now
        }


        [Serializable]
        public class LockTileList : ReorderableArray<LockTileManager> { }
    }
}

