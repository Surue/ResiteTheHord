﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyAttackCQC : NetworkBehaviour {

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] bool canMultipleAttack = false;
    List<GameObject> attackTargets = new List<GameObject>();
    [SerializeField] int damage;

    [Header("Animation")]
    [SerializeField] float rotationSpeed = 10;
    [SerializeField] ParticleSystem attackParticleSystem;

    EnemySound soundController;

    Animator animator;
    bool isAnimated = false;

    static float TIME_CHECK_TARGET = 0.5f;
    float timerCheckTarget = 0f;

    EnemyMovement movementController;

    enum State {
        IDLE,
        FIRE,
        WAIT_RELOAD
    }

    State state = State.IDLE;

    // Use this for initialization
    void Start () {
        soundController = GetComponent<EnemySound>();
        movementController = GetComponent<EnemyMovement>();
        animator = GetComponentInChildren<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
	    if (!isServer) {
	        return;
	    }

	    switch(state) {
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
	            foreach(GameObject attackTarget in attackTargets) {
	                Attack(attackTarget);
	            }

	            RpcStartAnimation();
                    
	            isAnimated = true;
	            state = State.WAIT_RELOAD;
	            break;

	        case State.WAIT_RELOAD:
	            if (!isAnimated) {
	                state = State.IDLE;
	            }
	            break;

	        default:
	            break;
	    }
	}

    [Server]
    bool CheckAttackTarget() {
        LayerMask mask = 1 << LayerMask.NameToLayer("Destroyable") | 1 << LayerMask.NameToLayer("Player");

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

    void Attack(GameObject target) {
        if (target.GetComponent<Health>()) {
            target.GetComponent<Health>().TakeDamage(damage);
        }
    }

    [ClientRpc]
    void RpcStartAnimation() {
        StartCoroutine(AnimationAttack());
    }

    IEnumerator AnimationAttack() {
        animator.SetTrigger("Attack");
        attackParticleSystem.Play();
        soundController.Attack();

        while(transform.eulerAngles.z < 360f / 3f) {
            transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + rotationSpeed);
            yield return new WaitForEndOfFrame();
        }

        transform.rotation = Quaternion.identity;

        isAnimated = false;
    }
}
