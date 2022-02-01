using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace VideoChat.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

        public GameObject localPrefab;
        public GameObject remotePrefab;

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

        public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
        {
            GameObject newPlayer;
            if (id == Client.instance.myId)
            {
                newPlayer = Instantiate(localPrefab, position, rotation);
            }
            else
            {
                newPlayer = Instantiate(remotePrefab, position, rotation);
            }

            newPlayer.GetComponent<PlayerManager>().id = id;
            newPlayer.GetComponent<PlayerManager>().username = username;
            newPlayer.GetComponent<PlayerManager>().playerNicknameText.text = username;
            
            players.Add(id, newPlayer.GetComponent<PlayerManager>());
        }
    }
}