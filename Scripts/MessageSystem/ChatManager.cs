using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  public class ChatManager : MonoBehaviour
  {
    [SerializeField]
    private GameObject messageList, openButton;
    [SerializeField]
    private Text openButtonTextEl;
    [SerializeField]
    private ChatListingManager chatMessagePrefab;
    [SerializeField]
    private ChatListingManager logMessagePrefab;
    private int newMessages = 0;
    private const string DEFAULT_BUTTON_TEXT = "OPEN MESSAGE LOG";
    private VerticalLayoutGroup layout;
    private RectTransform openButtonRect;
    private bool hasReceivedMessage = false;
    private Tween shakeTween;
    private void Awake()
    {
      openButtonRect = openButton.GetComponent<RectTransform>();
      openButtonTextEl.text = DEFAULT_BUTTON_TEXT;
    }
    // FOR TESTS
    // private void Update()
    // {
    //   if (Input.GetKeyDown(KeyCode.Escape))
    //   {
    //     InitializeMessage("Someone", "Hello kind sir!");
    //   }
    // }
    private void StartOpenButtonShake()
    {
      if (shakeTween == null)
      {
        shakeTween = openButtonRect.DOShakeRotation(1, 30, 3, 0).SetLoops(-1).SetId("message-shake");
      }
      else
      {
        shakeTween.Restart();
      }
    }
    public void InitializeMessage(string senderName, string message, bool isYourMessage = false)
    {
      // Keep "no messages yet" text until first message is received
      if (!hasReceivedMessage)
      {
        hasReceivedMessage = true;
        ClearMessages();
      }
      ChatListingManager listing = Instantiate<ChatListingManager>(chatMessagePrefab, messageList.transform);
      listing.SetInfo(senderName, message, isYourMessage);

      newMessages++;
      // If chat is already open -> check messages straight away
      if (messageList.gameObject.activeInHierarchy || isYourMessage)
      {
        CheckNewMessages();
      }
      else
      {
        openButtonTextEl.text = string.Format("{0} ({1})", DEFAULT_BUTTON_TEXT, newMessages);
        if (GeneralManager.LocalPlayer.IsHacker)
        {
          StartOpenButtonShake();
        }
      }
      UpdateCanvas();
    }

    public void InitializeMessage(string message)
    {
      // Keep "no messages yet" text until first message is received
      if (!hasReceivedMessage)
      {
        hasReceivedMessage = true;
        ClearMessages();
      }
      ChatListingManager listing = Instantiate<ChatListingManager>(logMessagePrefab, messageList.transform);
      listing.SetInfo(message, false);
      newMessages++;
      // If chat is already open -> check messages straight away
      if (messageList.gameObject.activeInHierarchy)
      {
        CheckNewMessages();
      }
      else
      {
        openButtonTextEl.text = string.Format("{0} ({1})", DEFAULT_BUTTON_TEXT, newMessages);
        if (GeneralManager.LocalPlayer.IsHacker)
        {
          StartOpenButtonShake();
        }
      }
      UpdateCanvas();
    }

    public void CheckNewMessages()
    {
      newMessages = 0;
      openButtonTextEl.text = DEFAULT_BUTTON_TEXT;
      DOTween.Pause("message-shake"); // Pause all shakes with id message-shake if more than one somehow got initialized
      openButtonRect.rotation = Quaternion.identity;
    }

    public void ClearMessages()
    {
      foreach (Transform message in messageList.transform)
      {
        Destroy(message.gameObject);
      }
      UpdateCanvas();
    }

    public void UpdateCanvas()
    {
      if (layout == null)
      {
        layout = messageList.GetComponent<VerticalLayoutGroup>();
      }
      LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
    }
  }
}
