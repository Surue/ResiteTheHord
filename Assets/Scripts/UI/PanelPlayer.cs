using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;

public class PanelPlayer : NetworkBehaviour {

    [SyncVar(hook = "OnIsReady")]
    public bool isReady = false;

    Toggle toggle;
    TextMeshProUGUI usernameText;
    
    [SyncVar(hook = "OnUsernameChanged")]
    [HideInInspector]
    public string username = "Player 0";

    PlayerInfo playerInfo;

    List<PanelPlayer> otherPlayer = new List<PanelPlayer>();

    Animator animator;

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
            GetComponentInChildren<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponentInChildren<Image>().color = new Color(255, 0, 0);
        }

        otherPlayer = FindObjectsOfType<PanelPlayer>().ToList();

        animator = GetComponentInChildren<Animator>();
    }

    IEnumerator Initialize() {
        yield return new WaitForEndOfFrame();

        transform.localScale = Vector3.one;
    }

    void Update() {
        if (isLocalPlayer) {
            CmdSetUsername(playerInfo.GetName());
        }

        if (otherPlayer.Count > 0) {
            //List<PanelPlayer> tmp = new List<PanelPlayer>();

            //bool shouldGoUp = false;

            //foreach (PanelPlayer panelPlayer in otherPlayer) {
            //    if (panelPlayer == null) {
            //        shouldGoUp = true;
            //    } else {
            //        tmp.Add(panelPlayer);
            //    }
            //}

            //if (shouldGoUp) {
            //    animator.SetTrigger("moveUp");
            //}

            //otherPlayer = tmp;
        }
    }

    void OnIsReady(bool ready) {
        if(ready) {
            GetComponentInChildren<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponentInChildren<Image>().color = new Color(255, 0, 0);
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

    public override void OnNetworkDestroy() {
    }
}
