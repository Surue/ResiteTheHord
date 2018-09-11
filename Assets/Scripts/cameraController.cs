using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour {
    
    public GameObject focusedObject;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
	    if (focusedObject) {
            transform.position = Vector2.Lerp(transform.position, focusedObject.transform.up * 3 + focusedObject.transform.position, Time.deltaTime * 6.5f);
            transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
	}
}
