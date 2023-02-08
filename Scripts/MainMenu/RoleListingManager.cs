using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  public class RoleListingManager : MonoBehaviour
  {
    [SerializeField]
    private Role role;
    [SerializeField]
    private TextMeshProUGUI selectedTextEl, descriptionTextEl, titleTextEl;
    [SerializeField]
    private Image image;
    private MainMenuManager menuManager;
    private void Awake()
    {
      SetRoleInfo();
      menuManager = GameObject.Find("MenuManager").GetComponent<MainMenuManager>();
    }

    private void OnValidate()
    {
      SetRoleInfo();
    }

    private void SetRoleInfo()
    {
      descriptionTextEl.text = role.Description;
      titleTextEl.text = role.Name;
      image.sprite = role.ImageSprite;
      name = role.Name;
    }

    public void SelectRole()
    {
      PhotonNetwork.LocalPlayer.SetRole(role.RoleType);
      menuManager.ShowWaitingRoom((int)role.RoleType);
    }
  }
}
