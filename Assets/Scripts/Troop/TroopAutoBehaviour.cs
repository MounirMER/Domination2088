using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class TroopAutoBehaviour : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [SerializeField]
    private NetworkVariable<NetworkObjectReference> targetRef = new(writePerm: NetworkVariableWritePermission.Server);

    private Transform target;

    private const float reachThreshold = 0.5f; // distance considérée comme "atteinte"

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.stoppingDistance = 0.3f; // ou un peu plus selon la taille de tes colliders
        }

        targetRef.OnValueChanged += OnTargetChanged;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (target == null)
        {
            if (targetRef.Value.TryGet(out NetworkObject netObj))
            {
                target = netObj.transform;
                Debug.Log($"[Troop][Recovery] Target assigned manually: {netObj.name} ({netObj.NetworkObjectId})");
            }
            else
            {
                return;
            }
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            animator.SetBool("Running", true);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                Debug.Log("[Troop] Destination reached (NavMesh), switching to player target");
                AssignPlayerTarget();
            }
        }
    }

    public void SetTarget(NetworkObject targetObject)
    {
        if (!IsServer || targetObject == null)
            return;

        if (targetRef.Value.TryGet(out NetworkObject existing) && existing == targetObject)
            return;

        targetRef.Value = targetObject;
        Debug.Log($"[Troop] SetTarget called with: {targetObject.name} ({targetObject.NetworkObjectId})");
    }

    private void OnTargetChanged(NetworkObjectReference oldVal, NetworkObjectReference newVal)
    {
        TryAssignTarget(newVal, "[Event]");
    }

    private void TryAssignTarget(NetworkObjectReference reference, string context = "")
    {
        if (reference.TryGet(out NetworkObject netObj))
        {
            target = netObj.transform;
            Debug.Log($"[Troop] Target assigned {context}: {netObj.name} ({netObj.NetworkObjectId})");
        }
        else
        {
            Debug.LogWarning($"[Troop] Failed to resolve targetRef {context}");
        }
    }

    private void AssignPlayerTarget()
    {
        ulong enemyClientId = NetworkManager.Singleton.LocalClientId == 0 ? 1UL : 0UL;

        foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var netObj = playerObj.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == enemyClientId)
            {
                SetTarget(netObj);
                Debug.Log($"[Troop] Nouvelle cible : {netObj.name}");
                return;
            }
        }

        Debug.LogWarning("[Troop] Aucun joueur adverse trouvé comme cible.");
    }

    private void OnDestroy()
    {
        targetRef.OnValueChanged -= OnTargetChanged;
    }
}
