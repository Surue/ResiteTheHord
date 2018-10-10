using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyServerPanel : NetworkBehaviour {
    
    [SerializeField] Button buttonLaunch;

    float refreshTime = 0;

	// Use this for initialization
	void Start () {
	    if (!isServer) {
            Destroy(gameObject);
        }
	    else {
	        buttonLaunch.interactable = false;
	    }


	}
	
	// Update is called once per frame
	void Update () {
	    if(Time.time >= refreshTime) {
	        RefreshInfos();
	    }
    }

    void RefreshInfos() {
        refreshTime = Time.time + 1f;

        List<PanelPlayer> panelPlayers = FindObjectsOfType<PanelPlayer>().ToList();

        bool allReady = true;

        foreach (PanelPlayer panelPlayer in panelPlayers) {
            if (!panelPlayer.isReady) {
                allReady = false;
            }
        }

        if (allReady) {
            buttonLaunch.interactable = true;
        }
        else {
            buttonLaunch.interactable = false;
        }
    }
}
