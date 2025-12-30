using System.Collections.Generic;
using Markers;
using UnityEngine;
using Utilities;
using Zenject;

[RequireComponent(typeof(Creature))]
public class CreatureController : MonoBehaviour
{
    public Creature Creature { get; protected set; }
    
    [Inject] private IPathfinding _pathfinding;

    protected virtual void Awake()
    {
        Creature = GetComponent<Creature>();
    }

    // TODO: this seems to be performance heavy, consider some optimizations
    // is it???
    public bool CanSee(Creature target)
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
        var layerMask = LayerMask.GetMask("CreatureHit");

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