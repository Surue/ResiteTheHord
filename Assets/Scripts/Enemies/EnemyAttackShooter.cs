using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyAttackShooter : NetworkBehaviour {

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] bool canMultipleAttack = false;
    List<GameObject> attackTargets = new List<GameObject>();
    [SerializeField] GameObject bulletPrefab;

    static float TIME_CHECK_TARGET = 0.5f;
    float timerCheckTarget = 0f;

    EnemyMovement movementController;

    static float TIME_RELOAD = 1f;
    float timerReload = 0;

    enum State {
        IDLE,
        FIRE,
        WAIT_RELOAD
    }

    State state = State.IDLE;

    void Start() {
        movementController = GetComponent<EnemyMovement>();
    }

    void Update() {
        if (!isServer) {
            return;
        }

        switch (state) {
            case State.IDLE:
                if(timerCheckTarget > TIME_CHECK_TARGET) {
                    timerCheckTarget = 0;

                    if(CheckAttackTarget()) {
                        state = State.FIRE;
                    }
                } else {
                    timerCheckTarget += Time.deltaTime;
                }
                break;

            case State.FIRE:
                movementController.CmdAttack(1);
                foreach (GameObject attackTarget in attackTargets) {
                    Fire(attackTarget.transform.position - transform.position);
                }

                timerReload = TIME_RELOAD;
                state = State.WAIT_RELOAD;
                break;

            case State.WAIT_RELOAD:
                timerReload -= Time.deltaTime;

                if (timerReload <= 0) {
                    timerReload = 0;

                    state = State.IDLE;
                }
                break;

            default:
                break;
        }
    }

    [Server]
    bool CheckAttackTarget() {
        LayerMask mask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Destroyable");

        attackTargets = new List<GameObject>();

        Collider2D[] possibleTarget = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);

        if (possibleTarget.Length <= 0) {
            return false;
        }

        if(canMultipleAttack) {
            foreach (Collider2D c in possibleTarget) {
                attackTargets.Add(c.gameObject);
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

                if (!(distance < minDistance)) {
                    continue;
                }

                indexMin = i;
                minDistance = distance;

            }

            attackTargets.Add(possibleTarget[indexMin].gameObject);

            return true;
        }

    }
    
    void Fire(Vector3 direction) {
        GameObject bullet = Instantiate(bulletPrefab);
        
        bullet.transform.position = transform.position + direction.normalized * 0.35f;

        Vector2 vel = direction.normalized * 8; //Compute velocity

        bullet.GetComponentInChildren<Rigidbody2D>().velocity = vel; //Add velocity

        NetworkServer.Spawn(bullet);

        bullet.GetComponent<Bullet>().RpcCompensatePosition(NetworkTransport.GetNetworkTimestamp(), vel); //Compensate position on client
    }
}
