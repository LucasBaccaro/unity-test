using UnityEngine;

/// <summary>
/// Tipos de zona en el MMO.
///
/// Safe: Zona segura (ciudad), PvP deshabilitado, no hay enemigos
/// Unsafe: Zona peligrosa, PvP habilitado, hay enemigos NPCs
/// </summary>
public enum ZoneType
{
    Safe,       // Ciudad segura
    Unsafe      // Zona peligrosa (PvP)
}

/// <summary>
/// Controlador base para zonas del MMO.
///
/// Este script se coloca en un GameObject con un Collider trigger que define
/// el área de la zona. Cuando un jugador entra o sale del trigger, se notifica
/// al jugador para que actualice su estado.
///
/// IMPORTANTE: NO es un NetworkBehaviour porque las zonas son estáticas,
/// existen solo en el servidor y no necesitan sincronizarse. Los jugadores
/// detectan la zona localmente usando triggers.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZoneController : MonoBehaviour
{
    #region Variables de Configuración

    [Header("Configuración de Zona")]
    [Tooltip("Tipo de zona (Safe o Unsafe)")]
    public ZoneType tipoZona = ZoneType.Safe;

    [Tooltip("Nombre de la zona (ej: 'Ciudad Principal', 'Bosque Oscuro')")]
    public string nombreZona = "Zona Sin Nombre";

    [Tooltip("Descripción de la zona")]
    [TextArea(2, 4)]
    public string descripcion = "";

    #endregion

    #region Variables Privadas

    // Referencia al collider (debe ser trigger)
    private Collider zoneCollider;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Inicializa el componente.
    /// Verifica que el collider sea un trigger.
    /// </summary>
    void Awake()
    {
        // Obtener collider
        zoneCollider = GetComponent<Collider>();

        // IMPORTANTE: El collider DEBE ser un trigger para detectar entradas/salidas
        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning($"[ZoneController] {nombreZona}: El collider NO es trigger. Se configurará automáticamente.");
            zoneCollider.isTrigger = true;
        }

        Debug.Log($"[ZoneController] Zona inicializada: {nombreZona} ({tipoZona})");
    }

    /// <summary>
    /// Se llama cuando otro collider ENTRA en el trigger de esta zona.
    ///
    /// IMPORTANTE: Este método se ejecuta tanto en el servidor como en los clientes,
    /// porque las zonas son objetos estáticos que existen en todas las instancias del juego.
    /// </summary>
    /// <param name="other">El collider que entró en la zona</param>
    void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró es un jugador
        ZoneDetector detector = other.GetComponent<ZoneDetector>();

        if (detector != null)
        {
            // Notificar al detector del jugador que entró a esta zona
            detector.EntrarZona(this);

            Debug.Log($"[ZoneController] {other.name} entró a zona: {nombreZona} ({tipoZona})");
        }
    }

    /// <summary>
    /// Se llama cuando otro collider SALE del trigger de esta zona.
    /// </summary>
    /// <param name="other">El collider que salió de la zona</param>
    void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que salió es un jugador
        ZoneDetector detector = other.GetComponent<ZoneDetector>();

        if (detector != null)
        {
            // Notificar al detector del jugador que salió de esta zona
            detector.SalirZona(this);

            Debug.Log($"[ZoneController] {other.name} salió de zona: {nombreZona}");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Verifica si el PvP está permitido en esta zona.
    /// </summary>
    /// <returns>True si el PvP está permitido, False si no</returns>
    public bool PvPPermitido()
    {
        return tipoZona == ZoneType.Unsafe;
    }

    /// <summary>
    /// Obtiene información de la zona en formato string.
    /// Útil para debugging y UI.
    /// </summary>
    public string ObtenerInfo()
    {
        return $"Zona: {nombreZona}\n" +
               $"Tipo: {tipoZona}\n" +
               $"PvP: {(PvPPermitido() ? "HABILITADO" : "DESHABILITADO")}\n" +
               $"Descripción: {descripcion}";
    }

    #endregion

    #region Debug

    /// <summary>
    /// Dibuja el área de la zona en el editor (solo visible en Scene view).
    /// Verde = Safe Zone, Rojo = Unsafe Zone
    /// </summary>
    void OnDrawGizmos()
    {
        // Color según tipo de zona
        Gizmos.color = tipoZona == ZoneType.Safe
            ? new Color(0, 1, 0, 0.3f)  // Verde semi-transparente
            : new Color(1, 0, 0, 0.3f); // Rojo semi-transparente

        // Dibujar el área del collider
        if (zoneCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            // Si es un BoxCollider
            BoxCollider boxCollider = zoneCollider as BoxCollider;
            if (boxCollider != null)
            {
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }

            // Si es un SphereCollider
            SphereCollider sphereCollider = zoneCollider as SphereCollider;
            if (sphereCollider != null)
            {
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            }
        }
    }

    /// <summary>
    /// Dibuja el nombre de la zona en el Scene view cuando está seleccionada.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Esto requiere usar Handles (solo en Editor)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"{nombreZona}\n({tipoZona})",
            new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = tipoZona == ZoneType.Safe ? Color.green : Color.red
                },
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            }
        );
        #endif
    }

    #endregion
}
