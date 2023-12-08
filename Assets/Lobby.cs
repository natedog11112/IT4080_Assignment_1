using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lobby : NetworkBehaviour
{
    public LobbyUi lobbyUi;
    public NetworkedPlayers networkedPlayers;
    void Start()
    {
        if (IsServer) {
            ServerPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ServerOnNetworkedPlayersChanged;
            lobbyUi.ShowStart(true);
            lobbyUi.OnStartClicked += ServerStartClicked;
        } else {
            ClientPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ClientNetPlayerChanged;
            lobbyUi.ShowStart(false);
            lobbyUi.OnReadyToggled += ClientOnReadyToggled;
        }
    }

    private void ServerOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent) {
        ServerPopulateCards();
    }

    private void ServerPopulateCards() {
        lobbyUi.playerCards.Clear();
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers) {
            PlayerCard pc = lobbyUi.playerCards.AddCard(info.name.ToString());
            pc.ready = info.ready;
            pc.clientId = info.clientId;
            pc.color = info.color;
            pc.playerName = info.name.ToString();
            if (info.clientId == NetworkManager.LocalClientId) {
                pc.ShowKick(false);
            } else {
                pc.ShowKick(true);
            }
            pc.UpdateDisplay();
        }
        lobbyUi.EnableStart(networkedPlayers.AllPlayersReady());
    }

    private void ClientPopulateCards() {
        lobbyUi.playerCards.Clear();
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers) {
            PlayerCard pc = lobbyUi.playerCards.AddCard(info.name.ToString());
            pc.clientId = info.clientId;
            pc.ready = info.ready;
            pc.color = info.color;
            pc.playerName = info.name.ToString();
            pc.ShowKick(false);
            pc.UpdateDisplay();
        }
    }

    private void ClientNetPlayerChanged (NetworkListEvent<NetworkPlayerInfo> changeEvent) {
        ClientPopulateCards();
    }

    private void ClientOnReadyToggled(bool newValue) {
        UpdateReadyServerRpc(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyServerRpc(bool newValue, ServerRpcParams rpcParams = default) {
        networkedPlayers.UpdateReady(rpcParams.Receive.SenderClientId, newValue);
    }

    private void ServerStartClicked() {
        NetworkManager.SceneManager.LoadScene(
            "Arena1", 
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}
