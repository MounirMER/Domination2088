using Unity.Netcode;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    public ulong ownerClientId; // À définir manuellement dans l'inspecteur (0 pour joueur 1, 1 pour joueur 2)

    private void OnMouseDown()
    {
        if (NetworkManager.Singleton.LocalClientId != ownerClientId) return;

        //if (TroopSelectionUI.SelectedTroopPrefab != null)
        //{
        //    SpawnTroopServerRpc(TroopSelectionUI.SelectedTroopIndex);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnTroopServerRpc(int troopIndex, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != ownerClientId) return;

        //GameObject troopPrefab = TroopManager.Instance.GetTroopPrefab(troopIndex);
        //if (troopPrefab == null) return;

        //GameObject spawnedTroop = Instantiate(troopPrefab, transform.position, Quaternion.identity);
        //NetworkObject netObj = spawnedTroop.GetComponent<NetworkObject>();
        //netObj.Spawn();
    }
}