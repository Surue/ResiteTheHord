using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

    public List<GameObject> enemyPrefabs;
    public int numberOfEnemies;
    public int numberOfEnemiesToSpawn;

    GameObject mainGoalForEnemies;

    static float TIME_BETWEEN_SPAWN = 0.75f;
    float timeSinceLastSpawn = 0;

    GameManager gameManager;

    void Start() {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SetMainGoal(GameObject goal) {
        mainGoalForEnemies = goal;
    }

    public void AddEnemiesToSpawn(int nb) {
        numberOfEnemiesToSpawn += nb;

        if (numberOfEnemiesToSpawn == 0) {
            gameManager.FinishedSpawn();
        }
    }

    void Update() {
        if (numberOfEnemiesToSpawn <= 0) {
            return;
        }

        if (timeSinceLastSpawn <= 0) {
            Spawn();
            timeSinceLastSpawn = TIME_BETWEEN_SPAWN;

            numberOfEnemiesToSpawn -= 1;
            if (numberOfEnemiesToSpawn > 0) {
                return;
            }

            gameManager.FinishedSpawn();
            timeSinceLastSpawn = 0;
        } else {
            timeSinceLastSpawn -= Time.deltaTime;
        }
    }
    
    void Spawn() {
        Vector3 spawnPosition = transform.position;

        GameObject enemyPrefab = enemyPrefabs[(int) (RandomSeed.GetValue() * enemyPrefabs.Count)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        enemy.transform.position = spawnPosition;

        if (enemy.GetComponent<EnemyMovement>()) {
            enemy.GetComponent<EnemyMovement>().Initialize(mainGoalForEnemies.transform);
        }

        NetworkServer.Spawn(enemy);
    }
}
