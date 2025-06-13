using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnTurret : NetworkBehaviour
{
    [SerializeField] private GameObject turretPrefab;

    private void OnMouseDown()
    {
        if (GetComponent<TilemapRenderer>() == null || !GetComponent<TilemapRenderer>().enabled) return;

        // Convertir le clic en position monde
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0; // s'assurer que la turret est bien posée sur le plan du jeu

        RequestTurretSpawnServerRpc(worldPos, NetworkManager.LocalClientId);

        GetComponent<TilemapRenderer>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTurretSpawnServerRpc(Vector3 spawnPosition, ulong senderClientId)
    {
        if (turretPrefab == null)
        {
            Debug.LogError("Turret prefab is null");
            return;
        }

        GameObject turretInstance = Instantiate(turretPrefab, spawnPosition, Quaternion.identity);

        // Assigne le bon layer en fonction du joueur
        string layerName = senderClientId == 0 ? "Player1" : "Player2";
        turretInstance.layer = LayerMask.NameToLayer(layerName);

        NetworkObject netObj = turretInstance.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.SpawnWithOwnership(senderClientId);
    }
}