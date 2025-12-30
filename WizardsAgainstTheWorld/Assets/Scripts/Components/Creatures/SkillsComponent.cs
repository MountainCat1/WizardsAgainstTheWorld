namespace Components.Creatures
{
    public enum StatsType
    {
        Percentage, // +20% speed
        Flat, // +0.4 armor
        Benefit // Fire Bullets
    }

    public enum Stat
    {
        Speed,
        DamageModifier,
        AttackSpeedModifier,
        AccuracyFlat,
        ArmorFlat,
        ArmorPercentage
    }
    
    public class SkillsComponent
    {
        // TODO: Move Skill Logic From Creature to here
    }
}