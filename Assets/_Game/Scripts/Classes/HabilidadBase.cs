using UnityEngine;

/// <summary>
/// Tipos de habilidad según cómo se ejecutan.
/// </summary>
public enum TipoHabilidad
{
    Instant,        // Se ejecuta instantáneamente (ej: curación, buff)
    Projectile,     // Lanza un proyectil (ej: bola de fuego)
    AoE,            // Área de efecto (ej: explosión, trampa)
    Channeled       // Canalizado (requiere mantener presionado)
}

/// <summary>
/// Tipos de objetivo que puede tener una habilidad.
/// </summary>
public enum TipoObjetivo
{
    Self,           // El propio jugador
    Enemy,          // Un enemigo (jugador u NPC)
    Ally,           // Un aliado (otro jugador)
    Ground          // Un punto en el suelo
}

/// <summary>
/// ScriptableObject que define una habilidad.
///
/// SCRIPTABLEOBJECT: Asset de datos reutilizable para habilidades.
/// Cada habilidad (Bola de Fuego, Curación, etc.) será un asset separado.
///
/// Las habilidades se asignan a las clases (ClaseBase) y luego el sistema
/// de combate las ejecuta cuando el jugador las usa.
/// </summary>
[CreateAssetMenu(fileName = "Nueva_Habilidad", menuName = "MMO/Habilidad", order = 2)]
public class HabilidadBase : ScriptableObject
{
    #region Información Básica

    [Header("Información de la Habilidad")]
    [Tooltip("Nombre de la habilidad")]
    public string nombreHabilidad = "Habilidad Sin Nombre";

    [Tooltip("Descripción de lo que hace la habilidad")]
    [TextArea(2, 4)]
    public string descripcion = "Una habilidad poderosa...";

    [Tooltip("Icono de la habilidad (para UI)")]
    public Sprite iconoHabilidad;

    #endregion

    #region Configuración de Ejecución

    [Header("Tipo de Habilidad")]
    [Tooltip("Cómo se ejecuta la habilidad")]
    public TipoHabilidad tipoHabilidad = TipoHabilidad.Instant;

    [Tooltip("Qué tipo de objetivo requiere")]
    public TipoObjetivo tipoObjetivo = TipoObjetivo.Enemy;

    #endregion

    #region Costos y Cooldown

    [Header("Costos")]
    [Tooltip("Costo de mana para usar la habilidad")]
    [Range(0, 100)]
    public int manaCost = 20;

    [Tooltip("Tiempo de cooldown en segundos")]
    [Range(0f, 60f)]
    public float cooldown = 5f;

    #endregion

    #region Rango y Targeting

    [Header("Rango y Targeting")]
    [Tooltip("Rango máximo de la habilidad (0 = melee)")]
    [Range(0f, 30f)]
    public float range = 10f;

    [Tooltip("Si es AoE, el radio del área de efecto")]
    [Range(0f, 10f)]
    public float aoeRadius = 0f;

    [Tooltip("Requiere línea de vista al objetivo")]
    public bool requiereLineaDeVista = true;

    #endregion

    #region Efectos

    [Header("Efectos de Daño/Curación")]
    [Tooltip("Cantidad de daño que hace (0 si no hace daño)")]
    [Range(0, 100)]
    public int damage = 0;

    [Tooltip("Cantidad de curación que hace (0 si no cura)")]
    [Range(0, 100)]
    public int healing = 0;

    [Tooltip("Multiplicador de daño basado en stats del jugador")]
    [Range(0f, 5f)]
    public float damageMultiplier = 1f;

    #endregion

    #region Efectos Visuales y Sonido

    [Header("Visuales y Sonido")]
    [Tooltip("Prefab del proyectil (si tipoHabilidad = Projectile)")]
    public GameObject projectilePrefab;

    [Tooltip("Efecto de partículas al usar la habilidad")]
    public GameObject efectoCast;

    [Tooltip("Efecto de partículas al impactar")]
    public GameObject efectoImpacto;

    [Tooltip("Sonido al usar la habilidad")]
    public AudioClip sonidoCast;

    [Tooltip("Sonido al impactar")]
    public AudioClip sonidoImpacto;

    [Tooltip("Color de la habilidad (para efectos)")]
    public Color colorHabilidad = Color.white;

    #endregion

    #region Configuración Adicional

    [Header("Configuración Adicional")]
    [Tooltip("Velocidad del proyectil (si es Projectile)")]
    [Range(5f, 50f)]
    public float velocidadProyectil = 20f;

    [Tooltip("Duración del canal (si es Channeled)")]
    [Range(0f, 10f)]
    public float duracionCanal = 0f;

    [Tooltip("Si la habilidad puede usarse mientras te mueves")]
    public bool usableEnMovimiento = true;

    #endregion

    #region Validación

    /// <summary>
    /// Valida que la habilidad esté configurada correctamente.
    /// </summary>
    void OnValidate()
    {
        // Validar nombre
        if (string.IsNullOrEmpty(nombreHabilidad))
        {
            Debug.LogWarning($"[HabilidadBase] La habilidad '{name}' no tiene nombre configurado.");
        }

        // Validar que haga algo (daño o curación)
        if (damage == 0 && healing == 0)
        {
            Debug.LogWarning($"[HabilidadBase] '{nombreHabilidad}' no hace daño ni cura. ¿Es correcto?");
        }

        // Validar que habilidades de proyectil tengan prefab
        if (tipoHabilidad == TipoHabilidad.Projectile && projectilePrefab == null)
        {
            Debug.LogWarning($"[HabilidadBase] '{nombreHabilidad}' es tipo Projectile pero no tiene projectilePrefab asignado.");
        }

        // Validar AoE
        if (tipoHabilidad == TipoHabilidad.AoE && aoeRadius == 0f)
        {
            Debug.LogWarning($"[HabilidadBase] '{nombreHabilidad}' es tipo AoE pero aoeRadius es 0.");
        }

        // Validar cooldown
        if (cooldown == 0f && manaCost == 0)
        {
            Debug.LogWarning($"[HabilidadBase] '{nombreHabilidad}' no tiene cooldown ni costo de mana. Puede ser spam infinito.");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Obtiene información de la habilidad en formato legible.
    /// Útil para tooltips en UI.
    /// </summary>
    public string ObtenerTooltip()
    {
        string tooltip = $"<b>{nombreHabilidad}</b>\n";
        tooltip += $"{descripcion}\n\n";

        if (damage > 0)
        {
            tooltip += $"<color=red>Daño: {damage}</color>\n";
        }

        if (healing > 0)
        {
            tooltip += $"<color=green>Curación: {healing}</color>\n";
        }

        tooltip += $"Costo: {manaCost} mana\n";
        tooltip += $"Cooldown: {cooldown}s\n";
        tooltip += $"Rango: {range}m\n";

        if (aoeRadius > 0)
        {
            tooltip += $"Radio AoE: {aoeRadius}m\n";
        }

        return tooltip;
    }

    /// <summary>
    /// Verifica si la habilidad puede ser usada con ciertos parámetros.
    /// </summary>
    public bool PuedeUsarse(int manaActual, bool enMovimiento)
    {
        // Verificar mana
        if (manaActual < manaCost) return false;

        // Verificar movimiento
        if (enMovimiento && !usableEnMovimiento) return false;

        return true;
    }

    /// <summary>
    /// Calcula el daño final basándose en las stats del jugador.
    /// </summary>
    public int CalcularDanioFinal(int damageJugador)
    {
        return Mathf.RoundToInt(damage + (damageJugador * damageMultiplier));
    }

    #endregion
}
