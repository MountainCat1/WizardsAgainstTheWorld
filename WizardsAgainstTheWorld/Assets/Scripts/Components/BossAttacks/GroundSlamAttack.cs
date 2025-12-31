using Components;
using Managers;
using Markers;
using UnityEngine;
using Zenject;

public class GroundSlamAttack : BossAttack
{
    [Header("Ground Slam Specifics")]
    public GameObject markerPrefab; // Assign a prefab for the marker (e.g., a red circle decal)
    public GameObject attackEffectPrefab; // Assign a prefab for the slam effect (e.g., particles, shockwave)
    public float attackRadius = 3f;
    public LayerMask playerLayer; // Set this in the inspector to your Player's layer

    [Inject] private ISpawnerManager _spawnerManager;
    
    protected override void ShowMarker(Vector2 position)
    {
        if (markerPrefab != null)
        {
            // Ensure marker is on the ground (raycast down if needed, or just set y=0 for flat ground)
            // For simplicity, we'll just use the y of the target position, assuming it's on the ground
            CurrentMarkerInstance = Instantiate(markerPrefab, position, Quaternion.identity);
            // You might want to rotate it to face upwards if it's a decal
            // currentMarkerInstance.transform.rotation = Quaternion.Euler(90, 0, 0);
            GameLogger.Log("Ground Slam Marker shown at: " + position);
        }
        else
        {
            GameLogger.LogError("Marker Prefab not assigned for GroundSlamAttack!");
        }
    }

    protected override void PerformActualAttack(Vector2 targetPosition)
    {
        GameLogger.Log("Ground Slam ATTACK at: " + targetPosition);

        // Instantiate visual effect
        if (attackEffectPrefab != null)
        {
            CurrentAttackEffectInstance = _spawnerManager.Spawn(attackEffectPrefab, targetPosition);
        }
        else
        {
            GameLogger.LogWarning("Attack Effect Prefab not assigned for GroundSlamAttack.");
        }

        // Apply damage in an area
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPosition, attackRadius, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            // Assuming player has a PlayerHealth script
            var creatureCollider = hitCollider.GetComponent<EntityCollider>();
            if(creatureCollider is null)
                continue;
            
            var damagable = creatureCollider.Entity as IDamageable;
            if (damagable != null)
            {
                GameLogger.Log($"Ground Slam hit player: {hitCollider.name} for {damageAmount} damage!");
                var attackCtx = new AttackContext()
                {
                    Attacker = bossCreature,
                    Target = creatureCollider.Entity as Creature,
                    Direction = ((Vector2)bossCreature.transform.position - targetPosition).normalized,
                    TargetPosition = targetPosition,
                };
                var hitContext = new HitContext(
                    attacker: bossCreature,
                    target: damagable,
                    damage: damageAmount,
                    position: hitCollider.transform.position,
                    pushFactor: 1f // Push factor can be adjusted if needed
                );
                
                damagable.Health.Damage(hitContext);
            }
        }
        // Add sound effects, screen shake, etc. here
    }

    // Optional: Custom cleanup if needed
    // protected override void CleanupAttack()
    // {
    //     base.CleanupAttack(); // Calls the base cleanup for attack effect
    //     // Add any GroundSlam specific cleanup here
    // }

    // Optional: Custom marker hide if needed
    // protected override void HideMarker()
    // {
    //    base.HideMarker();
    //    // Add any GroundSlam specific marker hiding logic
    // }

    void OnDrawGizmosSelected()
    {
        // Draw the attack radius in the editor for easy visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius); // Shows radius from where attack is attached
    }
}