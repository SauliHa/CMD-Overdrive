using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Malee.List;
using Photon.Realtime;

namespace Manticore
{
  public class GeneralUIManager : MonoBehaviour
  {
    [SerializeField]
    private GameObject chatPanel, promptContainer, hackerContainer, corporationContainer, moveCountsContainer;
    [SerializeField]
    private TextMeshProUGUI turnTitle, turnSubtitle, promptTitle, promptDescription, turnCounter, cooldownText;
    [SerializeField]
    private Button promptCancelButton, openChatButton, cancelButton;
    [SerializeField]
    private ScrollRect messageLogsScrollRect;
    [SerializeField]
    private RawImage turnBorder;
    private TextMeshProUGUI moveCountTextEl, actionCountTextEl;
    private ChatManager chatManager;
    private Action onPromptAccept;
    private Action onPromptDecline;
    private Action onSelectCancel;
    private HackerUIManager hackerUIManager;
    //private CorporationUIManager corporationUIManager;
    private int lastRound = 1;
    private List<string> personalDoorsOpened = new List<string>();
    public List<string> PersonalDoorsOpened { get => personalDoorsOpened; }
    private List<PromptMessage> promptMessageList = new List<PromptMessage>();

    private void Awake()
    {
      chatManager = GetComponent<ChatManager>();
      // Activate correct ui based on if player is hacker or not (== corporation)
      chatPanel.SetActive(false);
      promptContainer.SetActive(false);
      moveCountTextEl = moveCountsContainer.transform.Find("MovesLeft").GetComponent<TextMeshProUGUI>();
      actionCountTextEl = moveCountsContainer.transform.Find("ActionsLeft").GetComponent<TextMeshProUGUI>();
      hackerUIManager = hackerContainer.GetComponent<HackerUIManager>();
      //corporationUIManager = corporationContainer.GetComponent<CorporationUIManager>();
      HideMoveCounts();
      ToggleCancelButtonVisibility(false);

    }
    private void Start()
    {
      ActivateUI();
      // Don't show cooldown text in the beginning
      cooldownText.gameObject.SetActive(false);
    }
    public void OpenChatPanel()
    {
      if (GeneralManager.LocalPlayer.IsHacker)
      {
        hackerUIManager.lockCamera = true;
      }
      /*
      else
      {
        corporationUIManager.centerCamera = true;
      }
      */
      chatPanel.SetActive(true);
      chatManager.CheckNewMessages();
      chatManager.UpdateCanvas();
      StartCoroutine(GraphicsUtils.ForceScrollDown(messageLogsScrollRect));
    }

    public void CloseChatPanel()
    {
      if (GeneralManager.LocalPlayer.IsHacker)
      {
        hackerUIManager.lockCamera = false;
      }
      /*
      else
      {
        corporationUIManager.centerCamera = false;
      }
        */
      chatPanel.SetActive(false);
    }
    public void InitializeChatMessage(string sender, string message)
    {
      chatManager.InitializeMessage(sender, message);
    }
    public void InitializeLogMessage(string message)
    {
      chatManager.InitializeMessage(message);
    }
    public void SetTurnSubtitle(string text)
    {
      turnSubtitle.gameObject.SetActive(true);
      turnSubtitle.text = text;
    }
    public void SetTurnTitle(string text)
    {
      turnTitle.text = text;
    }
    public void SetTurnTitleColor(Color color)
    {
      turnTitle.color = color;
    }
    public void HideTurnSubtitle()
    {
      turnSubtitle.gameObject.SetActive(false);
    }
    public void ShowMoveCounts()
    {
      moveCountsContainer.SetActive(true);
    }
    public void HideMoveCounts()
    {
      moveCountsContainer.SetActive(false);
    }
    public void SetMoveCounts(int movesCount, int actionsCount)
    {
      moveCountTextEl.text = "Moves left: " + movesCount;
      actionCountTextEl.text = "Actions left: " + actionsCount;
    }

    public void SetCoolDownText(string text)
    {
      if (!cooldownText.gameObject.activeInHierarchy) cooldownText.gameObject.SetActive(true);
      cooldownText.text = text;
    }
    public void OpenLatestPromptMessage()
    {
      PromptMessage latestMessage = promptMessageList[0];
      onPromptAccept = latestMessage.OnAccept;
      onPromptDecline = latestMessage.OnCancel;
      promptTitle.text = latestMessage.Title;
      promptDescription.text = latestMessage.Description;
      promptContainer.SetActive(true);
      promptCancelButton.gameObject.SetActive(latestMessage.HasCancelButton);
    }
    public void OpenPrompt(string title, string description, Action onPromptAccept, Action onPromptCancel, bool hasCancelButton = true)
    {
      promptMessageList.Add(new PromptMessage(title, description, onPromptAccept, onPromptCancel, hasCancelButton));
      if (!promptContainer.activeInHierarchy) OpenLatestPromptMessage();
    }

    // Generic alarm prompt
    public void OpenAlarmPrompt(int roll2, float limit)
    {
      int roll = 100 - roll2;
      float limit2 = 100f - limit;
      OpenPrompt("FAILURE! ALARM! \n\nYou got " + roll + "\n\nYou needed " + limit2 + "\n\n",
        " You caused an alarm! Insert alarm marker at your location on the board to notify others."); // You got " + roll + " out of 100. You needed at least " + limit2 + ".");
    }
    public void OpenPrompt(string title, string description, Action onPromptAccept)
    {
      OpenPrompt(title, description, onPromptAccept, () => { }, true);
    }
    // Open prompt without cancel button and onPromptAccept action => only to notify user of something
    public void OpenPrompt(string title, string description)
    {
      OpenPrompt(title, description, () => { }, () => { }, false);
    }
    public void OnPromptAccept()
    {
      ClosePrompt();
      if (onPromptAccept != null)
      {
        onPromptAccept.Invoke();
      }
    }
    public void OnPromptCancel()
    {
      ClosePrompt();
      if (onPromptDecline != null)
      {
        onPromptDecline.Invoke();
      }
    }
    public void ClosePrompt()
    {
      promptMessageList.Remove(promptMessageList[0]);
      promptContainer.SetActive(false);
      if (promptMessageList.Count > 0)
      {
        OpenLatestPromptMessage();
      }
      else
      {
        GeneralManager.LocalPlayer.UpdateUI();
      }
    }
    public void ToggleButtons(bool chatEnabled)
    {
      openChatButton.interactable = chatEnabled;
    }
    public void DisableButtons()
    {
      ToggleButtons(false);
    }
    public void EnableButtons()
    {
      ToggleButtons(true);
    }

    public void ToggleChatButtonVisibility(bool visibility)
    {
      openChatButton.gameObject.SetActive(visibility);
    }
    public void ToggleCancelButtonVisibility(bool visibility)
    {
      cancelButton.gameObject.SetActive(visibility);
    }

    public void TriggerAlarm(int id)
    {
      GeneralManager.LocalPlayer.NetworkManager.photonView.RPC("OnAlarmTriggered", RpcTarget.AllBuffered, id);
    }
    public void IncreaseDoorsOpened(string roomName)
    {
      personalDoorsOpened.Add(roomName);
    }
    
    public void UpdateTurnCounter()
    {
      int round = GeneralManager.TurnCounter;
      if (round > lastRound)
      {
        if (GeneralManager.LocalPlayer.IsHacker)
        {
          hackerUIManager.NewRound();
        }
        turnCounter.text = "Round " + round;
        lastRound = round;
      }
      
    }
    public void OnCancelClick()
    {
      if (onSelectCancel != null)
      {
        onSelectCancel.Invoke();
      }
      ToggleCancelButtonVisibility(false);
      ToggleChatButtonVisibility(true);
    }

    public void ToggleSelectMode(Action onCancel)
    {
      ToggleChatButtonVisibility(false);
      ToggleCancelButtonVisibility(true);
    }

    public void ActivateUI()
    {
      hackerContainer.SetActive(GeneralManager.LocalPlayer != null && GeneralManager.LocalPlayer.IsHacker);
      corporationContainer.SetActive(GeneralManager.LocalPlayer != null && !GeneralManager.LocalPlayer.IsHacker);
    }

    public void SetTurnBorderColor(Color color)
    {
      Color tempColor = color;
      tempColor.a = 0.4f;
      turnBorder.color = tempColor;
    }
    public void ToggleTurnBorderVisibility(bool visibility)
    {
      turnBorder.gameObject.SetActive(visibility);
    }
  }

}
