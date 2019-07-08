using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInterface : MonoBehaviour
{
    [Header("Lobby User Interface")]
    public Color localPlayerColor;
    public Color playerColor;

    public GameObject lobbyWaitingPanel;
    public GameObject lobbyPanel;

    public Transform[] lobbyPlayers;

    [Header("Loading screen")]
    public GameObject loadingScreenPanel;
    public Image loadingBar;

    private bool _mapLoaded = false;

    public void RefreshLobbyPlayers()
    {
        for (int i = 0; i < lobbyPlayers.Length; i++)
        {
            Transform lobbyPlayer = lobbyPlayers[i];
            lobbyPlayer.GetComponent<Image>().color = playerColor;
            lobbyPlayer.Find("Avatar").gameObject.SetActive(false);
            lobbyPlayer.Find("Username").gameObject.SetActive(false);
            lobbyPlayer.Find("Waiting").gameObject.SetActive(true);
        }

        int li = 0;
        foreach (LobbyPlayer lPlayer in NetworkManager.LobbyPlayers)
        {
            Transform lobbyPlayer = lobbyPlayers[li];
            lobbyPlayer.GetComponent<Image>().color = NetworkManager.UID == lPlayer.UID ? localPlayerColor : playerColor;
            lobbyPlayer.Find("Avatar").gameObject.SetActive(true);
            lobbyPlayer.Find("Username").gameObject.SetActive(true);
            lobbyPlayer.Find("Username").GetComponent<Text>().text = lPlayer.Username;
            lobbyPlayer.Find("Waiting").gameObject.SetActive(false);
            li++;
        }
    }

    void OnEnable()
    {
        NetworkManager.JoiningLobby += OnJoiningLobby;
        NetworkManager.JoinedLobby += OnJoinedLobby;
        NetworkManager.AddLobbyPlayer += OnAddLobbyPlayer;
        NetworkManager.MapLoadingStarted += OnMapLoadingStarted;
        NetworkManager.MapLoaded += OnMapLoaded;
    }

    private void OnMapLoaded()
    {
        _mapLoaded = true;
        loadingScreenPanel.SetActive(false);
    }

    private void OnMapLoadingStarted(string sceneName)
    {
        _mapLoaded = false;
        lobbyWaitingPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        loadingScreenPanel.SetActive(true);
        StartCoroutine(LoadingBar());
    }

    private IEnumerator LoadingBar()
    {
        while (!_mapLoaded)
        {
            loadingBar.fillAmount = NetworkManager.LoadingProgress;
            yield return null;
        }
    }

    private void OnAddLobbyPlayer(string username)
    {
        RefreshLobbyPlayers();
    }

    private void OnJoinedLobby()
    {
        lobbyWaitingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    private void OnJoiningLobby()
    {
        lobbyWaitingPanel.SetActive(true);
    }

    void OnDisable()
    {
        NetworkManager.JoiningLobby -= OnJoiningLobby;
        NetworkManager.JoinedLobby -= OnJoinedLobby;
        NetworkManager.AddLobbyPlayer -= OnAddLobbyPlayer;
    }
}
