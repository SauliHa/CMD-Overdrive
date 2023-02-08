using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Manticore
{
  public class GameOverPanelManager : MonoBehaviour
  {
    [SerializeField]
    private TextMeshProUGUI subtitleTextEl, descriptionTextEl;
    [SerializeField]
    private MainMenuManager menuManager;
    private static string HACKERS_WON_TEXT = "Hackers got into the systems of MegaCorp, planted a DIVOC-91 malware destroying all the systems and escaped the building.  The world is now free of the control of MegaCorp!";
    private static string CORP_WON_TEXT = "MegaCorp caught all the hackers and locked them up. The chains of control of the MegaCorp still remain unbreaked.";

    private void Start()
    {
      if (GeneralManager.DidHackersWin)
      {
        subtitleTextEl.text = "Hackers won!";
        descriptionTextEl.text = HACKERS_WON_TEXT;
      }
      else
      {
        subtitleTextEl.text = "MegaCorp won!";
        descriptionTextEl.text = CORP_WON_TEXT;
      }
    }

    public void GoBackToMainMenu()
    {
      gameObject.SetActive(false);
      GeneralManager.Instance.ClearPreviousGame();
      menuManager.ActivateInitialMenuState();
    }
  }
}
