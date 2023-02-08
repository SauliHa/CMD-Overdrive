using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;


namespace Manticore
{
    public class QRCodeReader : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI errorText;
        [SerializeField]
        private RawImage backgroundImage;
        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;
        [SerializeField]
        private GameObject cameraOpenCanvas;
        [SerializeField]
        private GameObject cameraClosedCanvas;
        private WebCamTexture camTexture;
        private Rect screenRect;

        private void Start()
        {
            errorText.text = "";
            cameraOpenCanvas.SetActive(false);
            cameraClosedCanvas.SetActive(true);
        }

        private void Update()
        {
            // Camera not setup yet correctly
            if (camTexture == null || camTexture?.width < 100)
            {
                return;
            }

            RenderCamera();
            ReadQRCode();
        }

        private void ReadQRCode()
        {
            if (camTexture != null)
            {
                try
                {
                    IBarcodeReader barcodeReader = new BarcodeReader();
                    var result = barcodeReader.Decode(camTexture.GetPixels32(),
                      camTexture.width, camTexture.height);
                    if (result != null)
                    {
                        Debug.Log("Qr code read!");
                        OnQRCodeRead(result.Text);
                    }
                    else
                    {
                        Debug.Log("Failed to read code");
                    }
                }
                catch (Exception ex) { Debug.LogWarning(ex.Message); }
            }
        }

        private void RenderCamera()
        {
            // Rotate image to show correct orientation 
            Vector3 rotationVector = new Vector3(0f, 0f, 0f);
            rotationVector.z = Debug.isDebugBuild ? -90 : -camTexture.videoRotationAngle;

            // Don't flip vertically if debug assuming it remote camera is being used -> no vertical flip really happening
            if (!Debug.isDebugBuild && camTexture.videoVerticallyMirrored)
            {
                rotationVector.y = 180;
            }
            backgroundImage.rectTransform.localEulerAngles = rotationVector;

            // Set AspectRatioFitter's ratio
            float videoRatio =
                (float)camTexture.width / (float)camTexture.height;
            aspectRatioFitter.aspectRatio = videoRatio;

        }

        private void SetupCamera(string deviceName)
        {

            camTexture = new WebCamTexture(deviceName, Screen.width, Screen.height);
            camTexture.Play();

            backgroundImage.texture = camTexture;
        }

        public void OpenCamera()
        {

            WebCamDevice[] devices = WebCamTexture.devices;
            foreach (WebCamDevice device in devices)
            {
                // Get back camera
                if (!device.isFrontFacing)
                {
                    SetupCamera(device.name);
                }
            }
            if (camTexture != null)
            {
                cameraOpenCanvas.SetActive(true);
                cameraClosedCanvas.SetActive(false);
            }
            else
            {
                errorText.text = "Couldn't find back camera to use";
            }
        }

        public void CloseCamera()
        {
            camTexture.Stop();
            camTexture = null;
            cameraOpenCanvas.SetActive(false);
            cameraClosedCanvas.SetActive(true);
        }

        private void OnQRCodeRead(string result)
        {
            GeneralManager.LoadScene("QRResult", () =>
            {
                // Change result text after scene is loaded
                QRResultManager qrResultManager = GameObject.Find("Result Manager")?.GetComponent<QRResultManager>();
                if (qrResultManager)
                {
                    qrResultManager.SetResultText(result);
                }
            });
        }
    }
}