using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Manticore
{
    public class BugzManager : HackerManager
    {
        private List<DoorManager> adjacentDoorsWithMotionDetectors;
        public List<DoorManager> AdjacentDoorsWithMotionDetectors
        {
            get => adjacentDoorsWithMotionDetectors;
        }
        protected override void Awake()
        {
            base.Awake();
            adjacentDoorsWithMotionDetectors = new List<DoorManager>();
        }
        public override void MovePlayer(TileManager tile)
        {
            base.MovePlayer(tile);
            CheckForMotionDetectors();
        }

        public override void StartTurn()
        {
            base.StartTurn();
            CheckForMotionDetectors();
        }

        private void CheckForMotionDetectors()
        {
            //clear the list of adjacent motion detectors first
            if (adjacentDoorsWithMotionDetectors != null)
                adjacentDoorsWithMotionDetectors.Clear();
            //Don't check for motion detectors if starting tile is not selected yet
            if (CurrentTile == null)
                return;
            //First hide all motion detector icons
            foreach (DoorManager door in mapManager.Doors)
            {
                door.SetMotionDetectorIcon(false);
            }
            //Then check if any of current room's doors has a motion detector
            foreach (DoorManager door in CurrentTile.DoorList)
            {
                if (door.HasMotionDetector)
                {
                    adjacentDoorsWithMotionDetectors.Add(door);
                    door.SetMotionDetectorIcon(true);
                    UIManager.OpenPrompt("Motion detector nearby!", "Your visor detects a motion detector in this room!");
                }
            }
        }
    }


}
