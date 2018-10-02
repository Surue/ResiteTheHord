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
    [SerializeField] Animator animator;

    public void Initialize(MatchInfoSnapshot match, Transform panelTransform) {
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;

        this.match = match;

        matchName.text = this.match.name;

        GetComponentInChildren<Text>().text = "Join";
    }

    public void Initialize(LanConnectionInfo lan, Transform panelTransform) {
        transform.SetParent(panelTransform);

        GetComponentInChildren<Text>().text = "Join";

        transform.localScale = Vector3.one;

        isLan = true;
        this.lan = lan;

        matchName.text = this.lan.name;
    }

    public void MoveUp() {
        //animator.SetTrigger("MoveUp");
    }

    public void Disparition() {
        animator.SetTrigger("Disparition");

        float time = 0;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;    //Get Animator controller
        for(int i = 0;i < ac.animationClips.Length;i++)                 //For all animations
        {
            if(ac.animationClips[i].name == "Disparition")        //If it has the same name as your clip
            {
                time = ac.animationClips[i].length;
            }
        }
        Destroy(gameObject, time);
    }

    public void JoinMatch() {
        if (isLan) {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(lan);
        } else {
            FindObjectOfType<CustomNetworkManager>().JoinMatch(match);
        }
    }
}
