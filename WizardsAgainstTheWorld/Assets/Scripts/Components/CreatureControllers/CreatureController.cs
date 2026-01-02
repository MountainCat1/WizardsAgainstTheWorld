using System.Collections.Generic;
using CreatureControllers;
using Markers;
using UnityEngine;
using Utilities;
using Zenject;

[RequireComponent(typeof(Creature))]
public abstract class CreatureController : EntityController
{
    public Creature Creature { get; protected set; }
    
    [Inject] private IPathfinding _pathfinding;

    protected override void Awake()
    {
        base.Awake();
        Creature = GetComponent<Creature>();
    }

    // TODO: this seems to be performance heavy, consider some optimizations
    // is it???
    public bool CanSee(Entity target)
    {
        var distance = Vector2.Distance(transform.position, target.transform.position); 
        
        // If the target is too far away, we can't see it
        if (distance > Creature.SightRange)
            return false;

        var layerMask = CollisionUtility.BlockingVisionLayerMask;
        var hit = Physics2D.Raycast(
            Creature.transform.position, target.transform.position - Creature.transform.position,
            distance,
            layerMask
        );
        return !hit;
    }


    public ICollection<Creature> GetCreatureInLine(Vector2 towards, float range)
    {
        var layerMask = CollisionUtility.HitMask;

        RaycastHit2D[] results = new RaycastHit2D[10];
        var size = Physics2D.RaycastNonAlloc(Creature.transform.position, towards - (Vector2)Creature.transform.position, results, range, layerMask);

        var creatures = new List<Creature>();
        
        foreach (var hit in results)
        {
            if (hit.collider == null)
                break;
            
            var creature = hit.collider.GetComponent<CreatureCollider>()?.Creature;
            
            if (creature == null)
                continue;
            
            if (creature == Creature)
                continue;
            
            creatures.Add(creature);
        }
        
        return creatures;
    }
    
}