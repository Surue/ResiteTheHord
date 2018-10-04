using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : Health {

    public RectTransform healthBar;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [ClientRpc]
    public override void RpcOnHealthChanged(int health) {
        currentHealth = health;

        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }

    [ClientRpc]
    public override void RpcDestroy() {
        if(explosionParticleSystem != null) {
            GameObject instance = Instantiate(explosionParticleSystem).gameObject;
            instance.transform.position = transform.position;
        }

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }

        Destroy(gameObject);
    }

    [ClientRpc]
    public override void RpcRespawn() {
        if(isLocalPlayer) {
            currentHealth = MAX_HEALTH;

            RpcOnHealthChanged(currentHealth);

            transform.position = Vector3.zero;
        }
    }
}
