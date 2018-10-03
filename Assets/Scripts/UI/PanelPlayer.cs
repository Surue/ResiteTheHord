using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PanelPlayer : NetworkBehaviour {

    [SyncVar(hook = "OnIsReady")]
    public bool isReady = false;

    Toggle toggle;
    TextMeshProUGUI usernameText;
    
    [SyncVar(hook = "OnUsernameChanged")]
    [HideInInspector]
    public string username = "Player 0";

    PlayerInfo playerInfo;

    void Start() {
        toggle = GetComponentInChildren<Toggle>();
        usernameText = GetComponentInChildren<TextMeshProUGUI>();

        usernameText.text = username;

        GameObject panel = GameObject.Find("PanelPlayerList");
        transform.SetParent(panel.transform);

        if (!hasAuthority) {
            toggle.gameObject.SetActive(false);
        }

        if(isLocalPlayer) { 
            playerInfo = FindObjectOfType<PlayerInfo>();
        }
        
        StartCoroutine(Initialize());

        if(isReady) {
            GetComponent<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponent<Image>().color = new Color(255, 0, 0);
        }
    }

    IEnumerator Initialize() {
        yield return new WaitForEndOfFrame();

        transform.localScale = Vector3.one;
    }

    void Update() {
        if (isLocalPlayer) {
            CmdSetUsername(playerInfo.GetName());
        }
    }

    void OnIsReady(bool ready) {
        if(ready) {
            GetComponent<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponent<Image>().color = new Color(255, 0, 0);
        }
    }

    void OnUsernameChanged(string newUsername) {
        usernameText.text = newUsername;
    }

    [Command]
    public void CmdToggleReady(bool ready) {
        isReady = ready;
    }

    [Command]
    public void CmdSetUsername(string s) {
        username = s;
    }
}
