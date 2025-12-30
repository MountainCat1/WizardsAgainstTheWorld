using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using TMPro;

public class ErrorPopup : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private GameObject popupPanel;

    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openLogsButton;

    private string logFilePath;

    private void Awake()
    {
        // Prepare log path
        string logDir = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        logFilePath = Path.Combine(logDir, "game.log");

        // Start clean log file
        File.WriteAllText(logFilePath, "=== Game Session Log ===\n");

        if (popupPanel != null) popupPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(HidePopup);

        if (openLogsButton != null)
            openLogsButton.onClick.AddListener(OpenLogLocation);

        Application.logMessageReceived += HandleLog;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            OpenLogLocation();
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        // Always write logs to file
        File.AppendAllText(
            logFilePath,
            $"[{System.DateTime.UtcNow:o}] {type}: {condition}\n{stackTrace}\n"
        );

        // Show popup only for errors & exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            ShowPopup(condition, stackTrace);
        }
    }

    private void ShowPopup(string message, string stackTrace)
    {
        if (errorText != null)
            errorText.text = $"{message}\n\n{stackTrace}";

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    private void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private void OpenLogLocation()
    {
        // Write a readme file
        File.WriteAllText(
            Path.Combine(
                Path.GetDirectoryName(logFilePath)!,
                "ReadMe - Please post the error on the discord server.txt"
            ),
            ReadMeMessage
        );

        string folder = Path.GetDirectoryName(logFilePath);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Process.Start("explorer.exe", folder.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        Process.Start("open", folder);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        Process.Start("xdg-open", folder);
#endif
    }

    private const string ReadMeMessage = @"
 _._     _,-'""""`-._
(,-.`._,'(       |\`-/|
    `-.-' \ )-`( , o o)
          `-    \`_`""'-
THIS IS A BUG CATCHING CAT
it seems like you have found one!
So make sure that the bug reaches the BUG CATCHING cat otherwise he will get hungry and STARVE TO DEATH


-----
This folder contains log files for the game.
The main log file is 'game.log'.
I would suuuper appreciate if you would send the game.log, or even entire /Mountain Cat/Salvager folder To the discord server: https://discord.gg/3MyRg2Wh4dOr our telegram chat: https://t.me/fireballgamesstudio

Please make sure you copy a correct file, as posting random files from your PC can put you at risk!
I don't know if it is obvious but NEVER post your entire \LocalLow or home directories on the internet!!!

The files we care about the most should be located in:

Windows:
C:\Users\<USERNAME>\AppData\LocalLow\Mountain Cat\Salvager\Logs\game.log

Linux
/home/<USERNAME>/.config/unity3d/Mountain Cat/Salvager/Logs/game.log

macOS (we dont support macos yet, but just in case)
/Users/<USERNAME>/Library/Application Support/Mountain Cat/Salvager/Logs/game.log
----


      |\      _,,,---,,_
      /x`.-'`'    -.  ;-;;,_
     |x4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)  

^ This will be him IF YOU DON'T HELP
                                 ";
}