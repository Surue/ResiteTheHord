using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloModeScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    if (FindObjectOfType<CustomNetworkManager>().forceSoloMode) {
	        FindObjectOfType<CustomNetworkManager>().LoadSoloMode();
        }
	}
}
