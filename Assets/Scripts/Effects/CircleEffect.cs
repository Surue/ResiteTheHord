using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleEffect : MonoBehaviour {

    float speed = 0;

    static int ORDER_IN_LAYER = 0;

    public void Initialize(Vector3 pos, float s, Color c) {
        transform.localScale = Vector3.zero;
        transform.position = pos;

        speed = s;

        GetComponent<SpriteRenderer>().color = c;
        GetComponent<SpriteRenderer>().sortingOrder = ORDER_IN_LAYER;

        ORDER_IN_LAYER++;
    }

    void Update() {
        transform.localScale += Vector3.one * speed * Time.deltaTime;

        if (transform.localScale.x > speed * 100) {
            gameObject.SetActive(false);
        }
    }
}
