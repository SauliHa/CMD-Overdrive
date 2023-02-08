using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Manticore
{
  public class CorporationUIManager : MonoBehaviour
  {
    [SerializeField]
    private GameObject actionsContainer;
    [SerializeField]
    private Button motionDetectorButton, lockDoorButton, moveGuardsButton, skipActionButton;
    [SerializeField]
    private TextMeshProUGUI lockedDoorCount, motionDetectorCount;
    /*
    private CameraManager cameraManager;
    public bool centerCamera = false;
    
    private void Start()
    {
      cameraManager = GameObject.Find("Main Camera").GetComponent<CameraManager>();
    }
    
    private void Update()
    {
      if (centerCamera)
      {
        centerToFirstGuard();
      }
    }
    public void centerToFirstGuard()
    {
      cameraManager.CenterCamera(GeneralManager.Guards[0].gameObject);
    }
    */
    
    public void ToggleMotionDetectorMode(bool isActive)
    {
      Text textEl = motionDetectorButton.GetComponentInChildren<Text>();
      textEl.text = isActive ? "CANCEL" : "PUT DETECTOR";
    }
    
    public void ToggleLockDoorMode(bool isActive)
    {
      Text textEl = lockDoorButton.GetComponentInChildren<Text>();
      textEl.text = isActive ? "CANCEL" : "LOCK DOOR";
    }
    public void ToggleButtons(bool moveGuardsEnabled, bool motionDetectorEnabled, bool lockDoorEnabled, bool skipActionEnabled)
    {
      moveGuardsButton.interactable = moveGuardsEnabled;
      motionDetectorButton.interactable = motionDetectorEnabled;
      lockDoorButton.interactable = lockDoorEnabled;
      skipActionButton.interactable = skipActionEnabled;
    }
    public void DisableButtons()
    {
      ToggleButtons(false, false, false, false);
    }
    public void EnableButtons()
    {
      ToggleButtons(true, true, true, true);
    }
    public void HideActions()
    {
      actionsContainer.SetActive(false);
    }
    public void ShowActions()
    {
      actionsContainer.SetActive(true);
    }
    public void OnMoveGuardsButtonClick()
    {
      (GeneralManager.LocalPlayer as CorporationManager).MoveGuards();
    }
    public void OnMotionDetectorButtonClick()
    {
      (GeneralManager.LocalPlayer as CorporationManager).ToggleMotionDetectorMode();
    }
    public void OnDoorLockButtonClick()
    {
      (GeneralManager.LocalPlayer as CorporationManager).ToggleDoorLockMode();
    }
    public void OnSkipActionButtonClick()
    {
      (GeneralManager.LocalPlayer as CorporationManager).SkipAction();
    }
    public void UpdateLockAndMotionDetectorCounts(int currentLocked, int maxLocked, int currentDetectors, int maxDetectors)
    {
      motionDetectorCount.text = "Locked doors: " + currentLocked + "/" + maxLocked;
      lockedDoorCount.text = "Motion detectors: " + currentDetectors + "/" + maxDetectors;
    }
  }
}
