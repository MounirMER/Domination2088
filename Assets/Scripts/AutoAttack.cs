// AutoAttack.cs
using Unity.Netcode;
using UnityEngine;

public class AutoAttack : NetworkBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    private Animator animator;
    private Transform currentTarget;
    private float attackCooldown = 1.5f;
    private float nextAttackTime;
    private Transform firePoint;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        firePoint = transform.childCount > 0 ? transform.GetChild(0) : null;
    }

    private void Update()
    {
        if (!IsServer || currentTarget == null) return;

        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Shoot()
    {
        if (firePoint == null || laserPrefab == null || currentTarget == null) return;

        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);

        var laserNetwork = laser.GetComponent<NetworkObject>();
        if (laserNetwork != null)
        {
            laserNetwork.Spawn();
        }

        var laserScript = laser.GetComponent<LaserProjectile>();
        if (laserScript != null)
        {
            laserScript.Initialize(currentTarget);
        }

        animator?.SetBool("Shooting", true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (!IsValidTarget(other.gameObject)) return;

        var otherOwner = other.GetComponent<NetworkObject>()?.OwnerClientId ?? ulong.MaxValue;
        if (otherOwner == NetworkObject.OwnerClientId) return;

        currentTarget = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer || currentTarget == null) return;

        if (other.transform == currentTarget)
        {
            currentTarget = null;
        }
    }

    private bool IsValidTarget(GameObject go)
    {
        // Ignore les objets sur le même layer (même joueur)
        return gameObject.layer != go.layer &&
               (
                   (gameObject.CompareTag("Player") && (go.CompareTag("Player") || go.CompareTag("Troop"))) ||
                   (gameObject.CompareTag("Troop") && go.CompareTag("Player")) ||
                   (gameObject.CompareTag("Turret") && (go.CompareTag("Player") || go.CompareTag("Troop")))
               );
    }
}
