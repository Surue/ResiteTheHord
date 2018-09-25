using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

    public GameObject enemyPrefab;
    public int numberOfEnemies;
    public int numberOfEnemiesToSpawn;

    static float TIME_BETWEEEN_SPAWN = 0.5f;
    float timeSinceLastSpawn = 0;

    GameManager gameManager;

    void Start() {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void AddEnemiesToSpawn(int nb) {
        numberOfEnemiesToSpawn += nb;

        if (numberOfEnemiesToSpawn == 0) {
            gameManager.FinishedSpawn();
        }
    }

    public override void OnStartServer() {
        //for (int i = 0; i < numberOfEnemies; i++) {
        //    Vector3 spawnPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) + transform.position;

        //    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        //    NetworkServer.Spawn(enemy);
        //}
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            Spawn();
        }

        if (numberOfEnemiesToSpawn > 0) {
            if (timeSinceLastSpawn <= 0) {
                Spawn();
                timeSinceLastSpawn = TIME_BETWEEEN_SPAWN;

                numberOfEnemiesToSpawn -= 1;
                if(numberOfEnemiesToSpawn <= 0) {
                    gameManager.FinishedSpawn();
                    timeSinceLastSpawn = 0;
                }
            } else {
                timeSinceLastSpawn -= Time.deltaTime;
            }
        }
    }
    
    void Spawn() {
        Vector3 spawnPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) + transform.position;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(enemy);
    }
}
