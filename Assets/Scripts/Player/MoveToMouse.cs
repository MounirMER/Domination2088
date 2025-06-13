using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MoveToMouse : NetworkBehaviour
{
    private static List<MoveToMouse> moveableObjects = new();
    private Transform[] spawnPoints;
    private bool hasSpawned = false;
    private bool selected = false;
    private Vector3 target;

    private NavMeshAgent agent;
    private bool agentEnabled = false;
    
    private Animator animator;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        moveableObjects.Add(this);
        target = transform.position;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && selected)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            if (NavMesh.SamplePosition(mouseWorld, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log("Déplacement vers : " + hit.position);
                animator.SetBool("Running", true);
            }
            else
            {
                Debug.LogWarning("Zone cliquée hors NavMesh");
            }
        }
    }

    private void OnMouseDown()
    {
        if (!IsOwner) return;
        selected = true;
        animator.SetBool("Selected", true);
        foreach (var obj in moveableObjects)
        {
            if (obj != this)
                obj.selected = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SpawnWhenReady());
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        if (hasSpawned) yield break;

        // Attendre que le NavMesh soit prêt
        while (NavMesh.CalculateTriangulation().vertices.Length == 0)
            yield return null;

        // Récupérer les spawn points
        var found = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (found.Length == 0) yield break;

        spawnPoints = found.OrderBy(go => go.name).Select(go => go.transform).ToArray();
        int index = (int)(OwnerClientId % (ulong)spawnPoints.Length);
        transform.position = spawnPoints[index].position;

        // Attente d'une frame supplémentaire pour garantir le placement sur le NavMesh
        yield return null;

        if (agent != null)
        {
            agent.enabled = true;

            // S'assurer explicitement que l'agent est sur le NavMesh
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
                else
                    Debug.LogError("Impossible de placer l'agent sur le NavMesh même après SamplePosition.");
            }
        }

        hasSpawned = true;
    }
}
