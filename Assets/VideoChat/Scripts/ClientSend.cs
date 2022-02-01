using UnityEngine;

namespace VideoChat.Scripts
{
    public class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet);
        }


        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.udp.SendData(_packet);
        }
        
        #region  Packets

        public static void WelcomeReceived()
        {
            using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                packet.Write(Client.instance.myId);
                packet.Write(UIClient.instance.usernameField.text);

                SendTCPData(packet);
            }
        }
        #endregion

        public static void UserWebcam (byte[] texture2D)
        {
            using (Packet packet = new Packet((int) ClientPackets.playerWebcamTextureReceived))
            {
                packet.Write(texture2D.Length);
                packet.Write(GameManager.players[Client.instance.myId].SetWebCamFromServer(texture2D));
                packet.Write(640);
                packet.Write(360);
                SendUDPData(packet);
            }
        }
        
        public static void PlayerMovement(bool[] inputs)
        {
            using (Packet packet = new Packet((int)ClientPackets.playerMovement))
            {
                packet.Write(inputs.Length);
                foreach (bool input in inputs)
                {
                    packet.Write(input);
                }

                packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
                
                SendUDPData(packet);
            }
        }
    }
}