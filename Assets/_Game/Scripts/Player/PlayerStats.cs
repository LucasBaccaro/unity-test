using Mirror;
using UnityEngine;
using System;

/// <summary>
/// Gestiona todas las estadísticas del jugador (HP, Mana, Level, XP, etc.).
///
/// NETWORKING: Este componente sincroniza las stats del servidor a todos
/// los clientes usando SyncVars. Solo el servidor puede modificar las stats
/// para prevenir cheating.
///
/// Las stats base vienen de la clase seleccionada (ClaseBase), y luego
/// se modifican con el nivel, items, buffs, etc.
/// </summary>
public class PlayerStats : NetworkBehaviour
{
    #region Events (UI)
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;
    public event Action<int, int> OnXPChangedEvent;
    public event Action<int> OnLevelChangedEvent;
    public event Action<int> OnGoldChangedEvent;
    #endregion

    #region Variables de Red (Sincronizadas)

    [Header("Stats Principales")]

    /// <summary>
    /// Vida máxima del jugador.
    /// Determinada por: clase base + nivel + items
    /// </summary>
    [SyncVar(hook = nameof(OnMaxHPChanged))]
    public int maxHP = 100;

    /// <summary>
    /// Vida actual del jugador.
    /// Cuando llega a 0, el jugador muere.
    /// </summary>
    [SyncVar(hook = nameof(OnCurrentHPChanged))]
    public int currentHP = 100;

    /// <summary>
    /// Mana máximo del jugador.
    /// Usado para habilidades.
    /// </summary>
    [SyncVar(hook = nameof(OnMaxManaChanged))]
    public int maxMana = 100;

    /// <summary>
    /// Mana actual del jugador.
    /// </summary>
    [SyncVar(hook = nameof(OnCurrentManaChanged))]
    public int currentMana = 100;

    [Header("Stats de Combate")]

    /// <summary>
    /// Daño base del jugador.
    /// Afecta a todas las habilidades y ataques.
    /// </summary>
    [SyncVar]
    public int damage = 10;

    /// <summary>
    /// Defensa del jugador.
    /// Reduce el daño recibido.
    /// </summary>
    [SyncVar]
    public int defense = 5;

    /// <summary>
    /// Velocidad de movimiento del jugador.
    /// </summary>
    [SyncVar]
    public float speed = 5f;

    [Header("Progresión")]

    /// <summary>
    /// Nivel actual del jugador.
    /// </summary>
    [SyncVar(hook = nameof(OnLevelChanged))]
    public int level = 1;

    /// <summary>
    /// Experiencia actual del jugador.
    /// </summary>
    [SyncVar(hook = nameof(OnXPChanged))]
    public int currentXP = 0;

    /// <summary>
    /// Experiencia necesaria para subir al siguiente nivel.
    /// </summary>
    [SyncVar]
    public int xpToNextLevel = 100;

    [Header("Economía")]

    /// <summary>
    /// Oro del jugador (moneda).
    /// </summary>
    [SyncVar(hook = nameof(OnGoldChanged))]
    public int gold = 0;

    [Header("Clase")]

    /// <summary>
    /// Nombre de la clase del jugador (Mago, Paladin, etc.).
    /// </summary>
    [SyncVar]
    public string nombreClase = "Sin Clase";

    #endregion

    #region Variables Privadas

    // Stats base de la clase (sin modificadores de nivel/items)
    private int baseMaxHP;
    private int baseMaxMana;
    private int baseDamage;
    private int baseDefense;
    private float baseSpeed;

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
    }

    /// <summary>
    /// Se llama cuando el objeto se spawnea en el servidor.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Inicializar stats por defecto
        // (En FASE 3 completa, esto vendrá de la clase seleccionada)
        InicializarStatsPorDefecto();
    }

    #endregion

    #region Inicialización

    /// <summary>
    /// Inicializa stats con valores por defecto.
    /// Esto se ejecuta solo en el servidor.
    /// </summary>
    [Server]
    void InicializarStatsPorDefecto()
    {
        // Stats por defecto (sin clase seleccionada aún)
        maxHP = 100;
        currentHP = maxHP;
        maxMana = 100;
        currentMana = maxMana;
        damage = 10;
        defense = 5;
        speed = 5f;
        level = 1;
        currentXP = 0;
        xpToNextLevel = CalcularXPParaProximoNivel(level);
        gold = 100; // Oro inicial

        // Guardar stats base
        baseMaxHP = maxHP;
        baseMaxMana = maxMana;
        baseDamage = damage;
        baseDefense = defense;
        baseSpeed = speed;

        Debug.Log("[PlayerStats] Stats por defecto inicializadas.");
    }

    /// <summary>
    /// Inicializa las stats basándose en una clase específica.
    /// Se llama desde PlayerClassSelector cuando el jugador elige clase.
    /// </summary>
    /// <param name="claseBase">Los datos de la clase (ScriptableObject)</param>
    [Server]
    public void InicializarConClase(ClaseBase claseBase)
    {
        if (claseBase == null)
        {
            Debug.LogError("[PlayerStats] ClaseBase es null!");
            return;
        }

        // Aplicar stats base de la clase
        baseMaxHP = claseBase.hpBase;
        baseMaxMana = claseBase.manaBase;
        baseDamage = claseBase.damageBase;
        baseDefense = claseBase.defenseBase;
        baseSpeed = claseBase.speedBase;

        // Aplicar nombre de clase
        nombreClase = claseBase.nombreClase;

        // Recalcular stats con nivel actual
        RecalcularStats();

        // Restaurar HP y Mana al máximo
        currentHP = maxHP;
        currentMana = maxMana;

        Debug.Log($"[PlayerStats] Stats inicializadas con clase: {nombreClase}");
        Debug.Log($"[PlayerStats] HP: {maxHP}, Mana: {maxMana}, Damage: {damage}, Defense: {defense}, Speed: {speed}");
    }

    #endregion

    #region Recálculo de Stats

    /// <summary>
    /// Recalcula todas las stats basándose en:
    /// - Stats base de la clase
    /// - Nivel actual
    /// - Items equipados (TODO: FASE 4)
    /// - Buffs activos (TODO: futuro)
    /// </summary>
    [Server]
    public void RecalcularStats()
    {
        // Fórmula: stat = statBase + (bonusPorNivel * (level - 1))
        // Ejemplo: Si eres nivel 5, tienes 4 niveles de bonus

        int nivelesBonus = level - 1;

        // HP: +10 por nivel
        maxHP = baseMaxHP + (nivelesBonus * 10);

        // Mana: +5 por nivel
        maxMana = baseMaxMana + (nivelesBonus * 5);

        // Damage: +2 por nivel
        damage = baseDamage + (nivelesBonus * 2);

        // Defense: +1 por nivel
        defense = baseDefense + (nivelesBonus * 1);

        // Speed: +0.1 por nivel (sin exceder 10)
        speed = Mathf.Min(baseSpeed + (nivelesBonus * 0.1f), 10f);

        // TODO (FASE 4): Añadir bonos de items equipados
        // foreach (Item item in inventario.GetEquippedItems())
        // {
        //     maxHP += item.bonusHP;
        //     damage += item.bonusDamage;
        //     // etc.
        // }

        Debug.Log($"[PlayerStats] Stats recalculadas para nivel {level}.");
    }

    #endregion

    #region Gestión de HP

    /// <summary>
    /// Aplica daño al jugador.
    /// Solo el servidor puede llamar este método.
    /// </summary>
    /// <param name="cantidad">Cantidad de daño a aplicar</param>
    /// <param name="atacante">El jugador/NPC que hizo el daño (opcional)</param>
    [Server]
    public void RecibirDanio(int cantidad, GameObject atacante = null)
    {
        if (cantidad <= 0) return;

        // Calcular daño después de defensa
        // Fórmula simple: daño reducido = daño - (defensa / 2)
        int danioReducido = Mathf.Max(cantidad - (defense / 2), 1); // Mínimo 1 de daño

        // Aplicar daño
        currentHP = Mathf.Max(currentHP - danioReducido, 0);

        Debug.Log($"[PlayerStats] {playerController.nombreJugador} recibió {danioReducido} de daño (original: {cantidad}). HP: {currentHP}/{maxHP}");

        // Si murió, manejar muerte
        if (currentHP <= 0)
        {
            OnMuerte(atacante);
        }
    }

    /// <summary>
    /// Cura al jugador.
    /// Solo el servidor puede llamar este método.
    /// </summary>
    /// <param name="cantidad">Cantidad de HP a restaurar</param>
    [Server]
    public void Curar(int cantidad)
    {
        if (cantidad <= 0) return;

        int hpAnterior = currentHP;
        currentHP = Mathf.Min(currentHP + cantidad, maxHP);

        int curado = currentHP - hpAnterior;

        Debug.Log($"[PlayerStats] {playerController.nombreJugador} curado por {curado} HP. HP: {currentHP}/{maxHP}");
    }

    /// <summary>
    /// Restaura HP al máximo.
    /// </summary>
    [Server]
    public void RestaurarHP()
    {
        currentHP = maxHP;
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} HP restaurado completamente.");
    }

    #endregion

    #region Gestión de Mana

    /// <summary>
    /// Consume mana del jugador.
    /// </summary>
    /// <param name="cantidad">Cantidad de mana a consumir</param>
    /// <returns>True si había suficiente mana, False si no</returns>
    [Server]
    public bool ConsumirMana(int cantidad)
    {
        if (cantidad <= 0) return true;

        if (currentMana < cantidad)
        {
            Debug.LogWarning($"[PlayerStats] {playerController.nombreJugador} no tiene suficiente mana. Requiere: {cantidad}, Tiene: {currentMana}");
            return false;
        }

        currentMana -= cantidad;
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} consumió {cantidad} mana. Mana: {currentMana}/{maxMana}");
        return true;
    }

    /// <summary>
    /// Restaura mana al jugador.
    /// </summary>
    [Server]
    public void RestaurarMana(int cantidad)
    {
        if (cantidad <= 0) return;

        currentMana = Mathf.Min(currentMana + cantidad, maxMana);
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} mana restaurado. Mana: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// Restaura mana al máximo.
    /// </summary>
    [Server]
    public void RestaurarManaCompleto()
    {
        currentMana = maxMana;
    }

    #endregion

    #region Gestión de XP y Niveles

    /// <summary>
    /// Añade experiencia al jugador.
    /// Si alcanza el XP necesario, sube de nivel automáticamente.
    /// </summary>
    /// <param name="cantidad">Cantidad de XP a añadir</param>
    [Server]
    public void AgregarXP(int cantidad)
    {
        if (cantidad <= 0) return;

        currentXP += cantidad;

        Debug.Log($"[PlayerStats] {playerController.nombreJugador} ganó {cantidad} XP. Total: {currentXP}/{xpToNextLevel}");

        // Verificar si sube de nivel
        while (currentXP >= xpToNextLevel)
        {
            SubirNivel();
        }
    }

    /// <summary>
    /// Sube el nivel del jugador en 1.
    /// Recalcula stats y restaura HP/Mana.
    /// </summary>
    [Server]
    void SubirNivel()
    {
        // Incrementar nivel
        level++;

        // Restar XP usado para subir nivel
        currentXP -= xpToNextLevel;

        // Calcular nuevo XP necesario para siguiente nivel
        xpToNextLevel = CalcularXPParaProximoNivel(level);

        // Recalcular stats con nuevo nivel
        RecalcularStats();

        // Restaurar HP y Mana al máximo (recompensa por subir nivel)
        currentHP = maxHP;
        currentMana = maxMana;

        Debug.Log($"[PlayerStats] ¡{playerController.nombreJugador} subió a nivel {level}!");

        // Notificar a todos los clientes con efecto visual
        RpcMostrarEfectoLevelUp();
    }

    /// <summary>
    /// Calcula cuánto XP se necesita para subir de nivel.
    /// Fórmula: 100 * nivel^2
    /// </summary>
    int CalcularXPParaProximoNivel(int nivelActual)
    {
        // Nivel 1→2: 100 XP
        // Nivel 2→3: 400 XP
        // Nivel 3→4: 900 XP
        // Nivel 4→5: 1600 XP
        // etc.
        return 100 * (nivelActual * nivelActual);
    }

    #endregion

    #region Gestión de Oro

    /// <summary>
    /// Añade oro al jugador.
    /// </summary>
    [Server]
    public void AgregarOro(int cantidad)
    {
        if (cantidad <= 0) return;

        gold += cantidad;
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} ganó {cantidad} oro. Total: {gold}");
    }

    /// <summary>
    /// Gasta oro del jugador.
    /// </summary>
    /// <returns>True si tenía suficiente oro, False si no</returns>
    [Server]
    public bool GastarOro(int cantidad)
    {
        if (cantidad <= 0) return true;

        if (gold < cantidad)
        {
            Debug.LogWarning($"[PlayerStats] {playerController.nombreJugador} no tiene suficiente oro. Requiere: {cantidad}, Tiene: {gold}");
            return false;
        }

        gold -= cantidad;
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} gastó {cantidad} oro. Restante: {gold}");
        return true;
    }

    #endregion

    #region Muerte

    /// <summary>
    /// Maneja la muerte del jugador.
    /// </summary>
    /// <param name="asesino">El jugador/NPC que mató a este jugador (opcional)</param>
    [Server]
    void OnMuerte(GameObject asesino)
    {
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} ha muerto!");

        // TODO (FASE 6): Dropear todo el inventario (full loot)
        // PlayerInventory inventario = GetComponent<PlayerInventory>();
        // inventario.DropearTodoElInventario(transform.position);

        // TODO (FASE 6): Registrar kill/death stats
        // if (asesino != null)
        // {
        //     PlayerStats statsAsesino = asesino.GetComponent<PlayerStats>();
        //     if (statsAsesino != null)
        //     {
        //         statsAsesino.RegistrarKill();
        //     }
        // }

        // Notificar a todos los clientes
        RpcMostrarPantallaMuerte();

        // Respawnear después de un delay
        Invoke(nameof(Respawnear), 5f); // 5 segundos
    }

    /// <summary>
    /// Respawnea al jugador en la ciudad.
    /// </summary>
    [Server]
    void Respawnear()
    {
        Debug.Log($"[PlayerStats] {playerController.nombreJugador} respawneando...");

        // Restaurar HP y Mana
        currentHP = maxHP;
        currentMana = maxMana;

        // TODO: Teletransportar al spawn point de la ciudad
        // PlayerMovement movement = GetComponent<PlayerMovement>();
        // movement.TeletransportarA(NetworkManager_MMO.Instance.spawnPointCiudad.position);

        // Notificar al cliente que cerró la pantalla de muerte
        RpcOcultarPantallaMuerte();
    }

    #endregion

    #region ClientRpc (Servidor → Clientes)

    /// <summary>
    /// Muestra efecto visual de level up en todos los clientes.
    /// </summary>
    [ClientRpc]
    void RpcMostrarEfectoLevelUp()
    {
        Debug.Log($"[CLIENT] ¡{playerController.nombreJugador} subió de nivel!");

        // TODO (FASE 12): Mostrar efecto visual
        // - Partículas doradas alrededor del jugador
        // - Sonido de level up
        // - Flash de pantalla
        // - Texto flotante "LEVEL UP!"
    }

    /// <summary>
    /// Muestra pantalla de muerte en el cliente del jugador muerto.
    /// </summary>
    [ClientRpc]
    void RpcMostrarPantallaMuerte()
    {
        // Solo mostrar para el jugador local
        if (!isLocalPlayer) return;

        Debug.Log("[CLIENT] Has muerto. Respawnearás en 5 segundos...");

        // TODO (FASE 12): Mostrar UI de muerte
        // UIManager.Instance.MostrarPantallaMuerte();
    }

    /// <summary>
    /// Oculta pantalla de muerte.
    /// </summary>
    [ClientRpc]
    void RpcOcultarPantallaMuerte()
    {
        // Solo para el jugador local
        if (!isLocalPlayer) return;

        Debug.Log("[CLIENT] Respawneado exitosamente.");

        // TODO (FASE 12): Ocultar UI de muerte
        // UIManager.Instance.OcultarPantallaMuerte();
    }

    #endregion

    #region SyncVar Hooks

    void OnMaxHPChanged(int oldValue, int newValue)
    {
        // Actualizar UI de HP máximo
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarMaxHP(newValue);
        }
    }

    void OnCurrentHPChanged(int oldValue, int newValue)
    {
        // Actualizar UI de HP actual
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarHP(newValue);
            OnHealthChanged?.Invoke(newValue, maxHP);
        }

        // Cambiar color del modelo si HP está bajo
        if (newValue < maxHP * 0.3f) // Menos del 30% HP
        {
            // TODO: Tint rojo
        }
    }

    void OnMaxManaChanged(int oldValue, int newValue)
    {
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarMaxMana(newValue);
            OnManaChanged?.Invoke(currentMana, newValue);
        }
    }

    void OnCurrentManaChanged(int oldValue, int newValue)
    {
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarMana(newValue);
            OnManaChanged?.Invoke(newValue, maxMana);
        }
    }

    void OnLevelChanged(int oldLevel, int newLevel)
    {
        Debug.Log($"[SYNCVAR] Nivel cambió: {oldLevel} → {newLevel}");

        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarNivel(newLevel);
            OnLevelChangedEvent?.Invoke(newLevel);
        }
    }

    void OnXPChanged(int oldXP, int newXP)
    {
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarXP(newXP, xpToNextLevel);
            OnXPChangedEvent?.Invoke(newXP, xpToNextLevel);
        }
    }

    void OnGoldChanged(int oldGold, int newGold)
    {
        if (isLocalPlayer)
        {
            // TODO (FASE 12): UIManager.Instance.ActualizarOro(newGold);
        }
    }

    #endregion

    #region Métodos Públicos (Getters)

    public bool EstaVivo()
    {
        return currentHP > 0;
    }

    public float GetPorcentajeHP()
    {
        return (float)currentHP / maxHP;
    }

    public float GetPorcentajeMana()
    {
        return (float)currentMana / maxMana;
    }

    public float GetPorcentajeXP()
    {
        return (float)currentXP / xpToNextLevel;
    }

    #endregion
}
