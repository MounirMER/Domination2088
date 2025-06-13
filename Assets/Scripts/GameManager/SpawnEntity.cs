using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnEntity : NetworkBehaviour
{
    [SerializeField] private GameObject turretPrefab;

    public void ChoseTroop()
    {
        if (IsServer)
        {
            EnableSpawnersClientRpc(NetworkManager.LocalClientId);
        }
        else
        {
            RequestSpawnerEnableServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnerEnableServerRpc(ServerRpcParams rpcParams = default)
    {
        EnableSpawnersClientRpc(rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void EnableSpawnersClientRpc(ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        var playerLayer = clientId == 0 ? "Player1" : "Player2";
        int layerNumber = LayerMask.NameToLayer(playerLayer);

        var spawners = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (var spawner in spawners)
        {
            if (spawner.layer == layerNumber)
            {
                spawner.GetComponent<LineRenderer>().enabled = true;
            }
        }
    }

    public void ChoseTurret()
    {
        RequestTurretEnableServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTurretEnableServerRpc(ulong clientId)
    {
        EnableTurretClientRpc(clientId);
    }

    [ClientRpc]
    private void EnableTurretClientRpc(ulong clientId)
    {
        if (NetworkManager.LocalClientId != clientId) return;

        var spots = GameObject.FindGameObjectWithTag("TurretCanSpawn");
        if (spots == null)
        {
            Debug.LogWarning("Aucun objet avec le tag 'TurretCanSpawn' trouvé.");
            return;
        }

        var renderer = spots.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            Debug.Log("TilemapRenderer activé pour le client : " + clientId);
        }

        var playerMenuCanvas = GameObject.FindGameObjectWithTag(clientId == 0 ? "MenuPlayer1" : "MenuPlayer2");
        if (playerMenuCanvas != null)
        {
            var canvas = playerMenuCanvas.GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            var spots = GameObject.FindGameObjectWithTag("TurretCanSpawn");
            if (spots == null) return;

            var renderer = spots.GetComponent<TilemapRenderer>();
            if (renderer == null || !renderer.enabled) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            if (Physics2D.OverlapPoint(mouseWorld)) // tu peux remplacer par un check de tilemap aussi
            {
                RequestPlaceTurretServerRpc(mouseWorld);
                renderer.enabled = false; // Cache le highlight une fois le clic effectué
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlaceTurretServerRpc(Vector3 position)
    {
        if (turretPrefab == null) return;

        GameObject turret = Instantiate(turretPrefab, position, Quaternion.identity);
        turret.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.LocalClientId);
    }
}
