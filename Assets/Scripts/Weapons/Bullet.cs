using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        RpcDestroy();

        GameObject instance = Instantiate(explosionParticle).gameObject;
        instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity * 0.01f;

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = bulletColor;

        if(!isClient && isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    //[ClientRpc]
    //void RpcInsantiateExplosionParticles() {
    //    GameObject instance = Instantiate(explosionParticle).gameObject;
    //    instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity * 0.04f;

    //    ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
    //    main.startColor = bulletColor;

    //    Destroy(instance, 0.3f);
    //}

    [ClientRpc]
    void RpcDestroy() {
        GameObject instance = Instantiate(explosionParticle).gameObject;
        instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity * 0.04f;

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = bulletColor;

        Destroy(instance, 0.3f);

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }
}
