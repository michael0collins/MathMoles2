using UnityEngine;
using UnityEngine.UI;

public class MainMenuInterface : MonoBehaviour
{
    [Header("Main Menu User Interface")]
    public GameObject mainMenuPanel;
    public Button findMatchButton;

    void OnEnable()
    {
        NetworkManager.JoiningLobby += OnJoiningLobby;
        findMatchButton.onClick.AddListener(() => { NetworkManager.SFindMatch(); });
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
