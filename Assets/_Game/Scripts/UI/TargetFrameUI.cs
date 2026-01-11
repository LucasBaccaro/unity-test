using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI que muestra la información del objetivo seleccionado (Nombre, HP).
/// </summary>
public class TargetFrameUI : MonoBehaviour
{
    public static TargetFrameUI Instance;

    [Header("Referencias UI")]
    public GameObject panel;        // El panel completo (para ocultarlo si no hay target)
    public Text nombreText;         // Texto del nombre
    public Slider hpSlider;         // Barra de vida
    public Text hpText;             // Texto de vida (ej: 100/100)

    private NetworkIdentity targetActual;

    void Awake()
    {
        Instance = this;
        Ocultar();
    }

    void Update()
    {
        // Si tenemos un target, actualizar su info en cada frame (o usar eventos para optimizar)
        if (targetActual != null)
        {
            ActualizarInfo();
        }
    }

    public void SetTarget(NetworkIdentity target)
    {
        targetActual = target;
        Mostrar();
    }

    public void ClearTarget()
    {
        targetActual = null;
        Ocultar();
    }

    void Mostrar()
    {
        if (panel != null) panel.SetActive(true);
    }

    void Ocultar()
    {
        if (panel != null) panel.SetActive(false);
    }

    void ActualizarInfo()
    {
        if (targetActual == null) return;
        
        // Intentar obtener componentes del target
        // NOTA: Esto depende de PlayerStats o ClaseBase
        // Por ahora usamos nombres genéricos
        
        if (nombreText != null)
        {
            nombreText.text = targetActual.name;
        }

        // TODO: Leer HP real desde PlayerStats
        // var stats = targetActual.GetComponent<PlayerStats>();
        // if (stats != null) { hpSlider.value = stats.currentHealth; }
    }
}
