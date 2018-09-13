using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField]
    ParticleSystem explosionParticle;

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
        instance.transform.position = transform.position;
        
        Destroy(instance, 0.2f);

        Destroy(gameObject);
    }
}
