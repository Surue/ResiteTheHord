﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    [SerializeField] bool skipIntro = false;
    [SerializeField] bool hasToGenerateMap = false;
    GameObject mainGoalForEnnemies;

    List<PlayerController> players;
    PlayerSpawner[] playerSpawner;
    EnemySpawner[] enemySpawners;

    //Waves
    float timeBetweenWaves = 7.5f;
    float timeBeforeNextWaves = 0f;
    int numberOfEnemiesPerSpawn = 1;
    int spawnerFinishedSpawn = 0;

    //UI
    CountDownTimer countDownTimer;

    //Life system
    List<PlayerController> playersWaitingToRespawn = new List<PlayerController>();

    //Scene when died
    [SerializeField]
    string sceneWhenCoreDestroyed;

    //Random seed prefab
    [SerializeField] GameObject randomSeed;

    MapGenerator mapGenerator;

    enum State {
        INTRO,
        GENERATE_MAP,
        INITIALIZE,
        WAIT_NEXT_WAVE,
        SPAWN_ENEMIES,
        WAIT_END_SPAWN,
        END_GAME
    }

    State state = State.INTRO;

    void Awake() {
        players = new List<PlayerController>();
    }

    // Use this for initialization
    void Start () {

        mapGenerator = FindObjectOfType<MapGenerator>();

        if (!isServer) {
            if(hasToGenerateMap) {
                state = State.GENERATE_MAP;
                mapGenerator.StartGeneration();
            }
        }
        else {
            if (skipIntro) {
                state = State.INITIALIZE;
            }

            if (hasToGenerateMap) {
                state = State.GENERATE_MAP;
                mapGenerator.StartGeneration();
            }

            countDownTimer = FindObjectOfType<CountDownTimer>();

            GameObject instance = Instantiate(randomSeed);

            NetworkServer.Spawn(instance);
        }
    }
	
	// Update is called once per frame
	void Update () {
	    if (!isServer) {
	        switch (state) {
	            case State.GENERATE_MAP:
	                if(!mapGenerator.isGenerating) {
	                    GameObject.Find("PanelHider").SetActive(false);
                        state = State.INITIALIZE;
	                }
	                break;
	            case State.INITIALIZE:

	                break;
            }

	        return;
	    }

	    switch (state) {
            case State.INTRO:
                break;

            case State.GENERATE_MAP:
                if (!mapGenerator.isGenerating) {
                    state = State.INITIALIZE;
                }
                break;

            case State.INITIALIZE:
                //Find all playerSpawner
                playerSpawner = FindObjectsOfType<PlayerSpawner>();

                //Respawn all player
                playersWaitingToRespawn.AddRange(players);

                //Find motherboard
                mainGoalForEnnemies = FindObjectOfType<MainCore>().gameObject;

                //Find all enemySpawner
                enemySpawners = FindObjectsOfType<EnemySpawner>();

                //Set objectiv for enemies
                foreach (EnemySpawner enemySpawner in enemySpawners) {
                    enemySpawner.SetMainGoal(mainGoalForEnnemies);
                }

                //Find circle effect
                CircleEffect circle = FindObjectOfType<CircleEffect>();
                if (circle) {
                    circle.transform.position = mainGoalForEnnemies.transform.position;
                }

                GameObject.Find("PanelHider").SetActive(false);

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

                numberOfEnemiesPerSpawn += players.Count;
                timeBeforeNextWaves = timeBetweenWaves;
                state = State.WAIT_END_SPAWN;
                break;

            case State.WAIT_END_SPAWN:
                if (spawnerFinishedSpawn == enemySpawners.Length) {
                    countDownTimer.RpcSetTime(timeBeforeNextWaves);
                    state = State.WAIT_NEXT_WAVE;
                    spawnerFinishedSpawn = 0;
                }
                break;

            case State.END_GAME:
                break;
	    }

	    if(playersWaitingToRespawn.Count > 0) {
            //Select spawn point
	        bool found = false;

	        PlayerSpawner spawner = null;
	        for (int i = 0; i < playerSpawner.Length; i++) {
	            if (playerSpawner[i].isFree) {
	                spawner = playerSpawner[i];
                    playerSpawner[i].Spawn();
	                i = playerSpawner.Length;

	            }
	        }

	        if (spawner != null) {
	            playersWaitingToRespawn[playersWaitingToRespawn.Count - 1].GetComponent<PlayerHealth>().RpcRespawn(spawner.transform.position);
	            playersWaitingToRespawn.RemoveAt(playersWaitingToRespawn.Count - 1);
	        }
	    }
    }

    [Server]
    public void OnPlayerDeath(GameObject player) {
        Debug.Log("A player died");

        playersWaitingToRespawn.Add(player.GetComponent<PlayerController>());
    }

    [Server]
    public void OnMainCoreDestroyed() {
        CustomNetworkManager.singleton.ServerChangeScene(sceneWhenCoreDestroyed);
    }

    [Server]
    public void FinishedSpawn() {
        spawnerFinishedSpawn++;
    }

    [Server]
    public GameObject GetMainGoalForEnnemies() {
        return mainGoalForEnnemies;
    }

    [Command]
    public void CmdOnClientConnected(NetworkIdentity player) {
        if (state == State.GENERATE_MAP) {
            players.Add(player.GetComponent<PlayerController>());
        } else {
            players.Add(player.GetComponent<PlayerController>());
            playersWaitingToRespawn.Add(player.GetComponent<PlayerController>());
        }
    }
}
