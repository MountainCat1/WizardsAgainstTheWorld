namespace CreatureControllers
{
    public class BrainDeadUnitController : UnitController
    {
        protected override void Think()
        {
            // Brain-dead units do not think, they just stand still
        }
    }
}