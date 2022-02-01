using System.Net;
using UnityEngine;

namespace VideoChat.Scripts
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int myId = packet.ReadInt();

            Debug.Log($"Messages from server: {msg}");
            Client.instance.myId = myId;
            ClientSend.WelcomeReceived();

            Client.instance.udp.Connect(((IPEndPoint) Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void SpawnPlayer(Packet packet)
        {
            Debug.Log("Spawing player...");

            int id = packet.ReadInt();
            string username = packet.ReadString();
            Vector3 position = packet.ReadVector3();
            Quaternion rotation = packet.ReadQuaternion();

            GameManager.Instance.SpawnPlayer(id, username, position, rotation);
        }

        public static void GetTexture(Packet packet)
        {
            int id = packet.ReadInt();
            int length = packet.ReadInt();
            var texture2D = packet.ReadBytes(length);
            GameManager.players[id].SetWebCamFromServer(texture2D);
        }

        public static void PlayerPosition(Packet packet)
        {
            int id = packet.ReadInt();
            Vector3 position = packet.ReadVector3();
            GameManager.players[id].transform.position = position;
        }

        public static void PlayerRotation(Packet packet)
        {
            int id = packet.ReadInt();
            Quaternion rotation = packet.ReadQuaternion();
            GameManager.players[id].transform.rotation = rotation;
        }
    }
}