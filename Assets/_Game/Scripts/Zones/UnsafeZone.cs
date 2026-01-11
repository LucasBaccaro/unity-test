using UnityEngine;

/// <summary>
/// Componente helper para crear una zona peligrosa fácilmente.
///
/// Simplemente añade este componente a un GameObject con collider
/// y automáticamente se configurará como zona peligrosa (Unsafe Zone).
///
/// CARACTERÍSTICAS DE UNSAFE ZONE:
/// - PvP HABILITADO (jugadores pueden atacarse)
/// - Enemigos NPCs pueden spawnear aquí
/// - Full loot al morir
/// - Mayor riesgo = mayores recompensas
/// </summary>
[RequireComponent(typeof(ZoneController))]
public class UnsafeZone : MonoBehaviour
{
    #region Variables de Configuración

    [Header("Configuración de Zona Peligrosa")]
    [Tooltip("Material para el suelo/área peligrosa (rojo/oscuro)")]
    public Material materialSueloPeligroso;

    [Tooltip("Mostrar advertencia al entrar")]
    public bool mostrarAdvertencia = true;

    [Tooltip("Mensaje de advertencia al entrar a la zona")]
    public string mensajeAdvertencia = "¡PELIGRO! Has entrado a zona PvP. Otros jugadores pueden atacarte.";

    [Header("Spawning de Enemigos")]
    [Tooltip("Permite que enemigos NPCs spawnen en esta zona")]
    public bool permitirSpawnEnemigos = true;

    [Tooltip("Nivel de peligro (1-5). Afecta el tipo de enemigos que spawnean")]
    [Range(1, 5)]
    public int nivelPeligro = 1;

    #endregion

    #region Referencias

    private ZoneController zoneController;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Configura automáticamente el ZoneController como zona peligrosa.
    /// </summary>
    void Awake()
    {
        // Obtener o añadir ZoneController
        zoneController = GetComponent<ZoneController>();

        if (zoneController == null)
        {
            zoneController = gameObject.AddComponent<ZoneController>();
        }

        // IMPORTANTE: Configurar como zona PELIGROSA
        zoneController.tipoZona = ZoneType.Unsafe;

        // Configurar nombre por defecto si no tiene
        if (string.IsNullOrEmpty(zoneController.nombreZona) ||
            zoneController.nombreZona == "Zona Sin Nombre")
        {
            zoneController.nombreZona = $"Zona Peligrosa (Nivel {nivelPeligro})";
        }

        // Configurar descripción por defecto
        if (string.IsNullOrEmpty(zoneController.descripcion))
        {
            zoneController.descripcion = "¡Zona de PvP! Los jugadores pueden atacarse. Ten cuidado.";
        }

        Debug.Log($"[UnsafeZone] Zona peligrosa configurada: {zoneController.nombreZona} (Peligro: {nivelPeligro})");
    }

    void Start()
    {
        // Aplicar material al suelo si está configurado
        if (materialSueloPeligroso != null)
        {
            AplicarMaterialSuelo();
        }

        // TODO (FASE 7): Configurar spawners de enemigos si está permitido
        if (permitirSpawnEnemigos)
        {
            ConfigurarSpawnersEnemigos();
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Aplica el material de zona peligrosa al suelo/renderer de este objeto.
    /// </summary>
    void AplicarMaterialSuelo()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = materialSueloPeligroso;
            Debug.Log("[UnsafeZone] Material de suelo peligroso aplicado.");
        }
    }

    /// <summary>
    /// Configura los spawners de enemigos en esta zona.
    /// TODO (FASE 7): Implementar spawning de enemigos NPCs
    /// </summary>
    void ConfigurarSpawnersEnemigos()
    {
        // Por ahora solo log, implementaremos en FASE 7
        Debug.Log($"[UnsafeZone] Spawners de enemigos configurados (Nivel {nivelPeligro}).");

        // TODO (FASE 7):
        // - Crear spawners en puntos aleatorios dentro de la zona
        // - Configurar tipo de enemigos según nivelPeligro
        // - Configurar cantidad y frecuencia de spawn
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Método que pueden llamar otros scripts cuando un jugador entra.
    /// Muestra advertencia de PvP.
    /// </summary>
    public void OnJugadorEntra(GameObject jugador)
    {
        if (mostrarAdvertencia)
        {
            Debug.LogWarning($"[UnsafeZone] {jugador.name}: {mensajeAdvertencia}");

            // TODO (FASE 12): Mostrar advertencia en UI con efecto visual
            // UIManager.Instance.MostrarAdvertencia(mensajeAdvertencia);
        }
    }

    /// <summary>
    /// Obtiene el nivel de peligro de esta zona.
    /// Usado por el sistema de spawning (FASE 7).
    /// </summary>
    public int ObtenerNivelPeligro()
    {
        return nivelPeligro;
    }

    /// <summary>
    /// Verifica si se pueden spawnear enemigos en esta zona.
    /// </summary>
    public bool PuedenSpawnearEnemigos()
    {
        return permitirSpawnEnemigos;
    }

    #endregion
}
