using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour {

    public Vector3 positionOffset;
    public GameObject focusedObject;
    Vector3 aimPoint = Vector3.zero;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (focusedObject) {
	        Vector3 newAimPoint = focusedObject.transform.forward * 3;

	        Vector3 lerpedAimPoint = Vector3.Lerp(aimPoint, newAimPoint, Time.deltaTime * 7.5f);

            transform.position = focusedObject.transform.position + positionOffset + lerpedAimPoint;
	        transform.LookAt(focusedObject.transform.position + lerpedAimPoint);

	        aimPoint = lerpedAimPoint;
	    }
	}
}
