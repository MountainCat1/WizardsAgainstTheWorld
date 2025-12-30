using System.Collections.Generic;
using UnityEngine;

public enum Teams
{
    Passive,
    Player,
    Undead,
    Aliens,
    Ai,
    Bandits,
    Federation,
    SpawnedAlly,
    Cultists
}

public enum Attitude
{
    Friendly,
    Neutral,
    Hostile
}

namespace Managers
{
    public interface ITeamManager
    {
        Attitude GetAttitude(Teams team1, Teams team2);
    }
    
    public class TeamManager : MonoBehaviour, ITeamManager
    {
        // Public Constants

        // Static Variables and Methods

        // Public Variables

        // Serialized Private Variables

        // Injected Dependencies (using Zenject)

        // Private Variables
        private readonly Dictionary<Teams, Dictionary<Teams, Attitude>> _relations = new();
        
        // Properties

        // Events

        // Unity Callbacks
        private void Awake()
        {
            // First set all relations to neutral
            foreach (Teams team1 in System.Enum.GetValues(typeof(Teams)))
            {
                _relations[team1] = new Dictionary<Teams, Attitude>();
                foreach (Teams team2 in System.Enum.GetValues(typeof(Teams)))
                {
                    _relations[team1][team2] = Attitude.Neutral;
                }
            }

            // Then set all relations to friendly
            foreach (Teams team in System.Enum.GetValues(typeof(Teams)))
            {
                _relations[team][team] = Attitude.Friendly;
            }
            
            // Cultists
            AddRelation(Teams.Cultists, Teams.Player, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.Ai, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.Bandits, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.Undead, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.Aliens, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.Federation, Attitude.Hostile);
            AddRelation(Teams.Cultists, Teams.SpawnedAlly, Attitude.Hostile);
            
            // Player
            AddRelation(Teams.Player, Teams.Ai, Attitude.Hostile);
            AddRelation(Teams.Player, Teams.Bandits, Attitude.Hostile);
            AddRelation(Teams.Player, Teams.Undead, Attitude.Hostile);
            AddRelation(Teams.Player, Teams.Aliens, Attitude.Hostile);
            AddRelation(Teams.Player, Teams.Federation, Attitude.Hostile);
            AddRelation(Teams.Player, Teams.SpawnedAlly, Attitude.Friendly);
            
            // Ai
            AddRelation(Teams.Ai, Teams.Bandits, Attitude.Hostile);
            AddRelation(Teams.Ai, Teams.Undead, Attitude.Hostile);
            AddRelation(Teams.Ai, Teams.Aliens, Attitude.Hostile);
            AddRelation(Teams.Ai, Teams.Federation, Attitude.Friendly);
            AddRelation(Teams.Ai, Teams.SpawnedAlly, Attitude.Friendly);
            
            // Bandits
            AddRelation(Teams.Bandits, Teams.Undead, Attitude.Hostile);
            AddRelation(Teams.Bandits, Teams.Aliens, Attitude.Hostile);
            AddRelation(Teams.Bandits, Teams.Federation, Attitude.Hostile);
            AddRelation(Teams.Bandits, Teams.SpawnedAlly, Attitude.Hostile);
            
            // Undead
            AddRelation(Teams.Undead, Teams.Aliens, Attitude.Hostile);
            AddRelation(Teams.Undead, Teams.Federation, Attitude.Hostile);
            AddRelation(Teams.Undead, Teams.SpawnedAlly, Attitude.Hostile);
            
            // Aliens
            AddRelation(Teams.Aliens, Teams.Federation, Attitude.Hostile);
            AddRelation(Teams.Aliens, Teams.SpawnedAlly, Attitude.Hostile);
            
            // Federation
            AddRelation(Teams.Federation, Teams.SpawnedAlly, Attitude.Hostile);
            
            // Spawned Ally
        }


        // Public Methods
        public Attitude GetAttitude(Teams team1, Teams team2)
        {
            if (team1 == Teams.Passive || team2 == Teams.Passive)
                return Attitude.Neutral;
            
            return _relations[team1][team2];
        }
        
        // Virtual Methods

        // Abstract Methods

        // Private Methods
        private void AddRelation(Teams team1, Teams team2, Attitude attitude)
        {
            _relations[team1][team2] = attitude;
            _relations[team2][team1] = attitude;
        }
        
        // Event Handlers
    }
}