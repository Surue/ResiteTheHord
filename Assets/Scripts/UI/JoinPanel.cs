using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class JoinPanel : MonoBehaviour {

    MatchInfoSnapshot match;
    LanConnectionInfo lan;

    bool isLan = false;

    public void Initialize(MatchInfoSnapshot match, Transform panelTransform, int number) {
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.localPosition = Vector2.zero + new Vector2(0, GetComponent<RectTransform>().sizeDelta.y * number);

        this.match = match;

        GetComponentInChildren<Text>().text = "online";
    }

    public void Initialize(LanConnectionInfo lan, Transform panelTransform, int number) {
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        transform.localPosition = Vector2.zero + new Vector2(0, GetComponent<RectTransform>().sizeDelta.y * number);
        
        isLan = true;

        GetComponentInChildren<Text>().text = "local";
        this.lan = lan;
    }

    public void JoinMatch() {
        if (isLan) {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(lan);
        } else {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(match);
        }
    }
}
