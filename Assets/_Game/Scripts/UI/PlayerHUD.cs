using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Controla la interfaz del jugador (Barras de Vida y Mana).
/// Se conecta a los eventos de PlayerStats del jugador local.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI manaText;

    private PlayerStats localPlayerStats;

    void Start()
    {
        // Buscar al jugador local
        BuscarJugadorLocal();
    }

    void OnEnable()
    {
        // Si ya tenemos referencia, nos suscribimos (útil si se desactiva/activa el objeto)
        if (localPlayerStats != null)
        {
            SuscribirEventos();
        }
    }

    void OnDisable()
    {
        if (localPlayerStats != null)
        {
            DesuscribirEventos();
        }
    }

    void Update()
    {
        // Reintentar buscar al jugador si falló en Start (puede pasar por tiempos de carga)
        if (localPlayerStats == null)
        {
            BuscarJugadorLocal();
        }
    }

    void BuscarJugadorLocal()
    {
        if (NetworkClient.localPlayer != null)
        {
            localPlayerStats = NetworkClient.localPlayer.GetComponent<PlayerStats>();
            
            if (localPlayerStats != null)
            {
                Debug.Log("[PlayerHUD] Jugador local encontrado. Conectando eventos...");
                SuscribirEventos();
                
                // Actualización inicial
                ActualizarSalud(localPlayerStats.currentHP, localPlayerStats.maxHP);
                ActualizarMana(localPlayerStats.currentMana, localPlayerStats.maxMana);
            }
        }
    }

    void SuscribirEventos()
    {
        localPlayerStats.OnHealthChanged += ActualizarSalud;
        localPlayerStats.OnManaChanged += ActualizarMana;
    }

    void DesuscribirEventos()
    {
        if (localPlayerStats != null)
        {
            localPlayerStats.OnHealthChanged -= ActualizarSalud;
            localPlayerStats.OnManaChanged -= ActualizarMana;
        }
    }

    public void ActualizarSalud(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }
    }

    public void ActualizarMana(int current, int max)
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = max;
            manaSlider.value = current;
        }

        if (manaText != null)
        {
            manaText.text = $"{current} / {max}";
        }
    }
}
