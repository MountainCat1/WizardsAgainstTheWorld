using Components;
using UnityEngine;

namespace Markers
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageableCollider : MonoBehaviour
    {
        public IDamageable Damagable { get; private set; }
        
        private void Awake()
        {
            Damagable = GetComponent<IDamageable>() ?? GetComponentInParent<IDamageable>();
        }
    }
}