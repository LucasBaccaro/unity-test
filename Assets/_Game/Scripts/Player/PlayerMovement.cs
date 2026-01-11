using Mirror;
using UnityEngine;

/// <summary>
/// Maneja el movimiento del jugador en el MMO.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    #region Variables de Configuración

    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento del jugador (unidades por segundo)")]
    [Range(1f, 20f)]
    public float velocidadMovimiento = 5f;

    [Tooltip("Velocidad de rotación del jugador (grados por segundo)")]
    [Range(100f, 500f)]
    public float velocidadRotacion = 300f;

    [Tooltip("Si está activado, el jugador puede moverse")]
    public bool puedeMoverse = true;

    [Header("Configuración de Gravedad")]
    [Tooltip("Fuerza de gravedad personalizada")]
    public float gravedad = 20f;

    #endregion

    #region Referencias

    private CharacterController characterController;

    #endregion

    #region Variables Privadas

    // Vector de movimiento actual (Input)
    private Vector3 inputMovimiento;

    // Dirección de rotación actual (Input)
    private float inputRotacion;

    // Velocidad vertical acumulada (Gravedad)
    private float velocidadVertical;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Inicializa componentes.
    /// </summary>
    void Awake()
    {
        // Obtener CharacterController (requerido por [RequireComponent])
        characterController = GetComponent<CharacterController>();

        Debug.Log("[PlayerMovement] Inicializado.");
    }

    /// <summary>
    /// Start se llama antes del primer frame.
    /// </summary>
    void Start()
    {
        // Inicializar velocidad vertical
        velocidadVertical = 0f;
        
        // Verificar configuración del CharacterController
        Debug.Log($"[PlayerMovement] CharacterController - Radius: {characterController.radius}, Height: {characterController.height}, Center: {characterController.center}");
        
        if (characterController.radius <= 0.1f)
        {
            Debug.LogError("[PlayerMovement] ⚠️ CharacterController radius es muy pequeño! Debe ser al menos 0.3");
        }
        
        if (characterController.height <= 0.5f)
        {
            Debug.LogError("[PlayerMovement] ⚠️ CharacterController height es muy pequeño! Debe ser al menos 1.5");
        }

        // Verificar si hay suelo debajo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            Debug.Log($"[PlayerMovement] ✓ Suelo detectado a {hit.distance} unidades debajo. Objeto: {hit.collider.name}");
        }
        else
        {
            Debug.LogError("[PlayerMovement] ❌ NO HAY SUELO DEBAJO! Crea un plano con collider.");
        }
    }

    /// <summary>
    /// Update se llama cada frame.
    /// </summary>
    void Update()
    {
        // NETWORKING: Solo procesar si este es nuestro jugador local
        if (!isLocalPlayer) return;

        // DEBUG: Mostrar estado cada segundo
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PlayerMovement] isGrounded: {characterController.isGrounded}, Y: {transform.position.y:F2}, VelVertical: {velocidadVertical:F2}");
        }

        // 1. Procesar Input (solo si puede moverse)
        if (puedeMoverse)
        {
            ProcesarInput();
        }
        else
        {
            inputMovimiento = Vector3.zero;
            inputRotacion = 0f;
        }

        // 2. Aplicar Física y Movimiento (Siempre, para gravedad)
        AplicarFisicasYMovimiento();
    }

    #endregion

    #region Input y Movimiento

    /// <summary>
    /// Lee el input del teclado.
    /// </summary>
    void ProcesarInput()
    {
        // Leer input del teclado (WASD o flechas)
        float horizontal = Input.GetAxis("Horizontal"); // A/D o Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S o Up/Down

        // Calcular dirección en espacio local
        Vector3 direccion = new Vector3(horizontal, 0, vertical);

        // Normalizar si es necesario (evitar movimiento diagonal más rápido)
        if (direccion.magnitude > 1f)
        {
            direccion.Normalize();
        }

        inputMovimiento = direccion;

        // Rotación con Q y E
        if (Input.GetKey(KeyCode.Q)) inputRotacion = -1f;
        else if (Input.GetKey(KeyCode.E)) inputRotacion = 1f;
        else inputRotacion = 0f;

        // DEBUG: Mostrar input
        if (direccion.magnitude > 0.01f)
        {
            Debug.Log($"[Input] H: {horizontal:F2}, V: {vertical:F2}");
        }
    }

    /// <summary>
    /// Calcula movimientos finales y aplica al CharacterController.
    /// </summary>
    void AplicarFisicasYMovimiento()
    {
        // --- 1. Movimiento Horizontal (Input) ---
        Vector3 movimientoFinal = Vector3.zero;

        if (inputMovimiento.magnitude > 0.01f)
        {
            // Transformar input al espacio del mundo según la rotación del jugador
            Vector3 movimientoLocal = transform.TransformDirection(inputMovimiento);
            movimientoLocal.y = 0; // Eliminar componente vertical
            movimientoFinal = movimientoLocal * velocidadMovimiento;
        }

        // --- 2. Gravedad (Acumulativa) ---
        if (characterController.isGrounded)
        {
            // Si estamos en el suelo, resetear velocidad vertical
            // Usar -2f en lugar de 0 para mantener contacto firme con el suelo
            velocidadVertical = -2f; 
        }
        else
        {
            // Aplicar gravedad (caída libre)
            velocidadVertical -= gravedad * Time.deltaTime;
            
            // Limitar velocidad de caída para evitar atravesar el suelo
            velocidadVertical = Mathf.Max(velocidadVertical, -50f);
        }

        // Aplicar velocidad vertical
        movimientoFinal.y = velocidadVertical;

        // --- 3. Mover ---
        characterController.Move(movimientoFinal * Time.deltaTime);

        // --- 4. Rotación ---
        if (Mathf.Abs(inputRotacion) > 0.01f)
        {
            float rotacion = inputRotacion * velocidadRotacion * Time.deltaTime;
            transform.Rotate(0, rotacion, 0);
        }
    }

    #endregion

    #region Commands (Cliente → Servidor)

    [Command]
    void CmdValidarMovimiento(Vector3 nuevaPosicion)
    {
        float distancia = Vector3.Distance(transform.position, nuevaPosicion);

        if (distancia > velocidadMovimiento * 2f)
        {
            Debug.LogWarning($"[SERVER] Movimiento sospechoso detectado. Distancia: {distancia}");
            return;
        }

        transform.position = nuevaPosicion;
    }

    #endregion

    #region Métodos Públicos

    [Server]
    public void TeletransportarA(Vector3 posicion)
    {
        Debug.Log($"[SERVER] Teletransportando jugador a: {posicion}");

        characterController.enabled = false;
        transform.position = posicion;
        characterController.enabled = true;
        
        velocidadVertical = 0f;
    }

    public void PermitirMovimiento(bool puede)
    {
        puedeMoverse = puede;

        if (!puede)
        {
            inputMovimiento = Vector3.zero;
            inputRotacion = 0f;
        }

        Debug.Log($"[PlayerMovement] Movimiento {(puede ? "habilitado" : "bloqueado")}.");
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!isLocalPlayer) return;

        // Dibujar dirección de movimiento (verde)
        if (inputMovimiento.magnitude > 0.01f)
        {
            Gizmos.color = Color.green;
            Vector3 dir = transform.TransformDirection(inputMovimiento);
            Gizmos.DrawRay(transform.position + Vector3.up, dir * 2f);
        }

        // Dibujar dirección hacia donde mira (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);

        // Dibujar estado de suelo (rojo si no está en el suelo, verde si está)
        if (characterController != null)
        {
            Gizmos.color = characterController.isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }

    #endregion
}