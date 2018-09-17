using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PanelPlayer : NetworkBehaviour {

    [SyncVar]
    public int id;

    [SyncVar(hook = "OnIsReady")]
    public bool isReady = false;
    
    public void Ready(int id) {
        if (this.id == id) {
            isReady = !isReady;
        }
    }

    public void Initialize(Transform panelTransform) {
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    void OnIsReady(bool ready) {
        if(ready) {
            GetComponent<Image>().color = new Color(0, 255, 0);
        } else {
            GetComponent<Image>().color = new Color(255, 0, 0);
        }
    }
}
