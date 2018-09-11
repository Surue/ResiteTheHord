using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager:NetworkManager {

    float nextRefreshTime = 0;

    public NetworkDiscovery discovery;

    public void StartHosting() {
        StartMatchMaker();
        matchMaker.CreateMatch("Nico", 4, true, "", "", "", 0, 0, OnMatchCreated);
    }

    public override void OnStartHost() {
        discovery.Initialize();
        discovery.StartAsServer();
    }

    public override void OnStartClient(NetworkClient client) {
        discovery.showGUI = false;
    }

    public override void OnStopClient() {
        discovery.StopBroadcast();
        discovery.showGUI = true;
    }

    public void StartLan() {
        base.StartHost();
    }

    public void JoinLan() {
        base.StartClient();
    }

    void OnMatchCreated(bool success, string extendedinfo, MatchInfo responsedata) {
        base.StartHost(responsedata);
    }

    void Update() {
        if (Time.time >= nextRefreshTime && IsClientConnected()) {
            RefreshMatches();
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

    public void JoinMatch(MatchInfoSnapshot match) {
        if (matchMaker == null) {
            StartMatchMaker();
        }

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
    }

    void HandleJoinedMatch(bool success, string extendedinfo, MatchInfo responsedata) {
        StartClient(responsedata);
    }
}
