using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  public class ChatListingManager : MonoBehaviour
  {
    private TextMeshProUGUI senderTextEl, timestampTextEl, messageTextEl;
    private HorizontalLayoutGroup layout;
    private VerticalLayoutGroup senderInfoLayout;
    [SerializeField]
    private bool isLogMessage = false;
    private void Start()
    {
      if (isLogMessage)
      {
        SetLogReferences();
      }
      else
      {
        SetChatReferences();
      }
    }

    public void SetInfo(string senderName, string message, bool isYourMessage)
    {
      if (senderTextEl == null || timestampTextEl == null)
      {
        SetChatReferences();
      }
      senderTextEl.text = senderName;
      timestampTextEl.text = "Round: " + GeneralManager.TurnCounter;
      messageTextEl.text = message;
      senderTextEl.color = isYourMessage ? GeneralManager.BASE_RED : GeneralManager.BASE_GREEN;
      UpdateCanvas();
    }

    public void SetInfo(string message, bool isYourMessage)
    {
      if (timestampTextEl == null)
      {
        SetLogReferences();
      }
      timestampTextEl.text = "Round: " + GeneralManager.TurnCounter;
      messageTextEl.text = message;
      UpdateCanvas();
    }

    private void SetChatReferences()
    {
      senderInfoLayout = transform.Find("SenderInfo").GetComponent<VerticalLayoutGroup>(); ;
      senderTextEl = senderInfoLayout.transform.Find("SenderName").GetComponent<TextMeshProUGUI>();
      timestampTextEl = senderInfoLayout.transform.Find("TimeStamp").GetComponent<TextMeshProUGUI>();
      messageTextEl = transform.Find("Message").GetComponent<TextMeshProUGUI>();
      layout = GetComponent<HorizontalLayoutGroup>();
    }

    private void SetLogReferences()
    {
      senderInfoLayout = transform.Find("SenderInfo").GetComponent<VerticalLayoutGroup>(); ;
      timestampTextEl = senderInfoLayout.transform.Find("TimeStamp").GetComponent<TextMeshProUGUI>();
      messageTextEl = transform.Find("Message").GetComponent<TextMeshProUGUI>();
      layout = GetComponent<HorizontalLayoutGroup>();
    }


    // Need to force refresh canvas when initializing to fix layout groups ordering messages properly
    private void UpdateCanvas()
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(senderInfoLayout.GetComponent<RectTransform>());
      LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
    }
  }
}
