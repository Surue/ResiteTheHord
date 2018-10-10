using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
    public const int MAX_HEALTH = 100;

    public bool destroyOnDeath;
    
    public int currentHealth = MAX_HEALTH;
    
    [SerializeField] protected ParticleSystem explosionParticleSystem;
    [SerializeField] protected ParticleSystem spawnParticleSystem;

    [Server]
    public virtual void TakeDamage(int damage, PlayerController bulletOwner = null) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            if (destroyOnDeath) {
                RpcDestroy();

                Score score = GetComponent<Score>();
                if(score != null && bulletOwner != null) {
                    bulletOwner.CmdAddScore(score.transform.position, score.score);
                }
            } else {
                RpcRespawn(Vector3.zero);
            }
        } else {
            RpcOnHealthChanged(currentHealth);
        }
    }

    [ClientRpc]
    public virtual void RpcOnHealthChanged(int health) { }

    [ClientRpc]
    public virtual void RpcRespawn(Vector3 pos) { }

    [ClientRpc]
    public virtual void RpcDestroy() { }
}
