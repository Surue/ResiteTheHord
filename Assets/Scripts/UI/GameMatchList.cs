using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class GameMatchList : MonoBehaviour {

    [SerializeField] TextMeshProUGUI placeHolderText;

    [SerializeField]
    JoinPanel joinPanelPrefab;

    Dictionary<MatchInfoSnapshot, JoinPanel> matchesButtons = new Dictionary<MatchInfoSnapshot, JoinPanel>();
    Dictionary<LanConnectionInfo, JoinPanel> lanButtons = new Dictionary<LanConnectionInfo, JoinPanel>();

    void Start() {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
        AvailableMatchesList.OnAvailableLanChanged += AvailableMatchesList_OnAvailableLanChanged;

        if (matchesButtons.Count == 0 && lanButtons.Count == 0) {
            placeHolderText.gameObject.SetActive(true);
        }
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

        if(matches.Count != 0 || lan.Count != 0) {
            placeHolderText.gameObject.SetActive(false);
        } else {
            placeHolderText.gameObject.SetActive(true);
        }

        //Destroy unused button lan
        bool finished = true;
        do {
            finished = true;
            foreach(KeyValuePair<LanConnectionInfo, JoinPanel> keyValuePair in lanButtons) {
                if(!lan.Contains(keyValuePair.Key)) {
                    keyValuePair.Value.Disparition();
                    lanButtons.Remove(keyValuePair.Key);
                    finished = false;
                    shouldMoveUp = true;
                    break;
                } else {
                    if (shouldMoveUp) {
                        keyValuePair.Value.MoveUp();
                    }
                }
            }
        } while (!finished);

        //Build new button for lan
        foreach(LanConnectionInfo l in lan) {
            //Look if need to build new button
            if (lanButtons.ContainsKey(l)) {
                continue;
            }

            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(l, transform);

            lanButtons.Add(l, button);
        }

        //Destroy unused button matches
        do {
            finished = true;
            foreach(MatchInfoSnapshot matchesButtonsKey in matchesButtons.Keys) {
                bool stillExisting = false;

                foreach(MatchInfoSnapshot matchInfoSnapshot in matches) {
                    if (matchInfoSnapshot.hostNodeId != matchesButtonsKey.hostNodeId) {
                        continue;
                    }

                    stillExisting = true;
                    break;
                }

                if(!stillExisting) {
                    JoinPanel toDestroy;
                    matchesButtons.TryGetValue(matchesButtonsKey, out toDestroy);
                    
                    toDestroy.Disparition();
                    matchesButtons.Remove(matchesButtonsKey);
                    finished = false;
                    break;
                } else {
                    if (!shouldMoveUp) {
                        continue;
                    }

                    JoinPanel toMoveUp;
                    matchesButtons.TryGetValue(matchesButtonsKey, out toMoveUp);
                    toMoveUp.MoveUp();
                }
            }
        } while(!finished);

        //Build new button for matches
        foreach(MatchInfoSnapshot match in matches) {
            bool alreadyExisting = false;

            foreach (MatchInfoSnapshot matchesButtonsKey in matchesButtons.Keys) {
                if (match.hostNodeId == matchesButtonsKey.hostNodeId) {
                    alreadyExisting = true;
                }
            }

            if (alreadyExisting) {
                continue;
            }

            JoinPanel button = Instantiate(joinPanelPrefab);
            button.Initialize(match, transform);

            matchesButtons.Add(match, button);
        }

        
    }
}
