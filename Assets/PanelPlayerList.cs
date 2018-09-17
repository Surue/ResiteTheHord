using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PanelPlayerList : NetworkBehaviour {
    List<PanelPlayer> panelsPlayer = new List<PanelPlayer>();
    [SerializeField] RectTransform panelParent;

    [SerializeField] GameObject playerPanelPrefab;
    int numberOfPlayer = 0;

    public void AddPlayer(int id) {
        GameObject instance = Instantiate(playerPanelPrefab, panelParent);
        instance.GetComponent<PanelPlayer>().id = id;

        NetworkServer.Spawn(instance);

        instance.GetComponent<PanelPlayer>().Initialize(panelParent);

        FindObjectOfType<LobbyMenuController>().RpcSetId(id);

        panelsPlayer.Add(instance.GetComponent<PanelPlayer>());

        numberOfPlayer++;
    }
    
    public void Ready(int id) {
        if (!isServer) {
            return;
        }
        Debug.Log("Player " + id + "is ready");
        foreach (PanelPlayer panelPlayer in panelsPlayer) {
            panelPlayer.Ready(id);
        }

        UpdateLaunchButton(OnPlayerReady());
    }

    void UpdateLaunchButton(bool toShow) {

    }

    bool OnPlayerReady() {
        foreach (PanelPlayer panelPlayer in panelsPlayer) {
            if (!panelPlayer.isReady) {
                return false;
            }
        }

        return true;
    }
}
