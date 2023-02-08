using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Manticore
{
    public class CEOOfficeTileManager : TileManager, ISpecialTile
    {
        private GeneralUIManager uiManager;
        private GeneralManager generalManager;
        private HackerManager hacker;
        public bool IsAlreadyHacked { get => IsAlreadyHacked; set => IsAlreadyHacked = value; }
        public override void Start()
        {
            base.Start();
            uiManager = GameObject.Find("UI").GetComponent<GeneralUIManager>();
            generalManager = GameObject.Find("DDOL").GetComponent<GeneralManager>();
            hacker = GeneralManager.LocalPlayer as HackerManager;
        }
        public void OpenActionPrompt()
        {
            if (hacker.HasHackedComputer)
            {
                uiManager.OpenPrompt("Computer already hacked", "You have already hacked CEO computer so there is no reason for you to hack it again.");
                return;
            }
            else
            {
                uiManager.OpenPrompt("Try to hack CEO's computer?", "Hacking into CEO computer has a " + GeneralManager.CEOComputerHackChance + "% chance of success. Failure will cause an alarm.", () => { Action(); });
            }

        }
        public void Action()
        {

            int alarmRoll = Random.Range(1, 101);
            float chance = GeneralManager.CEOComputerHackChance;
            if (alarmRoll > chance)
            {
                uiManager.OpenAlarmPrompt(alarmRoll, chance);
                uiManager.TriggerAlarm(this.photonView.ViewID);
            }
            else
            {
                int roll = 100 - alarmRoll;
                float chance2 = 100 - chance;
                uiManager.OpenPrompt("SUCCESS!\n\nYou got " + roll + "\n\nYou needed " + chance2 + "\n\n", "You have successfully hacked into CEO's computer and are carrying it's important data! You should now leave the building before the guards catch you!");
                generalManager.SetCEOComputerAsHacked();
                hacker.HasHackedComputer = true;
                GeneralManager.LocalPlayer.photonView.RPC("OnCEOComputerHacked", RpcTarget.AllBuffered);
            }

            hacker.UseTurn(true);
        }


    }
}
