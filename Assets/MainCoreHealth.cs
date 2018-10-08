using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MainCoreHealth : Health {

    public RectTransform healthBar;

    [ClientRpc]
    public override void RpcDestroy() {
        if(explosionParticleSystem != null) {
            GameObject instance = Instantiate(explosionParticleSystem).gameObject;
            instance.transform.position = transform.position;
        }

        if(isServer) {
            FindObjectOfType<GameManager>().OnMainCoreDestroyed();
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    public override void RpcOnHealthChanged(int health) {
        currentHealth = health;
        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }
}
