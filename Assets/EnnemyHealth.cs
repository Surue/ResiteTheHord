using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnnemyHealth : NetworkBehaviour {

    [SerializeField] int lifePoint = 1;
    [SerializeField] ParticleSystem deathParticleSystem;

    [Server]
    public void TakeDamage(int damage, PlayerController bulletOwner) {

        lifePoint -= damage;

        if(lifePoint <= 0) {
            RpcDeath(); //For all clients

            Score score = GetComponent<Score>();
            if(score) {
                bulletOwner.CmdAddScore(GetComponent<Score>().transform.position, score.score);
            }

            if(!isClient && isServer) {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    [ClientRpc]
    void RpcDeath() {
        GameObject instance = Instantiate(deathParticleSystem).gameObject;
        instance.transform.position = transform.position;

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }

        Destroy(instance, 1f);
    }
}
