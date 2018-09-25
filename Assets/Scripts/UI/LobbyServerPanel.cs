using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyServerPanel : NetworkBehaviour {

    [SerializeField] Text serverNameText;

    float refreshTime = 0;

	// Use this for initialization
	void Start () {
	    if (!isServer) {
            Destroy(gameObject);
	    }


	}
	
	// Update is called once per frame
	void Update () {
	    if(Time.time >= refreshTime) {
	        RefreshInfos();
	    }
    }

    void RefreshInfos() {
        refreshTime = Time.time + 5f;
    }
}
