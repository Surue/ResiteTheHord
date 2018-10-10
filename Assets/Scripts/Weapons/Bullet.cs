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

    [SerializeField] LayerMask layerToIgnore;

    PlayerController owner;

    void Start() {
        sprite.color = bulletColor;
        spriteLight.color = bulletColor;
        trail.startColor = bulletColor;
        trail.endColor = bulletColor;
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 1 * transform.localScale.x);
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
    
    void OnTriggerEnter2D(Collider2D other) {
        if (!isServer) {
            return;
        }

        if(((1 << other.gameObject.layer) & layerToIgnore) != 0) {
            return;
        }

        GameObject hit = other.gameObject;
        Health health = hit.GetComponent<Health>();

        if (health != null) {
            health.TakeDamage(1);
        }

        EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

        if (enemy != null) {
            enemy.TakeDamage(1, owner);
        }

        RpcDestroy(transform.position);

        //if(!isClient && isServer) {
        //    NetworkServer.Destroy(gameObject);
        //}
    }

    [Command]
    public void CmdForceDestroy() {
        RpcDestroy(transform.position);
    }

    [ClientRpc]
    void RpcDestroy(Vector2 pos) {
        transform.position = pos;
        GameObject instance = Instantiate(explosionParticle).gameObject;
        instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity.normalized * 0.04f;

        Destroy(instance, 1f);

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = bulletColor;

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void RpcCompensatePosition(int time, Vector2 vel) {
        GetComponent<Rigidbody2D>().velocity = vel;

        byte e;
        int delay;

        NetworkConnection conn = CustomNetworkManager.singleton.client.connection;

        if(!isServer) {
            delay = NetworkTransport.GetRemoteDelayTimeMS(conn.hostId, conn.connectionId, time, out e);
        } else {
            delay = 0;
        }
        
        transform.position += (Vector3)(vel * (delay / 1000f));
    }

    public PlayerController GetOwner() {
        return owner;
    }

    public void Hide() {
        sprite.gameObject.SetActive(false);

        trail.gameObject.SetActive(false);
    }

    public void Show() {
        sprite.gameObject.SetActive(true);

        trail.gameObject.SetActive(true);
    }
}
