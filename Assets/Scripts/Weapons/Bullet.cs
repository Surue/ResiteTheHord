using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {
    [SerializeField]
    ParticleSystem explosionParticle;

    [SyncVar(hook = "OnColorChanged")]
    [SerializeField] Color bulletColor;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] SpriteRenderer spriteLight;

    [SerializeField] TrailRenderer trail;

    PlayerController owner;

    void Start() {
        sprite.color = bulletColor;
        spriteLight.color = bulletColor;
        trail.startColor = bulletColor;
        trail.endColor = bulletColor;
    }

    public void Initialize(PlayerController id) {
        owner = id;

        bulletColor = id.GetColor();
    }

    [Command]
    void CmdSetColor(Color c) {
        bulletColor = c;
    }

    void OnColorChanged(Color c) {
        bulletColor = c;

        sprite.color = c;
        spriteLight.color = c;
        trail.startColor = c;
        trail.endColor = c;
    }

    [Server]
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            return;
        }
        
        GameObject hit = other.gameObject;
        Health health = hit.GetComponent<Health>();

        if (health != null) {
            health.TakeDamage(1);
        }

        EnemyController enemy = hit.GetComponent<EnemyController>();

        if (enemy != null) {
            enemy.TakeDamage(1, owner);
        }

        RpcDestroy(transform.position);

        if(!isClient && isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    void RpcDestroy(Vector2 pos) {
        transform.position = pos;
        GameObject instance = Instantiate(explosionParticle).gameObject;
        instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity.normalized * 0.04f;

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = bulletColor;

        Destroy(instance, 0.3f);

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void RpcCompensatePosition(float time, Vector2 vel) {
        GetComponent<Rigidbody2D>().velocity = vel;
        transform.position += (Vector3)(vel * (time / 2000f + CustomNetworkManager.singleton.client.GetRTT() / 2000f));
    }
}
