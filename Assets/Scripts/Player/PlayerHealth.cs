using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : Health {

    public RectTransform healthBar;

    CameraShaking cameraShaking;

    // Use this for initialization
    void Start () {
        cameraShaking = FindObjectOfType<CameraShaking>();
    }

    [ClientRpc]
    public override void RpcOnHealthChanged(int health) {
        currentHealth = health;

        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);

        if (isLocalPlayer) {
            cameraShaking.Shake(0.09f, 0.1f);
        }
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
    public override void RpcRespawn(Vector3 pos) {
        if(isLocalPlayer) {
            currentHealth = MAX_HEALTH;

            RpcOnHealthChanged(currentHealth);

            transform.position = pos;
        }
    }
}
