using System;
using Constants;
using UnityEngine;
using Utilities;

namespace LevelSelector.Managers
{
    public interface ICrewGenerator
    {
        CreatureData GenerateCrew();
    }
    
    public class CrewGenerator : MonoBehaviour, ICrewGenerator
    {
        private static readonly Color[] CrewColors = new[]
        {
            new Color(0.47f, 0f, 0f),
            new Color(0f, 0.42f, 0f),
            new Color(0f, 0f, 0.46f),
            new Color(0.45f, 0.39f, 0.02f),
            new Color(0f, 0.46f, 0.46f),
            new Color(0.43f, 0f, 0.43f),
            new Color(0.44f, 0.44f, 0.44f),
        };
        
        private int _lastColorIndex = 0;
        
        public CreatureData GenerateCrew()
        {
            return new CreatureData()
            {
                Name = $"{Names.Human.RandomElement()} {Surnames.Human.RandomElement()}",
                SightRange = 5f,
                Inventory = new InventoryData()
                {
                },
                CreatureID = Guid.NewGuid().ToString(),
                Color = new ColorData(CrewColors[_lastColorIndex++ % CrewColors.Length]),
                Level = new LevelData()
            };
        }
    }


}