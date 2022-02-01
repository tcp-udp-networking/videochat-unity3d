using System;
using UnityEngine;

namespace VideoChat.Scripts.Managers
{
    public enum CameraFacing
    {
        FRONT,
        BACK
    }
    public class CameraManager : MonoBehaviour
    {
        private static WebCamTexture _webCamTexture;
        private string _selectedDevice;
        public CameraFacing eCameraFacing;
        
        private void Start()
        {
            /*if (_webCamTexture != null) return;
            _webCamTexture = new WebCamTexture();
            GetComponent<Renderer>().material.mainTexture = _webCamTexture;
                
            if(!_webCamTexture.isPlaying)
                _webCamTexture.Play();*/

            var devices = WebCamTexture.devices;

            foreach (var t in devices)
            {
                if (!t.isFrontFacing) continue;
                _selectedDevice = t.name;
                break;
            }

            _webCamTexture = new WebCamTexture(_selectedDevice, 960, 640);
            GetComponent<Renderer>().material.mainTexture = _webCamTexture;
            
            if(!_webCamTexture.isPlaying)
                _webCamTexture.Play();
        }

        public void SetCameraFace(CameraFacing facing)
        {
            switch (facing)
            {
                case CameraFacing.FRONT:
                    break;
                case CameraFacing.BACK:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
