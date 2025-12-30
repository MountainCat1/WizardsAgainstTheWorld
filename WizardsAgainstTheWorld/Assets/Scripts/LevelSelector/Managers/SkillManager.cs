using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Skills;
using UnityEngine;
using Utilities;
using Zenject;

namespace LevelSelector.Managers
{
    public interface ISkillManager
    {
        event Action SkillsChanged;
        SkillDescriptor GetSkillDescriptor(string skillID);
        void AddSkillToCreature(CreatureData creature, SkillData skill);
    }
    
    public class SkillManager : MonoBehaviour, ISkillManager
    {
        private const int TargetAvailableSkills = 3;
     
        public event Action SkillsChanged; 
        
        [Inject] private ICrewManager _crewManager;
        
        private ICollection<CreatureData> Crew => _crewManager.Crew;
        
        private ICollection<SkillDescriptor> _skillDescriptors;

        private void Start()
        {
            _crewManager.Changed += UpdateSkills;

            var skillPrefabs = Resources.LoadAll<GameObject>("Skills");
            
            _skillDescriptors = new List<SkillDescriptor>();
            
            foreach (var skillPrefab in skillPrefabs)
            {
                var skillDescriptor = skillPrefab.GetComponent<SkillDescriptor>(); 
                
                if (skillDescriptor == null)
                {
                    GameLogger.LogError($"Skill prefab {skillPrefab.name} does not have a StatusEffect component.");
                }
                _skillDescriptors.Add(skillDescriptor);
            }
        }

        private void UpdateSkills()
        {
            foreach (var creature in Crew)
            {
                if (creature == null)
                    continue;

                // Update the skills of the creature
                UpdateCreatureSkills(creature);
            }
        }

        private void UpdateCreatureSkills(CreatureData creature)
        {
            for (int i = TargetAvailableSkills - creature.Level.AvailableSkills.Count; i > 0; i--)
            {
                var newSkill = GetNewSkillData(creature);
                creature.Level.AvailableSkills.Add(newSkill.ToData());
            }
        }

        private SkillDescriptor GetNewSkillData(CreatureData creature)
        {
            return _skillDescriptors.RandomElement();
        }
        
        public SkillDescriptor GetSkillDescriptor(string skillID)
        {
            return _skillDescriptors.FirstOrDefault(skill => skill.GetIdentifier() == skillID) 
                   ?? throw new ArgumentException($"Skill with ID {skillID} not found.");
        }

        public void AddSkillToCreature(CreatureData creature, SkillData skill)
        {
            if (creature.Level.PointsToUse < skill.Cost)
            {
                GameLogger.LogError($"Creature {creature.Name} does not have enough points to buy skill {skill.SkillID}");
                return;
            }
            
            creature.Level.BuySkill(skill);
            
            UpdateCreatureSkills(creature);
            
            SkillsChanged?.Invoke();
        }
    }
}