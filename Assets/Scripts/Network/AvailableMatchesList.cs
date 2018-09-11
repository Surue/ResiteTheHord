using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class AvailableMatchesList:MonoBehaviour {

    public static event Action<List<MatchInfoSnapshot>> OnAvailableMatchesChanged = delegate { };

    static List<MatchInfoSnapshot> matches = new List<MatchInfoSnapshot>();

    public static void HandleNewMatchList(List<MatchInfoSnapshot> matchList) {
        OnAvailableMatchesChanged(matchList);
    }
}
