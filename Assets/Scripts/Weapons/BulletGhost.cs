using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGhost : MonoBehaviour {

    [SerializeField] Color bulletColor;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] SpriteRenderer spriteLight;

    [SerializeField] TrailRenderer trail;

    public void Initialize(PlayerController owner) {
        bulletColor = owner.GetColor();

        sprite.color = bulletColor;
        spriteLight.color = bulletColor;
        trail.startColor = bulletColor;
        trail.endColor = bulletColor;
    }
}
