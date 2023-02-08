using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Malee.List;
using Photon.Pun;

namespace Manticore
{
    /// <summary>
    /// This class is meant to handle any functionality that map tiles need to perform, such as highlighting adjacent tiles.
    /// </summary>
    public class TileManager : GraphicsElement
    {
        public DoorsList DoorList { get { return doorList; } }
        [SerializeField]
        private DoorsList doorList;
        public List<TileManager> AdjacentTiles { get; private set; } = new List<TileManager>();
        private MapManager mapManager;
        private SpriteRenderer tileIcon;
        [SerializeField]
        private Sprite defaultSprite, activeCameraSprite;
        public float TilePositionOffsetX { get => tilePositionOffsetX; }
        public float tilePositionOffsetX;
        public float TilePositionOffsetY { get => tilePositionOffsetY; }
        public float tilePositionOffsetY;
        [SerializeField]
        private bool hasActiveCamera;
        public bool HasActiveCamera
        {
            get => hasActiveCamera;
            set
            {
                hasActiveCamera = value;
                if (value == true)
                    tileIcon.sprite = activeCameraSprite;
                else
                    tileIcon.sprite = defaultSprite;
            }
        }
        [SerializeField]
        private bool isSignalRoom;
        public bool IsSignalRoom { get => isSignalRoom; }

        private bool hasUncheckedAlarm = false;
        public bool HasUncheckedAlarm { get => hasUncheckedAlarm; set => hasUncheckedAlarm = value; }
        protected override void Awake()
        {
            base.Awake();
            tileIcon = transform.Find("Icon").GetComponent<SpriteRenderer>();
            mapManager = transform.parent.parent.GetComponent<MapManager>();
            // Set tile as refernce to all linked doors
            foreach (DoorManager door in doorList)
            {
                door.SetTile(this);
            }
        }

        public virtual void Start()
        {
            // Initialize adjacentTiles list so this doesn't need to be generated every time HighlightAdjacentTiles is called
            foreach (DoorManager door in doorList)
            {
                AdjacentTiles.Add(door.GetOtherTile(this));
            }
        }


        public void HighlightAdjacentsTiles()
        {
            mapManager.HighlightTiles(AdjacentTiles, false);
            ChangeAlpha(GraphicsUtils.MEDIUM_DARK_ALPHA); // Change this tile to light dark so it is separate from other dark tiles
            List<DoorManager> doors = new List<DoorManager>(doorList.ToArray());
            mapManager.HighlightDoors(doors, false);
        }

        public void HighlightAdjacentsTiles(TileManager excludedTile)
        {
            List<TileManager> highlightedTiles = AdjacentTiles.FindAll((tile) => tile != excludedTile);
            List<DoorManager> doors = new List<DoorManager>(doorList.ToArray());
            List<DoorManager> highlightedDoors = new List<DoorManager>(doors.FindAll((door) => door != excludedTile.GetDoor(this)));
            mapManager.HighlightTiles(highlightedTiles, false);
            ChangeAlpha(GraphicsUtils.MEDIUM_DARK_ALPHA); // Change this tile to light dark so it is separate from other dark tiles
            mapManager.HighlightDoors(highlightedDoors, false);
        }

        public bool IsTileAdjacent(TileManager target)
        {
            foreach (TileManager tile in AdjacentTiles)
            {
                if (tile == target)
                    return true;
            }
            return false;
        }

        public DoorManager GetDoor(TileManager otherTile)
        {
            DoorManager matchingDoor = null;
            foreach (DoorManager door in doorList)
            {
                if (door.GetOtherTile(this) == otherTile)
                {
                    matchingDoor = door;
                }
            }
            return matchingDoor;
        }
        public void UpdateDoorSprites()
        {
            foreach (DoorManager door in doorList)
            {
                door.SetDoorSprite();
            }
        }
        public static string GetTileActionTitle(TileManager tile)
        {
            if (tile is LockTileManager)
            {
                LockTileManager lockTile = tile as LockTileManager;
                if (lockTile.IsLocked)
                    return "OPEN LOCK";
                else
                    return "LOCK OPENED";
            }
            else if (tile is FalseAlarmTileManager)
                return "CAUSE FALSE ALARM";
            else if (tile is CEOOfficeTileManager)
                return "HACK COMPUTER";
            else if (tile is StartTileManager && GeneralManager.CEOComputerHacked)
                return "ESCAPE";
            return "NO ACTION"; // Default => regular tile
        }
        [Serializable]
        public class TilesList : ReorderableArray<TileManager> { }
        [Serializable]
        public class DoorsList : ReorderableArray<DoorManager> { }
    }
}

