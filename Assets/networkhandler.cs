using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class networkhandler : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
    }
        private bool hasPrinted = false;
        private void printMe() {
            if (hasPrinted) {
                return;
            }
            Debug.Log("I AM");
            hasPrinted = true;
            if (IsServer) {
                Debug.Log($"  the Server! {NetworkManager.ServerClientId}");
            }
            if (IsHost) {
                Debug.Log($"  the Host! {NetworkManager.ServerClientId}/{NetworkManager.LocalClientId}");
            }
            if (IsClient) {
                Debug.Log($"  the Client! {NetworkManager.LocalClientId}");
            }
            if (!IsServer && !IsClient) {
                Debug.Log("  Nothing yet");
                hasPrinted = false;
            }
            
        }

        private void OnClientStarted()
        {
            NetworkManager.OnClientConnectedCallback += ClientOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
            NetworkManager.OnClientStopped += ClientOnClientStopped;
            printMe();
        }

        private void ClientOnClientConnected(ulong clientId)
        {
            printMe();
        }
        private void ClientOnClientDisconnected(ulong clientId)
        {
            
        }
        private void ClientOnClientStopped(bool indicator)
        {
            NetworkManager.OnClientConnectedCallback -= ClientOnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= ClientOnClientDisconnected;
            NetworkManager.OnClientStopped -= ClientOnClientStopped;
        }

        private void OnServerStarted()
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            NetworkManager.OnServerStopped += ServerOnServerStopped;
        }

        private void ServerOnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected to the server");
        }
        private void ServerOnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected to the server");
        }
        private void ServerOnServerStopped(bool indicator)
        {
            hasPrinted = false;
            NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
            NetworkManager.OnServerStopped -= ServerOnServerStopped;
        }
    }

