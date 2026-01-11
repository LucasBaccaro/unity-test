using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de combate del jugador.
/// Maneja inputs de ataque, uso de habilidades y comunicación con el servidor.
/// 
/// NETWORKING: 
/// - Cliente detecta input y envía [Command] al servidor.
/// - Servidor valida (Cooldown, Mana, Rango) y ejecuta.
/// - Servidor sincroniza resultados (Daño, efectos) a clientes.
/// </summary>
public class PlayerCombat : NetworkBehaviour
{
    #region Variables

    [Header("Referencias")]
    public PlayerStats stats;
    public TargetingSystem targetingSystem;
    public Transform castPoint; // Punto desde donde salen los proyectiles

    [Header("Habilidades")]
    public HabilidadBase habilidad1; // Asignar en Inspector o dinámicamente
    public HabilidadBase habilidad2;

    // Cooldowns (Solo Servidor)
    private double cooldownHabilidad1;
    private double cooldownHabilidad2;

    public event System.Action OnAbilitiesChanged;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        ManejarInputCombate();
    }

    #endregion

    #region Input

    void ManejarInputCombate()
    {
        // Tecla 1 -> Habilidad 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (habilidad1 != null)
            {
                // Obtener el netId del target seleccionado (0 si no hay target)
                uint targetNetId = (targetingSystem.objetivoActual != null)
                    ? targetingSystem.objetivoActual.netId
                    : 0;

                if (targetingSystem.objetivoActual != null)
                {
                    Debug.Log($"[CLIENTE] Usando habilidad 1 con target: {targetingSystem.objetivoActual.name} (netId: {targetNetId})");
                }
                else
                {
                    Debug.Log($"[CLIENTE] Usando habilidad 1 sin target (curación/self)");
                }

                CmdUsarHabilidad(1, targetNetId);
            }
        }

        // Tecla 2 -> Habilidad 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (habilidad2 != null)
            {
                // Obtener el netId del target seleccionado (0 si no hay target)
                uint targetNetId = (targetingSystem.objetivoActual != null)
                    ? targetingSystem.objetivoActual.netId
                    : 0;

                if (targetingSystem.objetivoActual != null)
                {
                    Debug.Log($"[CLIENTE] Usando habilidad 2 con target: {targetingSystem.objetivoActual.name} (netId: {targetNetId})");
                }
                else
                {
                    Debug.Log($"[CLIENTE] Usando habilidad 2 sin target (curación/self)");
                }

                CmdUsarHabilidad(2, targetNetId);
            }
        }
    }

    #endregion

    #region Networking (Commands)

    /// <summary>
    /// Intenta usar una habilidad por índice (1 o 2).
    ///
    /// NETWORKING: El cliente envía el netId del target seleccionado.
    /// El servidor obtiene el NetworkIdentity y valida todo antes de ejecutar.
    /// </summary>
    /// <param name="index">Índice de habilidad (1 o 2)</param>
    /// <param name="targetNetId">NetId del target seleccionado (0 si no hay target)</param>
    [Command]
    void CmdUsarHabilidad(int index, uint targetNetId)
    {
        HabilidadBase habilidad = (index == 1) ? habilidad1 : habilidad2;

        if (habilidad == null)
        {
            Debug.LogWarning($"[COMBAT] Habilidad {index} no está asignada.");
            return;
        }

        Debug.Log($"[COMBAT] Intentando usar habilidad {index}: {habilidad.nombreHabilidad} (targetNetId: {targetNetId})");

        // 1. Validar Cooldown
        double cooldownFin = (index == 1) ? cooldownHabilidad1 : cooldownHabilidad2;
        if (NetworkTime.time < cooldownFin)
        {
            Debug.Log($"[COMBAT] Habilidad {index} en cooldown.");
            return;
        }

        // 2. Obtener el NetworkIdentity del target desde el netId (si se proporcionó)
        NetworkIdentity target = null;
        if (targetNetId != 0)
        {
            // Buscar el NetworkIdentity por netId
            if (NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity targetIdentity))
            {
                target = targetIdentity;
                Debug.Log($"[COMBAT] Target encontrado: {target.name} (netId: {targetNetId})");
            }
            else
            {
                Debug.LogWarning($"[COMBAT] Target con netId {targetNetId} no encontrado en el servidor.");
                return;
            }
        }

        // 3. Validar que hay target si la habilidad lo requiere
        if (habilidad.damage > 0 && habilidad.tipoObjetivo == TipoObjetivo.Enemy)
        {
            if (target == null)
            {
                Debug.LogWarning($"[COMBAT] Habilidad de daño requiere un target. Debes clickear un enemigo primero.");
                return;
            }
        }

        // 4. Validar Mana DESPUÉS de confirmar que se puede usar la habilidad
        if (!stats.ConsumirMana(habilidad.manaCost))
        {
            Debug.Log("[COMBAT] No hay suficiente mana.");
            return;
        }

        // 5. Activar Cooldown ANTES de ejecutar (para evitar spam)
        if (index == 1) cooldownHabilidad1 = NetworkTime.time + habilidad.cooldown;
        else cooldownHabilidad2 = NetworkTime.time + habilidad.cooldown;

        // 6. Ejecutar habilidad
        EjecutarHabilidad(habilidad, target);
    }

    [Server]
    void EjecutarHabilidad(HabilidadBase habilidad, NetworkIdentity target)
    {
        Debug.Log($"[SERVER] Ejecutando habilidad: {habilidad.nombreHabilidad}");

        // Tipo: Instant (Curación o Daño directo)
        if (habilidad.tipoHabilidad == TipoHabilidad.Instant)
        {
            // Si es curación (Target: Self o Ally)
            if (habilidad.healing > 0)
            {
                if (habilidad.tipoObjetivo == TipoObjetivo.Self)
                {
                    stats.Curar(habilidad.healing);
                    Debug.Log($"[SERVER] ✓ Curación aplicada: +{habilidad.healing} HP");
                }
                // TODO FASE 6: Curar aliado
            }
            // Si es daño (Target: Enemy)
            else if (habilidad.damage > 0)
            {
                // El target ya fue validado en CmdUsarHabilidad, pero hacemos validaciones adicionales

                // Obtener PlayerStats del target
                PlayerStats enemigoStats = target.GetComponent<PlayerStats>();
                if (enemigoStats == null)
                {
                    Debug.LogWarning($"[SERVER] El target seleccionado no es un jugador válido (no tiene PlayerStats).");
                    return;
                }

                // Verificar que no sea el mismo jugador
                if (enemigoStats == stats)
                {
                    Debug.LogWarning($"[SERVER] No puedes atacarte a ti mismo.");
                    return;
                }

                // Verificar distancia
                float distancia = Vector3.Distance(transform.position, target.transform.position);

                if (distancia > habilidad.range)
                {
                    Debug.LogWarning($"[SERVER] Target fuera de rango. Distancia: {distancia:F2}m, Rango máximo: {habilidad.range}m");
                    return;
                }

                // Validar PvP (usando ZoneDetector)
                // NOTA: Comentado temporalmente para testing. Descomentar para producción.
                /*
                ZoneDetector zoneDetector = GetComponent<ZoneDetector>();
                if (zoneDetector != null && !zoneDetector.PvPPermitido())
                {
                    Debug.LogWarning($"[SERVER] No puedes atacar en zona segura.");
                    return;
                }
                */
                Debug.Log($"[DEBUG] Validación de PvP deshabilitada para testing.");

                // ¡Aplicar daño!
                Debug.Log($"[SERVER] ✓ Aplicando {habilidad.damage} de daño a {enemigoStats.name} (Distancia: {distancia:F2}m)");
                DamageSystem.AplicarDanioHabilidad(enemigoStats, habilidad, stats, gameObject);
            }
        }
        else if (habilidad.tipoHabilidad == TipoHabilidad.Projectile)
        {
            // TODO FASE 5: Spawn proyectil
            Debug.LogWarning("[SERVER] Proyectiles pendientes de implementar (requiere ProjectileController.cs).");
        }
        else if (habilidad.tipoHabilidad == TipoHabilidad.AoE)
        {
            // TODO FASE 6: Implementar AoE
            Debug.LogWarning("[SERVER] Habilidades AoE pendientes de implementar.");
        }
    }

    #endregion

    #region Public Methods

    public void NotificarCambioHabilidades()
    {
        OnAbilitiesChanged?.Invoke();
    }

    #endregion
}
