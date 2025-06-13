using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SpawnTroop : NetworkBehaviour
{
    [SerializeField] private GameObject troopPrefab;
    [SerializeField] private Canvas playerMenuCanvas;

    private void OnMouseDown()
    {
        if (!GetComponent<LineRenderer>().enabled) return;

        RequestSpawnTroopServerRpc(transform.position);

        // Désactiver les LineRenderers du joueur
        string playerLayer = IsHost ? "Player1" : "Player2";
        int layerNumber = LayerMask.NameToLayer(playerLayer);

        foreach (var spawner in GameObject.FindGameObjectsWithTag("Spawner"))
        {
            if (spawner.layer == layerNumber)
            {
                var lr = spawner.GetComponent<LineRenderer>();
                if (lr != null) lr.enabled = false;
            }
        }

        if (playerMenuCanvas != null)
            playerMenuCanvas.enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnTroopServerRpc(Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        SpawnTroopAtPosition(spawnPosition, rpcParams.Receive.SenderClientId);
    }

    private void SpawnTroopAtPosition(Vector3 spawnPosition, ulong clientId)
    {
        if (troopPrefab == null)
        {
            Debug.LogError("Troop prefab not found!");
            return;
        }

        GameObject troopInstance = Instantiate(troopPrefab, spawnPosition, Quaternion.identity);

        string layerName = clientId == 0 ? "Player1" : "Player2";
        troopInstance.layer = LayerMask.NameToLayer(layerName);
        
        NetworkObject networkObject = troopInstance.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("No NetworkObject on troop prefab!");
            return;
        }

        networkObject.SpawnWithOwnership(clientId);

        var behaviour = troopInstance.GetComponent<TroopAutoBehaviour>();
        var targetObject = transform.childCount > 0 ? transform.GetChild(0).GetComponent<NetworkObject>() : null;

        if (behaviour != null && targetObject != null)
        {
            behaviour.SetTarget(targetObject);
        }

        var agent = troopInstance.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        troopInstance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        CloseMenuClientRpc(clientId);
    }

    [ClientRpc]
    private void CloseMenuClientRpc(ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        if (playerMenuCanvas != null)
            playerMenuCanvas.enabled = false;
    }
}
