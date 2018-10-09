using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;
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

    #region Transform extrapolation

    Vector3[] lastPositions = new Vector3[10];
    float[] lastPositionsTimes = new float[10];
    Vector3 extrapolatedPosition = new Vector3();

    #endregion

    #region Variables
    //Visual
    [SyncVar(hook = "OnColorChanged")] Color color = Color.black;
    CameraShaking cameraShaking;

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

	        PlayerInfo tmp = FindObjectOfType<PlayerInfo>();

	        cameraShaking = FindObjectOfType<CameraShaking>();

	        if (tmp) {
	            CmdSetColor(tmp.GetColor());
	            CmdSetName(tmp.GetName());
            }
	    }

	    body = GetComponent<Rigidbody2D>();

        for (int i = 0; i < 10; i++) {
            lastPositions[i] = transform.position;
        }
	}

    void FixedUpdate() {
        if (!isLocalPlayer) {
            return;
        }

        movement = new Vector3(movement.x * speed, movement.y * speed);

        body.velocity = movement;

        CmdSetTransform(GetCurrentNetworkedTransform(), CustomNetworkManager.singleton.client.GetRTT());
    }

    public NetworkedTransform GetCurrentNetworkedTransform() {
        return new NetworkedTransform {
            rotation = transform.eulerAngles.z,
            posX = transform.position.x,
            posY = transform.position.y
        };
    }

    [Command]
    public void CmdSetTransform(NetworkedTransform networkedTransform, float time) {
        RpcSetTransform(networkedTransform, time);
    }

    [ClientRpc]
    public void RpcSetTransform(NetworkedTransform networkedTransform, float time) {

        nextNetworkedTransform = networkedTransform;

        extrapolatedPosition = networkedTransform.GetPosition();
        return;

        if (Time.time == lastPositionsTimes[0]) {
            return;
        }
        nextNetworkedTransform = networkedTransform;

        //Extrapolation
        Vector3[] tmpLastPosition = new Vector3[10];
        for (int i = 0; i < 10; i++) {
            tmpLastPosition[i] = lastPositions[i];
        }

        lastPositions[0] = networkedTransform.GetPosition();

        for (int i = 0; i < 9; i++) {
            lastPositions[i + 1] = tmpLastPosition[i];
        }

        //Time 
        float[] tmpLastPositionTimes = new float[10];
        for(int i = 0;i < 10;i++) {
            tmpLastPositionTimes[i] = lastPositionsTimes[i];
        }

        lastPositionsTimes[0] = Time.time;

        for(int i = 0;i < 9;i++) {
            lastPositionsTimes[i + 1] = tmpLastPositionTimes[i];
        }

        //Compute extrapolatedPosition
        int limit = 3;
        float[] t = new float[limit];
        for(int i = 0;i < limit;i++) {
            t[i] = lastPositionsTimes[i];
        }


        float[] x = new float[limit];
        for (int i = 0; i < limit; i++) {
            x[i] = lastPositions[i].x;
        }

        float[] y = new float[limit];
        for(int i = 0;i < limit;i++) {
            y[i] = lastPositions[i].y;
        }

        Vector2[] controlsPointX = new Vector2[limit];
        for (int i = 0; i < limit; i++) {
            controlsPointX[i] = new Vector2(lastPositionsTimes[i], lastPositions[i].x);
        }

        Vector2[] controlsPointY = new Vector2[limit];
        for(int i = 0;i < limit;i++) {
            controlsPointY[i] = new Vector2(lastPositionsTimes[i], lastPositions[i].y);
        }

        float ti = lastPositionsTimes[0] + time / 2000f + CustomNetworkManager.singleton.client.GetRTT() / 2000f +
                  (lastPositionsTimes[0] - lastPositionsTimes[1]);

        Debug.Log("time =================>");
        Debug.Log("Time.time : " + Time.time);
        Debug.Log("Time : " + time / 2000f);
        Debug.Log("ping : " + CustomNetworkManager.singleton.client.GetRTT() / 2000f);
        Debug.Log("lastTime[0] : " + lastPositionsTimes[0]);
        Debug.Log("ti : " + ti);
        Debug.Log("<================ time");


        if (lastPositions[0] != lastPositions[1]) {
            extrapolatedPosition = new Vector2(Extrapolation.InterpolateX(t, x, ti),
                Extrapolation.InterpolateX(t, y, ti));
        }


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
        Vector3 offsetExtrapolation = extrapolatedPosition;
        Vector3 offsetInterpolation = nextNetworkedTransform.GetPosition();

        if (!isLocalPlayer && isClient) {
	        transform.eulerAngles = nextNetworkedTransform.GetRotationEulerAngle();
	        transform.position = Vector2.Lerp(transform.position, offsetExtrapolation, Time.deltaTime * speed);
	        return;
	    }

        if (!isLocalPlayer) {
            return;
        } 

        Vector3 topLeft = new Vector2(-0.5f,0.5f);
        Vector3 topRight = new Vector2(0.5f,0.5f);
        Vector3 bottomLeft = new Vector2(-0.5f,-0.5f);
        Vector3 bottomRight = new Vector2(0.5f,-0.5f);

        Debug.DrawLine(offsetInterpolation + topLeft, offsetInterpolation + topRight, Color.blue);
        Debug.DrawLine(offsetInterpolation + topRight, offsetInterpolation + bottomRight, Color.blue);
        Debug.DrawLine(offsetInterpolation + bottomRight, offsetInterpolation + bottomLeft, Color.blue);
        Debug.DrawLine(offsetInterpolation + bottomLeft, offsetInterpolation + topLeft, Color.blue);
        
        Debug.DrawLine(offsetExtrapolation + topLeft, offsetExtrapolation + topRight, Color.red);
        Debug.DrawLine(offsetExtrapolation + topRight, offsetExtrapolation + bottomRight, Color.red);
        Debug.DrawLine(offsetExtrapolation + bottomRight, offsetExtrapolation + bottomLeft, Color.red);
        Debug.DrawLine(offsetExtrapolation + bottomLeft, offsetExtrapolation + topLeft, Color.red);

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

            CmdFire(bulletSpawn.position + offset, bulletSpawn.rotation, NetworkTransport.GetNetworkTimestamp(), GetComponent<NetworkIdentity>());
            SpawnGhost(bulletSpawn.position + offset, bulletSpawn.rotation);

            cameraShaking.Shake(0.005f, timeBetweenFire);
	    }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            FindObjectOfType<MainMenuController>().ToggleMenuPause();
        }

        timeSinceLastFire -= Time.deltaTime;
	}

    void SpawnGhost(Vector3 pos, Quaternion rot, GameObject target = null) {
        GameObject bullet = Instantiate(ghostBulletPrefab, pos, rot);

        Vector2 vel = bullet.transform.up * bulletSpeed; //Compute velocity
        bullet.GetComponent<Rigidbody2D>().velocity = vel; //Add velocity

        ghostsBullet.Add(bullet);
        
        bullet.GetComponent<BulletGhost>().Initialize(this, target);
    }

    [Command]
    void CmdFire(Vector3 pos, Quaternion rot, int time, NetworkIdentity id) {
        byte e;
        int delay;

        if(id.isLocalPlayer) {
            delay = (int)(Time.fixedDeltaTime * 1000);
        } else {
            delay = NetworkTransport.GetRemoteDelayTimeMS(connectionToClient.hostId, connectionToClient.connectionId, time, out e);
        }

        GameObject bullet = Instantiate(bulletPrefab, pos, rot);

        Vector2 vel = bullet.transform.up * bulletSpeed; //Compute velocity

        //Compensate position on server
        bullet.transform.position += ((Vector3)vel * ((float)delay / 1000f)); //Compensate position on server with no-collision

        LayerMask layerMask = ~((1 << LayerMask.NameToLayer("Bullet")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemy")));

        RaycastHit2D hit = Physics2D.Raycast(pos, bullet.transform.up, Vector2.Distance(pos, bullet.transform.position) + 0.2f, layerMask);

        if(hit) {
            TargetDestroyGhostBullet(id.connectionToClient);

            Destroy(bullet);
        } else {
            LayerMask layerMaskEnemy = 1 << LayerMask.NameToLayer("Enemy");

            RaycastHit2D hitEnemy = Physics2D.Raycast(pos, bullet.transform.up, Vector2.Distance(pos, bullet.transform.position) + 0.2f, layerMaskEnemy);
            if(hitEnemy) {
                bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

                bullet.GetComponent<Bullet>().Initialize(this); //Setup color information

                NetworkServer.Spawn(bullet);
                TargetDestroyGhostBullet(id.connectionToClient);
                Health health = hitEnemy.collider.GetComponent<Health>();

                if(health) {
                    health.TakeDamage(1, this);
                }

                bullet.GetComponent<Bullet>().CmdForceDestroy();
            } else {
                bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

                bullet.GetComponent<Bullet>().Initialize(this); //Setup color information

                NetworkServer.Spawn(bullet);

                bullet.GetComponent<Bullet>().RpcCompensatePosition(NetworkTransport.GetNetworkTimestamp(), vel); //Compensate position on client

                TargetDestroyGhostBullet(id.connectionToClient);
                RpcSpawnGhostBullet(bullet);
            }
        }
    }

    [ClientRpc]
    void RpcDebug(string s) {
        Debug.Log(s);
    }

    [ClientRpc]
    public void RpcSpawnGhostBullet(GameObject bullet) {
        if (!isLocalPlayer) {
            SpawnGhost(bulletSpawn.position, bulletSpawn.rotation, bullet);
        }
    }

    [TargetRpc]
    public void TargetDestroyGhostBullet(NetworkConnection conn) {
        Destroy(ghostsBullet[0], 0.1f);
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
