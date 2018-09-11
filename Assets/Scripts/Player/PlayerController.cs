using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    //Weapon
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    //Movement
    Rigidbody body;
    Vector3 movement;

	// Use this for initialization
	void Start () {
        if(isLocalPlayer)
	    FindObjectOfType<cameraController>().focusedObject = gameObject;

	    body = GetComponent<Rigidbody>();
	}

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        movement = new Vector3(movement.x * 3, body.velocity.y, movement.z * 3);

        body.velocity = movement;
    }

    // Update is called once per frame
	void Update () {
	    if (!isLocalPlayer) {
	        return;
	    }

	    float x = Input.GetAxis("Horizontal");
	    float z = Input.GetAxis("Vertical");
        
        movement = new Vector3(x, 0, z);

	    if (Input.GetButton("Fire1")) {
	        CmdFire();
	    }
	}

    [Command]
    void CmdFire() {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, 2f);
    }

    public override void OnStartLocalPlayer() {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
