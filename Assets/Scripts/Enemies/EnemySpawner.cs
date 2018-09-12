using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

    public GameObject enemyPrefab;
    public int numberOfEnemies;

    public override void OnStartServer() {
        //for (int i = 0; i < numberOfEnemies; i++) {
        //    Vector3 spawnPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) + transform.position;

        //    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        //    NetworkServer.Spawn(enemy);
        //}
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            CmdSpawn();
        }
    }

    [Command]
    void CmdSpawn() {
        Vector3 spawnPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) + transform.position;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(enemy);
    }
}
