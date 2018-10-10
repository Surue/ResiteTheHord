using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyHealth : Health {

    EnemySound soundController;

    void Start() {
        EnemySound soundController = GetComponent<EnemySound>();
        GameObject instance = Instantiate(spawnParticleSystem, transform.position, Quaternion.identity).gameObject;

        soundController.Spawn();

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = Color.red;
    }

    [ClientRpc]
    public override void RpcDestroy() {
        GameObject instance = Instantiate(explosionParticleSystem).gameObject;
        instance.transform.position = transform.position;
        Destroy(instance, 1f);

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }
}
