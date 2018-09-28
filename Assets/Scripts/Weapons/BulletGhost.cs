using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGhost : MonoBehaviour {

    [SerializeField] Color bulletColor;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] SpriteRenderer spriteLight;

    [SerializeField] TrailRenderer trail;

    Bullet targetBullet ;

    Rigidbody2D body;

    float t = 0;

    void Start() {
        body = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (targetBullet != null) {
            transform.position = Vector2.Lerp(transform.position, targetBullet.transform.position, t);

            if (Vector2.Distance(transform.position, targetBullet.transform.position) < 0.05f) {
                Destroy(gameObject);
                targetBullet.Show();
            }

            t += 0.1f;
        }
    }

    public void Initialize(PlayerController owner, GameObject target = null) {
        bulletColor = owner.GetColor();

        if (target) {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            targetBullet = target.GetComponent<Bullet>();
            targetBullet.Hide();
        }

        sprite.color = bulletColor;
        spriteLight.color = bulletColor;
        trail.startColor = bulletColor;
        trail.endColor = bulletColor;
    }
}
