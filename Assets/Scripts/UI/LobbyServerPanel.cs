using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyServerPanel : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	    if (!isServer) {
            Destroy(gameObject);
	    }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
