using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class JoinButton : MonoBehaviour {

    Text buttonText;
    MatchInfoSnapshot match;
    
	void Awake () {
	    buttonText = GetComponentInChildren<Text>();
	}

    public void Initialize(MatchInfoSnapshot match, Transform panelTransform) {
        buttonText.text = match.name;
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        this.match = match;
    }

    public void JoinMatch() {
        FindObjectOfType<CustomNetworkManager>().JoinMatch(match);
    }
}
