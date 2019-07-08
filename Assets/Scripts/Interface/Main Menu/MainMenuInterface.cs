using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuInterface : MonoBehaviour
{
    [Header("Main Menu User Interface")]
    public GameObject mainMenuPanel;

    void OnEnable()
    {
        NetworkManager.JoiningLobby += OnJoiningLobby;
    }

    private void OnJoiningLobby()
    {
        mainMenuPanel.SetActive(false);
    }

    void OnDisable()
    {
        NetworkManager.JoiningLobby -= OnJoiningLobby;
    }
}
