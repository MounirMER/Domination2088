using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MoveToMouse : NetworkBehaviour
{
    private Transform[] spawnPoints;
    private bool hasSpawned = false;
    
    public static List<MoveToMouse> moveableObjects = new List<MoveToMouse>();
    [SerializeField] private float speed = 5f;
    private Vector3 target;
    private bool selected;

    private void Start()
    {
        moveableObjects.Add(this);
        target = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && selected)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0; 
        }
        
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        selected = true;
        // gameObject.GetComponent<SpriteRenderer>().color = Color.red;

        foreach (MoveToMouse moveableObject in moveableObjects)
        {
            if (moveableObject != this)
            {
                moveableObject.selected = false;
                // moveableObject.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // Si on est déjà dans la scène de jeu, on peut tenter d'initialiser tout de suite
        TrySpawn();
        
        // Et on s'abonne à l'événement de chargement de scène au cas où on arrive après
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        // Nettoyage
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Dès que la nouvelle scène est prête, on retente le spawn
        TrySpawn();
    }

    private void TrySpawn()
    {
        if (hasSpawned) return;

        // Trouve tes SpawnPoint (tag "SpawnPoint")  
        var s = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (s.Length == 0) return;  // pas encore dispo → on sort et on réessaiera plus tard

        // Trie-les par nom, récupère uniquement leur Transform
        spawnPoints = s
            .OrderBy(go => go.name)
            .Select(go => go.transform)
            .ToArray();

        // Calcule un index stable à partir de l'OwnerClientId
        int index = (int)(OwnerClientId % (ulong)spawnPoints.Length);

        // Positionne ton joueur
        transform.position = spawnPoints[index].position;

        hasSpawned = true;         // on ne recommencera pas
    }
}
