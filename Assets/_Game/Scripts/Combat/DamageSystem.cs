using Mirror;
using UnityEngine;

/// <summary>
/// Sistema centralizado de cálculo y aplicación de daño.
///
/// NETWORKING: Todos los métodos son [Server] para garantizar Server Authority.
/// Solo el servidor puede aplicar daño. Los clientes ven los resultados
/// a través de SyncVars (HP del jugador).
///
/// VENTAJAS de centralizar el daño:
/// - Un solo lugar para balancear/modificar fórmulas
/// - Fácil de debuggear
/// - Previene inconsistencias
/// - Anti-cheat: el cliente NO puede modificar daño
///
/// IMPORTANTE: Este es un static class, no necesita instancia.
/// Se llama directamente: DamageSystem.AplicarDanio(...)
/// </summary>
public static class DamageSystem
{
    #region Aplicación de Daño

    /// <summary>
    /// Aplica daño a un jugador desde otro jugador/NPC.
    ///
    /// NETWORKING: Solo el servidor debe llamar este método.
    /// El daño se calcula con defensa, críticos, etc.
    /// </summary>
    /// <param name="objetivo">El jugador que recibe el daño</param>
    /// <param name="danioBase">Daño base antes de modificadores</param>
    /// <param name="atacante">El jugador/NPC que hace el daño (puede ser null)</param>
    /// <param name="esCritico">Si el golpe fue crítico (más daño)</param>
    /// <returns>Cantidad de daño efectivo aplicado</returns>
    public static int AplicarDanio(PlayerStats objetivo, int danioBase, GameObject atacante = null, bool esCritico = false)
    {
        if (objetivo == null || danioBase <= 0)
        {
            return 0;
        }

        // IMPORTANTE: Verificar que estamos en el servidor
        if (!NetworkServer.active)
        {
            Debug.LogError("[DamageSystem] AplicarDanio llamado en cliente! Esto NO debe pasar.");
            return 0;
        }

        // Calcular daño con defensa
        int danioFinal = CalcularDanioConDefensa(danioBase, objetivo.defense);

        // Aplicar crítico si corresponde
        if (esCritico)
        {
            danioFinal = Mathf.RoundToInt(danioFinal * 1.5f); // 150% daño en crítico
            Debug.Log($"[DamageSystem] ¡Golpe CRÍTICO! Daño aumentado a {danioFinal}");
        }

        // Asegurar que haga al menos 1 de daño
        danioFinal = Mathf.Max(danioFinal, 1);

        // Aplicar el daño al jugador
        objetivo.RecibirDanio(danioFinal, atacante);

        // Logging para debugging
        string atacanteNombre = atacante != null ? atacante.name : "Desconocido";
        PlayerController controllerObj = objetivo.GetComponent<PlayerController>();
        string objetivoNombre = controllerObj != null ? controllerObj.nombreJugador : objetivo.name;

        Debug.Log($"[DamageSystem] {atacanteNombre} hizo {danioFinal} de daño a {objetivoNombre}.");

        return danioFinal;
    }

    /// <summary>
    /// Aplica daño de una habilidad a un jugador.
    /// Incluye el modificador de daño del atacante.
    /// </summary>
    /// <param name="objetivo">El objetivo del daño</param>
    /// <param name="habilidad">La habilidad que hace el daño</param>
    /// <param name="statsAtacante">Stats del atacante (para multiplicador)</param>
    /// <param name="atacante">GameObject del atacante</param>
    /// <returns>Daño final aplicado</returns>
    public static int AplicarDanioHabilidad(PlayerStats objetivo, HabilidadBase habilidad, PlayerStats statsAtacante, GameObject atacante)
    {
        if (habilidad == null || objetivo == null)
        {
            return 0;
        }

        // Calcular daño final de la habilidad
        int danioFinal = habilidad.CalcularDanioFinal(statsAtacante.damage);

        // Aplicar el daño
        return AplicarDanio(objetivo, danioFinal, atacante, CalcularCritico());
    }

    #endregion

    #region Cálculos

    /// <summary>
    /// Calcula el daño después de aplicar defensa.
    ///
    /// Fórmula simple: daño - (defensa / 2)
    /// Mínimo 1 de daño siempre.
    ///
    /// NOTA: Esta fórmula se puede ajustar para balanceo.
    /// Ejemplos de otras fórmulas:
    /// - daño * (100 / (100 + defensa)) // Porcentual
    /// - daño - defensa // Lineal
    /// - daño * (1 - (defensa / 100)) // Reducción porcentual
    /// </summary>
    static int CalcularDanioConDefensa(int danio, int defensa)
    {
        // Fórmula simple para MVP
        int reduccion = defensa / 2;
        int danioFinal = danio - reduccion;

        // Siempre hacer al menos 1 de daño
        return Mathf.Max(danioFinal, 1);
    }

    /// <summary>
    /// Calcula si un golpe es crítico (azar).
    /// TODO: Hacer que dependa de una stat (crit chance) del jugador.
    /// </summary>
    static bool CalcularCritico()
    {
        // 10% de probabilidad de crítico para MVP
        float critChance = 0.10f;
        return Random.value < critChance;
    }

    #endregion

    #region Validaciones de PvP

    /// <summary>
    /// Verifica si un jugador puede atacar a otro.
    ///
    /// REGLAS:
    /// - No atacar a sí mismo
    /// - Ambos deben estar en zona unsafe
    /// - Ambos deben estar vivos
    /// </summary>
    /// <param name="atacante">El jugador atacante</param>
    /// <param name="objetivo">El jugador objetivo</param>
    /// <returns>True si el ataque es válido</returns>
    public static bool PuedeAtacar(PlayerController atacante, PlayerController objetivo)
    {
        if (atacante == null || objetivo == null)
        {
            return false;
        }

        // No atacar a sí mismo
        if (atacante == objetivo)
        {
            Debug.LogWarning("[DamageSystem] Intentando atacarse a sí mismo.");
            return false;
        }

        // Verificar que ambos estén vivos
        PlayerStats statsAtacante = atacante.GetComponent<PlayerStats>();
        PlayerStats statsObjetivo = objetivo.GetComponent<PlayerStats>();

        if (statsAtacante == null || statsObjetivo == null)
        {
            return false;
        }

        if (!statsAtacante.EstaVivo() || !statsObjetivo.EstaVivo())
        {
            Debug.LogWarning("[DamageSystem] Uno de los jugadores está muerto.");
            return false;
        }

        // Verificar zonas (PvP)
        ZoneDetector zoneAtacante = atacante.GetComponent<ZoneDetector>();
        ZoneDetector zoneObjetivo = objetivo.GetComponent<ZoneDetector>();

        if (zoneAtacante == null || zoneObjetivo == null)
        {
            Debug.LogWarning("[DamageSystem] No se pudo verificar zona de jugadores.");
            return true; // Permitir por defecto si no hay detector
        }

        // Verificar que AMBOS estén en zona unsafe
        if (!zoneAtacante.PvPPermitido())
        {
            Debug.LogWarning($"[DamageSystem] {atacante.nombreJugador} está en zona segura. PvP no permitido.");
            return false;
        }

        if (!zoneObjetivo.PvPPermitido())
        {
            Debug.LogWarning($"[DamageSystem] {objetivo.nombreJugador} está en zona segura. PvP no permitido.");
            return false;
        }

        // Todo validado, puede atacar
        return true;
    }

    #endregion

    #region Validaciones de Rango

    /// <summary>
    /// Verifica si el objetivo está en rango para una habilidad.
    /// </summary>
    /// <param name="atacante">Posición del atacante</param>
    /// <param name="objetivo">Posición del objetivo</param>
    /// <param name="rango">Rango máximo de la habilidad</param>
    /// <returns>True si está en rango</returns>
    public static bool EstaEnRango(Vector3 atacante, Vector3 objetivo, float rango)
    {
        float distancia = Vector3.Distance(atacante, objetivo);
        return distancia <= rango;
    }

    /// <summary>
    /// Verifica si hay línea de vista entre dos transforms.
    /// Útil para evitar atacar a través de paredes.
    /// </summary>
    /// <param name="desde">Transform de inicio</param>
    /// <param name="hacia">Transform de destino</param>
    /// <returns>True si hay línea de vista</returns>
    public static bool TieneLineaDeVista(Transform desde, Transform hacia)
    {
        if (desde == null || hacia == null)
        {
            return false;
        }

        // Offset para lanzar el ray desde la altura del personaje (no los pies)
        Vector3 desdeOffset = desde.position + Vector3.up * 1f;
        Vector3 haciaOffset = hacia.position + Vector3.up * 1f;

        Vector3 direccion = haciaOffset - desdeOffset;
        float distancia = direccion.magnitude;

        // Lanzar raycast
        if (Physics.Raycast(desdeOffset, direccion.normalized, out RaycastHit hit, distancia))
        {
            // Si el ray golpeó algo antes de llegar al objetivo, no hay línea de vista
            // A menos que haya golpeado al objetivo mismo
            if (hit.collider.transform.IsChildOf(hacia) ||
                hit.collider.transform == hacia)
            {
                return true; // Golpeó al objetivo, hay línea de vista
            }

            Debug.LogWarning($"[DamageSystem] No hay línea de vista. Obstruido por: {hit.collider.name}");
            return false;
        }

        // No golpeó nada, hay línea de vista despejada
        return true;
    }

    #endregion

    #region Efectos Visuales (ClientRpc helpers)

    /// <summary>
    /// Muestra efecto visual de daño en el objetivo.
    /// Esto debería ser llamado por PlayerStats o PlayerCombat con un ClientRpc.
    /// </summary>
    public static void MostrarEfectoDanio(Vector3 posicion, int cantidad, bool critico)
    {
        // TODO (FASE 12): Implementar efectos visuales
        // - Números flotantes mostrando daño
        // - Color diferente para críticos (rojo brillante)
        // - Partículas de sangre/impacto
        // - Shake de cámara si es el jugador local

        Debug.Log($"[DamageSystem] Efecto de daño: {cantidad} {(critico ? "CRÍTICO" : "")} en {posicion}");
    }

    #endregion

    #region Utilidades

    /// <summary>
    /// Calcula el daño total de múltiples fuentes.
    /// Útil para habilidades AoE que golpean múltiples objetivos.
    /// </summary>
    public static int CalcularDanioTotal(params int[] danios)
    {
        int total = 0;
        foreach (int danio in danios)
        {
            total += danio;
        }
        return total;
    }

    /// <summary>
    /// Obtiene información de combate para debugging.
    /// </summary>
    public static string ObtenerInfoCombate(PlayerStats atacante, PlayerStats objetivo)
    {
        string info = "=== INFO DE COMBATE ===\n";
        info += $"Atacante: {atacante.gameObject.name}\n";
        info += $"  - Damage: {atacante.damage}\n";
        info += $"  - HP: {atacante.currentHP}/{atacante.maxHP}\n\n";

        info += $"Objetivo: {objetivo.gameObject.name}\n";
        info += $"  - Defense: {objetivo.defense}\n";
        info += $"  - HP: {objetivo.currentHP}/{objetivo.maxHP}\n";
        info += "=======================";

        return info;
    }

    #endregion
}
