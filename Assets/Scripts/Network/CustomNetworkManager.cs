using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager:NetworkManager {

    float nextRefreshTime = 0;

    public CustomNetworkDiscovery discovery;

    public GameObject lobbyPlayerPrefab;

    enum State {
        MATCH_MAKING,
        LOBBY,
        IN_GAME
    }

    State state = State.MATCH_MAKING;

    bool isClient = false;
    bool isServer = false;

    public void StartHosting() {
        isServer = true;
        StartMatchMaker();
        matchMaker.CreateMatch("Match", 4, true, "", "", "", 0, 0, OnMatchCreated);
    }

    public void StartLan() {
        isServer = true;
        base.StartHost();
        discovery.StartBroadcast();
    }

    public void JoinLan() {
        isClient = true;
        base.StartClient();
    }

    public void SetNetworkAddress(string a) {
        networkAddress = a;
    }

    void OnMatchCreated(bool success, string extendedInfo, MatchInfo responseData) {
        base.StartHost(responseData);
    }

    void Update() {

        switch (state) {
            case State.MATCH_MAKING:
                if(Time.time >= nextRefreshTime && !IsClientConnected()) {
                    RefreshMatches();
                }

                if (IsClientConnected()) {
                    state = State.LOBBY;
                }
                break;

            case State.LOBBY:
                PanelPlayer[] players = FindObjectsOfType<PanelPlayer>();
                bool ready = true;

                foreach (PanelPlayer panelPlayer in players) {
                    if (!panelPlayer.isReady) {
                        ready = false;
                        break;
                    }
                }

                if (SceneManager.GetActiveScene().name == "lobby") {
                    if (ready) {
                        GameObject.Find("ButtonLaunch").GetComponent<Button>().interactable = true;
                    } else {
                        GameObject.Find("ButtonLaunch").GetComponent<Button>().interactable = false;
                    }
                }

                break;

            case State.IN_GAME:
                break;
        }
    }

    void RefreshMatches() {
        nextRefreshTime = Time.time + 5f;

        if(matchMaker == null) {
            StartMatchMaker();
        }
        
        matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleListMatchesComplete);
    }

    void HandleListMatchesComplete(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData) {
        if(!IsClientConnected())
        AvailableMatchesList.HandleNewMatchList(responseData);
    }

    public void JoinMatch(LanConnectionInfo lan) {
        isClient = true;
        networkAddress = lan.ipAddress;
        discovery.StopBroadcast();
        base.StartClient();
    }

    public void JoinMatch(MatchInfoSnapshot match) {
        isClient = true;
        if (matchMaker == null) {
            StartMatchMaker();
        }

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
    }

    void HandleJoinedMatch(bool success, string extendedInfo, MatchInfo responseData) {
        StartClient(responseData);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        if (networkSceneName == "Lobby") {
            GameObject instance = Instantiate(lobbyPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, instance, playerControllerId);
        } else {
            GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
    }

    public void Disconnect() {

        while (discovery.running) {
            discovery.StopBroadcast();
        }

        if(matchMaker != null) {
            StopMatchMaker();
            matchMaker = null;
        }

        if (isServer) {

            StopHost();
            isClient = false;
        }

        if (isClient) {
            StopClient();
            isClient = false;
        }

        StartCoroutine(ReconnectBroadcast());
    }

    IEnumerator ReconnectBroadcast() {
        yield return new WaitForSeconds(2);

        discovery.Initialize();
        discovery.StartAsClient();
    }
}