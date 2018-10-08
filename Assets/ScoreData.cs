using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreData : MonoBehaviour {

    public struct PlayersScore {
        public string name;
        public int score;
    }

    public List<PlayersScore> playersScores;

    public bool filled = false;

	// Use this for initialization
	void Start () {
        playersScores = new List<PlayersScore>();
	    DontDestroyOnLoad(gameObject);	
	}

    public void StoreData(List<PlayersScore> scores) {
        playersScores = scores;
        filled = true;
    }
}
