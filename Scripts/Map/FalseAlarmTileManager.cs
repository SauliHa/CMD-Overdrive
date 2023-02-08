using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Manticore
{
  public class FalseAlarmTileManager : TileManager, ISpecialTile
  {
    private GeneralUIManager uiManager;
    private HackerUIManager hackerUIManager;
    private GameObject selectedTile;
    public override void Start()
    {
      base.Start();
      uiManager = GameObject.Find("UI").GetComponent<GeneralUIManager>();
      hackerUIManager = GameObject.Find("UI").transform.Find("HackerUI").GetComponent<HackerUIManager>();
    }

    public void OpenActionPrompt()
    {
      uiManager.OpenPrompt("Try to cause false alarm?", "Do you want to cause alarm to tile of your choice? This has " + GeneralManager.FalseAlarmChance +
                                                        " % chance to succeed. Failure will cause alarm on this tile instead.",
          () =>
          {
            StartCoroutine(hackerUIManager.WaitForTileSelect(tile => Action(tile)));
          });
    }

    public void Action()
    {
      // useless now, but can't be deleted
    }


    public void Action(int tileID)
    {
      PlayerManager player = GeneralManager.LocalPlayer;
      if (player.IsHacker)
      {
        hackerUIManager.ToggleActionButtons(true, true, true); 
        int falseAlarmRoll = Random.Range(1, 101);
        float chance = GeneralManager.FalseAlarmChance;
        if (falseAlarmRoll > chance )
        {
          uiManager.OpenAlarmPrompt(falseAlarmRoll, chance);

          TileManager myTile = hackerUIManager.GetCurrentTile();
          // poissa että ei tule turhaa tupla promptia
          //uiManager.OpenPrompt("False alarm failed!", "You have caused false alarm on your room. Place alarm marker on the table on that room.");
          uiManager.TriggerAlarm(this.photonView.ViewID);
        }
        else
        {
          int roll = 100 - falseAlarmRoll;
          float chance2 = 100 - chance;
          uiManager.OpenPrompt(
            "SUCCESS!\n\nYou got " + roll + "\n\nYou needed " + chance2 + "\n\n",
            "You have successfully caused false alarm on room " + tileID +
            "\". Place alarm marker on the table to the tile you selected.");
          TileManager chosenTile = GameObject.Find("Room " + tileID).GetComponent<TileManager>();
          uiManager.TriggerAlarm(chosenTile.photonView.ViewID);

        }
        HackerManager hacker = player as HackerManager;
        hacker.UseTurn(true);
      }
    }


  }
}
