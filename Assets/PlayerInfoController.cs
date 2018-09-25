using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoController : MonoBehaviour {

    static PlayerInfoController instance;
    public static PlayerInfoController Instance {
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
    }

    [SerializeField] PlayerInfo playerInfo;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public string GetName() {
        return playerInfo.GetName();
    }

    public void UpdateUsername(string newName) {
        playerInfo.SetName(newName);
    }

    public Color GetColor() {
        return playerInfo.GetColor();
    }

    public void UpdateColor(Color color) {
        playerInfo.SetColor(color);
    }
}
