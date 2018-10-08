using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyMovement : NetworkBehaviour {

    #region Transform interpolation
    public struct NetworkedTransform {
        public float posX;
        public float posY;

        public Vector3 GetPosition() {
            return new Vector3(posX, posY, 0);
        }
    }

    NetworkedTransform nextNetworkedTransform;

    Vector3 interpollatedPosition = new Vector3();
    Vector3 extrapolatedPosition = new Vector3();
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] float speed = 2f;
    [SerializeField] float avoidingRadius = 2f;

    List<Vector2> path = new List<Vector2>();

    float timeBetweenPathChecking = 2f;
    float timerCheckPath = 0f;

    Vector2 targetPosition;
    Transform mainGoal;

    PathFinding pathFinder;
    Rigidbody2D body;

    float timerStop = 0f;
    #endregion

    #region State
    enum State {
        INITIALIZE,
        IDLE,
        MOVE_TO_BASE,
        MOVE_TO_TARGET,
        STOPPED
    }

    State state = State.INITIALIZE;
    #endregion 

    #region Debug
    [Header("Debug")]
    [SerializeField]bool showSyncPosition = false;
    #endregion

    // Use this for initialization
    void Start () {
        body = GetComponent<Rigidbody2D>();
        pathFinder = FindObjectOfType<PathFinding>();
	}

    public static Vector2 Rotate(Vector2 v, float degrees) {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return new Vector2(v.x, v.y);
    }

    void FixedUpdate() {
        if(isServer) {
            body.velocity = (targetPosition - (Vector2)transform.position).normalized * speed;

            RaycastHit2D[] hist = Physics2D.RaycastAll(transform.position, body.velocity.normalized, avoidingRadius);
            foreach (RaycastHit2D raycastHit2D in hist) {
                if (raycastHit2D.transform.root != transform.root) {

                    //RaycastHit2D[] tmp = Physics2D.RaycastAll(transform.position, Rotate(body.velocity, 90), avoidingRadius);
                    
                    //if (tmp.Length == 1) {
                    //    Debug.Log("90");
                    //    body.velocity = Rotate(body.velocity, 90) * speed;
                    //} else {
                    //    tmp = Physics2D.RaycastAll(transform.position, Rotate(body.velocity, -90), avoidingRadius);

                    //    if (tmp.Length == 1) {
                    //        body.velocity = Rotate(body.velocity, -90) * speed;
                    //        Debug.Log("-90");
                    //    } else {
                    //        body.velocity = Vector2.zero;
                    //        Debug.Log("0");
                    //    }
                    //}



                    //if(!Physics2D.Raycast(transform.position, Rotate(body.velocity, 90), avoidingRadius)) {
                    //    Debug.Log("ICI");
                    //    body.velocity = Rotate(body.velocity, 90) * speed;
                    //} else if(!Physics2D.Raycast(transform.position, Rotate(body.velocity, -90), avoidingRadius)) {
                    //    Debug.Log("Là");
                    //    body.velocity = Rotate(body.velocity, -90) * speed;
                    //} else {
                    //    Debug.Log("failed");
                    //    body.velocity = Vector2.zero;
                    //}
                }
            }

            //Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + (Vector3)body.velocity.normalized * 0.5f, avoidingRadius, 1 << LayerMask.NameToLayer("Enemy"));
            //Debug.Log(colliders.Length);

            //foreach(Collider2D d in colliders) {
            //    if(d.transform.root != this.transform.root) {
            //        float distance = Vector2.Distance(transform.position, d.transform.position);

            //        if(distance == 0) {
            //            distance = 0.1f;
            //        }

            //        body.velocity += (Vector2)(transform.position - d.transform.position).normalized *
            //                         (2f / distance) * speed;
            //    }
            //}

            //body.velocity = body.velocity.normalized * speed;

            if (state == State.STOPPED) {
                body.velocity = Vector2.zero;
            }

            CmdUpdatePosition();
        }
    }

    // Update is called once per frame
    void Update () {
	    if (showSyncPosition) {
	        Vector3 topLeft = new Vector2(-0.5f, 0.5f);
	        Vector3 topRight = new Vector2(0.5f, 0.5f);
	        Vector3 bottomLeft = new Vector2(-0.5f, -0.5f);
	        Vector3 bottomRight = new Vector2(0.5f, -0.5f);

	        Debug.DrawLine(interpollatedPosition + topLeft, interpollatedPosition + topRight, Color.blue);
	        Debug.DrawLine(interpollatedPosition + topRight, interpollatedPosition + bottomRight, Color.blue);
	        Debug.DrawLine(interpollatedPosition + bottomRight, interpollatedPosition + bottomLeft, Color.blue);
	        Debug.DrawLine(interpollatedPosition + bottomLeft, interpollatedPosition + topLeft, Color.blue);

	        Debug.DrawLine(extrapolatedPosition + topLeft, extrapolatedPosition + topRight, Color.red);
	        Debug.DrawLine(extrapolatedPosition + topRight, extrapolatedPosition + bottomRight, Color.red);
	        Debug.DrawLine(extrapolatedPosition + bottomRight, extrapolatedPosition + bottomLeft, Color.red);
	        Debug.DrawLine(extrapolatedPosition + bottomLeft, extrapolatedPosition + topLeft, Color.red);
	    }

	    if(isClient) {
	        transform.position = Vector2.Lerp(transform.position, extrapolatedPosition, Time.deltaTime * speed * 5);
        }

        if (!isServer) {
            return;
        }

        targetPosition = transform.position;

        switch(state) {
            case State.INITIALIZE:
                state = State.IDLE;
                break;

            case State.IDLE:
                path.Clear();
                path = pathFinder.GetPathFromTo(transform, mainGoal);
                state = State.MOVE_TO_BASE;
                break;

            case State.MOVE_TO_BASE:
                if(path == null || path.Count == 0) {
                    state = State.IDLE;
                } else {
                    if(Vector2.Distance((Vector3)transform.position, path[0]) < 0.1f) {
                        path.RemoveAt(0);

                        if(path.Count == 0) {
                            state = State.IDLE;
                        } else {
                            targetPosition = path[0];
                        }
                    } else {
                        targetPosition = path[0];
                    }
                }

                if(timerCheckPath > timeBetweenPathChecking) {
                    timerCheckPath = 0;
                    state = State.IDLE;
                } else {
                    timerCheckPath += Time.deltaTime;
                }
                break;

            case State.MOVE_TO_TARGET:
                break;

            case State.STOPPED:
                timerStop -= Time.deltaTime;

                if (timerStop <= 0) {
                    state = State.IDLE;
                    timerStop = 0;
                }
                break;

            default:
                break;
        }
    }

    [Command]
    void CmdUpdatePosition() {
        if(path == null || path.Count == 0 || state == State.STOPPED) {
            RpcUpdatePosition(transform.position, transform.position, NetworkTransport.GetNetworkTimestamp());
        } else {
            RpcUpdatePosition(transform.position, path[0], NetworkTransport.GetNetworkTimestamp());
        }
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 lastPosition, Vector3 nextPos, int time) {
        nextNetworkedTransform = new NetworkedTransform {
            posX = lastPosition.x,
            posY = lastPosition.y
        };

        interpollatedPosition = lastPosition;

        extrapolatedPosition = lastPosition;

        if (lastPosition == nextPos) {
            return;
        }

        NetworkConnection conn = CustomNetworkManager.singleton.client.connection;

        byte e;
        int delay;

        if(!isServer) {
            delay = NetworkTransport.GetRemoteDelayTimeMS(conn.hostId, conn.connectionId, time, out e);
        } else {
            delay = (int)(Time.fixedDeltaTime * 1000);
        }

        if(Mathf.Abs(lastPosition.x - nextPos.x) < 0.1f) { //Vertical movement
            if(lastPosition.y > nextPos.y) {
                extrapolatedPosition += Vector3.down * speed * (delay / 1000f);
            } else {
                extrapolatedPosition += Vector3.up * speed * (delay / 1000f);
            }
        } else if(Mathf.Abs(lastPosition.y - nextPos.y) < 0.1f) { //Horizontal movement
            if(lastPosition.x > nextPos.x) {
                extrapolatedPosition += Vector3.left * speed * (delay / 1000f);
            } else {
                extrapolatedPosition += Vector3.right * speed * (delay / 1000f);
            }
        } else {
            if(lastPosition.y > nextPos.y) {
                extrapolatedPosition += Vector3.down * speed * (delay / 1000f);
            } else {
                extrapolatedPosition += Vector3.up * speed * (delay / 1000f);
            }

            if(lastPosition.x > nextPos.x) {
                extrapolatedPosition += Vector3.left * speed * (delay / 1000f);
            } else {
                extrapolatedPosition += Vector3.right * speed * (delay / 1000f);
            }
        }
    }

    [Command]
    public void CmdGoToTarget() {

    }

    [Command]
    public void CmdAttack(float time) {
        timerStop = time;
        state = State.STOPPED;
        targetPosition = transform.position;
    }

    void OnDrawGizmos() {
        if (path == null) {
            return;
        }

        foreach(Vector2 pos in path) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y, 0), 0.1f);
        }
    }

    public void Initialize(Transform mainGoalPos) {
        mainGoal = mainGoalPos;
    }
}
