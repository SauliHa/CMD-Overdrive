using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Manticore;

namespace Manticore
{
    public class QRResultManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI resultText;

        public void OpenQrReader()
        {
            GeneralManager.OpenQrReader();
            // Test commit
        }

        public void SetResultText(string text)
        {
            resultText.text = text;
        }
    }
}
