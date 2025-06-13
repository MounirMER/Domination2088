using Unity.Netcode;
using UnityEngine;

public class LaserProjectile : NetworkBehaviour
{
    private Transform target;
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;

    public void Initialize(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (!IsServer || target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            var health = target.GetComponent<HealthHandler>();
            if (health != null)
            {
                health.TakeDamageServerRpc(damage);
            }
            NetworkObject.Despawn();
        }
    }
}