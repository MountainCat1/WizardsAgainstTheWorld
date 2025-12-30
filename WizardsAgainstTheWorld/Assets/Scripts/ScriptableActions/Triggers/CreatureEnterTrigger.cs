using Markers;
using Triggers;
using UnityEngine;

public class CreatureEnterTrigger : TriggerBase
{
    [SerializeField] private ColliderEventProducer[] eventProducers;
    [SerializeField] private Teams expectedTeam = Teams.Player;

    protected override void OnStart()
    {
        if (eventProducers == null)
        {
            GameLogger.LogError("Event producer is not assigned in CreatureEnterTrigger.");
            return;
        }

        foreach (var eventProducer in eventProducers)
        {
            eventProducer.TriggerEnter += OnCreatureEnter;
        }
    }

    private void OnCreatureEnter(Collider2D obj)
    {
        var creatureCollider = obj.GetComponent<CreatureCollider>();

        if (creatureCollider is null)
            return;

        var entity = creatureCollider.Entity;

        if (entity is not Creature creature)
            return;

        if (!creature.isActiveAndEnabled)
            return;

        if (creature.Team != expectedTeam)
            return;

        RunActions();
    }
}