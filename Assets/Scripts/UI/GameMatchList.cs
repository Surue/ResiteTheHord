using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class GameMatchList : MonoBehaviour {

    [SerializeField]
    JoinPanel joinPanelPrefab;

    Dictionary<MatchInfoSnapshot, JoinPanel> matchesButton = new Dictionary<MatchInfoSnapshot, JoinPanel>();
    Dictionary<LanConnectionInfo, JoinPanel> lan = new Dictionary<LanConnectionInfo, JoinPanel>();

    void Start() {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged += AvailableMatchesList_OnAvailableLanChanged;
    }

    void OnDestroy() {
        AvailableMatchesList.OnAvailableMatchesChanged -= AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged -= AvailableMatchesList_OnAvailableLanChanged;
    }

    void AvailableMatchesList_OnAvailableMatchesChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        ClearExistingButton();
        CreateNewJoinGameButtons(matches, lan);
    }

    void AvailableMatchesList_OnAvailableLanChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        ClearExistingButton();
        CreateNewJoinGameButtons(matches, lan);
    }

    void UpdateButton(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        foreach (LanConnectionInfo l in lan) {

        }

        foreach(MatchInfoSnapshot match in matches) {
        }
    }

    void ClearExistingButton() {
        JoinPanel[] buttons = GetComponentsInChildren<JoinPanel>();
        foreach(JoinPanel button in buttons) {
            Destroy(button.gameObject);
        }
    }

    void CreateNewJoinGameButtons(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        foreach(LanConnectionInfo l in lan) {
            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(l, transform);
        }

        foreach(MatchInfoSnapshot match in matches) {
            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(match, transform);
        }
    }
}
