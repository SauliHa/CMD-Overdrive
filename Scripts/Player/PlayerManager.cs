using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// This class is for all functions involving the player, such as movement on the board
/// </summary>
namespace Manticore
{
    [RequireComponent(typeof(PlayerNetworkManager))]
    public abstract class PlayerManager : MonoBehaviourPun
    {
        public Role Role;
        public GeneralUIManager UIManager { get; private set; }
        public PlayerNetworkManager NetworkManager { get; private set; }
        protected MapManager mapManager;
        private int moveCount = 0;
        private int actionCount = 0;
        public int MoveCount
        {
            get => moveCount; set
            {
                moveCount = value;
                photonView.Owner.SetCustomProp("MoveCount", value);
            }
        }
        public int ActionCount
        {
            get => actionCount; set
            {
                actionCount = value;
                photonView.Owner.SetCustomProp("ActionCount", value);
            }
        }
        public bool IsPlayerInTurn { get { return MoveCount > 0 || ActionCount > 0; } }
        public bool IsHacker
        {
            get => this is HackerManager;
        }
        protected bool IsMineOrIsDev
        {
            get => photonView.IsMine || GeneralManager.IsDev;
        }

        protected GameObject edgeHologram;
        [SerializeField]
        private Sprite edgeNormalSprite, edgeHologramSprite;
        protected SpriteRenderer hologramSprite;
        protected virtual void Awake()
        {
            NetworkManager = GetComponent<PlayerNetworkManager>();
            UIManager = GameObject.Find("UI").GetComponent<GeneralUIManager>();
            mapManager = GameObject.Find("Map").GetComponent<MapManager>();
            if (IsMineOrIsDev)
            {
                UIManager.SetTurnBorderColor(Role.RoleColor);
                UIManager.ToggleTurnBorderVisibility(false);
            }
            // When first starting object, sync Photon Player server props to match initial state to avoid nulls in server.
            if (photonView.IsMine)
            {
                object hasDoneInitialSync = PhotonNetwork.LocalPlayer.GetCustomProp("HasDoneInitialSync");
                if (hasDoneInitialSync == null || !(bool)hasDoneInitialSync)
                {
                    SyncServerWithState();
                }
            }

            //Initialize edge's hologram
            edgeHologram = GameObject.Find("EdgeHologram");
            hologramSprite = edgeHologram.GetComponent<SpriteRenderer>();
            RoleType localPlayerRole = PhotonNetwork.LocalPlayer.GetRole();
            GraphicsUtils.ChangeAlpha(hologramSprite, 0f);
            string playerRole = localPlayerRole.ToString();
            if (playerRole == "Edge")
                hologramSprite.sprite = edgeHologramSprite;
            else
                hologramSprite.sprite = edgeNormalSprite;

        }
        public virtual void StartTurn()
        {
            GeneralManager.AudioManager.PlaySuccess();
            UIManager.CloseChatPanel();
            UIManager.ToggleTurnBorderVisibility(true);
        }
        public virtual void EndTurn()
        {
            UIManager.ToggleTurnBorderVisibility(false);
            // Dev without server => Change turn back to current user
            if (PhotonNetwork.CurrentRoom == null)
            {
                GeneralManager.Instance.ChangeTurn(0);
            }
            else
            {
                PhotonNetwork.CurrentRoom.SetPlayerInTurnIndex(GeneralManager.PlayerInTurnIndex + 1);
            }
        }
        public virtual void UpdateUI()
        {
            if (IsMineOrIsDev)
            {
                Player localPlayer = PhotonNetwork.LocalPlayer;
                UIManager.EnableButtons(); // Chat open by default
                UIManager.UpdateTurnCounter();
                if (GeneralManager.PlayerInTurn == localPlayer || PhotonNetwork.CurrentRoom == null)
                {
                    UIManager.SetTurnTitle("Your turn");
                    UIManager.SetTurnTitleColor(GeneralManager.BASE_RED);
                    UIManager.ShowMoveCounts();
                    UIManager.SetMoveCounts(MoveCount, ActionCount);
                }
                // Someone elses turn
                else
                {
                    UIManager.HideMoveCounts();
                    UIManager.SetTurnTitle(GeneralManager.PlayerInTurn.NickName + "'s turn");
                    UIManager.SetTurnTitleColor(GeneralManager.BASE_GREEN);
                }
            }
        }

        public void ClearMoveCount()
        {
            MoveCount = 0;
        }
        public void ClearActionCount()
        {
            ActionCount = 0;
        }
        public virtual void SyncStateWithServer()
        {
            Player player = PhotonNetwork.LocalPlayer;
            MoveCount = (int)player.GetCustomProp("MoveCount");
            ActionCount = (int)player.GetCustomProp("ActionCount");
            GraphicsUtils.ChangeAlpha(hologramSprite, 1f);
        }

        public virtual void SyncServerWithState()
        {
            Player player = PhotonNetwork.LocalPlayer;
            player.SetCustomProp("MoveCount", MoveCount);
            player.SetCustomProp("ActionCount", ActionCount);
            player.SetCustomProp("HasDoneInitialSync", true);
        }
    }
}


