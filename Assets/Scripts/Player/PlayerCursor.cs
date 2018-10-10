using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursor : MonoBehaviour {

    [SerializeField] AnimationCurve inCurve;

    float animationPoint = 0;

    float initialScale;

	// Use this for initialization
	void Start () {
	    Cursor.visible = false;

	    initialScale = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
	    transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        transform.localRotation = Quaternion.Inverse(transform.parent.rotation);

	    if (Input.GetButton("Fire1")) {
	        animationPoint = Mathf.Clamp01(animationPoint + Time.deltaTime * 2);
	        transform.localScale = Vector3.one * initialScale * inCurve.Evaluate(animationPoint) ;
        }
	    else {
	        animationPoint = Mathf.Clamp01(animationPoint - Time.deltaTime * 2);
            transform.localScale = Vector3.one * initialScale * inCurve.Evaluate(animationPoint) ;
        }
	}

    void OnDestroy() {
        Cursor.visible = true;
    }
}
