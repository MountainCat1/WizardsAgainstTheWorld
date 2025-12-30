using System;
using System.Linq;
using System.Text;
using Managers;
using Newtonsoft.Json;
using Pathfinding.Serialization;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class DebugInfoPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [Inject] private IMapGenerator _mapGenerator;
        [Inject] private ITeamManager _teamManager;
        [Inject] private IEnemySpawner _enemySpawner;
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private JsonSerializerSettings _serializerSettings;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                text.gameObject.SetActive(!text.gameObject.activeSelf);
            }
            
            if(Input.GetKeyDown(KeyCode.F7))
            {
                SaveLocationData();
            }

            if (!text.gameObject.activeSelf)
            {
                return;
            }
            
            if (text == null)
            {
                GameLogger.LogError("DebugInfoPanelUI: Text component is not assigned.");
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"Map Seed: {_mapGenerator.Settings.seed}");
            sb.AppendLine($"MapSettings Seed: {GameManager.GameSetup.Location.MapSettings.seed}");
            // sb.AppendLine($"Region Seed: {_mapGenerator.Settings.regionSeed}");
            sb.AppendLine($"Difficulty: {GameSettings.Instance.Difficulty}");
            sb.AppendLine();
            sb.AppendLine($"Mana: {_enemySpawner.GatheredMana:F2}");
            sb.AppendLine($"Mana growth: {_enemySpawner.ManaGain:F2} per second");
            sb.AppendLine($"Creatures: {_creatureManager.GetCreatures().Count:D}");
            AddTeamCounts(sb);
            sb.AppendLine();
            sb.AppendLine($"Press F7 to save the location data to a file");
            
            text.text = sb.ToString();
        }

        private void AddTeamCounts(StringBuilder sb)
        {
            sb.AppendLine("Teams:");
            foreach (var team in Enum.GetValues(typeof(Teams)))
            {
                var creatureCount = _creatureManager.GetCreaturesAliveActive().Count(c => c.Team == (Teams)team);
                sb.AppendLine($"  {team}: {creatureCount}");
            }
        }

        private void SaveLocationData()
        {
            var path = $"{Application.persistentDataPath}/location_data.json";
            try
            {
                var locationData = GameManager.GameSetup.Location.MapSettings;
                string json = JsonConvert.SerializeObject(locationData, Formatting.Indented, _serializerSettings);
                
                System.IO.File.WriteAllText(path, json);
                GameLogger.Log($"Location data saved to: {path}");
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"Failed to save location data: {e.Message}");
            }
        }
    }
}