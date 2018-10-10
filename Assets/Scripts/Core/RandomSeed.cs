using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RandomSeed:NetworkBehaviour {

    public float curSeed;
    public float forcedSeed = 0;

    [SyncVar]
    public static float SEED_PROCEDURAL_GENERATION = 12;

    //Value for Linear congruential generator NEVER CHANGE THEM!!!
    static long multiplier = 1103515245;
    static long incrementer = 12345;
    static long modulus = (long)Mathf.Pow(2f, 31f);

    // Use this for initialization
    void Start() {
        if (!isServer) {
            return;
        }

        if (forcedSeed != 0) {
            SEED_PROCEDURAL_GENERATION = forcedSeed;
            curSeed = forcedSeed;
        } else {
            SEED_PROCEDURAL_GENERATION = Random.value;
            curSeed = SEED_PROCEDURAL_GENERATION;
        }
    }

    // Use only for procedural generation
    public static float GetValue() {

        SEED_PROCEDURAL_GENERATION = (multiplier * SEED_PROCEDURAL_GENERATION + incrementer) % modulus;

        float randomValue = SEED_PROCEDURAL_GENERATION / (float)modulus;

        return randomValue;
    }
}
