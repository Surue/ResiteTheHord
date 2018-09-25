using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "player_info", menuName = "")]
public class PlayerInfo : ScriptableObject {

    [SerializeField] string username = "Nomad";
    [SerializeField] Color color = Color.blue;

    public string GetName() {
        return username;
    }

    public void SetName(string newName) {
        username = newName;
    }

    public Color GetColor() {
        return color;
    }

    public void SetColor(Color newColor) {
        color = newColor;
    }
}
