using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PanelPlayer : NetworkBehaviour {

    [SyncVar(hook = "OnIsReady")]
    public bool isReady = false;

    public Toggle toggle;

    void Start() {

        GameObject panel = GameObject.Find("PanelPlayerList");
        transform.SetParent(panel.transform);

        if(!hasAuthority) {
            toggle.gameObject.SetActive(false);
        }
        
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize() {
        yield return new WaitForEndOfFrame();

        transform.localScale = Vector3.one;
    }

    void OnIsReady(bool ready) {
        if(ready) {
            GetComponent<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponent<Image>().color = new Color(255, 0, 0);
        }
    }

    [Command]
    public void CmdToggleReady(bool ready) {
        Debug.Log(ready);
        isReady = ready;
    }
}
