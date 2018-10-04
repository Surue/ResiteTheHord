using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnnemyHealth : Health {

    [Server]
    public void TakeDamage(int damage, PlayerController bulletOwner) {
        currentHealth -= damage;

        if(currentHealth <= 0) {
            RpcDeath();

            Score score = GetComponent<Score>();
            if(score) {
                bulletOwner.CmdAddScore(GetComponent<Score>().transform.position, score.score);
            }
        }
    }

    [ClientRpc]
    void RpcDeath() {
        GameObject instance = Instantiate(explosionParticleSystem).gameObject;
        instance.transform.position = transform.position;

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }

        Destroy(instance, 1f);
    }
}
