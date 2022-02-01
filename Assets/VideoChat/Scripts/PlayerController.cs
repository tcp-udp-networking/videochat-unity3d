using System;
using UnityEngine;
using UnityEngine.UI;

namespace VideoChat.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        //private WebCamTexture _webCamTexture;
        public WebCamTexture _webCamTexture;
        public byte[] webcamTextureBytes;
        public RawImage _camTexture;
        public Color32[] pixels;

        private void Start()
        {
            if (_webCamTexture == null)
            {
                string selectedDeviceName = "";
                WebCamDevice[] allDevices = WebCamTexture.devices;
                for (int i = 0; i < allDevices.Length; i++)
                {
                    if (allDevices[i].isFrontFacing)
                    {
                        selectedDeviceName = allDevices[i].name;
                        break;
                    }
                }
                _webCamTexture = new WebCamTexture(selectedDeviceName, 640 ,360);
                //pixels = _webCamTexture.GetPixels32();
            }

            if (!_webCamTexture.isPlaying)
            {
                _webCamTexture.Play();
            }
        }

        private void FixedUpdate()
        {
            SendInputServer();
            SendWebCamTextureToServer();
        }

        private void SendWebCamTextureToServer()
        {
            // With webcam
            var text = new Texture2D(_webCamTexture.requestedWidth, _webCamTexture.requestedHeight);
            webcamTextureBytes = text.EncodeToJPG();
            ClientSend.UserWebcam(webcamTextureBytes);
        }

        private void SendInputServer()
        {
            bool[] inputs = new[]
            {
                Input.GetKey(KeyCode.W),
                Input.GetKey(KeyCode.S),
                Input.GetKey(KeyCode.A),
                Input.GetKey(KeyCode.D)
            };

            ClientSend.PlayerMovement(inputs);
        }
    }
}