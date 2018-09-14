using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class GameMatchList : MonoBehaviour {

    [SerializeField]
    JoinPanel joinPanelPrefab;

    List<LanConnectionInfo> lanAdresses = new List<LanConnectionInfo>();

    void Awake() {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged += AvailableMatchesList_OnAvailableLanChanged;
    }

    private void AvailableMatchesList_OnAvailableMatchesChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        ClearExistingButton();
        CreateNewJoinGameButtons(matches, lan);
    }

    private void AvailableMatchesList_OnAvailableLanChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        ClearExistingButton();
        CreateNewJoinGameButtons(matches, lan);
    }

    void ClearExistingButton() {
        JoinPanel[] buttons = GetComponentsInChildren<JoinPanel>();
        foreach(JoinPanel button in buttons) {
            Destroy(button.gameObject);
        }
    }

    void CreateNewJoinGameButtons(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        int number = 0;

        foreach(LanConnectionInfo l in lan) {
            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(l, transform, number);
            number++;
        }

        foreach(MatchInfoSnapshot match in matches) {
            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(match, transform, number);
            number++;
        }
    }
}
