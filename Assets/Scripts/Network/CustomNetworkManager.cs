using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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

    public void StartHosting() {
        StartMatchMaker();
        matchMaker.CreateMatch("Nico", 4, true, "", "", "", 0, 0, OnMatchCreated);
    }

    public override void OnStartHost() {
        //discovery.Initialize();
    }

    public override void OnStartClient(NetworkClient client) {
        //discovery.showGUI = false;
    }

    public override void OnStopClient() {
        //discovery.StopBroadcast();
        //discovery.showGUI = true;
    }

    public void StartLan() {
        base.StartHost();
        discovery.StartBroadcast();
    }

    public void JoinLan() {
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

                if (ready) {
                    GameObject.Find("ButtonLaunch").GetComponent<Button>().interactable = true;
                } else {
                    GameObject.Find("ButtonLaunch").GetComponent<Button>().interactable = false;
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

    void HandleListMatchesComplete(bool success, string extendedinfo, List<MatchInfoSnapshot> responsedata) {
        if(!IsClientConnected())
        AvailableMatchesList.HandleNewMatchList(responsedata);
    }

    public void JoinMatch(LanConnectionInfo lan) {
        networkAddress = lan.ipAddress;

        base.StartClient();
    }

    public void JoinMatch(MatchInfoSnapshot match) {
        if (matchMaker == null) {
            StartMatchMaker();
        }

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
    }

    void HandleJoinedMatch(bool success, string extendedinfo, MatchInfo responsedata) {
        StartClient(responsedata);
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
}