using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoFiller : MonoBehaviour {

    [SerializeField] TextMeshProUGUI nameText;

    PlayerInfo playerInfo;

    [SerializeField] SpriteRenderer playerGhost;

    // Use this for initialization
    void Start () {
	    StartCoroutine(FillInfo());
	}

    IEnumerator FillInfo() {
        playerInfo = FindObjectOfType<PlayerInfo>();
        while (playerInfo == null) {
            playerInfo = FindObjectOfType<PlayerInfo>();

            yield return new WaitForEndOfFrame();
        }

        nameText.text = playerInfo.GetName();

        playerGhost.color = playerInfo.GetColor();
    }

    public void UpdateUsername(string newName) {
        playerInfo.SetName(newName);
    }

    public void UpdateColor(Image button) {
        playerGhost.color = button.color;

        playerInfo.SetColor(button.color);
    }
}
