using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CountDownTimer : NetworkBehaviour {

    public TextMeshProUGUI timerText;
    public AnimationCurve scaleCurve;

    float timer = 0;

	// Use this for initialization
	void Start () {
	    timerText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
	    timer -= Time.deltaTime;

        if (timer > 0) {
	        timerText.text = (timer % 60).ToString() + "." + (timer - (timer % 60)).ToString();
	        timerText.text = timerText.text.Substring(0, 4);

	        float scale = Mathf.Clamp(timer, 0, 2) / 2;

	        scale = scaleCurve.Evaluate(scale) * 2 + 1;

	        timerText.transform.localScale = Vector3.one * scale;
        } else {
	        timerText.text = "";
	    }
	}

    [ClientRpc]
    public void RpcSetTime(float time) {
        timer = time;
    }

    public void AddTime(float time) {
        timer += time;
    }
}
