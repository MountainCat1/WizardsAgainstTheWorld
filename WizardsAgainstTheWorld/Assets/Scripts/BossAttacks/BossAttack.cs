using UnityEngine;
using System.Collections;

public abstract class BossAttack : MonoBehaviour
{
    [Header("Attack Timing")]
    public float markerDuration = 2.0f; // How long the marker is visible
    public float attackDelayAfterMarker = 0.5f; // Delay between marker disappearing and attack hitting
    public float attackEffectDuration = 1.0f; // How long the attack visual/effect lasts (if any)

    [Header("Attack Properties")]
    public int damageAmount = 10;

    protected GameObject CurrentMarkerInstance;
    protected GameObject CurrentAttackEffectInstance;
    public bool IsAttacking { get; protected set; } = false;

    public Creature bossCreature; // Reference to the boss creature

    // --- Public method to start the attack ---
    public Coroutine StartAttack(Vector3 targetPosition)
    {
        if (IsAttacking)
        {
            GameLogger.LogWarning($"{gameObject.name}: Tried to start attack while already attacking.");
            return null;
        }
        IsAttacking = true;
        return StartCoroutine(AttackSequence(targetPosition));
    }

    // --- Attack Sequence Coroutine ---
    protected virtual IEnumerator AttackSequence(Vector3 targetPosition)
    {
        // 1. Show Marker Phase
        ShowMarker(targetPosition);
        if (CurrentMarkerInstance)
            yield return new WaitForSeconds(markerDuration);
        else
            GameLogger.LogWarning("No marker instance was created by ShowMarker(). Skipping marker duration.");

        // 2. Hide Marker (or transition)
        HideMarker();
        yield return new WaitForSeconds(attackDelayAfterMarker);

        // 3. Perform Actual Attack Phase
        PerformActualAttack(targetPosition);
        if (CurrentAttackEffectInstance)
            yield return new WaitForSeconds(attackEffectDuration);
        else
            GameLogger.LogWarning("No attack effect instance was created. Skipping effect duration.");

        // 4. Cleanup
        CleanupAttack();
        IsAttacking = false;
        OnAttackFinished(); // Callback for when the attack is fully done
    }

    // --- Abstract methods to be implemented by derived classes ---

    /// <summary>
    /// Called to display the attack marker at the target position.
    /// Should instantiate and store the marker in 'currentMarkerInstance'.
    /// </summary>
    protected abstract void ShowMarker(Vector2 position);

    /// <summary>
    /// Called to perform the actual attack logic (damage, effects, etc.).
    /// Should instantiate and store any visual effect in 'currentAttackEffectInstance'.
    /// </summary>
    protected abstract void PerformActualAttack(Vector2 targetPosition);


    // --- Virtual methods for optional overrides ---

    /// <summary>
    /// Called to hide/destroy the marker.
    /// </summary>
    protected virtual void HideMarker()
    {
        if (CurrentMarkerInstance != null)
        {
            Destroy(CurrentMarkerInstance);
            CurrentMarkerInstance = null;
        }
    }

    /// <summary>
    /// Called to clean up any attack effects.
    /// </summary>
    protected virtual void CleanupAttack()
    {
    }

    /// <summary>
    /// Optional callback when the entire attack sequence is finished.
    /// </summary>
    protected virtual void OnAttackFinished()
    {
        // Can be used by a BossController to know when it can start another attack
        GameLogger.Log($"{gameObject.name} attack finished.");
    }
}