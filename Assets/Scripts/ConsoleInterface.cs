using System;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleInterface : MonoBehaviour
{
    [Header("User Interface")]
    public GameObject consolePanel;
    public Transform consoleMessagesList;
    public GameObject consoleMessagePrefab;

    [Header("Variables")]
    public int maxMessages = 50;

    private void Awake()
    {
        Application.logMessageReceived += OnUnityLogReceived;
    }

    private void OnUnityLogReceived(string condition, string stackTrace, LogType type)
    {
        AppendConsoleMessages(condition, type, stackTrace);
    }

    private void AppendConsoleMessages(string message, LogType logType = LogType.Log, string trace = "")
    {
        if (consoleMessagesList.childCount > maxMessages)
        {
            GameObject messageObject = consoleMessagesList.GetChild(0).gameObject;
            messageObject.transform.SetAsLastSibling();
            FormatMessage(messageObject, message, logType, trace);
        }
        else
            FormatMessage(Instantiate(consoleMessagePrefab, consoleMessagesList), message, logType, trace);
    }

    private void FormatMessage(GameObject messageObject, string message, LogType logType, string trace)
    {
        string colouredMessage;
        switch (logType)
        {
            case LogType.Exception:
                colouredMessage = $"<color='#f44336'>[{DateTime.Now:T}] {message}\n{trace}</color>";
                break;
            case LogType.Assert:
            case LogType.Error:
                colouredMessage = $"<color='#f44336'>[{DateTime.Now:T}] {message}\n{trace}</color>";
                break;
            case LogType.Warning:
                colouredMessage = $"<color='#ffc107'>[{DateTime.Now:T}] {message}</color>";
                break;
            default:
                colouredMessage = $"[{DateTime.Now:T}] {message}";
                break;
        }
        messageObject.GetComponent<Text>().text = colouredMessage;
    }

    public void SwitchConsoleVisibility()
    {
        consolePanel.SetActive(!consolePanel.activeSelf);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnUnityLogReceived;
    }
}
