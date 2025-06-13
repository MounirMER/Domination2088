using System;
using Unity.Netcode;
using UnityEngine;

public class OpenMenu : NetworkBehaviour
{
    private Canvas thisCanvas;
    public Canvas playerMenuCanvas;
    public ulong ownerClientId;
    public Sprite openMenuSprite;
    public Sprite closeMenuSprite;
    
    // désactiver le canvas si il n'est pas le propriétaire
    private void Awake()
    {
        thisCanvas = GetComponentInParent<Canvas>();
        if (thisCanvas == null)
        {
            Debug.LogError("Canvas component not found on OpenMenu script.");
            return;
        }
        
        if (ownerClientId != NetworkManager.Singleton.LocalClientId)
            thisCanvas.enabled = false; // Start with the menu closed
    }

    public void OpenPlayerMenu()
    {
        if (NetworkManager.Singleton.LocalClientId != ownerClientId) return;
        
        playerMenuCanvas.enabled = !playerMenuCanvas.enabled;
        
        GetComponent<UnityEngine.UI.Image>().sprite = playerMenuCanvas.enabled ? openMenuSprite : closeMenuSprite;
    }
}
