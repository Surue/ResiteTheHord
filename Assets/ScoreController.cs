using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreController : NetworkBehaviour {

    [SerializeField] GameObject scoreText;

    static ScoreController instance;
    public static ScoreController Instance {
        get {
            return instance;
        }
    }

    private void Awake() {
        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }
    }

    [TargetRpc]
    public void TargetDisplayScore(NetworkConnection conn, Vector3 p, int score) {
        GameObject instance = Instantiate(scoreText);
        instance.transform.position = p;

        instance.GetComponentInChildren<TextMeshProUGUI>().text = "+" + score.ToString();
    }
}
