using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : Health {

    //Sounds
    PlayerSounds soundsController;

    public RectTransform healthBar;

    CameraShaking cameraShaking;
    
    // Use this for initialization
    void Awake() {
        soundsController = GetComponent<PlayerSounds>();
    }

    void Start () {
        cameraShaking = FindObjectOfType<CameraShaking>();
    }

    [Server]
    public override void TakeDamage(int damage, PlayerController bulletOwner = null) {
        currentHealth -= damage;

        if(currentHealth <= 0) {
            if(destroyOnDeath) {
                RpcDestroy();

                Score score = GetComponent<Score>();
                if(score != null && bulletOwner != null) {
                    bulletOwner.CmdAddScore(score.transform.position, score.score);
                }
            } else {
                FindObjectOfType<GameManager>().OnPlayerDeath(this.gameObject);
            }
        } else {
            RpcOnHealthChanged(currentHealth);
        }
    }

    [ClientRpc]
    public override void RpcOnHealthChanged(int health) {
        currentHealth = health;

        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);

        if (isLocalPlayer) {
            soundsController.Damage();
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
        soundsController.Spawn();

        GameObject instance = Instantiate(spawnParticleSystem, pos, Quaternion.identity).gameObject;

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = GetComponent<PlayerController>().GetColor();

        if(isLocalPlayer) {
            currentHealth = MAX_HEALTH;

            healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);

            transform.position = pos;
        }
    }
}
