using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class MatchListPanel:MonoBehaviour {

    [SerializeField]
    JoinButton joinButtonPrefab;

    void Awake() {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
    }

    private void AvailableMatchesList_OnAvailableMatchesChanged(List<MatchInfoSnapshot> matches) {
        ClearExistingButton();
        CreateNewJoinGameButtons(matches);
    }

    void ClearExistingButton() {
        JoinButton[] buttons = GetComponentsInChildren<JoinButton>();
        foreach(JoinButton button in buttons) {
            Destroy(button.gameObject);
        }
    }

    void CreateNewJoinGameButtons(List<MatchInfoSnapshot> matches) {
        Debug.Log(matches.Count);
        foreach(MatchInfoSnapshot match in matches) {
            JoinButton button = Instantiate(joinButtonPrefab);
            button.Initialize(match, transform);
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
