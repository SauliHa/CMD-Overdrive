using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
    /// <summary>
    /// Class for managing a single available game listing.
    /// </summary>
    public class GameListingManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI gameName;
        [SerializeField]
        private Button joinGameButton;
        public RoomInfo RoomInfo { get; private set; }

        public void SetGameInfo(RoomInfo info, Action onJoinButtonClick)
        {
            gameName.text = info.Name;
            RoomInfo = info;
            joinGameButton.onClick.AddListener(delegate { onJoinButtonClick(); });
        }
    }
}
