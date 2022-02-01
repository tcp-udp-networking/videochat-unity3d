using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VideoChat.Scripts
{
    public class DatabaseManager
    {
        public static IEnumerator PostRequest(string url, string data, Action<UnityWebRequest> callback)
        {
            using UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            callback(request);
            Debug.Log("Status Code: " + request.responseCode);
        }
        
        
        public static IEnumerator GetRequest(string url, string apiKey, Action<UnityWebRequest> callback)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", apiKey);
            yield return request.SendWebRequest();
            callback(request);
        }
    }
}