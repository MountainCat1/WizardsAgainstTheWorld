using Managers;
using Zenject;

namespace CreatureControllers
{
    public class HordeAttackerController : UnitController
    {
        protected override void UpdateMemory()
        {
            // base.UpdateMemory();

            foreach (var creatureManagerPlayerCreature in EntityManager.PlayerEntities)
            {
                Memorize(creatureManagerPlayerCreature);
            }
        }
    }
}