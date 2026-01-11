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
                CmdUsarHabilidad(1);
            }
        }

        // Tecla 2 -> Habilidad 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (habilidad2 != null)
            {
                CmdUsarHabilidad(2);
            }
        }
    }

    #endregion

    #region Networking (Commands)

    /// <summary>
    /// Intenta usar una habilidad por índice (1 o 2).
    /// </summary>
    [Command]
    void CmdUsarHabilidad(int index)
    {
        HabilidadBase habilidad = (index == 1) ? habilidad1 : habilidad2;
        
        if (habilidad == null) return;

        // 1. Validar Cooldown
        double cooldownFin = (index == 1) ? cooldownHabilidad1 : cooldownHabilidad2;
        if (NetworkTime.time < cooldownFin)
        {
            Debug.Log($"[COMBAT] Habilidad {index} en cooldown.");
            return;
        }

        // 2. Validar Mana
        if (!stats.ConsumirMana(habilidad.manaCost))
        {
            Debug.Log("[COMBAT] No hay suficiente mana.");
            return;
        }

        // 3. Validar Target/Rango (si requiere enemigo)
        NetworkIdentity target = targetingSystem.objetivoActual; // Nota: TargetingSystem necesita sincronizar su target al servidor
        // ERROR: TargetingSystem.objetivoActual es local del cliente. 
        // Solución MVP: Enviar target netId en el Command o usar el target sincronizado si existiera.
        // Para MVP, vamos a asumir que el cliente TIENE que enviar el target, o simplificamos atacando lo que el servidor crea que mira.
        // MEJOR: Enviar el NetID del target como parámetro extra. (Lo haremos en Fase 6 refinada).
        // Por ahora, usaremos overlap sphere o raycast en servidor si es habilidad de target.
        
        // CORRECCIÓN RAPIDA: El Command debería recibir el target NetId. 
        // Pero para no complicar la firma ahora, asumiremos 'self' o 'raycast forward' simple server-side.
        
        // Ejecutar lógica según tipo
        EjecutarHabilidad(habilidad, target);

        // Activar Cooldown
        if (index == 1) cooldownHabilidad1 = NetworkTime.time + habilidad.cooldown;
        else cooldownHabilidad2 = NetworkTime.time + habilidad.cooldown;
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
                }
                // TODO: Curar aliado
            }
            // Si es daño (Target: Enemy)
            else if (habilidad.damage > 0)
            {
                Debug.Log($"[SERVER] Habilidad de daño detectada. Damage: {habilidad.damage}, Range: {habilidad.range}");
                
                // Para MVP simple: Raycast forward desde el servidor para encontrar enemigo
                Vector3 rayOrigin = transform.position + Vector3.up;
                Vector3 rayDirection = transform.forward;
                
                Debug.Log($"[SERVER] Lanzando raycast desde {rayOrigin} en dirección {rayDirection}");
                
                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, habilidad.range))
                {
                    Debug.Log($"[SERVER] Raycast impactó: {hit.collider.gameObject.name} a distancia {hit.distance}");
                    
                    PlayerStats enemigoStats = hit.collider.GetComponent<PlayerStats>();
                    if (enemigoStats != null && enemigoStats != stats)
                    {
                        Debug.Log($"[SERVER] Enemigo encontrado! Aplicando daño...");
                        DamageSystem.AplicarDanioHabilidad(enemigoStats, habilidad, stats, gameObject);
                    }
                    else
                    {
                        Debug.LogWarning($"[SERVER] El objeto impactado no tiene PlayerStats o es el mismo jugador.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SERVER] Raycast no impactó nada. Asegúrate de que el enemigo esté frente a ti y dentro del rango.");
                }
            }
        }
        else if (habilidad.tipoHabilidad == TipoHabilidad.Projectile)
        {
            // TODO: Spawn proyectil
            Debug.LogWarning("[SERVER] Proyectiles pendientes de implementar.");
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
