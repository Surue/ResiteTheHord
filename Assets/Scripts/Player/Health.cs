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

    [Server]
    public void TakeDamage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            if (destroyOnDeath) {
                RpcDestroy();

                NetworkServer.Destroy(gameObject);
            } else {
                currentHealth = MAX_HEALTH;
                RpcRespawn();
            }
        } else {
            RpcOnHealthChanged(currentHealth);
        }
    }

    [ClientRpc]
    public virtual void RpcOnHealthChanged(int health) { }

    [ClientRpc]
    public virtual void RpcRespawn() { }

    [ClientRpc]
    public virtual void RpcDestroy() { }
}
