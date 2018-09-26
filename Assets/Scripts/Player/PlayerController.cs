using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour {
    #region Transform interpolation
    public struct NetworkedTransform {
        public float posX;
        public float posY;
        public float rotation;

        public Vector3 GetPosition() {
            return new Vector3(posX, posY, 0);
        }

        public Vector3 GetRotationEulerAngle() {
            return new Vector3(0, 0, rotation);
        }
    }

    NetworkedTransform nextNetworkedTransform;
    #endregion

    #region Variables
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

    //Time
    float timer = 0;
    #endregion

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
        timer += Time.fixedDeltaTime;

        if (!isLocalPlayer) {
            return;
        }

        movement = new Vector3(movement.x * speed, movement.y * speed);

        body.velocity = movement;

        CmdSetTransform(GetCurrentNetworkedTransform());
    }

    public NetworkedTransform GetCurrentNetworkedTransform() {
        return new NetworkedTransform {
            rotation = transform.eulerAngles.z,
            posX = transform.position.x,
            posY = transform.position.y
        };
    }

    [Command]
    public void CmdSetTransform(NetworkedTransform networkedTransform) {
        RpcSetTransform(networkedTransform);
    }

    [ClientRpc]
    public void RpcSetTransform(NetworkedTransform networkedTransform) {
        nextNetworkedTransform = networkedTransform;
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

	    if (!isLocalPlayer && isClient) {
	        transform.eulerAngles = nextNetworkedTransform.GetRotationEulerAngle();
	        transform.position = Vector2.Lerp(transform.position, nextNetworkedTransform.GetPosition(), Time.deltaTime * speed * 20);

	        return;
	    }

        if (!isLocalPlayer && isServer) {
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
	        CmdFire(bulletSpawn.position, bulletSpawn.rotation, CustomNetworkManager.singleton.client.GetRTT());
	    }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Time.timeScale = 0.5f;
        }

	    timeSinceLastFire -= Time.deltaTime;
	}

    [Command]
    void CmdFire(Vector3 pos, Quaternion rot, float time) {
        GameObject bullet = Instantiate(bulletPrefab, pos, rot);

        bullet.transform.position += Random.Range(-0.1f, 0.1f) * bulletSpawn.right; //Lateral offset        

        Vector2 vel = bullet.transform.up * bulletSpeed; //Compute velocity

        bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

        bullet.GetComponent<Bullet>().Initialize(this); //Setup color information

        NetworkServer.Spawn(bullet);

        bullet.GetComponent<Bullet>().RpcCompensatePosition(time, vel); //Compensate position on client

        //Compensate position on server
        bullet.transform.position += ((Vector3)vel * (time / 2000f)); //Compensate position on server

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
