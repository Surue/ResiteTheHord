using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

    public RectTransform healthBar;

    public const int MAX_HEALTH = 100;

    public bool destroyOnDeath;

    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = MAX_HEALTH;
    
    [SerializeField] ParticleSystem explosionParticleSystem;

    public void TakeDamage(int damage) {
        if (!isServer) {
            return;
        }
        
        currentHealth -= damage;

        if (currentHealth <= 0) {
            if (destroyOnDeath) {
                if (explosionParticleSystem != null) {
                    GameObject instance = Instantiate(explosionParticleSystem).gameObject;
                    instance.transform.position = transform.position;
                }

                Destroy(gameObject);
            } else {
                currentHealth = MAX_HEALTH;
                GetComponent<PlayerController>().CmdOnDeath();
            }
        }
    }

    [ClientRpc]
    public void RpcRespawn() {
        if (isLocalPlayer) {
            transform.position = Vector3.zero;
        }
    }

    void OnChangeHealth(int health) {
        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }
}
