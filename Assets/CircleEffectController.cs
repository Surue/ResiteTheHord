using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleEffectController : MonoBehaviour {

    Color activeColor = Color.blue;

    // Use this for initialization
    void Start() {

        StartCoroutine(VisualEffect());
    }

    IEnumerator VisualEffect() {
        yield return new WaitForEndOfFrame();

        while (true) {
            GameObject circle = ObjectsPooler.instance.GetPooledObjects();

            if(circle != null) {
                circle.transform.position = Vector2.zero;
                circle.transform.rotation = Quaternion.identity;
                circle.SetActive(true);

                circle.GetComponent<CircleEffect>().Initialize(transform.position, Mathf.PingPong(Time.time, 4) + 4, activeColor);

                activeColor = new Color((activeColor.r + Mathf.PerlinNoise(Time.deltaTime, Time.deltaTime) * Random.Range(0, 5) / 5f) % 1,
                    (activeColor.g + Mathf.PerlinNoise(Time.deltaTime, Time.deltaTime) * Random.Range(0, 5) / 5f) % 1,
                    (activeColor.b + Mathf.PerlinNoise(Time.deltaTime, Time.deltaTime) * Random.Range(0, 5) / 5f) % 1);

            }
            else {
                Debug.Log("EMPTY");
            }

            yield return new WaitForSeconds(4);
        }
    }
}
