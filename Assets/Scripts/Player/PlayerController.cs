using System;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour {

    //Visual
    [SyncVar(hook = "OnColorChanged")] Color color = Color.black;

    //Weapon
    [Header("Weapon")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    [SerializeField] float timeBetweenFire = 0.2f;
    float timeSinceLastFire = 0;
    [SerializeField] float bulletSpeed = 10;

    //Movement
    [Header("Movement")]
    Rigidbody2D body;
    Vector3 movement;
    [SerializeField] float speed = 4f;

    //Score
    [SyncVar]
    int score = 0;
    [SyncVar]
    string username = "";

	//Use this for initialization
	void Start () {
	    GetComponentInChildren<SpriteRenderer>().color = color;

	    if (isLocalPlayer) {
	        FindObjectOfType<cameraController>().focusedObject = gameObject;
	        CmdSetColor(FindObjectOfType<PlayerInfoController>().GetColor());
            CmdSetName(FindObjectOfType<PlayerInfoController>().GetName());
	    }

	    body = GetComponent<Rigidbody2D>();
	}

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        movement = new Vector3(movement.x * speed, movement.y * speed);

        body.velocity = movement;
    }

    [Command]
    public void CmdSetColor(Color c) {
        color = c;
    }

    [Command]
    public void CmdSetName(string n) {
        username = n;
    }

    void OnColorChanged(Color c) {
        GetComponentInChildren<SpriteRenderer>().color = c;
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

        bullet.GetComponent<Bullet>().Initialize(this);

        NetworkServer.Spawn(bullet);

        Destroy(bullet, 2f);
    }

    [Command]
    public void CmdOnDeath() {
        FindObjectOfType<GameManager>().OnPlayerDeath(gameObject);

        transform.position = new Vector3(transform.position.x, transform.position.y, 10);
    }

    public override void OnStartLocalPlayer() {
        GetComponentInChildren<SpriteRenderer>().color = Color.blue;
    }

    [Command]
    public void CmdAddScore(Vector3 pos, int score) {
        this.score += score;
        ScoreController.Instance.TargetDisplayScore(connectionToClient, pos, score);
    }

    public int GetScore() {
        return score;
    }

    public string GetName() {
        return username;
    }

    public Color GetColor() {
        return color;
    }
}
