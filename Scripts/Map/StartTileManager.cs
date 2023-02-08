using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Manticore
{
    public class StartTileManager : TileManager, ISpecialTile
    {
        private GeneralUIManager uiManager;
        public override void Start()
        {
            base.Start();
            uiManager = GameObject.Find("UI").GetComponent<GeneralUIManager>();
        }
        public void Action()
        {
            GeneralManager.LocalPlayer.photonView.RPC("OnHackerEscapedBuilding", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
            HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
        }

        public void OpenActionPrompt()
        {
            HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
            if (GeneralManager.CEOComputerHacked && hacker.HasHackedComputer)
            {
                uiManager.OpenPrompt("Escape the building?", "Since you have the data this will end the game in hacker victory!", () => Action());
            }
            else
            {
                uiManager.OpenPrompt("You haven't hacked CEO computer yet", "You need to hack CEO's computer before escaping!");
            }
        }

    }
}
