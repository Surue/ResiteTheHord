using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PlayerInfo : MonoBehaviour {

    [SerializeField] string username;
    [SerializeField] Color color;

    static PlayerInfo instance;
    public static PlayerInfo Instance {
        get {
            return instance;
        }
    }

    private void Awake() {
        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        username = PlayerPrefs.GetString("username");
        color = new Color(PlayerPrefs.GetFloat("r"), PlayerPrefs.GetFloat("g"), PlayerPrefs.GetFloat("b"));
    }

    public string GetName() {
        return username;
    }

    public void SetName(string newName) {
        username = newName;
        PlayerPrefs.SetString("username", newName);
    }

    public Color GetColor() {
        return color;
    }

    public void SetColor(Color newColor) {
        color = newColor;
        PlayerPrefs.SetFloat("r", newColor.r);
        PlayerPrefs.SetFloat("g", newColor.g);
        PlayerPrefs.SetFloat("b", newColor.b);
    }
}
