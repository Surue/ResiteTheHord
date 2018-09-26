using System;
using System.Collections.Generic;
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

    public GameObject ghostBulletPrefab;
    List<GameObject> ghostsBullet = new List<GameObject>();

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
        color = c;
        GetComponentInChildren<SpriteRenderer>().color = c;
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer && isClient) {
	        transform.eulerAngles = nextNetworkedTransform.GetRotationEulerAngle();
	        transform.position = Vector2.Lerp(transform.position, nextNetworkedTransform.GetPosition(), Time.deltaTime * speed);
	        return;
	    }

        if (!isLocalPlayer && isServer) {
            return;
        } 

        Vector3 topLeft = new Vector2(-0.5f,0.5f);
        Vector3 topRight = new Vector2(0.5f,0.5f);
        Vector3 bottomLeft = new Vector2(-0.5f,-0.5f);
        Vector3 bottomRight = new Vector2(0.5f,-0.5f);

        Vector3 offsetInterpolation = nextNetworkedTransform.GetPosition();

        Debug.DrawLine(offsetInterpolation + topLeft, offsetInterpolation + topRight);
        Debug.DrawLine(offsetInterpolation + topRight, offsetInterpolation + bottomRight);
        Debug.DrawLine(offsetInterpolation + bottomRight, offsetInterpolation + bottomLeft);
        Debug.DrawLine(offsetInterpolation + bottomLeft, offsetInterpolation + topLeft);

        //Movement
	    float x = Input.GetAxis("Horizontal");
	    float y = Input.GetAxis("Vertical");
        
        movement = new Vector3(x, y);

        //Rotation
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position ).normalized;
	    transform.up = direction;

        if (Input.GetButton("Fire1") && timeSinceLastFire <= 0) {
            timeSinceLastFire = timeBetweenFire;

            Vector3 offset = Random.Range(-0.1f, 0.1f) * bulletSpawn.right;

            CmdFire(bulletSpawn.position + offset, bulletSpawn.rotation, CustomNetworkManager.singleton.client.GetRTT(), GetComponent<NetworkIdentity>());
            SpawnGhost(bulletSpawn.position + offset, bulletSpawn.rotation);
	    }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Time.timeScale = 0.5f;
        }

	    timeSinceLastFire -= Time.deltaTime;
	}

    void SpawnGhost(Vector3 pos, Quaternion rot) {
        GameObject bullet = Instantiate(ghostBulletPrefab, pos, rot);

        Vector2 vel = bullet.transform.up * bulletSpeed; //Compute velocity
        bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

        ghostsBullet.Add(bullet);

        bullet.GetComponent<BulletGhost>().Initialize(this);
    }

    [Command]
    void CmdFire(Vector3 pos, Quaternion rot, float time, NetworkIdentity id) {
        GameObject bullet = Instantiate(bulletPrefab, pos, rot);    

        Vector2 vel = bullet.transform.up * bulletSpeed; //Compute velocity

        bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

        bullet.GetComponent<Bullet>().Initialize(this); //Setup color information

        NetworkServer.Spawn(bullet);

        bullet.GetComponent<Bullet>().RpcCompensatePosition(time, vel); //Compensate position on client

        //Compensate position on server
        bullet.transform.position += ((Vector3)vel * (time / 2000f)); //Compensate position on server

        Destroy(bullet, 2f);

        TargetDestroyGhostBullet(id.connectionToClient);
    }

    [TargetRpc]
    public void TargetDestroyGhostBullet(NetworkConnection conn) {
        Destroy(ghostsBullet[0]);
        ghostsBullet.RemoveAt(0);
    }

    [Command]
    public void CmdOnDeath() {
        FindObjectOfType<GameManager>().OnPlayerDeath(gameObject);

        transform.position = new Vector3(transform.position.x, transform.position.y, 10);
    }

    public override void OnStartLocalPlayer() {
        
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
