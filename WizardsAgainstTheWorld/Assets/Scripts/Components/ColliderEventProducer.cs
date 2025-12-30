using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderEventProducer : MonoBehaviour
{
    public event Action<Collider2D> TriggerEnter;
    public event Action<Collider2D> TriggerExit;
    public event Action<Collider2D> TriggerStay;

    private void Start()
    {
        var collider2DAttached = GetComponent<Collider2D>();

        if (collider2DAttached == null)
            throw new Exception("ColliderEventProducer requires a Collider2D component");

        if (!collider2DAttached.isTrigger)
            throw new Exception("ColliderEventProducer requires a trigger collider");

        var rigidBody2D = GetComponent<Rigidbody2D>();
        if (rigidBody2D == null)
            throw new Exception("ColliderEventProducer requires a Rigidbody2D component");

        if (rigidBody2D.bodyType != RigidbodyType2D.Kinematic && rigidBody2D.bodyType != RigidbodyType2D.Dynamic)
            throw new Exception("ColliderEventProducer requires a Kinematic or Dynamic Rigidbody2D component");
    }

    private void OnValidate()
    {
        var collider2D = GetComponent<Collider2D>();
        if (collider2D != null && !collider2D.isTrigger)
        {
            Debug.LogWarning($"{nameof(ColliderEventProducer)}: Collider2D must be set to 'Is Trigger'.", this);
        }

        var rigidBody2D = GetComponent<Rigidbody2D>();
        if (rigidBody2D == null)
        {
            Debug.LogWarning($"{nameof(ColliderEventProducer)}: Rigidbody2D component is required.", this);
        }
        else if (rigidBody2D.bodyType != RigidbodyType2D.Kinematic && rigidBody2D.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning($"{nameof(ColliderEventProducer)}: Rigidbody2D must be Kinematic or Dynamic.", this);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerEnter?.Invoke(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TriggerExit?.Invoke(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TriggerStay?.Invoke(other);
    }
}