﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    //Weapon
    [Header("Weapon")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    [SerializeField] float timeBetweenFire = 0.1f;
    float timeSinceLastFire = 0;
    [SerializeField] float bulletSpeed = 10;

    //Movement
    [Header("Movement")]
    Rigidbody2D body;
    Vector3 movement;
    [SerializeField] float speed = 4f;

	// Use this for initialization
	void Start () {
        if(isLocalPlayer)
	    FindObjectOfType<cameraController>().focusedObject = gameObject;

	    body = GetComponent<Rigidbody2D>();
	}

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        movement = new Vector3(movement.x * speed, movement.y * speed);

        body.velocity = movement;
    }

    // Update is called once per frame
	void Update () {
	    if (!isLocalPlayer) {
	        return;
	    }

        //Movement
	    float x = Input.GetAxis("Horizontal");
	    float y = Input.GetAxis("Vertical");
        
        movement = new Vector3(x, y);

        //Rotation
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position ).normalized;
	    transform.up = direction;

        if (Input.GetButton("Fire1") && timeSinceLastFire <= 0) {
            timeSinceLastFire = timeBetweenFire;
	        CmdFire();
	    }

	    timeSinceLastFire -= Time.deltaTime;
	}

    [Command]
    void CmdFire() {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        bullet.transform.position += Random.Range(-0.1f, 0.1f) * bulletSpawn.right;

        bullet.GetComponentInChildren<Rigidbody2D>().velocity = bullet.transform.up * bulletSpeed;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, 2f);
    }

    public override void OnStartLocalPlayer() {
        GetComponentInChildren<SpriteRenderer>().color = Color.blue;
    }
}