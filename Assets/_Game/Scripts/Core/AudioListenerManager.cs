using UnityEngine;
using Mirror;

/// <summary>
/// Asegura que solo el jugador local tenga AudioListener activo.
/// Previene el warning de múltiples AudioListeners.
/// </summary>
[RequireComponent(typeof(AudioListener))]
public class AudioListenerManager : NetworkBehaviour
{
    private AudioListener audioListener;

    void Awake()
    {
        audioListener = GetComponent<AudioListener>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        // Solo el jugador local debe tener AudioListener activo
        if (audioListener != null)
        {
            audioListener.enabled = true;
            Debug.Log("[AudioListenerManager] AudioListener activado para jugador local.");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Desactivar AudioListener para jugadores remotos
        if (!isLocalPlayer && audioListener != null)
        {
            audioListener.enabled = false;
            Debug.Log("[AudioListenerManager] AudioListener desactivado para jugador remoto.");
        }
    }
}
