using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class EnnemyHealth : Health {

    [ClientRpc]
    public override void RpcDestroy() {
        GameObject instance = Instantiate(explosionParticleSystem).gameObject;
        instance.transform.position = transform.position;

        Destroy(instance, 1f);

        if(isServer) {
            NetworkServer.Destroy(gameObject);
        } else {
            Destroy(instance);
        }
    }
}
