using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Security.Cryptography;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];
    private ulong[] soleclient = new ulong[1];
    private ulong[] connectedclients = new ulong[0];
    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if(IsServer)
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if(IsHost)
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the Host AND Client {NetworkManager.LocalClientId}");
            }
            else
            {
                DisplayMessageLocally(SYSTEM_ID, "You are the Server");
            }
            
        }
        else
        {
            DisplayMessageLocally(SYSTEM_ID, $"You are the Client {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        
        soleclient[0] = clientId;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = soleclient;
        ReceiveChatMessageClientRpc("I see you joined", 0, rpcParams);
        SendChatMessageServerRpc($"Player {clientId} has joined");
    }
    private void ServerOnClientDisconnected(ulong clientId)
    {
        SendChatMessageServerRpc($"Player {clientId} has left");
    }

    private void DisplayMessageLocally(ulong from, string message)
    {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;

        if(from == NetworkManager.LocalClientId)
        {
            fromStr = $"you";
            textColor = Color.magenta;
        }
        else if(from == SYSTEM_ID)
        {
            fromStr = "SYS";
            textColor = Color.green;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
            SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);

            ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
        }
        else
        {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }
    
    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        //clientIds[0] = from;
        //ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);

        //clientIds[0] = to;
        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
    }
}
