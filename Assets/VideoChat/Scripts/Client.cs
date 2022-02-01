﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace VideoChat.Scripts
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        public static int dataBufferSize = 4096;
        public bool isConnected;

        public string ip = "10.0.2.2"; //"127.0.0.1";
        public int port = 26950;
        public int myId;
        public TCP tcp;
        public UDP udp;

        private delegate void PacketHandler(Packet packet);

        private static Dictionary<int, PacketHandler> packetHandlers;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != null)
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public bool ConnecToServer()
        {
            tcp.Connect();

            InitializeClientData();
            
            if(!tcp.socket.Connected)
            {
                Debug.Log("Connection failed, check if server is running!");
                //StartCoroutine(TryReconnect());
                return false;
            }

            isConnected = true;
            Debug.Log("You have connected from Unity!");
            return true;
        }

        /// <summary>
        /// Move to UIClient
        /// </summary>
        /// <returns></returns>
        private IEnumerator TryReconnect()
        {
            float tryInTime = 5;
            float lapsedTime = 0f;

            while (lapsedTime < tryInTime)
            {
                lapsedTime += Time.deltaTime;
                Debug.Log($"Trying reconnect in {lapsedTime} seconds");
                yield return null;
            }

            tcp.Connect();
            if (!tcp.socket.Connected)
            {
                StartCoroutine(TryReconnect());
            }
            else
            {
                yield break;
            }
            yield return null;
        }

        /// <summary>
        /// TCP Protocol
        /// </summary>
        public class TCP
        {
            public TcpClient socket;
            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveTimeout = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                _receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    return;
                }

                _stream = socket.GetStream();

                _receivedData = new Packet();

                _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = _stream.EndRead(result);

                    if (byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);

                    //TODO: handle data
                    _receivedData.Reset(HandleData(data));
                    _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Disconnect();
                    throw;
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                _receivedData.SetBytes(data);

                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                {
                    byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            packetHandlers[packetId](packet);
                        }
                    });

                    packetLength = 0;

                    if (_receivedData.UnreadLength() >= 4)
                    {
                        packetLength = _receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                return packetLength <= 1;
            }
            
            
            public void Disconnect()
            {
                instance.Disconnect();
                _stream = null;
                _receivedData = null;
                _receiveBuffer = null;
                socket = null;
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {ex}.");
                }
            }
        }

        /// <summary>
        /// UDP Protocol
        /// </summary>
        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data via UDP {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < 4)
                    {
                        instance.Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception ex)
                {
                    Disconnect();
                }
            }

            private void HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread((() =>
                {
                    using (Packet packet = new Packet(data))
                    {
                        int packetId = packet.ReadInt();
                        packetHandlers[packetId](packet);
                    }
                }));
            }

            public void Disconnect()
            {
                instance.Disconnect();
                endPoint = null;
                socket = null;
            }
        }

        /// <summary>
        /// Initializes clients packages
        /// </summary>
        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int) ServerPackets.welcome, ClientHandle.Welcome},
                {(int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
                {(int) ServerPackets.playerPosition, ClientHandle.PlayerPosition},
                {(int) ServerPackets.playerRotation, ClientHandle.PlayerRotation},
                {(int) ServerPackets.playerWebcamTexture, ClientHandle.GetTexture}
            };
            Debug.Log("Initialized packets.");
        }

        /// <summary>
        /// Disconnects from server
        /// </summary>
        private void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
                
                Debug.Log("Disconnected from server.");
            }
        }
    }
}