using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Malee.List;

namespace Manticore
{
    public class LockTileManager : TileManager, ISpecialTile
    {
        private GeneralUIManager uiManager;
        public bool IsLocked { get => isLocked; set => isLocked = value; }
        [SerializeField]
        private bool isLocked;
        public bool HasCorporationCheckedLock { get => hasCorporationCheckedLock; set => hasCorporationCheckedLock = value; }
        private bool hasCorporationCheckedLock = false;

        public override void Start()
        {
            base.Start();
            uiManager = GameObject.Find("UI").GetComponent<GeneralUIManager>();
            isLocked = true;
        }
        public void OpenActionPrompt()
        {
            if (!isLocked)
            {
                uiManager.OpenPrompt("Lock is open!", "The lock is already open! Somebody else got here before you:)");
                return;
            }
            uiManager.OpenPrompt("Try to open lock?", "Attempt to hack one of the locks needed to get to the main office? This has a " + GeneralManager.CEOLockHackChance + "% chance of success.", () =>
             {
                 Action();
             });
        }
        public void Action()
        {
            int hackingRoll = UnityEngine.Random.Range(1, 101);
            float chance = GeneralManager.CEOLockHackChance;
            if (hackingRoll > chance)
            {
                uiManager.TriggerAlarm(this.photonView.ViewID);
                uiManager.OpenAlarmPrompt(hackingRoll, chance);

            }
            else
            {
                GeneralManager.LocalPlayer.NetworkManager.photonView.RPC("OnLockOpened", RpcTarget.AllBuffered, this.photonView.ViewID);
                isLocked = false;
                uiManager.IncreaseDoorsOpened(this.gameObject.name);
                int roll = 100 - hackingRoll;
                float chance2 = 100f - chance;
                uiManager.OpenPrompt("SUCCESS\n\nYou got " + roll + "\n\nYou needed " + chance2 + "\n\n", "The lock is now open!");

            }
            HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
            hacker.UseTurn(true);
        }

        [Serializable]
        public class CEODoorsList : ReorderableArray<DoorManager> { }
    }
}
