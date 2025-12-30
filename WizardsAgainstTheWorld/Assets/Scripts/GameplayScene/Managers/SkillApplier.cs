using System.Collections.Generic;
using System.Linq;
using Skills;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ISkillApplier
    {
        void ApplySkillToCreature(Creature creature, CreatureData data);
    }
    
    public class SkillApplier : MonoBehaviour, ISkillApplier
    {
        private ICollection<SkillDescriptor> _skillDescriptors;
        
        [Inject] private ISpawnerManager _spawnerManager;
        
        private void Start()
        {
            _skillDescriptors = Resources
                .LoadAll<GameObject>("Skills")
                .Select(x => x.GetComponent<SkillDescriptor>())
                .ToArray();
        }

        public void ApplySkillToCreature(Creature creature, CreatureData data)
        {
            var skillDatas = data.Level.Skills;
            foreach (var skillData in skillDatas)
            {
                var skillDescriptor = _skillDescriptors
                    .FirstOrDefault(x => x.GetIdentifier() == skillData.SkillID);

                if (skillDescriptor == null)
                {
                    GameLogger.LogError($"Skill descriptor not found for ID: {skillData.SkillID}");
                    continue;
                }
                
                var skill = _spawnerManager.Spawn(skillDescriptor, creature.transform);
                
                if (skill == null)
                {
                    GameLogger.LogError($"Failed to spawn skill for ID: {skillData.SkillID}");
                    continue;
                }
                
                skill.ApplySkill(creature);
            }
            
        }
    }
}