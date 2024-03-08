using TMPro;
using UnityEngine;

public class Redirector : MonoBehaviour
{
    [SerializeField] private TMP_Text txt;

    private void OnEnable()
    {
        Application.logMessageReceived += LogMe;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= LogMe;
    }

    public void LogMe(string condition, string stacktrace, LogType type)
    {
        txt.text += $"Message:\n{condition}\nStackTrace:\n{stacktrace}\n\n";
    }

    [ContextMenu("Print Test")]
    public void PrintTest()
    {
        print("Hello World!");
    }

    [ContextMenu("Clear Consoles")]
    public void ClearConsole()
    {
        txt.text = "[Console]\n";
    }
}
