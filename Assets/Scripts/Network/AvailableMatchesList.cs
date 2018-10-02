using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class AvailableMatchesList : MonoBehaviour {

    public static event Action<List<MatchInfoSnapshot>, List<LanConnectionInfo>> OnAvailableMatchesChanged = delegate { };
    public static event Action<List<MatchInfoSnapshot>, List<LanConnectionInfo>> OnAvailableLanChanged = delegate { };

    static List<MatchInfoSnapshot> matches = new List<MatchInfoSnapshot>();
    static List<LanConnectionInfo> lan = new List<LanConnectionInfo>();

    public static void HandleNewMatchList(List<MatchInfoSnapshot> matchList) {
        if (matches == matchList || matchList.Count == 0) {
            return;
        } else {
            if (matches.Count == matchList.Count) {

                for (int i = 0; i < matchList.Count; i++) {
                    if (matchList[i].hostNodeId != matches[i].hostNodeId) {
                        matches = matchList;
                        OnAvailableMatchesChanged(matches, lan);
                        return;
                    }
                }
            } else {
                matches = matchList;
                OnAvailableMatchesChanged(matches, lan);
            }
        }
    }

    public static void HandleNewLanList(List<LanConnectionInfo> matchList) {
        if(lan == matchList) {
            return;
        }

        lan = matchList;
        OnAvailableLanChanged(matches, lan);
    }
}
