using Unity.Netcode;
using UnityEngine;

public class HealthHandler : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new(writePerm: NetworkVariableWritePermission.Server);

    public bool IsDead => currentHealth.Value <= 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        if (newVal <= 0)
        {
            Die();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int amount)
    {
        currentHealth.Value = Mathf.Max(currentHealth.Value - amount, 0);
    }

    private void Die()
    {
        if (IsServer)
            NetworkObject.Despawn(true);
    }

    private void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
}