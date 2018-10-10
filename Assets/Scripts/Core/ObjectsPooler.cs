using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsPooler : MonoBehaviour {

    public static ObjectsPooler instance;

    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

    void Awake() {
        instance = this;
    }

    void Start() {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < amountToPool; i++) {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObjects() {
        foreach (GameObject o in pooledObjects) {
            if (!o.activeInHierarchy) {
                return o;
            }
        }

        return null;
    }
}
