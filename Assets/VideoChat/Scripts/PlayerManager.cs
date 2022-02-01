using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VideoChat.Scripts
{
    public class PlayerManager : MonoBehaviour
    {
        public TextMeshProUGUI playerNicknameText;
       // public Texture2D playerTexture2d;

        public RawImage _webcamTexture;
        public int id;
        public string username;

        public byte[] SetWebCamFromServer(byte[] textureFromServer)
        {
            var text = new Texture2D(640, 360);
            text.LoadImage(textureFromServer);
            text.Apply();

            /*var newText = new Texture2D(640, 360, TextureFormat.RGB24, false);
            newText.LoadImage(textureFromServer);
            newText.Apply();*/

            var newText = new Texture2D(text.width, text.height, text.format, false);
            newText.Apply();
            newText.LoadRawTextureData(textureFromServer);
            
            _webcamTexture.texture = newText;
            _webcamTexture.material.mainTexture = newText;
            GetComponent<Renderer>().material.mainTexture = newText;
            return textureFromServer;
        }
    }
}