using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manticore
{
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class MessageOptionManager : MonoBehaviour, IPointerClickHandler
  {
    public Action Action;
    private TextMeshProUGUI textElement;
    private HackerUIManager hackerUIManager;

    public const float DARK_ALPHA = 0.5f;
    public const float HIGHLIGHTED_ALPHA = 1f;

    private void Awake()
    {
      textElement = GetComponent<TextMeshProUGUI>();
      hackerUIManager = GameObject.Find("UI").transform.Find("HackerUI").GetComponent<HackerUIManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      // Action has to handle sending message if it is set
      if (Action != null)
      {
        Action();
      }
      else
      {
        hackerUIManager.SendMessageRPC(textElement.text);
      }
    }

    public void DarkenText()
    {
      if (textElement == null)
      {
        textElement = GetComponent<TextMeshProUGUI>();
      }
      textElement.color = GraphicsUtils.Darken(textElement.color);
    }

    public void LightenText()
    {
      if (textElement == null)
      {
        textElement = GetComponent<TextMeshProUGUI>();
      }
      textElement.color = GraphicsUtils.Lighten(textElement.color);
    }

  }
}
