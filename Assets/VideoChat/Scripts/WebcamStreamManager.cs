using UnityEngine;
using UnityEngine.UI;

namespace VideoChat.Scripts
{
    public class WebcamStreamManager : MonoBehaviour
    {
        public static WebcamStreamManager Instance;
        public GameObject streamer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"An instance for {Instance} already exists");
                Destroy(this);
            }
        }
        
        public void Stream(int id, string username, byte[] bytes)
        {
            Texture2D tex = new Texture2D(2,2);
            tex.LoadRawTextureData(bytes);
            tex.Apply();
            streamer.GetComponent<RawImage>().texture = tex;
        }
    }
}