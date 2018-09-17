using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyMenuController : NetworkBehaviour {

    public int id;

    [ClientRpc]
    public void RpcSetId(int newId) {
        Debug.Log("Set id " + newId);
        if (true) {
            Debug.Log("Match for local player " + newId);
            id = newId;
        }
    }
    
    public void Ready() {
        Debug.Log(" >=== READY BUTTON PRESSED ==== ");
        FindObjectOfType<PanelPlayerList>().Ready(id);
        Debug.Log(" ==== READY BUTTON PRESSED ===< ");
    }

    public void LoadScene(string name) {
        FindObjectOfType<CustomNetworkManager>().ServerChangeScene(name);
    }
}
