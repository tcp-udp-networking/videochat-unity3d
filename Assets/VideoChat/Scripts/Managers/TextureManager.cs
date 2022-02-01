using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace VideoChat.Scripts.Managers
{
    public class TextureManager : MonoBehaviour
    {
        public Texture2D myTexture;
        public Material _Material;
        public RawImage raw;

        private delegate void SendTextureCallback();

        private delegate void GetTextureCallback();

        private void Start()
        {
            //StartCoroutine(GetTexture());
            /*StartCoroutine(SendTexture(myTexture, () =>
            {
                Debug.Log("Imagen enviada!");
            }));*/
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                StartCoroutine(SendTexture(myTexture, () => { Debug.Log("Imagen enviada!"); }));
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                GetTexture(() => { Debug.Log("Get Texture!"); });
            }
        }

        private IEnumerator SendTexture(Texture2D texture2D, SendTextureCallback callback)
        {
            var bytes = texture2D.EncodeToJPG();

            var model = new Streaming();
            model.id = 1;
            var s = string.Join(",", bytes);
            model.image = s;
            string objJson = JsonUtility.ToJson(model);


            var request = new UnityWebRequest("localhost:3001/webcam", UnityWebRequest.kHttpVerbPOST);

            request.SetRequestHeader("Content-Type", "application/json");
            var jsonBytes = Encoding.UTF8.GetBytes(objJson);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);

            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
                Debug.Log(request.downloadHandler.text);
                yield return null;
            }
            else
            {
                Debug.Log("Form upload complete!");
                callback();
            }

            Debug.Log(objJson);
        }

        private void GetTexture(GetTextureCallback callback)
        {
            Datas webcam = new Datas();
            Texture2D tex = new Texture2D(64, 64, TextureFormat.PVRTC_RGBA4, false);
            IEnumerator query = DatabaseManager.GetRequest("localhost:3001/webcam/getwebcam/1",
                "",
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    var jsonObject = JsonHelper.FixJson(request.downloadHandler.text);
                    Debug.Log(jsonObject);
                    webcam = JsonUtility.FromJson<Datas>(jsonObject);

                    tex.LoadRawTextureData(webcam.Items[0].image.data);
                    tex.Apply();
                    GetComponent<Renderer>().material.mainTexture = tex;

                    callback();
                });

            StartCoroutine(query);
        }
    }
}


[Serializable]
public class Datas
{
    public Items[] Items;
}

[Serializable]
public class Items
{
    public int id;
    public image image;
}

[Serializable]
public class image
{
    public string type;
    public byte[] data;
}