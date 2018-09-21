using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {
    [SerializeField]
    ParticleSystem explosionParticle;

    [SerializeField] Color bulletColor;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] SpriteRenderer spriteLight;

    [SerializeField] TrailRenderer trail;

    void Start() {
        sprite.color = bulletColor;
        spriteLight.color = bulletColor;
        trail.startColor = bulletColor;
        trail.endColor = bulletColor;
    }
    
    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            return;
        }
        
        GameObject hit = other.gameObject;
        Health health = hit.GetComponent<Health>();

        if (health != null) {
            health.TakeDamage(10);
        }

        GameObject instance = Instantiate(explosionParticle).gameObject;
        instance.transform.position = (Vector2)transform.position - GetComponent<Rigidbody2D>().velocity * 0.05f;

        ParticleSystem.MainModule main = instance.GetComponent<ParticleSystem>().main;
        main.startColor = bulletColor;

        Destroy(instance, 0.3f);

        Destroy(gameObject);
    }
}
