using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardPanel : MonoBehaviour {

    [SerializeField] TextMeshProUGUI text;

    PlayerController[] players;

    List<ScoreData.PlayersScore> playersScores;

    // Use this for initialization
    void Start () {
        playersScores = new List<ScoreData.PlayersScore>();
	    
        ScoreData scoreData = FindObjectOfType<ScoreData>();

        if(!scoreData || !scoreData.filled) {
            StartCoroutine(LookForPlayer());
        } else {
            string t = "";

            foreach (ScoreData.PlayersScore scoreDataPlayersScore in scoreData.playersScores) {
                t += scoreDataPlayersScore.name + " : " + scoreDataPlayersScore.score.ToString();
                t += "\n";
            }

            text.text = t;

            Destroy(scoreData.gameObject);
        }
    }
    
    IEnumerator LookForPlayer() {
        while (true) {
            playersScores = new List<ScoreData.PlayersScore>();
            players = FindObjectsOfType<PlayerController>();

            string t = "";

            List<PlayerController> orderedPlayers = players.OrderByDescending(x => x.GetScore()).ToList();

            foreach (PlayerController player in orderedPlayers) {
                ScoreData.PlayersScore s = new ScoreData.PlayersScore();
                s.name = player.GetName();
                s.score = player.GetScore();
                playersScores.Add(s);

                t += player.GetName() + " : " + player.GetScore().ToString();
                t += "\n";
            }

            text.text = t;

            FindObjectOfType<ScoreData>().StoreData(playersScores);

            yield return new WaitForSeconds(1);
        }
    }
}
