using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyController : NetworkBehaviour {

    public const int MAX_HEALTH = 100;

    List<Vector2> path = new List<Vector2>();

    enum State {
        IDLE,
        MOVE,
        MOVE_TO_PLAYER,
        ATTACK
    }

    State state = State.IDLE;

    Rigidbody2D body;

    // Use this for initialization
    void Start () {
        if (!isServer) {
            return;
        }

        body = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
	    if (!isServer) {
            return;
	    }

	    switch (state) {
            case State.IDLE:
                if(path.Count == 0 || path == null) {
                    if (FindObjectOfType<PlayerController>() != null) {
                        path = FindObjectOfType<PathFinding>().GetPathFromTo(transform, FindObjectOfType<PlayerController>().transform);
                        state = State.MOVE;
                    }
                }
                break;

            case State.MOVE:
                if (path == null || path.Count == 0) {
                    state = State.IDLE;
                }

                body.velocity = (path[0] - (Vector2)transform.position).normalized * 2f;

                if (Vector2.Distance((Vector3) transform.position, path[0]) < 0.1f) {
                    path.RemoveAt(0);

                    if (path.Count == 0) {
                        state = State.IDLE;
                    }
                }
                
                break;
	    }
	}

    private void OnDrawGizmos() {
        if(path != null) {
            foreach(Vector2 pos in path) {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(new Vector3(pos.x, pos.y, 0), 0.1f);
            }
        }
        
    }
}
