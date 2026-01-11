using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Controla la barra de acción (Action Bar) donde se ven las habilidades.
/// </summary>
public class ActionBarUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image iconSlot1;
    [SerializeField] private Image iconSlot2;
    [SerializeField] private TextMeshProUGUI textSlot1;
    [SerializeField] private TextMeshProUGUI textSlot2;

    [Header("Placeholder Icons")]
    // TODO: En Fase final, los iconos vendrán del ScriptableObject de Habilidad
    [SerializeField] private Sprite defaultIcon;

    private PlayerCombat localPlayerCombat;
    private PlayerClassSelector localClassSelector;

    void Start()
    {
        BuscarJugadorLocal();
    }

    void Update()
    {
        if (localPlayerCombat == null)
        {
            BuscarJugadorLocal();
        }
    }

    void BuscarJugadorLocal()
    {
        if (NetworkClient.localPlayer != null)
        {
            localPlayerCombat = NetworkClient.localPlayer.GetComponent<PlayerCombat>();
            localClassSelector = NetworkClient.localPlayer.GetComponent<PlayerClassSelector>();

            if (localPlayerCombat != null)
            {
                Debug.Log("[ActionBarUI] Jugador local encontrado.");
                localPlayerCombat.OnAbilitiesChanged += ActualizarIconos;
                ActualizarIconos();
            }
        }
    }

    void OnDisable()
    {
        if (localPlayerCombat != null)
        {
            localPlayerCombat.OnAbilitiesChanged -= ActualizarIconos;
        }
    }

    /// <summary>
    /// LLamar cuando se selecciona la clase o cambian las habilidades.
    /// </summary>
    public void ActualizarIconos()
    {
        if (localPlayerCombat == null) return;

        // Slot 1
        if (localPlayerCombat.habilidad1 != null)
        {
            if (iconSlot1 != null)
            {
                iconSlot1.sprite = localPlayerCombat.habilidad1.iconoHabilidad != null ? localPlayerCombat.habilidad1.iconoHabilidad : defaultIcon;
                iconSlot1.enabled = true;
            }
            if (textSlot1 != null) textSlot1.text = "1";
        }
        else
        {
            if (iconSlot1 != null) iconSlot1.enabled = false;
            if (textSlot1 != null) textSlot1.text = "";
        }

        // Slot 2
        if (localPlayerCombat.habilidad2 != null)
        {
            if (iconSlot2 != null)
            {
                iconSlot2.sprite = localPlayerCombat.habilidad2.iconoHabilidad != null ? localPlayerCombat.habilidad2.iconoHabilidad : defaultIcon;
                iconSlot2.enabled = true;
            }
            if (textSlot2 != null) textSlot2.text = "2";
        }
        else
        {
            if (iconSlot2 != null) iconSlot2.enabled = false;
            if (textSlot2 != null) textSlot2.text = "";
        }
    }
}
