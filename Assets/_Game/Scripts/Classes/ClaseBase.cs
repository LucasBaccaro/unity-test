using UnityEngine;

/// <summary>
/// ScriptableObject base que define una clase de personaje.
///
/// SCRIPTABLEOBJECT: Este es un asset de datos reutilizable.
/// Se crea desde el editor (Create > MMO > Clase) y se guarda en el proyecto.
///
/// Ventajas de ScriptableObjects:
/// - Fácil de crear y editar sin código
/// - Reutilizable entre múltiples jugadores
/// - Se puede modificar en tiempo de desarrollo sin recompilar
/// - Perfecto para data-driven design
///
/// Cada clase (Mago, Paladin, etc.) será un asset separado con valores diferentes.
/// </summary>
[CreateAssetMenu(fileName = "Nueva_Clase", menuName = "MMO/Clase", order = 1)]
public class ClaseBase : ScriptableObject
{
    #region Información Básica

    [Header("Información de la Clase")]
    [Tooltip("Nombre de la clase (ej: 'Mago', 'Paladin')")]
    public string nombreClase = "Clase Sin Nombre";

    [Tooltip("Descripción de la clase y su rol")]
    [TextArea(3, 5)]
    public string descripcion = "Una clase poderosa...";

    [Tooltip("Icono de la clase (para UI de selección)")]
    public Sprite iconoClase;

    #endregion

    #region Stats Base (Nivel 1)

    [Header("Stats Base (Nivel 1)")]
    [Tooltip("Puntos de vida base")]
    [Range(50, 200)]
    public int hpBase = 100;

    [Tooltip("Puntos de mana base")]
    [Range(30, 200)]
    public int manaBase = 100;

    [Tooltip("Daño base")]
    [Range(5, 30)]
    public int damageBase = 10;

    [Tooltip("Defensa base")]
    [Range(0, 20)]
    public int defenseBase = 5;

    [Tooltip("Velocidad de movimiento base")]
    [Range(3f, 8f)]
    public float speedBase = 5f;

    #endregion

    #region Habilidades

    [Header("Habilidades de la Clase")]
    [Tooltip("Lista de habilidades que tiene esta clase (máximo 2 para MVP)")]
    public HabilidadBase[] habilidades = new HabilidadBase[2];

    #endregion

    #region Preferencias Visuales

    [Header("Visualización")]
    [Tooltip("Color representativo de la clase (para UI y efectos)")]
    public Color colorClase = Color.white;

    [Tooltip("Prefab del modelo 3D de esta clase (opcional, para después)")]
    public GameObject modeloPrefab;

    #endregion

    #region Métodos de Validación

    /// <summary>
    /// Valida que la clase esté configurada correctamente.
    /// Se llama en el Editor para detectar errores de configuración.
    /// </summary>
    void OnValidate()
    {
        // Validar que el nombre no esté vacío
        if (string.IsNullOrEmpty(nombreClase))
        {
            Debug.LogWarning($"[ClaseBase] La clase '{name}' no tiene nombre configurado.");
        }

        // Validar que tenga al menos una habilidad
        bool tieneHabilidades = false;
        foreach (var habilidad in habilidades)
        {
            if (habilidad != null)
            {
                tieneHabilidades = true;
                break;
            }
        }

        if (!tieneHabilidades)
        {
            Debug.LogWarning($"[ClaseBase] La clase '{nombreClase}' no tiene habilidades asignadas.");
        }

        // Validar rangos de stats
        if (hpBase < 50)
        {
            Debug.LogWarning($"[ClaseBase] '{nombreClase}' tiene HP muy bajo ({hpBase}). Mínimo recomendado: 50");
        }

        if (damageBase <= 0)
        {
            Debug.LogError($"[ClaseBase] '{nombreClase}' tiene damage en 0 o negativo!");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Obtiene información de la clase en formato legible.
    /// Útil para UI de selección de clase.
    /// </summary>
    public string ObtenerInfoCompleta()
    {
        string info = $"=== {nombreClase} ===\n";
        info += $"{descripcion}\n\n";
        info += $"Stats Base:\n";
        info += $"• HP: {hpBase}\n";
        info += $"• Mana: {manaBase}\n";
        info += $"• Damage: {damageBase}\n";
        info += $"• Defense: {defenseBase}\n";
        info += $"• Speed: {speedBase}\n\n";
        info += $"Habilidades:\n";

        for (int i = 0; i < habilidades.Length; i++)
        {
            if (habilidades[i] != null)
            {
                info += $"• [{i + 1}] {habilidades[i].nombreHabilidad}\n";
            }
        }

        return info;
    }

    /// <summary>
    /// Obtiene el número de habilidades configuradas.
    /// </summary>
    public int GetNumeroHabilidades()
    {
        int count = 0;
        foreach (var habilidad in habilidades)
        {
            if (habilidad != null) count++;
        }
        return count;
    }

    /// <summary>
    /// Obtiene una habilidad específica por índice.
    /// </summary>
    public HabilidadBase GetHabilidad(int indice)
    {
        if (indice < 0 || indice >= habilidades.Length)
        {
            Debug.LogError($"[ClaseBase] Índice de habilidad fuera de rango: {indice}");
            return null;
        }

        return habilidades[indice];
    }

    #endregion
}
