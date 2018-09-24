using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardPanel : MonoBehaviour {

    [SerializeField] TextMeshProUGUI text;

	// Use this for initialization
	void Start () {
	    StartCoroutine(LookForPlayer());
	}
    
    IEnumerator LookForPlayer() {
        while (true) {
            PlayerController[] players = FindObjectsOfType<PlayerController>();

            string t = "";

            int i = 0;

            List<PlayerController> orderedPlayers = players.OrderByDescending(x => x.GetScore()).ToList();

            foreach (PlayerController player in orderedPlayers) {
                t += "P" + player.netId + " : " + player.GetScore().ToString();
                t += "\n";
                i++;
            }

            text.text = t;

            yield return new WaitForSeconds(1);
        }
    }
}
