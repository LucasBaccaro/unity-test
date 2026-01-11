using Mirror;
using UnityEngine;

/// <summary>
/// Network Manager personalizado para nuestro MMO.
///
/// NETWORKING: Este script hereda de Mirror.NetworkManager, que es el cerebro
/// del sistema de networking. Gestiona conexiones de clientes, spawning de jugadores,
/// y toda la lógica de servidor/cliente.
///
/// Este es el PRIMER script que se ejecuta cuando se inicia el servidor o cliente.
/// </summary>
public class NetworkManager_MMO : NetworkManager
{
    #region Variables de Configuración

    [Header("Configuración del MMO")]
    [Tooltip("Prefab del jugador que se instanciará cuando un cliente se conecte")]
    public GameObject playerPrefabMMO;

    [Tooltip("Posición de spawn en la ciudad (zona segura)")]
    public Transform spawnPointCiudad;

    [Tooltip("Máximo de jugadores simultáneos (2-5 para MVP)")]
    [Range(2, 10)]
    public int maxJugadores = 5;

    [Tooltip("Si es true, el Host se iniciará automáticamente al dar Play en el editor")]
    public bool autoStartInEditor = true;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Se llama cuando el NetworkManager se inicializa.
    /// Configuramos los límites del servidor aquí.
    /// </summary>
    public override void Start()
    {
        base.Start();

        // Configurar máximo de conexiones
        maxConnections = maxJugadores;

        Debug.Log($"[NetworkManager_MMO] Inicializado. Máximo de jugadores: {maxJugadores}");

        // Auto-start en editor para facilitar testing
        if (autoStartInEditor && Application.isEditor)
        {
            Debug.Log("[NetworkManager_MMO] Auto-iniciando Host en Editor...");
            StartHost();
        }
    }

    #endregion

    #region Server Callbacks

    /// <summary>
    /// Se llama EN EL SERVIDOR cuando un nuevo jugador se conecta.
    ///
    /// NETWORKING: Este método es CRÍTICO. Es donde spawneamos (creamos) el jugador
    /// en el mundo del juego para que todos los clientes lo vean.
    ///
    /// IMPORTANTE: Este código SOLO se ejecuta en el servidor, NUNCA en los clientes.
    /// </summary>
    /// <param name="conn">La conexión del cliente que acaba de conectarse</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"[SERVER] Nuevo jugador conectándose. ConnectionID: {conn.connectionId}");

        // Determinar posición de spawn
        // Si tenemos un spawn point configurado, usarlo. Si no, usar posición por defecto.
        Vector3 posicionSpawn = spawnPointCiudad != null
            ? spawnPointCiudad.position
            : Vector3.zero;

        // Añadir un pequeño offset aleatorio para que los jugadores no spawnen exactamente
        // en la misma posición (evitar que se superpongan)
        posicionSpawn += new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f)
        );

        // NETWORKING: Instanciar el prefab del jugador en la posición de spawn
        // Importante: Usar el playerPrefab de NetworkManager, no nuestro custom
        GameObject jugador = Instantiate(
            playerPrefab,           // Prefab del jugador (configurado en el Inspector)
            posicionSpawn,          // Posición donde spawneamos
            Quaternion.identity     // Rotación inicial (sin rotación)
        );

        // NETWORKING: NetworkServer.AddPlayerForConnection hace DOS cosas importantes:
        // 1. Registra este GameObject como el jugador de esta conexión
        // 2. Spawnea el objeto en la red (lo hace visible para todos los clientes)
        // 3. Le da "authority" al cliente para controlar este jugador
        NetworkServer.AddPlayerForConnection(conn, jugador);

        Debug.Log($"[SERVER] Jugador spawneado en posición: {posicionSpawn}");
    }

    /// <summary>
    /// Se llama EN EL SERVIDOR cuando un cliente se desconecta.
    ///
    /// NETWORKING: Aquí podríamos guardar datos del jugador antes de que se vaya.
    /// En la FASE 11 (Persistencia) usaremos esto para guardar el progreso.
    /// </summary>
    /// <param name="conn">La conexión del cliente que se desconectó</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"[SERVER] Jugador desconectado. ConnectionID: {conn.connectionId}");

        // TODO (FASE 11): Guardar datos del jugador en memoria
        // PlayerData data = SavePlayerData(conn);
        // ServerManager.Instance.SavePlayerData(data);

        // Llamar al método base para limpiar la conexión
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// Se llama EN EL SERVIDOR cuando el servidor se inicia exitosamente.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[SERVER] Servidor iniciado exitosamente.");
        Debug.Log($"[SERVER] Esperando jugadores... (máximo: {maxJugadores})");
    }

    /// <summary>
    /// Se llama EN EL SERVIDOR cuando el servidor se detiene.
    /// </summary>
    public override void OnStopServer()
    {
        Debug.Log("[SERVER] Servidor detenido.");
        base.OnStopServer();
    }

    #endregion

    #region Client Callbacks

    /// <summary>
    /// Se llama EN EL CLIENTE cuando se conecta exitosamente al servidor.
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("[CLIENT] Conectado al servidor exitosamente.");
    }

    /// <summary>
    /// Se llama EN EL CLIENTE cuando se desconecta del servidor.
    /// </summary>
    public override void OnClientDisconnect()
    {
        Debug.Log("[CLIENT] Desconectado del servidor.");
        base.OnClientDisconnect();
    }

    /// <summary>
    /// Se llama EN EL CLIENTE cuando el cliente se inicia.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("[CLIENT] Cliente iniciado.");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Método helper para verificar si el servidor está lleno.
    /// Útil para mostrar mensajes de "servidor lleno" en la UI.
    /// </summary>
    public bool ServidorLleno()
    {
        return numPlayers >= maxJugadores;
    }

    #endregion

    #region GUI Testing (Temporal)

    void OnGUI()
    {
        // Solo mostrar si no estamos conectados
        if (!NetworkServer.active && !NetworkClient.isConnected)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            
            GUILayout.Label("Modo de Conexión:");
            
            if (GUILayout.Button("Host (Server + Client)"))
            {
                StartHost();
            }
            
            if (GUILayout.Button("Client"))
            {
                networkAddress = "localhost"; // Asegurar localhost
                StartClient();
            }

            if (GUILayout.Button("Server Only"))
            {
                StartServer();
            }

            GUILayout.EndArea();
        }
    }

    #endregion
}
