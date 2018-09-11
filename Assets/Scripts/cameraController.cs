using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour {
    
    public GameObject focusedObject;
    Vector2 aimPoint = Vector2.zero;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (focusedObject) {
	        Vector2 newAimPoint = focusedObject.transform.up * 3;

	        Vector2 lerpedAimPoint = Vector3.Lerp(aimPoint, newAimPoint, Time.deltaTime * 6.5f);

	        transform.position = (Vector2) focusedObject.transform.position + lerpedAimPoint;
	        transform.position = new Vector3(transform.position.x, transform.position.y, -10);

	        aimPoint = lerpedAimPoint;
	    }
	}
}
