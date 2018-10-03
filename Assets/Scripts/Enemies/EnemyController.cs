using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyController : NetworkBehaviour {

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

    [Header("Movement")]
    [SerializeField] float speed = 2f;
    List<Vector2> path;

    static float TIME_CHECK_PATH = 1.5f;
    float timerCheckPath = 0f;

    static float TIME_CHECK_TARGET = 0.5f;
    float timerCheckTarget = 0f;

    [Header("Health")]
    public const int MAX_HEALTH = 100;
    [SerializeField] int lifePoint = 1;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] bool canMultipleAttack = false;
    List<GameObject> attackTargets = new List<GameObject>();

    [Header("Animation")]
    [SerializeField] float rotationSpeed = 10;
    [SerializeField] ParticleSystem attackParticleSystem;
    [SerializeField] ParticleSystem deathParticleSystem;
    Animator animator;
    bool isAnimated = false;

    [SyncVar]
    Vector2 targetPosition;

    enum State {
        INITIALIZE,
        IDLE,
        MOVE,
        MOVE_TO_PLAYER,
        ATTACK,
        ATTACK_ANIMATION
    }

    State state = State.INITIALIZE;

    Rigidbody2D body;

    PlayerController player;
    PathFinding pathFinder;

    // Use this for initialization
    void Start() {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if(!isServer) {
            return;
        }

        path = new List<Vector2>();

        StartCoroutine(FindAllObjects());
    }

    IEnumerator FindAllObjects() {
        while(player == null || pathFinder == null) {
            player = FindObjectOfType<PlayerController>();
            pathFinder = FindObjectOfType<PathFinding>();
            yield return null;
        }

        state = State.IDLE;
    }

    void FixedUpdate() {
        if (isServer) {
            body.velocity = (targetPosition - (Vector2)transform.position).normalized * speed;
            CmdUpdatePosition();
        }
    }

    [Command]
    void CmdUpdatePosition() {
        if (path == null || path.Count == 0) {
            RpcUpdatePosition(transform.position, transform.position);
        } else {
            RpcUpdatePosition(transform.position, path[0]);
        }
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 lastPosition, Vector3 nextPos) {
        nextNetworkedTransform = new NetworkedTransform {
            posX = lastPosition.x,
            posY = lastPosition.y
        };

        interpollatedPosition = lastPosition;

        extrapolatedPosition = lastPosition;
        
        if(Mathf.Abs(lastPosition.x - nextPos.x) < 0.1f) { //Vertical movement
            if(lastPosition.y > nextPos.y) {
                extrapolatedPosition += Vector3.down * speed * (Time.fixedDeltaTime + (CustomNetworkManager.singleton.client.GetRTT() / 2000f));
            } else {
                extrapolatedPosition += Vector3.up * speed * (Time.fixedDeltaTime + (CustomNetworkManager.singleton.client.GetRTT() / 2000f));
            }
        } else { //Horizontal movement
            if(lastPosition.x > nextPos.x) {
                extrapolatedPosition += Vector3.left * speed * (Time.fixedDeltaTime + (CustomNetworkManager.singleton.client.GetRTT() / 2000f));
            } else {
                extrapolatedPosition += Vector3.right * speed * (Time.fixedDeltaTime + (CustomNetworkManager.singleton.client.GetRTT() / 2000f));
            }
        }
    }

    void Update() {
        Vector3 topLeft = new Vector2(-0.5f,0.5f);
        Vector3 topRight = new Vector2(0.5f,0.5f);
        Vector3 bottomLeft = new Vector2(-0.5f,-0.5f);
        Vector3 bottomRight = new Vector2(0.5f,-0.5f);

        Debug.DrawLine(interpollatedPosition + topLeft, interpollatedPosition + topRight, Color.blue);
        Debug.DrawLine(interpollatedPosition + topRight, interpollatedPosition + bottomRight, Color.blue);
        Debug.DrawLine(interpollatedPosition + bottomRight, interpollatedPosition + bottomLeft, Color.blue);
        Debug.DrawLine(interpollatedPosition + bottomLeft, interpollatedPosition + topLeft, Color.blue);

        Debug.DrawLine(extrapolatedPosition + topLeft, extrapolatedPosition + topRight, Color.red);
        Debug.DrawLine(extrapolatedPosition + topRight, extrapolatedPosition + bottomRight, Color.red);
        Debug.DrawLine(extrapolatedPosition + bottomRight, extrapolatedPosition + bottomLeft, Color.red);
        Debug.DrawLine(extrapolatedPosition + bottomLeft, extrapolatedPosition + topLeft, Color.red);

        if (isClient) {
            transform.position = Vector2.Lerp(transform.position, extrapolatedPosition, Time.deltaTime * speed * 5);
        }

        if(isServer){
            targetPosition = transform.position;
            
            switch (state) {
                case State.INITIALIZE:
                    break;

                case State.IDLE:
                    path.Clear();
                    if (path.Count == 0 || path == null) {
                        GetPath();
                        state = State.MOVE;
                    }

                    break;

                case State.MOVE:
                    if (path == null || path.Count == 0) {
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

                    if (timerCheckPath > TIME_CHECK_PATH) {
                        timerCheckPath = 0;
                        GetPath();
                    } else {
                        timerCheckPath += Time.deltaTime;
                    }

                    break;

                case State.ATTACK:
                    body.velocity = Vector2.zero;
                    foreach (GameObject target in attackTargets) {
                        if (target.GetComponent<Health>()) {
                            target.GetComponent<Health>().TakeDamage(1);
                        }
                    }

                    path.Clear();

                    RpcStartAnimation();
                    state = State.ATTACK_ANIMATION;
                    isAnimated = true;
                    break;

                case State.ATTACK_ANIMATION:
                    if (!isAnimated) {
                        state = State.IDLE;
                    }
                    break;
            }

            if (timerCheckTarget > TIME_CHECK_TARGET && state != State.ATTACK) {
                timerCheckTarget = 0;
                if (CheckAttackTarget()) {
                    state = State.ATTACK;
                }
            } else {
                timerCheckTarget += Time.deltaTime;
            }
        }
    }

    [ClientRpc]
    void RpcDebug(string s) {
        Debug.Log(s);
    }

    void GetPath() {
        path = pathFinder.GetPathFromTo(transform, player.transform);
    }

    [ClientRpc]
    void RpcStartAnimation() {
        StartCoroutine(AnimationAttack());
    }
    
    IEnumerator AnimationAttack() {
        animator.SetTrigger("Attack");
        attackParticleSystem.Play();

        while (transform.eulerAngles.z < 360f / 3f) {
            transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + rotationSpeed);
            yield return new WaitForEndOfFrame();
        }

        transform.rotation = Quaternion.identity;

        isAnimated = false;
    }

    [Server]
    bool CheckAttackTarget() {
        Collider2D[] possibleTarget;
        LayerMask mask = 1 << LayerMask.NameToLayer("Player");

        possibleTarget = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);

        if(possibleTarget.Length > 0) {
            if(canMultipleAttack) {
                for(int i = 0;i < possibleTarget.Length;i++) {
                    attackTargets.Add(possibleTarget[i].gameObject);
                }

                return true;
            } else {
                int indexMin = 0;
                float minDistance = Mathf.Infinity;

                for(int i = 0;i < possibleTarget.Length;i++) {
                    if(possibleTarget[i].gameObject.layer == LayerMask.NameToLayer("Player")) {
                        attackTargets.Add(possibleTarget[i].gameObject);
                        return true;
                    }

                    float distance = Vector2.Distance(transform.position, possibleTarget[i].transform.position);

                    if(distance < minDistance) {
                        indexMin = i;
                        minDistance = distance;
                    }

                }

                attackTargets.Add(possibleTarget[indexMin].gameObject);

                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos() {
        if(path != null) {
            foreach(Vector2 pos in path) {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y, 0), 0.1f);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    [Server]
    public void TakeDamage(int damage, PlayerController bulletOwner) {

        lifePoint -= damage;

        if (lifePoint <= 0) {
            RpcDeath(); //For all clients

            Score score = GetComponent<Score>();
            if(score) {
                bulletOwner.CmdAddScore(GetComponent<Score>().transform.position, score.score);
            }

            //if(!isClient && isServer) {
            //    NetworkServer.Destroy(gameObject);
            //}
        }
    }

    [ClientRpc]
    void RpcDeath() {
        GameObject instance = Instantiate(deathParticleSystem).gameObject;
        instance.transform.position = transform.position;

        if (isServer) {
            NetworkServer.Destroy(gameObject);
        }

        Destroy(instance, 1f);
    }
}
