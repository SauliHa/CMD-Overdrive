using System.Collections;
using System.Collections.Generic;
using Manticore;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsManager : MonoBehaviour
{
    [SerializeField]
    private Dropdown dropdown;
    [SerializeField]
    private GameObject hackerSettingsButton;
    [SerializeField]
    private GameObject corporationSettingsButton;
    [SerializeField]
    private GameObject probabilitySettingsButton;

    [SerializeField]
    private TextMeshProUGUI hackerMovesText;
    [SerializeField]
    private TextMeshProUGUI hackerActionsText;
    [SerializeField]
    private TextMeshProUGUI actionsToMovementText;
    [SerializeField]
    private TextMeshProUGUI shiftCDText;
    [SerializeField]
    private TextMeshProUGUI chromaCDText;

    [SerializeField]
    private TextMeshProUGUI corporationMovesText;
    [SerializeField]
    private TextMeshProUGUI corporationActionsText;
    [SerializeField]
    private TextMeshProUGUI amountOfGuardsText;
    [SerializeField]
    private TextMeshProUGUI lockedDoorsText;
    [SerializeField]
    private TextMeshProUGUI motionDetectorText;

    [SerializeField]
    private TextMeshProUGUI windowMoveText;
    [SerializeField]
    private TextMeshProUGUI doorMoveText;
    [SerializeField]
    private TextMeshProUGUI lockHackText;
    [SerializeField]
    private TextMeshProUGUI falseAlarmText;
    [SerializeField]
    private TextMeshProUGUI ceoComputerHackText;

    [SerializeField]
    private GameObject hackerSettings;
    [SerializeField]
    private GameObject corporationSettings;
    [SerializeField]
    private GameObject probabilitySettings;


    private PhotonView ph;
    private bool custom = false; 

    private int maxMoves = 3;
    private int maxActions = 3;

    private int hackerMoves = 1;
    private int hackerActions = 1;
    private bool actionToMoves = true;

    private int chromaCD = 5;
    private int maxChromaCD = 15;
    private int minChromaCD = 1;
    private int shiftCD = 3;
    private int maxShiftCD = 15;
    private int minShiftCD = 1;

    private int corporationMoves = 2;
    private int corporationActions = 1;
    private int maxGuards = 3;
    private int amountOfGuards = 2;
    private int lockedDoors = 3;
    private int maxLockedDoors = 15;
    private int motionDetectors = 3;
    private int maxMotionDetectors = 15;

    private int windowMoveChance = 75;
    private int doorMoveChance = 60;
    private int lockHackChance = 60;
    private int falseAlarmChance = 80;
    private int ceoComputerHackChance = 70;

    // Start is called before the first frame update
    void Start()
    {
        //int moves = generalManager.GetMoves();
        ph = PhotonNetwork.GetPhotonView(1);
        hackerMovesText.text = "Moves per turn = " + hackerMoves;
        hackerActionsText.text = "Actions per turn = " + hackerActions;
        actionsToMovementText.text = "Action to movement = ON";

        chromaCDText.text = "Chroma's cooldown = " + chromaCD;
        shiftCDText.text = "Shift's cooldown = " + shiftCD;

        corporationMovesText.text = "Moves per turn = " + corporationMoves;
        corporationActionsText.text = "Actions per turn = " + corporationActions;
        amountOfGuardsText.text = "Amount of guards = " + amountOfGuards;
        lockedDoorsText.text = "Maximum amount of doors corporation can lock = " + lockedDoors;
        motionDetectorText.text = "Maximum amount of motion detectors corporation can have at the same time = " + motionDetectors;

        windowMoveText.text = "Chance of moving through a window = " + windowMoveChance + "%";
        doorMoveText.text = "Chance of moving through a locked door = " + doorMoveChance + "%";
        lockHackText.text = "Chance of opening a lock = " + lockHackChance + "%";
        falseAlarmText.text = "Chance of making a successful false alarm = " + falseAlarmChance + "%";
        ceoComputerHackText.text = "Chance of hacking CEO's Computer = " + ceoComputerHackChance + "%";
        //GeneralManager.ShiftAbilityCooldown = shiftCD;
        //GeneralManager.ChromaAbilityCooldown = chromaCD;
        dropdown.onValueChanged.AddListener(delegate { OpenAllSettings(dropdown.value); });

    }

    //general
    public void CloseSettings()
    {
        SetSettings();
        InformSettings();
        gameObject.SetActive(false);
    }

    private void SetSettings()
    {
        int option = dropdown.value;
        custom = false;
        switch (option)
        {
            case 0:
                {
                    SetStandard();
                    break;
                }
            case 1:
                {
                    SetSlow();
                    break;
                }
            case 2:
                {
                    SetRapid();
                    break;
                }
            case 3:
                {
                    custom = true;
                    break;
                }
            default:
                SetStandard();
                break;
        }
    }

    private void SetStandard()
    {
        hackerMoves = 1;
        hackerActions = 1;
        actionToMoves = true;
        corporationMoves = 2;
        corporationActions = 1;
    }

    private void SetSlow()
    {
        hackerMoves = 1;
        hackerActions = 1;
        actionToMoves = false;
        corporationMoves = 1;
        corporationActions = 1;
    }

    private void SetRapid()
    {
        hackerMoves = 2;
        hackerActions = 1;
        actionToMoves = false;
        corporationMoves = 2;
        corporationActions = 1;
    }

    private void OpenAllSettings(int num)
    {
        if (num == 3)
        {
            hackerSettingsButton.SetActive(true);
            corporationSettingsButton.SetActive(true);
            probabilitySettingsButton.SetActive(true);
        }
        else
        {
            hackerSettingsButton.SetActive(false);
            corporationSettingsButton.SetActive(false);
            probabilitySettingsButton.SetActive(false);
        }
    }

    public void OpenHackerSettings()
    {
        hackerSettings.SetActive(true);
    }

    public void OpenCorporationSettings()
    {
        corporationSettings.SetActive(true);
    }

    public void OpenProbabilitySettings()
    {
        probabilitySettings.SetActive(true);
    }

    public void CloseHackerSettings()
    {
        hackerSettings.SetActive(false);
    }

    public void CloseCorporationSettings()
    {
        corporationSettings.SetActive(false);
    }

    public void CloseProbabilitySettings()
    {
        probabilitySettings.SetActive(false);
    }

    //hacker
    public void IncreaseHackerMoves()
    {
        if (maxMoves > hackerMoves)
        {
            hackerMoves++;
        }
        hackerMovesText.text = "Moves per turn = " + hackerMoves;
    }
    public void DecreaseHackerMoves()
    {
        if (1 < hackerMoves)
        {
            hackerMoves--;
        }
        hackerMovesText.text = "Moves per turn = " + hackerMoves;
    }

    public void IncreaseHackerActions()
    {
        if (maxActions > hackerActions)
        {
            hackerActions++;
        }
        hackerActionsText.text = "Actions per turn = " + hackerActions;
    }
    public void DecreaseHackerActions()
    {
        if (1 < hackerActions)
        {
            hackerActions--;
        }
        hackerActionsText.text = "Actions per turn = " + hackerActions;
    }

    public void IncreaseShiftCD()
    {
        if (shiftCD < maxShiftCD)
        {
            shiftCD++;
            shiftCDText.text = "Shift's cooldown = " + shiftCD;
        }
    }

    public void ActionToMoveOn()
    {
        actionToMoves = true;
        actionsToMovementText.text = "Action to movement = ON";
    }

    public void ActionToMoveOff()
    {
        actionToMoves = false;
        actionsToMovementText.text = "Action to movement = OFF";
    }

    //Roles
    public void DecreaseShiftCD()
    {
        if (shiftCD > minShiftCD)
        {
            shiftCD--;
            shiftCDText.text = "Shift's cooldown = " + shiftCD;
        }
    }

    public void IncreaseChromaCD()
    {
        if (chromaCD < maxChromaCD)
        {
            chromaCD++;
            chromaCDText.text = "Chroma's cooldown = " + chromaCD;
        }
    }
    public void DecreaseChromaCD()
    {
        if (chromaCD > minChromaCD)
        {
            chromaCD--;
            chromaCDText.text = "Chroma's cooldown = " + chromaCD;
        }
    }

    //Corporation
    public void IncreaseCorporationMoves()
    {
        if (maxMoves > corporationMoves)
        {
            corporationMoves++;
            corporationMovesText.text = "Moves per turn = " + corporationMoves;
        }
    }

    public void DecreaseCorporationMoves()
    {
        if (1 < corporationMoves)
        {
            corporationMoves--;
            corporationMovesText.text = "Moves per turn = " + corporationMoves;
        }
    }

    public void IncreaseCorporationActions()
    {
        if (maxActions > corporationActions)
        {
            corporationActions++;
            corporationActionsText.text = "Actions per turn = " + corporationActions;
        }
    }
    public void DecreaseCorporationActions()
    {
        if (1 < corporationActions)
        {
            corporationActions--;
            corporationActionsText.text = "Actions per turn = " + corporationActions;
        }
    }

    public void IncreaseTheAmountOfGuards()
    {
        if (maxGuards > amountOfGuards)
        {
            amountOfGuards++;
            amountOfGuardsText.text = "Amount of guards = " + amountOfGuards;
        }
    }

    public void DecreaseTheAmountOfGuards()
    {
        if (1 < amountOfGuards)
        {
            amountOfGuards--;
            amountOfGuardsText.text = "Amount of guards = " + amountOfGuards;
        }
    }

    public void IncreaseLockedDoors()
    {
        if (maxLockedDoors > lockedDoors)
        {
            lockedDoors++;
            lockedDoorsText.text = "Maximum amount of doors corporation can lock = " + lockedDoors;
        }
    }

    public void DecreaseLockedDoors()
    {
        if (0 < lockedDoors)
        {
            lockedDoors--;
            lockedDoorsText.text = "Maximum amount of doors corporation can lock = " + lockedDoors;
        }
    }

    public void IncreaseMotionDetectors()
    {
        if (maxMotionDetectors > motionDetectors)
        {
            motionDetectors++;
            motionDetectorText.text = "Maximum amount of motion detectors corporation can have at the same time = " + motionDetectors;
        }
    }

    public void DecreaseMotionDetectors()
    {
        if (0 < motionDetectors)
        {
            motionDetectors--;
            motionDetectorText.text = "Maximum amount of motion detectors corporation can have at the same time = " + motionDetectors;
        }
    }

    //Probability
    public void IncreaseWindowChance()
    {
        if (windowMoveChance < 100)
        {
            windowMoveChance += 5;
            windowMoveText.text = "Chance of moving thought a window = " + windowMoveChance + "%";
        }
    }
    public void DecreaseWindowChance()
    {
        if (windowMoveChance > 0)
        {
            windowMoveChance -= 5;
            windowMoveText.text = "Chance of moving thought a window = " + windowMoveChance + "%";
        }
    }

    public void IncreaseDoorChance()
    {
        if (doorMoveChance < 100)
        {
            doorMoveChance += 5;
            doorMoveText.text = "Chance of moving thought a locked door = " + doorMoveChance + "%";
        }
    }
    public void DecreaseDoorChance()
    {
        if (doorMoveChance > 0)
        {
            doorMoveChance -= 5;
            doorMoveText.text = "Chance of moving thought a locked door = " + doorMoveChance + "%";
        }
    }

    public void IncreaseLockChance()
    {
        if (lockHackChance < 100)
        {
            lockHackChance += 5;
            lockHackText.text = "Chance of opening a lock = " + lockHackChance + "%";
        }
    }
    public void DecreaseLockChance()
    {
        if (lockHackChance > 0)
        {
            lockHackChance -= 5;
            lockHackText.text = "Chance of opening a lock = " + lockHackChance + "%";
        }
    }

    public void IncreaseFalseAlarmChance()
    {
        if (falseAlarmChance < 100)
        {
            falseAlarmChance += 5;
            falseAlarmText.text = "Chance of making a successful false alarm = " + falseAlarmChance + "%";
        }
    }
    public void DecreaseFalseAlarmChance()
    {
        if (falseAlarmChance > 0)
        {
            falseAlarmChance -= 5;
            falseAlarmText.text = "Chance of making a successful false alarm = " + falseAlarmChance + "%";
        }
    }


    public void IncreaseCEOComputerHackChance()
    {
        if (ceoComputerHackChance < 100)
        {
            ceoComputerHackChance += 5;
            ceoComputerHackText.text = "Chance of hacking CEO's Computer = " + ceoComputerHackChance + "%";
        }
    }
    public void DecreaseCEOComputerHackChance()
    {
        if (ceoComputerHackChance > 0)
        {
            ceoComputerHackChance -= 5;
            ceoComputerHackText.text = "Chance of hacking CEO's Computer = " + ceoComputerHackChance + "%";
        }
    }

    public void InformSettings()
    {
        if (ph == null) ph = PhotonNetwork.GetPhotonView(1);
        ph.RPC("SetHackerMoves", RpcTarget.All, hackerMoves);
        ph.RPC("SetHackerActions", RpcTarget.All, hackerActions);
        ph.RPC("ActionToMoves", RpcTarget.All, actionToMoves);

        ph.RPC("SetChromaCD", RpcTarget.All, chromaCD);
        ph.RPC("SetShiftCD", RpcTarget.All, shiftCD);

        if (!custom && PhotonNetwork.CurrentRoom.PlayerCount > 3)
        {
            amountOfGuards = 2;
        }
        else if (!custom)
        {
            amountOfGuards = 1;
        }

        ph.RPC("SetCorporationMoves", RpcTarget.All, corporationMoves);
        ph.RPC("SetCorporationActions", RpcTarget.All, corporationActions);
        ph.RPC("SetGuardCount", RpcTarget.All, amountOfGuards);
        ph.RPC("SetLockedDoors", RpcTarget.All, lockedDoors);
        ph.RPC("SetMotionDetectors", RpcTarget.All, motionDetectors);

        ph.RPC("SetWindowMoveChance", RpcTarget.All, windowMoveChance);
        ph.RPC("SetDoorMoveChance", RpcTarget.All, doorMoveChance);
        ph.RPC("SetLockHackChance", RpcTarget.All, lockHackChance);
        ph.RPC("SetFalseAlarmChance", RpcTarget.All, falseAlarmChance);
        ph.RPC("SetCEOComputerHackChance", RpcTarget.All, ceoComputerHackChance);

        // Save settings to custom props so they can be loaded on reconnect
        Room room = PhotonNetwork.CurrentRoom;
        room.SetCustomProp("HackerMoves", hackerMoves);
        room.SetCustomProp("HackerActions", hackerActions);
        room.SetCustomProp("ActionToMoves", actionToMoves);

        room.SetCustomProp("ChromaCD", chromaCD);
        room.SetCustomProp("ShiftCD", shiftCD);

        room.SetCustomProp("CorporationMoves", corporationMoves);
        room.SetCustomProp("CorporationActions", corporationActions);
        room.SetCustomProp("AmountOfGuards", amountOfGuards);
        room.SetCustomProp("LockedDoors", lockedDoors);
        room.SetCustomProp("MotionDetectors", motionDetectors);

        room.SetCustomProp("WindowMoveChance", windowMoveChance);
        room.SetCustomProp("DoorMoveChance", doorMoveChance);
        room.SetCustomProp("LockHackChance", lockHackChance);
        room.SetCustomProp("FalseAlarmChance", falseAlarmChance);
        room.SetCustomProp("CEOComputerHackChance", ceoComputerHackChance);
    }
}
