using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class MainMenuController : MonoBehaviour {

    public void LoadSceneByName(string name) {
        SceneManager.LoadScene(name);
    }

    public void LoadSceneByNameOnServer(string name) {
        FindObjectOfType<CustomNetworkManager>().ServerChangeScene(name);
    }

    public void Disconnect() {
        Debug.Log("#Disconnect");
        CustomNetworkManager networkManager = FindObjectOfType<CustomNetworkManager>();

        networkManager.Disconnect();
    }

    public void Quit() {
        Application.Quit();
    }

    public void StartLan() {
        FindObjectOfType<CustomNetworkManager>().StartLan();
    }

    public void StartMatch() {
        FindObjectOfType<CustomNetworkManager>().StartHosting();
    }

    public void Shutdown() {
        CustomNetworkManager customNetworkManager = FindObjectOfType<CustomNetworkManager>();
        Destroy(customNetworkManager.gameObject);
        CustomNetworkManager.Shutdown();

        LoadSceneByName("MainMenu");
    }
}
