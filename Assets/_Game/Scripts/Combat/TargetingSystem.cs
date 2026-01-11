using Mirror;
using UnityEngine;

/// <summary>
/// Sistema de selección de objetivos (Targeting).
/// Permite al jugador seleccionar otros jugadores o NPCs con clic izquierdo o Tab.
/// </summary>
public class TargetingSystem : NetworkBehaviour
{
    #region Variables

    [Header("Configuración")]
    [Tooltip("Rango máximo para seleccionar un objetivo")]
    public float rangoSeleccion = 50f;
    
    [Tooltip("LayerMask para filtrar qué objetos se pueden seleccionar")]
    public LayerMask capasSeleccionables;

    [Header("Estado (Solo Local)")]
    // El objetivo actual seleccionado localmente
    public NetworkIdentity objetivoActual;

    #endregion

    #region Unity Callbacks

    void Update()
    {
        // Solo procesar input para el jugador local
        if (!isLocalPlayer) return;

        ManejarSeleccionMouse();
        ManejarDeshacerSeleccion();
    }

    #endregion

    #region Lógica de Selección

    /// <summary>
    /// Intenta seleccionar un objetivo bajo el mouse al hacer clic.
    /// </summary>
    void ManejarSeleccionMouse()
    {
        if (Input.GetMouseButtonDown(0)) // Clic Izquierdo
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Debug.Log($"[Targeting] Click detectado. Lanzando raycast...");

            if (Physics.Raycast(ray, out hit, rangoSeleccion, capasSeleccionables))
            {
                Debug.Log($"[Targeting] Raycast impactó: {hit.collider.gameObject.name}");

                // Verificar si tiene NetworkIdentity (es un objeto "vivo" en la red)
                NetworkIdentity identidad = hit.collider.GetComponentInParent<NetworkIdentity>();

                if (identidad != null && identidad != netIdentity) // No seleccionarse a sí mismo
                {
                    SeleccionarObjetivo(identidad);
                }
                else if (identidad == netIdentity)
                {
                    Debug.LogWarning($"[Targeting] No puedes seleccionarte a ti mismo.");
                }
                else
                {
                    Debug.LogWarning($"[Targeting] El objeto clickeado no tiene NetworkIdentity.");
                }
            }
            else
            {
                Debug.LogWarning($"[Targeting] Raycast no impactó nada. Verifica que el jugador tenga collider y esté en la capa correcta.");
            }
        }
    }

    /// <summary>
    /// Deselecciona si se presiona Escape.
    /// </summary>
    void ManejarDeshacerSeleccion()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LimpiarObjetivo();
        }
    }

    public void SeleccionarObjetivo(NetworkIdentity nuevoObjetivo)
    {
        if (objetivoActual != nuevoObjetivo)
        {
            objetivoActual = nuevoObjetivo;
            Debug.Log($"[Targeting] Objetivo seleccionado: {nuevoObjetivo.name}");
            
            // TODO: Notificar a la UI (TargetFrame)
            // TargetFrameUI.Instance.SetTarget(objetivoActual);
        }
    }

    public void LimpiarObjetivo()
    {
        if (objetivoActual != null)
        {
            objetivoActual = null;
            Debug.Log("[Targeting] Objetivo deseleccionado.");
            
            // TODO: Limpiar UI
            // TargetFrameUI.Instance.ClearTarget();
        }
    }

    #endregion
}
