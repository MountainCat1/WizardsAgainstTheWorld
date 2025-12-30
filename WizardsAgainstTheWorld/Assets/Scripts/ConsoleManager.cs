using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LevelSelector.GameEvents;
using LevelSelector.Managers;
using Managers;
using Managers.LevelSelector;
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

        RegisterCommand("callmedaddy", (di, args) =>
        {
            var itemManager = di.Resolve<IItemManager>();
            var godGunPrefab = itemManager
                .GetItems()
                .First(x => x.GetIdentifier() == "GodPistol");
            var godAugmentPrefab = itemManager
                .GetItems()
                .First(x => x.GetIdentifier() == "GodAugment");

            var godGun = ItemData.FromItem(godGunPrefab);
            var godAugment = ItemData.FromItem(godAugmentPrefab);

            if (SceneManager.GetActiveScene().name == Scenes.LevelSelector)
            {
                Log("Daddy has arrived!");
                var crewManager = di.Resolve<ICrewManager>();
                crewManager.Inventory.AddItem(godGun);
                crewManager.Inventory.AddItem(godAugment);
            }

            if (SceneManager.GetActiveScene().name == Scenes.GameplayScene)
            {
                Log("Daddy has arrived!");
                var creatureManager = di.Resolve<ICreatureManager>();
                creatureManager.PlayerCreatures.First().Inventory.AddItem(godGun);
                creatureManager.PlayerCreatures.First().Inventory.AddItem(godAugment);
            }
        });

        RegisterCommand("travel", (di, args) =>
        {
            if (SceneManager.GetActiveScene().name == Scenes.LevelSelector)
            {
                var travelManager = di.Resolve<ITravelManager>();
                var regionManager = di.Resolve<IRegionManager>();
                var nextRegion = regionManager.NextRegions.First();
                travelManager.TravelToRegion(nextRegion);
                Log($"Traveling to {nextRegion.Name}...");
            }
            else
            {
                Log("This command can only be used in the Level Selector scene.");
            }
        });

        RegisterCommand("items", (di, args) =>
        {
            var itemManager = di.Resolve<IItemManager>() 
                              ?? throw new InvalidOperationException("IItemManager not registered");

            var items = itemManager.GetItems();

            var sb = new StringBuilder();

            sb.AppendLine("Item IDs:");

            foreach (var item in items)
            {
                sb.Append($"{item.GetIdentifier()}, ");
            }
            
            Log(sb.ToString());
        });
        
        RegisterCommand("give", (di, args) =>
        {
            if (args.Length < 1)
            {
                Log("Usage: give <item_id> [count=1]");
                return;
            }

            var itemId = args[0];
            if (args.Length >= 2 && (!int.TryParse(args[1], out var count) || count <= 0))
            {
                Log("Invalid count. Must be a positive integer.");
                return;
            }
            var finalCount = args.Length >= 2 ? int.Parse(args[1]) : 1;

            var itemManager = di.Resolve<IItemManager>() 
                              ?? throw new InvalidOperationException("IItemManager not registered");
            var crewManager = di.Resolve<ICrewManager>() 
                              ?? throw new InvalidOperationException("ICrewManager not registered");

            var item = itemManager.GetItems()
                .FirstOrDefault(x => x.GetIdentifier().Contains(itemId, StringComparison.OrdinalIgnoreCase));

            if (item is null)
            {
                Log($"Item '{itemId}' not found.");
                return;
            }

            var itemData = ItemData.FromItem(item);
            itemData.Count = finalCount;

            crewManager.Inventory.AddItem(itemData);
            Log($"Gave {finalCount}x {item.GetIdentifier()}.");
        });

        
        RegisterCommand("event", (di, args) =>
        {
            if(args.Length < 1)
            {
                Log("Usage: event <event_name>");
                return;
            }
            
            var eventName = args[0];
            
            var eventManager = di.Resolve<IGameEventManager>();
            
            var eventFound = GameEventRegistry.GetAll().FirstOrDefault(x => x.Id.ToLower().Contains(eventName.ToLower()));
            
            if(eventFound == null)
            {
                Log($"Event '{eventName}' not found.");
                return;
            }
            
            eventManager.TriggerEvent(eventFound);
        });
        
        RegisterCommand("eventlist", (di, args) =>
        {
            var eventFound = GameEventRegistry.GetAll();
            
            Log($"Available events:\n====\n{string.Join("\n", eventFound.Select(x => x.Id))}\n====");
        });


        RegisterCommand("venezuela", (di, args) =>
        {
            if (SceneManager.GetActiveScene().name == Scenes.LevelSelector)
            {
                Log("$$$");
                var crewManager = di.Resolve<ICrewManager>();
                crewManager.Resources.AddFuel(999);
                crewManager.Resources.AddJuice(999);
                crewManager.Resources.AddMoney(999);
            }
            else
            {
                Log("Only works in Level Selector scene.");
            }
        });

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