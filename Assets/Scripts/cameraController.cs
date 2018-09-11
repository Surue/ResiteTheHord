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
	        Vector2 newAimPoint = transform.position;

	        Vector2 lerpedAimPoint = Vector2.Lerp(transform.position, focusedObject.transform.up * 3 + focusedObject.transform.position, Time.deltaTime * 6.5f);
            Debug.Log(transform.position);
            Debug.Log(focusedObject.transform.up * 3 + focusedObject.transform.position);
            Debug.Log("================");

	        transform.position = lerpedAimPoint;
	        transform.position = new Vector3(transform.position.x, transform.position.y, -10);

	        aimPoint = focusedObject.transform.up * 3;
        }
	}
}
