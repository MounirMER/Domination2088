using Unity.Netcode;
using UnityEngine;
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
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Arena", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnClientClicked()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.StartClient();
    }
}