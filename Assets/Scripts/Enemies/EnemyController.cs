using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyController : NetworkBehaviour {

    public const int MAX_HEALTH = 100;

    List<Vector2> path = new List<Vector2>();

    static float TIME_CHECK_PATH = 1.5f;
    float timerCheckPath = 0f;

    static float TIME_CHECK_TARGET = 0.5f;
    float timerCheckTarget = 0f;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] bool canMultipleAttack = false;
    List<GameObject> attackTargets = new List<GameObject>();

    [Header("Animation")]
    [SerializeField] float rotationSpeed = 10;
    [SerializeField] ParticleSystem attackParticleSystem;
    Animator animator;

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
        body.velocity = (targetPosition - (Vector2)transform.position).normalized * 2f;
    }

    // Update is called once per frame
    [Server]
    void Update() {
        targetPosition = transform.position;

        switch(state) {
            case State.INITIALIZE:
                break;

            case State.IDLE:
                if(path.Count == 0 || path == null) {
                    path = pathFinder.GetPathFromTo(transform, player.transform);
                    state = State.MOVE;
                }
                break;

            case State.MOVE:
                if(path == null || path.Count == 0) {
                    state = State.IDLE;
                }

                targetPosition = path[0];

                if(Vector2.Distance((Vector3)transform.position, path[0]) < 0.1f) {
                    path.RemoveAt(0);

                    if(path.Count == 0) {
                        state = State.IDLE;
                    }
                }

                if(timerCheckPath > TIME_CHECK_PATH) {
                    timerCheckPath = 0;
                    path = pathFinder.GetPathFromTo(transform, player.transform);
                } else {
                    timerCheckPath += Time.deltaTime;
                }
                break;

            case State.ATTACK:
                body.velocity = Vector2.zero;
                foreach(GameObject target in attackTargets) {
                    if(target.GetComponent<Health>()) {
                        target.GetComponent<Health>().TakeDamage(1);
                    }
                }

                path = new List<Vector2>();

                RpcStartAnimation();
                state = State.ATTACK_ANIMATION;
                break;

            case State.ATTACK_ANIMATION:
                break;
        }

        if(timerCheckTarget > TIME_CHECK_TARGET && state != State.ATTACK) {
            timerCheckTarget = 0;
            if(CheckAttackTarget()) {
                state = State.ATTACK;
            }
        } else {
            timerCheckTarget += Time.deltaTime;
        }
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

        state = State.IDLE;
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
}
