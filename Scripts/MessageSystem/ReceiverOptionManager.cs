using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manticore
{
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class ReceiverOptionManager : MonoBehaviour, IPointerClickHandler
  {
    private TextMeshProUGUI textElement;
    private HackerUIManager hackerUIManager;
    private Player player;

    private void Awake()
    {
      textElement = GetComponent<TextMeshProUGUI>();
      hackerUIManager = GameObject.Find("UI").transform.Find("HackerUI").GetComponent<HackerUIManager>();
    }

    public void SetPlayer(Player player)
    {
      this.player = player;
      RoleType role = player.GetRole();
      name = player.NickName; // gameobj name
                              // Ensure textelement is set
      textElement = GetComponent<TextMeshProUGUI>();
      textElement.text = player.GetNameWithRole();
    }
    public void SetPlayer()
    {
      this.player = null;
      name = "Everyone!";
      textElement = GetComponent<TextMeshProUGUI>();
      textElement.color = Color.cyan;
      textElement.text = "Everyone";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      if (player != null)
      {
        hackerUIManager.SelectMessageReceiver(player);
      }
      else
      {
        hackerUIManager.SelectMessageReceiver();
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
