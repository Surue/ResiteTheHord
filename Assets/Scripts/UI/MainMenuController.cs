using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    public void LoadSceneByName(string name) {
        SceneManager.LoadScene(name);
    }

    public void LoadSceneByNameOnServer(string name) {
        FindObjectOfType<CustomNetworkManager>().ServerChangeScene(name);
    }

    public void Quit() {
        Application.Quit();
    }
}
