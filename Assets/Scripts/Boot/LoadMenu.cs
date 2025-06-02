using UnityEngine;

public class LoadMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
