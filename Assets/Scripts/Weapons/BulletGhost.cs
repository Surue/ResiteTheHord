using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGhost : MonoBehaviour {

    [SerializeField] Color bulletColor;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] SpriteRenderer spriteLight;

    [SerializeField] TrailRenderer trail;

    Bullet targetBullet ;

    bool hadTarget = false;

    float t = 0;

    void Update() {
        if (targetBullet != null) {
            transform.position = Vector2.Lerp(transform.position, targetBullet.transform.position, t);

            if (Vector2.Distance(transform.position, targetBullet.transform.position) < 0.05f) {
                Destroy(gameObject);
                targetBullet.Show();
            }

            t += 0.15f;
        } else {
            if (hadTarget) {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(PlayerController owner, GameObject target = null) {
        bulletColor = owner.GetColor();

        if (target) {
            hadTarget = true;
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
