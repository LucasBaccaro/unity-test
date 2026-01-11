using Mirror;
using UnityEngine;

/// <summary>
/// Muestra información de debugging del estado del networking en pantalla.
///
/// Esto te ayuda a identificar problemas como:
/// - ¿El servidor está activo?
/// - ¿Cuántos jugadores están conectados?
/// - ¿El player prefab está asignado?
/// - ¿Por qué no se spawnea el jugador?
///
/// USO:
/// Añade este script a cualquier GameObject en la escena.
/// La información aparecerá en la esquina superior izquierda del Game View.
/// </summary>
public class NetworkDebugger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Mostrar debug info en pantalla")]
    public bool mostrarDebugUI = true;

    [Tooltip("Tamaño del texto")]
    public int tamañoTexto = 16;

    private NetworkManager networkManager;
    private GUIStyle estilo;

    void Start()
    {
        // Buscar NetworkManager en la escena
        networkManager = FindFirstObjectByType<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("[NetworkDebugger] No se encontró NetworkManager en la escena!");
        }

        // Configurar estilo de texto
        estilo = new GUIStyle();
        estilo.fontSize = tamañoTexto;
        estilo.normal.textColor = Color.white;
        estilo.padding = new RectOffset(10, 10, 10, 10);
    }

    void OnGUI()
    {
        if (!mostrarDebugUI || networkManager == null)
        {
            return;
        }

        // Fondo semi-transparente
        GUI.Box(new Rect(10, 10, 400, 350), "");

        string info = "=== NETWORK DEBUG INFO ===\n\n";

        // Estado del servidor
        info += $"<b>Estado del Servidor:</b>\n";
        info += $"  • Server Active: {NetworkServer.active}\n";
        info += $"  • Client Connected: {NetworkClient.isConnected}\n";
        info += $"  • Num Players: {NetworkServer.connections.Count}\n\n";

        // Información del NetworkManager
        if (networkManager != null)
        {
            info += $"<b>Network Manager:</b>\n";
            info += $"  • Player Prefab: {(networkManager.playerPrefab != null ? "✓ Asignado" : "✗ NO ASIGNADO")}\n";
            info += $"  • Max Connections: {networkManager.maxConnections}\n";
            info += $"  • Network Address: {networkManager.networkAddress}\n\n";
        }

        // Información del jugador local
        if (NetworkClient.localPlayer != null)
        {
            info += $"<b>Jugador Local:</b>\n";
            info += $"  • Spawneado: ✓ SÍ\n";
            info += $"  • Nombre: {NetworkClient.localPlayer.name}\n";
            info += $"  • NetID: {NetworkClient.localPlayer.netId}\n\n";
        }
        else
        {
            info += $"<b>Jugador Local:</b>\n";
            info += $"  • Spawneado: ✗ NO\n\n";

            // Diagnosticar por qué no se spawnea
            if (!NetworkServer.active && !NetworkClient.isConnected)
            {
                info += $"<color=red><b>PROBLEMA:</b> Ni servidor ni cliente activos.</color>\n";
                info += $"<color=yellow>SOLUCIÓN: Añade AutoStartHost al NetworkManager\n";
                info += $"o haz clic en Start Host manualmente.</color>\n\n";
            }
            else if (networkManager != null && networkManager.playerPrefab == null)
            {
                info += $"<color=red><b>PROBLEMA:</b> Player Prefab NO asignado.</color>\n";
                info += $"<color=yellow>SOLUCIÓN: Arrastra Player.prefab al campo\n";
                info += $"'Player Prefab' en el NetworkManager.</color>\n\n";
            }
        }

        // Todos los jugadores en el servidor
        if (NetworkServer.active && NetworkServer.connections.Count > 0)
        {
            info += $"<b>Jugadores Conectados:</b>\n";
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn.identity != null)
                {
                    info += $"  • {conn.identity.name} (ID: {conn.connectionId})\n";
                }
            }
        }

        // Mostrar en pantalla
        GUI.Label(new Rect(20, 20, 380, 330), info, estilo);
    }

    void Update()
    {
        // Logs en consola cada 5 segundos (menos spam)
        if (Time.frameCount % 300 == 0)
        {
            LogEstado();
        }
    }

    void LogEstado()
    {
        Debug.Log("=== NETWORK STATUS ===");
        Debug.Log($"Server Active: {NetworkServer.active}");
        Debug.Log($"Client Connected: {NetworkClient.isConnected}");
        Debug.Log($"Num Players: {NetworkServer.connections.Count}");

        if (NetworkClient.localPlayer != null)
        {
            Debug.Log($"Local Player: {NetworkClient.localPlayer.name}");
        }
        else
        {
            Debug.LogWarning("Local Player: NO SPAWNEADO");

            // Diagnosticar
            if (networkManager != null)
            {
                if (networkManager.playerPrefab == null)
                {
                    Debug.LogError("PROBLEMA: Player Prefab NO está asignado en NetworkManager!");
                }

                if (!NetworkServer.active && !NetworkClient.isConnected)
                {
                    Debug.LogError("PROBLEMA: Ni servidor ni cliente están activos. ¿Hiciste Start Host?");
                }
            }
        }

        Debug.Log("=====================");
    }
}
