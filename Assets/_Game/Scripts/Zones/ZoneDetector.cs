using Mirror;
using UnityEngine;

/// <summary>
/// Detector de zonas que se coloca en el jugador.
///
/// NETWORKING: Este componente detecta cuando el jugador entra/sale de zonas
/// y sincroniza la zona actual a través de la red.
///
/// El detector usa triggers locales para detectar zonas, pero luego informa
/// al servidor para que valide y sincronice la zona actual a todos los clientes.
///
/// IMPORTANTE: Debe estar en el mismo GameObject que tiene el CharacterController
/// del jugador (para que el trigger funcione correctamente).
/// </summary>
public class ZoneDetector : NetworkBehaviour
{
    #region Variables de Red (Sincronizadas)

    /// <summary>
    /// Zona actual en la que se encuentra el jugador.
    ///
    /// NETWORKING: [SyncVar] sincroniza la zona del servidor a todos los clientes.
    /// El hook OnZonaCambiada() se llama cuando el valor cambia.
    /// </summary>
    [SyncVar(hook = nameof(OnZonaCambiada))]
    private ZoneType zonaActual = ZoneType.Safe; // Por defecto en zona segura (spawn)

    /// <summary>
    /// Nombre de la zona actual (para mostrar en UI).
    /// </summary>
    [SyncVar]
    private string nombreZonaActual = "Ciudad Principal";

    #endregion

    #region Variables Privadas

    // Referencia a la zona en la que estamos actualmente
    private ZoneController zonaActualController;

    // Referencia al PlayerController
    private PlayerController playerController;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Inicializa componentes.
    /// </summary>
    void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("[ZoneDetector] No se encontró PlayerController en el mismo GameObject.");
        }
    }

    /// <summary>
    /// Se llama cuando el objeto se spawnea en la red.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Actualizar UI con la zona inicial
        ActualizarUIZona();
    }

    #endregion

    #region Detección de Zonas (Local)

    /// <summary>
    /// Llamado por ZoneController cuando el jugador ENTRA a una zona.
    ///
    /// NETWORKING: Este método se ejecuta localmente en todos los clientes,
    /// pero solo el cliente con authority (el dueño del jugador) debe
    /// informar al servidor.
    /// </summary>
    /// <param name="zona">El ZoneController de la zona a la que se entró</param>
    public void EntrarZona(ZoneController zona)
    {
        // Solo procesar si tenemos authority sobre este jugador
        if (!isOwned) return;

        Debug.Log($"[ZoneDetector] Entrando a zona: {zona.nombreZona} ({zona.tipoZona})");

        // Guardar referencia a la zona actual
        zonaActualController = zona;

        // Informar al servidor sobre el cambio de zona
        CmdCambiarZona(zona.tipoZona, zona.nombreZona);

        // Llamar a los scripts de zona (Safe/Unsafe)
        NotificarEntradaZona(zona);
    }

    /// <summary>
    /// Llamado por ZoneController cuando el jugador SALE de una zona.
    /// </summary>
    /// <param name="zona">El ZoneController de la zona de la que se salió</param>
    public void SalirZona(ZoneController zona)
    {
        // Solo procesar si tenemos authority sobre este jugador
        if (!isOwned) return;

        Debug.Log($"[ZoneDetector] Saliendo de zona: {zona.nombreZona}");

        // Si salimos de la zona actual, limpiar referencia
        if (zonaActualController == zona)
        {
            zonaActualController = null;

            // TODO: Determinar si entramos a otra zona o quedamos en "tierra de nadie"
            // Por ahora, asumimos que siempre estamos en alguna zona
        }
    }

    /// <summary>
    /// Notifica a los componentes SafeZone/UnsafeZone sobre la entrada del jugador.
    /// </summary>
    void NotificarEntradaZona(ZoneController zona)
    {
        // Notificar a SafeZone si existe
        SafeZone safeZone = zona.GetComponent<SafeZone>();
        if (safeZone != null)
        {
            safeZone.OnJugadorEntra(gameObject);
        }

        // Notificar a UnsafeZone si existe
        UnsafeZone unsafeZone = zona.GetComponent<UnsafeZone>();
        if (unsafeZone != null)
        {
            unsafeZone.OnJugadorEntra(gameObject);
        }
    }

    #endregion

    #region Commands (Cliente → Servidor)

    /// <summary>
    /// Command para informar al servidor sobre el cambio de zona.
    ///
    /// NETWORKING: El cliente detecta la zona localmente (más rápido, menos lag),
    /// pero el servidor valida y sincroniza a todos los clientes.
    ///
    /// FLUJO:
    /// 1. Cliente detecta trigger de zona (OnTriggerEnter)
    /// 2. Cliente llama CmdCambiarZona()
    /// 3. Servidor recibe y valida el cambio
    /// 4. Servidor actualiza zonaActual (SyncVar)
    /// 5. SyncVar se sincroniza a todos los clientes
    /// 6. Hook OnZonaCambiada() se ejecuta en todos los clientes
    /// </summary>
    /// <param name="nuevaZona">El tipo de zona (Safe o Unsafe)</param>
    /// <param name="nombreZona">El nombre de la zona</param>
    [Command]
    void CmdCambiarZona(ZoneType nuevaZona, string nombreZona)
    {
        // Este código se ejecuta EN EL SERVIDOR

        // TODO: Validar que el cambio de zona es legítimo
        // (el jugador realmente está en esa posición, no está hackeando)

        // Actualizar zona actual (se sincronizará automáticamente)
        zonaActual = nuevaZona;
        nombreZonaActual = nombreZona;

        Debug.Log($"[SERVER] {playerController.nombreJugador} cambió a zona: {nombreZona} ({nuevaZona})");

        // Notificar a otros sistemas sobre el cambio de zona
        NotificarCambioZonaServidor(nuevaZona);
    }

    /// <summary>
    /// Notifica a otros sistemas del servidor sobre el cambio de zona.
    /// Ejecuta lógica server-side cuando cambia la zona.
    /// </summary>
    [Server]
    void NotificarCambioZonaServidor(ZoneType nuevaZona)
    {
        // TODO (FASE 5): Si el jugador está en combate y entra a zona segura,
        // cancelar combate y limpiar targets
        // if (nuevaZona == ZoneType.Safe && playerCombat.EnCombate())
        // {
        //     playerCombat.CancelarCombate();
        // }

        // TODO (FASE 7): Si el jugador sale de zona segura,
        // los NPCs enemigos pueden detectarlo y atacarlo
    }

    #endregion

    #region SyncVar Hooks (Callbacks de Sincronización)

    /// <summary>
    /// Se llama automáticamente cuando 'zonaActual' cambia.
    ///
    /// NETWORKING: Se ejecuta en TODOS los clientes cuando el servidor
    /// modifica el valor de zonaActual.
    ///
    /// Perfecto para actualizar UI, cambiar comportamiento del jugador, etc.
    /// </summary>
    /// <param name="zonaAnterior">La zona anterior</param>
    /// <param name="zonaNueva">La nueva zona</param>
    void OnZonaCambiada(ZoneType zonaAnterior, ZoneType zonaNueva)
    {
        Debug.Log($"[SYNCVAR] Zona cambió: {zonaAnterior} → {zonaNueva}");

        // Actualizar UI
        ActualizarUIZona();

        // TODO (FASE 5): Actualizar estado de combate
        // Si entramos a zona segura, desactivar PvP
        // if (zonaNueva == ZoneType.Safe && playerCombat != null)
        // {
        //     playerCombat.DesactivarPvP();
        // }
        // else if (zonaNueva == ZoneType.Unsafe && playerCombat != null)
        // {
        //     playerCombat.ActivarPvP();
        // }

        // Mostrar efecto visual de cambio de zona (opcional)
        MostrarEfectoTransicionZona(zonaNueva);
    }

    #endregion

    #region Métodos de UI

    /// <summary>
    /// Actualiza la UI con información de la zona actual.
    /// </summary>
    void ActualizarUIZona()
    {
        // Solo actualizar UI para el jugador local
        if (!isLocalPlayer) return;

        // TODO (FASE 12): Actualizar UI de zona
        // UIManager.Instance.ActualizarZona(nombreZonaActual, zonaActual);

        // Por ahora, solo log
        string estadoPvP = zonaActual == ZoneType.Safe ? "DESHABILITADO" : "HABILITADO";
        Debug.Log($"[UI] Zona actual: {nombreZonaActual} | PvP: {estadoPvP}");
    }

    /// <summary>
    /// Muestra un efecto visual cuando se cambia de zona.
    /// </summary>
    void MostrarEfectoTransicionZona(ZoneType zonaNueva)
    {
        // Solo para el jugador local
        if (!isLocalPlayer) return;

        // TODO (FASE 12): Implementar efecto visual
        // - Flash de pantalla (verde para safe, rojo para unsafe)
        // - Sonido de transición
        // - Partículas en el jugador

        // Por ahora, solo cambiar color del jugador temporalmente
        // (esto se puede ver visualmente)
        Color colorFlash = zonaNueva == ZoneType.Safe ? Color.green : Color.red;
        // TODO: Aplicar color flash
    }

    #endregion

    #region Métodos Públicos (Getters)

    /// <summary>
    /// Obtiene la zona actual en la que está el jugador.
    /// </summary>
    public ZoneType ObtenerZonaActual()
    {
        return zonaActual;
    }

    /// <summary>
    /// Obtiene el nombre de la zona actual.
    /// </summary>
    public string ObtenerNombreZonaActual()
    {
        return nombreZonaActual;
    }

    /// <summary>
    /// Verifica si el jugador está en una zona segura.
    /// </summary>
    public bool EstaEnZonaSegura()
    {
        return zonaActual == ZoneType.Safe;
    }

    /// <summary>
    /// Verifica si el PvP está permitido en la zona actual.
    /// Útil para el sistema de combate (FASE 5).
    /// </summary>
    public bool PvPPermitido()
    {
        return zonaActual == ZoneType.Unsafe;
    }

    #endregion
}
