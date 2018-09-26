using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoFiller : MonoBehaviour {

    [SerializeField] TextMeshProUGUI nameText;

    PlayerInfoController playerInfoController;

    [SerializeField] SpriteRenderer playerGhost;

    // Use this for initialization
    void Start () {
	    StartCoroutine(FillInfo());
	}

    IEnumerator FillInfo() {
        playerInfoController = FindObjectOfType<PlayerInfoController>();
        while (playerInfoController == null) {
            playerInfoController = FindObjectOfType<PlayerInfoController>();

            yield return new WaitForEndOfFrame();
        }

        nameText.text = playerInfoController.GetName();

        playerGhost.color = playerInfoController.GetColor();
    }

    public void UpdateUsername(string newName) {
        playerInfoController.UpdateUsername(newName);
    }

    public void UpdateColor(Image button) {
        playerGhost.color = button.color;

        playerInfoController.UpdateColor(button.color);
    }
}
