using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField] private GameObject consoleUI;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;

    private readonly Dictionary<string, Action<DiContainer, string[]>> _commands = new();

    [Inject] private SceneContextRegistry _sceneRegistry;

    private DiContainer GetDi()
    {
        return _sceneRegistry.GetContainerForScene(SceneManager.GetActiveScene());
    }

    private void Awake()
    {
        RegisterCommand("help", (di, args) => { PrintHelp(); });
        
        consoleUI.SetActive(false);

        outputText.text = "";

        inputField.onSubmit.AddListener((text) => { OnInputSubmitted(); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            consoleUI.SetActive(!consoleUI.activeSelf);
            if (consoleUI.activeSelf)
            {
                inputField.text = "";
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
    }

    public void RegisterCommand(string name, Action<DiContainer, string[]> callback)
    {
        _commands[name.ToLower()] = callback;
    }

    public void OnInputSubmitted()
    {
        string input = inputField.text.Trim();
        inputField.text = "";

        if (string.IsNullOrEmpty(input))
            return;

        string[] parts = input.Split(' ');
        string cmd = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(cmd, out var action))
        {
            Log("Executing command: " + cmd);
            try
            {
                action.Invoke(GetDi(), args);
            }
            catch (Exception e)
            {
                Log($"Something went wrong while executing command '{cmd}': {e.Message}");
                GameLogger.LogException(e);
            }
        }
        else
        {
            Log($"Unknown command: {cmd}\nUse 'help' to see available commands.");
        }
    }

    private void PrintHelp()
    {
        var message = "Available commands:\n";
        foreach (var cmd in _commands.Keys)
        {
            message += $"- {cmd}\n";
        }

        Log(message);
    }

    public void Log(string message)
    {
        outputText.text = message + "\n\n\n" + outputText.text;
    }
}