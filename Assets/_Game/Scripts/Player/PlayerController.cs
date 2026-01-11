using Mirror;
using UnityEngine;

/// <summary>
/// Controlador principal del jugador en el MMO.
///
/// NETWORKING: Este script coordina TODOS los sistemas del jugador:
/// - Movimiento (PlayerMovement)
/// - Combate (PlayerCombat) - FASE 5
/// - Inventario (PlayerInventory) - FASE 4
/// - Stats (PlayerStats) - FASE 3
///
/// Este script actúa como "orquestador" - no hace todo el trabajo,
/// sino que coordina a otros componentes especializados.
///
/// IMPORTANTE: Hereda de NetworkBehaviour, lo que significa que existe
/// tanto en el servidor como en todos los clientes. Debemos tener cuidado
/// con qué código ejecutamos dónde.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerClassSelector))]
[RequireComponent(typeof(ZoneDetector))]
public class PlayerController : NetworkBehaviour
{
    #region Variables de Red (Sincronizadas)

    /// <summary>
    /// Nombre del jugador visible para todos.
    ///
    /// NETWORKING: [SyncVar] sincroniza esta variable automáticamente
    /// del servidor a TODOS los clientes. Solo el servidor puede modificarla.
    ///
    /// El 'hook' hace que OnNombreChanged() se llame cada vez que cambia.
    /// </summary>
    [SyncVar(hook = nameof(OnNombreChanged))]
    public string nombreJugador = "Jugador";

    /// <summary>
    /// ID único del jugador en esta sesión.
    /// Útil para identificar jugadores sin usar el nombre.
    /// </summary>
    [SyncVar]
    public int playerID;

    #endregion

    #region Referencias a Componentes

    [Header("Referencias")]
    [Tooltip("Componente de movimiento del jugador")]
    private PlayerMovement movimiento;

    // TODO (FASE 3): Añadir referencia a PlayerStats
    // private PlayerStats stats;

    // TODO (FASE 4): Añadir referencia a PlayerInventory
    // private PlayerInventory inventario;

    // TODO (FASE 5): Añadir referencia a PlayerCombat
    // private PlayerCombat combate;

    #endregion

    #region Configuración Visual

    [Header("Configuración Visual")]
    [Tooltip("Material para el jugador local (tu personaje)")]
    public Material materialJugadorLocal;

    [Tooltip("Material para jugadores remotos (otros jugadores)")]
    public Material materialJugadorRemoto;

    [Tooltip("Renderer del modelo del jugador")]
    private Renderer modelRenderer;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Se llama cuando el script se inicializa.
    /// Obtenemos referencias a todos los componentes que necesitamos.
    /// </summary>
    void Awake()
    {
        // Obtener componente de movimiento
        movimiento = GetComponent<PlayerMovement>();

        // Obtener renderer del modelo (el cubo por ahora)
        modelRenderer = GetComponentInChildren<Renderer>();

        Debug.Log("[PlayerController] Componentes inicializados.");
    }

    /// <summary>
    /// NETWORKING: Se llama cuando el objeto se spawnea en la red.
    /// Se ejecuta en TODOS los clientes y en el servidor.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log($"[CLIENT] PlayerController iniciado. Nombre: {nombreJugador}");

        // Configurar visual según si es jugador local o remoto
        ConfigurarVisual();
    }

    /// <summary>
    /// NETWORKING: Se llama SOLO para el jugador local (TU jugador).
    /// NO se ejecuta para los jugadores remotos que ves en pantalla.
    ///
    /// IMPORTANTE: Usa esto para configuración específica del jugador local,
    /// como configurar la cámara, habilitar controles, etc.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Debug.Log("[LOCAL PLAYER] ¡Este es TU jugador!");

        // Configurar cámara para seguir a este jugador
        ConfigurarCamara();

        // Generar nombre aleatorio si no tiene (temporal, en FASE 3 usaremos selección de clase)
        if (isServer)
        {
            nombreJugador = $"Jugador_{Random.Range(1000, 9999)}";
            playerID = (int)netId;
        }
        else
        {
            // Cliente solicita al servidor que le asigne nombre
            CmdAsignarNombre($"Jugador_{Random.Range(1000, 9999)}");
        }
    }

    #endregion

    #region Commands (Cliente → Servidor)

    /// <summary>
    /// Command para que el cliente solicite un nombre al servidor.
    ///
    /// NETWORKING: [Command] hace que este método se ejecute EN EL SERVIDOR,
    /// aunque sea llamado desde el cliente.
    ///
    /// FLUJO:
    /// 1. Cliente llama CmdAsignarNombre("MiNombre")
    /// 2. Mensaje se envía por red al servidor
    /// 3. Servidor ejecuta este método
    /// 4. Servidor modifica nombreJugador (SyncVar)
    /// 5. SyncVar se sincroniza a todos los clientes automáticamente
    ///
    /// IMPORTANTE: Los Commands SIEMPRE deben empezar con "Cmd" (convención de Mirror).
    /// </summary>
    /// <param name="nombre">El nombre que el cliente quiere usar</param>
    [Command]
    void CmdAsignarNombre(string nombre)
    {
        // Este código se ejecuta EN EL SERVIDOR
        Debug.Log($"[SERVER] Cliente solicita nombre: {nombre}");

        // TODO: Validar que el nombre no esté ya en uso
        // TODO: Validar que el nombre no tenga caracteres prohibidos

        // Asignar nombre (se sincronizará automáticamente a todos los clientes)
        nombreJugador = nombre;
        playerID = (int)netId;

        Debug.Log($"[SERVER] Nombre asignado: {nombreJugador} (ID: {playerID})");
    }

    #endregion

    #region SyncVar Hooks (Callbacks de Sincronización)

    /// <summary>
    /// Se llama automáticamente cuando 'nombreJugador' cambia.
    ///
    /// NETWORKING: Los hooks de SyncVar se ejecutan EN TODOS LOS CLIENTES
    /// cuando el servidor modifica el valor.
    ///
    /// Perfecto para actualizar UI o mostrar efectos visuales cuando algo cambia.
    /// </summary>
    /// <param name="nombreAnterior">Valor anterior del nombre</param>
    /// <param name="nombreNuevo">Nuevo valor del nombre</param>
    void OnNombreChanged(string nombreAnterior, string nombreNuevo)
    {
        Debug.Log($"[SYNCVAR] Nombre cambió: '{nombreAnterior}' → '{nombreNuevo}'");

        // TODO (FASE 12): Actualizar UI con el nuevo nombre
        // UIManager.Instance.ActualizarNombreJugador(nombreNuevo);
    }

    #endregion

    #region Métodos de Configuración

    /// <summary>
    /// Configura la cámara para seguir a este jugador.
    /// Solo se llama para el jugador local.
    /// </summary>
    void ConfigurarCamara()
    {
        // Buscar la cámara principal
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // TODO: Implementar cámara que siga al jugador
            // Por ahora, simplemente posicionamos la cámara arriba y atrás del jugador
            mainCamera.transform.position = transform.position + new Vector3(0, 10, -10);
            mainCamera.transform.LookAt(transform);

            Debug.Log("[LOCAL PLAYER] Cámara configurada.");
        }
        else
        {
            Debug.LogWarning("[LOCAL PLAYER] No se encontró Camera.main en la escena.");
        }
    }

    /// <summary>
    /// Configura el material visual del jugador.
    /// Jugador local (verde) vs jugadores remotos (azul).
    /// </summary>
    void ConfigurarVisual()
    {
        if (modelRenderer == null) return;

        // Si este es nuestro jugador local, usar material verde
        // Si es un jugador remoto, usar material azul
        if (isLocalPlayer)
        {
            if (materialJugadorLocal != null)
            {
                modelRenderer.material = materialJugadorLocal;
                Debug.Log("[LOCAL PLAYER] Material verde aplicado.");
            }
        }
        else
        {
            if (materialJugadorRemoto != null)
            {
                modelRenderer.material = materialJugadorRemoto;
                Debug.Log("[REMOTE PLAYER] Material azul aplicado.");
            }
        }
    }

    #endregion

    #region Debug y Utilidades

    /// <summary>
    /// Método helper para obtener información del jugador.
    /// Útil para debugging.
    /// </summary>
    public string ObtenerInfo()
    {
        return $"Jugador: {nombreJugador} (ID: {playerID})\n" +
               $"Posición: {transform.position}\n" +
               $"IsLocalPlayer: {isLocalPlayer}\n" +
               $"IsOwned: {isOwned}\n" +
               $"IsServer: {isServer}";
    }

    private void OnGUI()
    {
        if (!isLocalPlayer) return;

        GUILayout.BeginArea(new Rect(10, 200, 300, 200));
        GUILayout.Box("PLAYER DEBUG");
        GUILayout.Label($"Nombre: {nombreJugador}");
        GUILayout.Label($"Pos: {transform.position}");
        GUILayout.Label($"Camera.main: {(Camera.main != null ? "FOUND" : "NULL")}");
        GUILayout.Label($"Renderer: {(modelRenderer != null ? "FOUND" : "NULL")}");
        if (movimiento != null)
        {
             GUILayout.Label($"Input H: {Input.GetAxis("Horizontal"):F2}");
             GUILayout.Label($"Input V: {Input.GetAxis("Vertical"):F2}");
        }
        GUILayout.EndArea();
    }

    #endregion
}
