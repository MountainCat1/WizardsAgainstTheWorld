using UnityEngine;
using Utilities;

public class DashAttack : BossAttack
{
    [Header("Dash Settings")]
    public float dashForce = 10f; // How strong the dash is

    protected override void ShowMarker(Vector2 position)
    {
        // No marker for dash
    }

    protected override void PerformActualAttack(Vector2 targetPosition)
    {
        GameLogger.Log("Boss is dashing!");

        // Determine direction from current position to target
        Vector2 direction = VectorUtilities.GetRandomDirection2D();

        // Use Movement.Push to dash
        bossCreature.Movement.Push(direction * dashForce);
    }
}