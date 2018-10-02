using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class GameMatchList : MonoBehaviour {

    [SerializeField]
    JoinPanel joinPanelPrefab;

    Dictionary<MatchInfoSnapshot, JoinPanel> matchesButtons = new Dictionary<MatchInfoSnapshot, JoinPanel>();
    Dictionary<LanConnectionInfo, JoinPanel> lanButtons = new Dictionary<LanConnectionInfo, JoinPanel>();

    void Start() {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged += AvailableMatchesList_OnAvailableLanChanged;
    }

    void OnDestroy() {
        AvailableMatchesList.OnAvailableMatchesChanged -= AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged -= AvailableMatchesList_OnAvailableLanChanged;
    }

    void AvailableMatchesList_OnAvailableMatchesChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        UpdateButton(matches, lan);
    }

    void AvailableMatchesList_OnAvailableLanChanged(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        UpdateButton(matches, lan);
    }

    void UpdateButton(List<MatchInfoSnapshot> matches, List<LanConnectionInfo> lan) {
        bool shouldMoveUp = false;

        //Destroy unused button lan
        bool finised = true;
        do {
            finised = true;
            foreach(KeyValuePair<LanConnectionInfo, JoinPanel> keyValuePair in lanButtons) {
                if(!lan.Contains(keyValuePair.Key)) {
                    keyValuePair.Value.Disparition();
                    lanButtons.Remove(keyValuePair.Key);
                    finised = false;
                    shouldMoveUp = true;
                    break;
                } else {
                    if (shouldMoveUp) {
                        keyValuePair.Value.MoveUp();
                    }
                }
            }
        } while (!finised);

        //Build new button for lan
        foreach(LanConnectionInfo l in lan) {
            //Look if need to build new button
            if(!lanButtons.ContainsKey(l)) {
                JoinPanel button = Instantiate(joinPanelPrefab);
                button.Initialize(l, transform);

                lanButtons.Add(l, button);
            }
        }

        //Destroy unused button matches
        do {
            finised = true;
            foreach(MatchInfoSnapshot matchesButtonsKey in matchesButtons.Keys) {
                bool stillExisting = false;

                foreach(MatchInfoSnapshot matchInfoSnapshot in matches) {
                    if(matchInfoSnapshot.hostNodeId == matchesButtonsKey.hostNodeId) {
                        stillExisting = true;
                        break;
                    }
                }

                if(!stillExisting) {
                    JoinPanel toDestroy;
                    matchesButtons.TryGetValue(matchesButtonsKey, out toDestroy);
                    Debug.Log("DestroyMatch");
                    toDestroy.Disparition();
                    matchesButtons.Remove(matchesButtonsKey);
                    finised = false;
                    break;
                } else {
                    if (shouldMoveUp) {
                        JoinPanel toMoveUp;
                        matchesButtons.TryGetValue(matchesButtonsKey, out toMoveUp);
                        toMoveUp.MoveUp();
                    }
                }
            }
        } while(!finised);

        //Build new button for matches
        foreach(MatchInfoSnapshot match in matches) {
            bool alreadyExisting = false;

            foreach (MatchInfoSnapshot matchesButtonsKey in matchesButtons.Keys) {
                if (match.hostNodeId == matchesButtonsKey.hostNodeId) {
                    alreadyExisting = true;
                }
            }

            if (!alreadyExisting) {
                JoinPanel button = Instantiate(joinPanelPrefab);
                button.Initialize(match, transform);

                matchesButtons.Add(match, button);
            }
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
