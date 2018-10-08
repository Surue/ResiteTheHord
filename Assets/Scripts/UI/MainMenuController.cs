using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    PauseMenu pauseMenu;

    // Use this for initialization
    void Start() {
        pauseMenu = FindObjectOfType<PauseMenu>();
        if(!pauseMenu) {
            Debug.LogWarning("The pause menu hasn't been found and can be missing from the scene");
        } else {
            pauseMenu.Hide();
        }
    }

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
        string serverName = GetServerName();

        if(serverName != "") {
            FindObjectOfType<CustomNetworkManager>().StartLan(serverName);
        } else {
            FindObjectOfType<CustomNetworkManager>().StartLan();
        }
    }

    string GetServerName() {
        string serverName = "";

        GameObject serverNameUI = GameObject.Find("ServerNameText");

        if(serverNameUI != null) {
            if(serverNameUI.GetComponent<TextMeshProUGUI>()) {
                serverName = serverNameUI.GetComponent<TextMeshProUGUI>().text;
            } else {
                Debug.Log(serverNameUI.name + " doesn't contain any text information");
            }
        } else {
            Debug.LogWarning("The object containing the server name in not in the scene");
        }

        return serverName;
    }

    public void StartMatch() {
        string serverName = GetServerName();

        if (serverName != "") {
            FindObjectOfType<CustomNetworkManager>().StartHosting(serverName);
        }
        else {
            FindObjectOfType<CustomNetworkManager>().StartHosting();
        }
    }

    public void Shutdown() {
        CustomNetworkManager customNetworkManager = FindObjectOfType<CustomNetworkManager>();
        Destroy(customNetworkManager.gameObject);
        CustomNetworkManager.Shutdown();

        LoadSceneByName("MainMenu");
    }

    public void ToggleMenuPause() {
        if(pauseMenu.gameObject.activeSelf) {
            pauseMenu.Hide();
        } else {
            pauseMenu.Show();
        }
    }
}
