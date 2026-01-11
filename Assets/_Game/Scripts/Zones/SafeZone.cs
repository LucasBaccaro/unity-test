using UnityEngine;

/// <summary>
/// Componente helper para crear una zona segura fácilmente.
///
/// Simplemente añade este componente a un GameObject con collider
/// y automáticamente se configurará como zona segura (Safe Zone).
///
/// CARACTERÍSTICAS DE SAFE ZONE:
/// - PvP deshabilitado (jugadores no pueden atacarse)
/// - No hay enemigos NPCs
/// - Ideal para ciudades, puntos de spawn, vendors
/// </summary>
[RequireComponent(typeof(ZoneController))]
public class SafeZone : MonoBehaviour
{
    #region Variables de Configuración

    [Header("Configuración de Zona Segura")]
    [Tooltip("Permite cambiar el color del suelo/área (opcional)")]
    public Material materialSueloSeguro;

    [Tooltip("Mostrar mensaje de bienvenida al entrar")]
    public bool mostrarMensajeBienvenida = true;

    [Tooltip("Mensaje personalizado al entrar a la zona")]
    public string mensajeBienvenida = "Has entrado a una Zona Segura. PvP deshabilitado.";

    #endregion

    #region Referencias

    private ZoneController zoneController;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Configura automáticamente el ZoneController como zona segura.
    /// </summary>
    void Awake()
    {
        // Obtener o añadir ZoneController
        zoneController = GetComponent<ZoneController>();

        if (zoneController == null)
        {
            zoneController = gameObject.AddComponent<ZoneController>();
        }

        // IMPORTANTE: Configurar como zona SEGURA
        zoneController.tipoZona = ZoneType.Safe;

        // Configurar nombre por defecto si no tiene
        if (string.IsNullOrEmpty(zoneController.nombreZona) ||
            zoneController.nombreZona == "Zona Sin Nombre")
        {
            zoneController.nombreZona = "Ciudad Segura";
        }

        // Configurar descripción por defecto
        if (string.IsNullOrEmpty(zoneController.descripcion))
        {
            zoneController.descripcion = "Zona protegida. No se permite combate entre jugadores.";
        }

        Debug.Log($"[SafeZone] Zona segura configurada: {zoneController.nombreZona}");
    }

    void Start()
    {
        // Aplicar material al suelo si está configurado
        if (materialSueloSeguro != null)
        {
            AplicarMaterialSuelo();
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Aplica el material de zona segura al suelo/renderer de este objeto.
    /// </summary>
    void AplicarMaterialSuelo()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = materialSueloSeguro;
            Debug.Log("[SafeZone] Material de suelo seguro aplicado.");
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Método que pueden llamar otros scripts cuando un jugador entra.
    /// Por ejemplo, para mostrar UI de bienvenida.
    /// </summary>
    public void OnJugadorEntra(GameObject jugador)
    {
        if (mostrarMensajeBienvenida)
        {
            Debug.Log($"[SafeZone] {jugador.name}: {mensajeBienvenida}");

            // TODO (FASE 12): Mostrar mensaje en UI
            // UIManager.Instance.MostrarNotificacion(mensajeBienvenida);
        }
    }

    #endregion
}
