using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class CameraShaking : MonoBehaviour {

    public Camera mainCamera;

    float shakeAmount = 0;

    void Awake() {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Shake(0.1f, 0.2f);
        }
    }

    public void Shake(float amount, float length) {
        shakeAmount = amount;
        InvokeRepeating("DoShake", 0, 0.01f);
        Invoke("StopShake", length);
    }

    void DoShake() {
        if (shakeAmount > 0) {
            float offsetAmountX = Random.value * shakeAmount * 2 - shakeAmount;
            float offsetAmountY = Random.value * shakeAmount * 2 - shakeAmount;

            Vector3 camPos = mainCamera.transform.position;

            camPos.x += offsetAmountX;
            camPos.y += offsetAmountY;

            mainCamera.transform.position = camPos;
        }
    }

    void StopShake() {
        CancelInvoke("DoShake");
        mainCamera.transform.localPosition = Vector3.zero;
    }
}
