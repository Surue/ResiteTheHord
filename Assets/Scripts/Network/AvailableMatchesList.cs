using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class AvailableMatchesList:MonoBehaviour {

    public static event Action<List<MatchInfoSnapshot>, List<LanConnectionInfo>> OnAvailableMatchesChanged = delegate { };
    public static event Action<List<MatchInfoSnapshot>, List<LanConnectionInfo>> OnAvailableLanChanged = delegate { };

    static List<MatchInfoSnapshot> matches = new List<MatchInfoSnapshot>();
    static List<LanConnectionInfo> lan = new List<LanConnectionInfo>();

    public static void HandleNewMatchList(List<MatchInfoSnapshot> matchList) {
        matches = matchList;
        OnAvailableMatchesChanged(matches, lan);
    }

    public static void HandleNewLanList(List<LanConnectionInfo> matchList) {
        lan = matchList;
        OnAvailableLanChanged(matches, lan);
    }
}
