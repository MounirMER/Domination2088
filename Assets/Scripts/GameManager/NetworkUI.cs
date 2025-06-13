using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Arena", LoadSceneMode.Single);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;

        var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .OrderBy(sp => sp.name).ToArray();

        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Vector3 spawnPosition = spawnPoints[playerCount % spawnPoints.Length].transform.position;

        response.Position = spawnPosition;
    }


    private void OnClientClicked()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.StartClient();
    }
}