using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    [SerializeField] bool skipIntro = false;

    PlayerController[] players;
    EnemySpawner[] enemySpawners;

    //Waves
    float timeBetweenWaves = 7.5f;
    float timeBeforeNextWaves = 0f;
    int numberOfEnemiesPerSpawn = 1;
    int spawnerFinishedSpawn = 0;

    static GameManager instance;
    public static GameManager Instance {
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

    enum State {
        INTRO,
        INITIALIZE,
        WAIT_NEXT_WAVE,
        SPAWN_ENEMIES,
        WAIT_END_SPAWN,
        END_GAME
    }

    State state = State.INTRO;

    // Use this for initialization
    void Start () {
        if(skipIntro) {
            state = State.INITIALIZE;
        }
    }
	
	// Update is called once per frame
	void Update () {

        //Debug.Log(state);

	    switch (state) {
            case State.INTRO:
                break;

            case State.INITIALIZE:
                players = FindObjectsOfType<PlayerController>();

                enemySpawners = FindObjectsOfType<EnemySpawner>();
                if(players.Length != 0)
                state = State.WAIT_NEXT_WAVE;
                break;

            case State.WAIT_NEXT_WAVE:
                if (timeBeforeNextWaves <= 0) {
                    state = State.SPAWN_ENEMIES;
                } else {
                    timeBeforeNextWaves -= Time.deltaTime;
                }
                break;

            case State.SPAWN_ENEMIES:
                int[] enemiesPerSpawner = new int[enemySpawners.Length];

                int enemiesToSpawn = numberOfEnemiesPerSpawn;

                for (int i = 0; i < enemySpawners.Length; i++) {
                    if (i == enemySpawners.Length - 1) {
                        enemiesPerSpawner[i] = enemiesToSpawn;
                        break;
                    }

                    enemiesPerSpawner[i] = Random.Range(0, enemiesToSpawn);
                    enemiesToSpawn -= enemiesPerSpawner[i];

                    if (enemiesToSpawn <= 0) {
                        break;
                    }
                }

                for (int i = 0; i < enemySpawners.Length; i++) {
                    enemySpawners[i].AddEnemiesToSpawn(enemiesPerSpawner[i]);
                }

                numberOfEnemiesPerSpawn += players.Length;
                timeBeforeNextWaves = timeBetweenWaves;
                state = State.WAIT_END_SPAWN;
                break;

            case State.WAIT_END_SPAWN:
                if (spawnerFinishedSpawn == enemySpawners.Length) {
                    Debug.Log("FINISHED");
                    state = State.WAIT_NEXT_WAVE;
                    spawnerFinishedSpawn = 0;
                }
                break;

            case State.END_GAME:
                break;
	    }
	}

    public void FinishedSpawn() {
        spawnerFinishedSpawn++;
    }
}
