using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class JoinPanel : MonoBehaviour {

    MatchInfoSnapshot match;
    LanConnectionInfo lan;

    bool isLan = false;

    [SerializeField]TextMeshProUGUI matchName;

    public void Initialize(MatchInfoSnapshot match, Transform panelTransform, int number) {
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;

        this.match = match;

        matchName.text = this.match.name;

        GetComponentInChildren<Text>().text = "Join";
    }

    public void Initialize(LanConnectionInfo lan, Transform panelTransform, int number) {
        transform.SetParent(panelTransform);

        GetComponentInChildren<Text>().text = "Join";

        transform.localScale = Vector3.one;

        isLan = true;
        this.lan = lan;

        matchName.text = this.lan.name;
    }

    public void JoinMatch() {
        if (isLan) {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(lan);
        } else {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(match);
        }
    }
}
