using Mirror;
using UnityEngine;

/// <summary>
/// Permite al jugador seleccionar su clase al inicio del juego.
///
/// NETWORKING: El cliente solicita una clase, el servidor valida y asigna.
/// Una vez asignada, la clase no se puede cambiar (para el MVP).
///
/// FLUJO:
/// 1. Jugador se conecta → aún no tiene clase
/// 2. UI muestra pantalla de selección de clase
/// 3. Jugador hace clic en una clase
/// 4. Se llama a CmdSeleccionarClase()
/// 5. Servidor valida y asigna la clase
/// 6. Servidor inicializa stats basadas en la clase
/// 7. Cliente oculta UI de selección y empieza a jugar
/// </summary>
public class PlayerClassSelector : NetworkBehaviour
{
    #region Variables de Red

    /// <summary>
    /// Si el jugador ya seleccionó su clase.
    /// Una vez true, no se puede volver a seleccionar.
    /// </summary>
    [SyncVar(hook = nameof(OnClaseSeleccionada))]
    private bool claseYaSeleccionada = false;

    #endregion

    #region Variables de Configuración

    [Header("Referencias a Clases")]
    [Tooltip("Asset del Mago")]
    public ClaseBase claseMago;

    [Tooltip("Asset del Paladin")]
    public ClaseBase clasePaladin;

    [Tooltip("Asset del Clerigo")]
    public ClaseBase claseClerigo;

    [Tooltip("Asset del Cazador")]
    public ClaseBase claseCazador;

    #endregion

    #region Referencias

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerStats == null)
        {
            Debug.LogError("[PlayerClassSelector] No se encontró PlayerStats!");
        }

        if (playerMovement == null)
        {
            Debug.LogError("[PlayerClassSelector] No se encontró PlayerMovement!");
        }
    }

    /// <summary>
    /// Se llama cuando el jugador local se spawnea.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Si aún no ha seleccionado clase, mostrar UI de selección
        if (!claseYaSeleccionada)
        {
            MostrarUISeleccionClase();
        }
    }

    #endregion

    #region UI de Selección

    /// <summary>
    /// Muestra la UI de selección de clase.
    /// Bloquea el movimiento del jugador hasta que seleccione.
    /// </summary>
    void MostrarUISeleccionClase()
    {
        Debug.Log("[PlayerClassSelector] Mostrando UI de selección de clase...");

        // Bloquear movimiento hasta que seleccione clase
        if (playerMovement != null)
        {
            playerMovement.PermitirMovimiento(false);
        }

        // TODO (FASE 12): Mostrar UI real de selección
        // UIManager.Instance.MostrarSeleccionClase(this);

        // Por ahora, log con las opciones
        Debug.Log("=== SELECCIÓN DE CLASE ===");
        Debug.Log("Opciones:");
        Debug.Log("1. Mago - Alto daño mágico, poca vida");
        Debug.Log("2. Paladin - Tanque con auto-curación");
        Debug.Log("3. Clerigo - Soporte/curación");
        Debug.Log("4. Cazador - Daño físico rápido");
        Debug.Log("==========================");
        Debug.Log("Por defecto se asignará Mago en 3 segundos...");

        // Auto-seleccionar Mago después de 3 segundos (temporal para testing)
        // Invoke(nameof(SeleccionarMagoAutomatico), 3f);
    }

    /// <summary>
    /// Oculta la UI de selección de clase.
    /// Desbloquea el movimiento del jugador.
    /// </summary>
    void OcultarUISeleccionClase()
    {
        Debug.Log("[PlayerClassSelector] Clase seleccionada. Desbloqueando movimiento...");

        // Desbloquear movimiento
        if (playerMovement != null)
        {
            playerMovement.PermitirMovimiento(true);
        }

        // TODO (FASE 12): Ocultar UI
        // UIManager.Instance.OcultarSeleccionClase();
    }

    #endregion

    #region Selección de Clase (Para llamar desde UI)

    /// <summary>
    /// Selecciona la clase MAGO.
    /// Llama este método desde los botones de UI.
    /// </summary>
    public void SeleccionarMago()
    {
        if (claseYaSeleccionada)
        {
            Debug.LogWarning("[PlayerClassSelector] Ya has seleccionado una clase.");
            return;
        }

        Debug.Log("[PlayerClassSelector] Seleccionando Mago...");
        CmdSeleccionarClase("Mago");
    }

    /// <summary>
    /// Selecciona la clase PALADIN.
    /// </summary>
    public void SeleccionarPaladin()
    {
        if (claseYaSeleccionada)
        {
            Debug.LogWarning("[PlayerClassSelector] Ya has seleccionado una clase.");
            return;
        }

        Debug.Log("[PlayerClassSelector] Seleccionando Paladin...");
        CmdSeleccionarClase("Paladin");
    }

    /// <summary>
    /// Selecciona la clase CLERIGO.
    /// </summary>
    public void SeleccionarClerigo()
    {
        if (claseYaSeleccionada)
        {
            Debug.LogWarning("[PlayerClassSelector] Ya has seleccionado una clase.");
            return;
        }

        Debug.Log("[PlayerClassSelector] Seleccionando Clerigo...");
        CmdSeleccionarClase("Clerigo");
    }

    /// <summary>
    /// Selecciona la clase CAZADOR.
    /// </summary>
    public void SeleccionarCazador()
    {
        if (claseYaSeleccionada)
        {
            Debug.LogWarning("[PlayerClassSelector] Ya has seleccionado una clase.");
            return;
        }

        Debug.Log("[PlayerClassSelector] Seleccionando Cazador...");
        CmdSeleccionarClase("Cazador");
    }

    /// <summary>
    /// Método temporal para auto-seleccionar Mago (solo para testing).
    /// Se puede eliminar cuando tengamos UI real.
    /// </summary>
    void SeleccionarMagoAutomatico()
    {
        if (!claseYaSeleccionada && isLocalPlayer)
        {
            Debug.Log("[PlayerClassSelector] Auto-seleccionando Mago (temporal)...");
            SeleccionarMago();
        }
    }

    #endregion

    #region Commands (Cliente → Servidor)

    /// <summary>
    /// Command para solicitar al servidor que asigne una clase.
    ///
    /// NETWORKING: El cliente solicita, el servidor valida y ejecuta.
    /// Esto previene que el cliente haga trampa eligiendo múltiples clases
    /// o modificando las stats directamente.
    /// </summary>
    /// <param name="nombreClase">Nombre de la clase a seleccionar</param>
    [Command]
    void CmdSeleccionarClase(string nombreClase)
    {
        // Verificar que no haya seleccionado ya una clase
        if (claseYaSeleccionada)
        {
            Debug.LogWarning($"[SERVER] {GetComponent<PlayerController>().nombreJugador} intentó seleccionar clase pero ya tiene una.");
            return;
        }

        Debug.Log($"[SERVER] {GetComponent<PlayerController>().nombreJugador} solicita clase: {nombreClase}");

        // Obtener el asset de la clase según el nombre
        ClaseBase claseSeleccionada = ObtenerClasePorNombre(nombreClase);

        if (claseSeleccionada == null)
        {
            Debug.LogError($"[SERVER] Clase '{nombreClase}' no encontrada!");
            return;
        }

        // Aplicar la clase al jugador
        AplicarClase(claseSeleccionada);

        // Marcar que ya seleccionó clase
        claseYaSeleccionada = true;

        Debug.Log($"[SERVER] Clase '{nombreClase}' asignada exitosamente.");

        // Notificar al cliente para que actualice su UI y habilidades locales
        TargetAsignarClase(connectionToClient, nombreClase);
    }

    #endregion

    #region Aplicación de Clase (Server)

    /// <summary>
    /// Aplica una clase al jugador.
    /// Solo se ejecuta en el servidor.
    /// </summary>
    /// <param name="clase">El asset de la clase a aplicar</param>
    [Server]
    void AplicarClase(ClaseBase clase)
    {
        if (clase == null || playerStats == null)
        {
            Debug.LogError("[PlayerClassSelector] No se puede aplicar clase: clase o playerStats es null!");
            return;
        }

        // Inicializar stats con la clase
        playerStats.InicializarConClase(clase);

        // Aplicar velocidad de movimiento
        if (playerMovement != null)
        {
            playerMovement.velocidadMovimiento = clase.speedBase;
        }

        // Asignar habilidades de la clase (FASE 5)
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
             if (clase.habilidades.Length > 0) combat.habilidad1 = clase.habilidades[0];
             if (clase.habilidades.Length > 1) combat.habilidad2 = clase.habilidades[1];
             Debug.Log($"[SERVER] Habilidades asignadas: {combat.habilidad1?.name}, {combat.habilidad2?.name}");
        }

        Debug.Log($"[SERVER] Clase '{clase.nombreClase}' aplicada con éxito.");
    }

    /// <summary>
    /// Obtiene un asset de clase por su nombre.
    /// </summary>
    ClaseBase ObtenerClasePorNombre(string nombre)
    {
        switch (nombre.ToLower())
        {
            case "mago":
                return claseMago;
            case "paladin":
                return clasePaladin;
            case "clerigo":
                return claseClerigo;
            case "cazador":
                return claseCazador;
            default:
                Debug.LogError($"[PlayerClassSelector] Clase desconocida: {nombre}");
                return null;
        }
    }

    #endregion

    #region Client RPC

    [TargetRpc]
    void TargetAsignarClase(NetworkConnection target, string nombreClase)
    {
        Debug.Log($"[CLIENT] Recibida clase del servidor: {nombreClase}");
        ClaseBase clase = ObtenerClasePorNombre(nombreClase);
        
        if (clase != null)
        {
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat != null)
            {
                 if (clase.habilidades.Length > 0) combat.habilidad1 = clase.habilidades[0];
                 if (clase.habilidades.Length > 1) combat.habilidad2 = clase.habilidades[1];
                 
                 // Notificar a la UI
                 combat.NotificarCambioHabilidades();
            }
        }
    }

    #endregion

    #region SyncVar Hooks

    /// <summary>
    /// Se llama cuando claseYaSeleccionada cambia.
    /// </summary>
    void OnClaseSeleccionada(bool oldValue, bool newValue)
    {
        if (newValue && isLocalPlayer)
        {
            // Ocultar UI de selección
            OcultarUISeleccionClase();

            Debug.Log($"[CLIENT] ¡Clase seleccionada! Ya puedes jugar.");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Verifica si el jugador ya seleccionó clase.
    /// </summary>
    public bool YaSeleccionoClase()
    {
        return claseYaSeleccionada;
    }

    #endregion
}
