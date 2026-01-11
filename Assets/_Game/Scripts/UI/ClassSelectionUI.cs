using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// UI para seleccionar la clase del jugador al inicio del juego.
/// </summary>
public class ClassSelectionUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button btnMago;
    [SerializeField] private Button btnPaladin;
    [SerializeField] private Button btnClerigo;
    [SerializeField] private Button btnCazador;

    private PlayerClassSelector localPlayerSelector;

    void Start()
    {
        Debug.Log("[ClassSelectionUI] Start() llamado.");
        
        // Mostrar panel por defecto (se ocultará si ya seleccionó clase)
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("[ClassSelectionUI] Panel activado por defecto.");
        }
        else
        {
            Debug.LogError("[ClassSelectionUI] ¡Panel es NULL! Verifica las referencias en el Inspector.");
        }
        
        // Buscar al jugador local
        BuscarJugadorLocal();
        
        // Configurar botones
        if (btnMago != null) btnMago.onClick.AddListener(SeleccionarMago);
        if (btnPaladin != null) btnPaladin.onClick.AddListener(SeleccionarPaladin);
        if (btnClerigo != null) btnClerigo.onClick.AddListener(SeleccionarClerigo);
        if (btnCazador != null) btnCazador.onClick.AddListener(SeleccionarCazador);
    }

    void Update()
    {
        // Reintentar buscar si no se encontró
        if (localPlayerSelector == null)
        {
            BuscarJugadorLocal();
        }
    }

    void BuscarJugadorLocal()
    {
        if (NetworkClient.localPlayer != null)
        {
            localPlayerSelector = NetworkClient.localPlayer.GetComponent<PlayerClassSelector>();
            
            if (localPlayerSelector != null)
            {
                Debug.Log("[ClassSelectionUI] Jugador local encontrado.");
                
                // Si ya seleccionó clase, ocultar panel
                if (localPlayerSelector.YaSeleccionoClase())
                {
                    Debug.Log("[ClassSelectionUI] Jugador ya seleccionó clase, ocultando panel.");
                    OcultarPanel();
                }
                else
                {
                    Debug.Log("[ClassSelectionUI] Jugador NO ha seleccionado clase, mostrando panel.");
                    MostrarPanel();
                }
            }
            else
            {
                Debug.LogWarning("[ClassSelectionUI] NetworkClient.localPlayer existe pero no tiene PlayerClassSelector.");
            }
        }
        else
        {
            Debug.Log("[ClassSelectionUI] NetworkClient.localPlayer es NULL (esperando conexión...).");
        }
    }

    void SeleccionarMago()
    {
        Debug.Log("[ClassSelectionUI] Botón MAGO clickeado!");
        if (localPlayerSelector != null)
        {
            Debug.Log("[ClassSelectionUI] Llamando a SeleccionarMago() en PlayerClassSelector...");
            localPlayerSelector.SeleccionarMago();
            OcultarPanel();
        }
        else
        {
            Debug.LogError("[ClassSelectionUI] ¡localPlayerSelector es NULL! No se puede seleccionar clase.");
        }
    }

    void SeleccionarPaladin()
    {
        Debug.Log("[ClassSelectionUI] Botón PALADIN clickeado!");
        if (localPlayerSelector != null)
        {
            localPlayerSelector.SeleccionarPaladin();
            OcultarPanel();
        }
        else
        {
            Debug.LogError("[ClassSelectionUI] ¡localPlayerSelector es NULL!");
        }
    }

    void SeleccionarClerigo()
    {
        Debug.Log("[ClassSelectionUI] Botón CLERIGO clickeado!");
        if (localPlayerSelector != null)
        {
            localPlayerSelector.SeleccionarClerigo();
            OcultarPanel();
        }
        else
        {
            Debug.LogError("[ClassSelectionUI] ¡localPlayerSelector es NULL!");
        }
    }

    void SeleccionarCazador()
    {
        Debug.Log("[ClassSelectionUI] Botón CAZADOR clickeado!");
        if (localPlayerSelector != null)
        {
            localPlayerSelector.SeleccionarCazador();
            OcultarPanel();
        }
        else
        {
            Debug.LogError("[ClassSelectionUI] ¡localPlayerSelector es NULL!");
        }
    }

    void MostrarPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("[ClassSelectionUI] Panel mostrado.");
        }
    }

    void OcultarPanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("[ClassSelectionUI] Panel ocultado.");
        }
    }
}
