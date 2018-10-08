using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public struct LanConnectionInfo {
    public string ipAddress;
    public int port;
    public string name;

    public LanConnectionInfo(string fromAddress, string data) {
        ipAddress = fromAddress.Substring(fromAddress.LastIndexOf(":") + 1, fromAddress.Length - (fromAddress.LastIndexOf(":") + 1));
        //string portText = data.Substring(data.LastIndexOf(":") + 1, data.Length - (data.LastIndexOf(":") + 1));
        string portText = data.Substring(data.IndexOf("localhost:") + 10, data.IndexOf("|") - (data.IndexOf("localhost:") + 10));
        port = 7777;

        int.TryParse(portText, out port);

        name = data.Substring(data.IndexOf("serverName:") + 11, data.Length - (data.IndexOf("serverName:") + 11));
    }
}

public class CustomNetworkDiscovery : NetworkDiscovery {

    Dictionary<LanConnectionInfo, float> lanAddresses = new Dictionary<LanConnectionInfo, float>();

    public string ip;

    float timeout = 1f;

    void Awake() {
        base.Initialize();
        if (!base.StartAsClient()) {
            StartCoroutine(ReconnectDiscovery());
        }
        StartCoroutine(CleanupExpireEntries());
    }

    IEnumerator ReconnectDiscovery() {
        while (!base.StartAsClient()) {
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator CleanupExpireEntries() {
        while (true) {
            bool changed = false;

            var keys = lanAddresses.Keys.ToList();
            foreach (var key in keys) {
                if (lanAddresses[key] <= Time.time) {
                    lanAddresses.Remove(key);
                    changed = true;
                }
            }

            if (changed) {
                UpdateMatchInfos();
            }

            yield return new WaitForSeconds(timeout);
        }
    }

    public void StartBroadcast(string serverName) {
        StopBroadcast();
        

        base.Initialize();
        broadcastData += "|serverName:" + serverName;
        base.StartAsServer();

        Debug.Log(broadcastData);
    }

    public override void OnReceivedBroadcast(string fromAddress, string data) {
        base.OnReceivedBroadcast(fromAddress, data);

        LanConnectionInfo info = new LanConnectionInfo(fromAddress, data);

        if (!lanAddresses.ContainsKey(info) ) {
            lanAddresses.Add(info, Time.time + timeout);
            UpdateMatchInfos();

            GetComponent<CustomNetworkManager>().networkAddress = info.ipAddress;
        } else {
            lanAddresses[info] = Time.time + timeout;
        }
    }

    void UpdateMatchInfos() {
        //Debug.Log(lanAddresses.Keys.ToList());

        //var keys = lanAddresses.Keys.ToList();
        //foreach(var key in keys) {
        //    Debug.Log(key.ipAddress);
        //    Debug.Log(key.name);
        //    Debug.Log(key.port);
        //}

        AvailableMatchesList.HandleNewLanList(lanAddresses.Keys.ToList());
    }
}
