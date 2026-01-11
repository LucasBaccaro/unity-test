using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de datos singleton de todos los items del juego.
///
/// PATRON SINGLETON: Solo existe una instancia de ItemDatabase en el juego.
/// Todos los scripts acceden a ella vía ItemDatabase.Instance.
///
/// PROPÓSITO:
/// Mantiene un diccionario de todos los ItemData del juego cargados en memoria
/// para acceso rápido por ID.
///
/// NETWORKING:
/// - Cada cliente tiene su propia copia de ItemDatabase (no se sincroniza)
/// - Solo se sincronizan los IDs de items, no los ScriptableObjects
/// - Los clientes buscan el ID en su base de datos local
///
/// IMPORTANTE: Todos los clientes DEBEN tener los mismos ItemData assets
/// en sus proyectos. Si un cliente no tiene un item, verá un error.
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    #region Singleton

    private static ItemDatabase _instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemDatabase>();

                if (_instance == null)
                {
                    // Crear GameObject con ItemDatabase si no existe
                    GameObject go = new GameObject("ItemDatabase");
                    _instance = go.AddComponent<ItemDatabase>();
                    DontDestroyOnLoad(go);

                    Debug.Log("[ItemDatabase] Instancia creada automáticamente.");
                }
            }

            return _instance;
        }
    }

    #endregion

    #region Variables de Configuración

    [Header("Configuración de Items")]
    [Tooltip("Lista de todos los ItemData del juego")]
    public List<ItemData> todosLosItems = new List<ItemData>();

    [Tooltip("Cargar automáticamente items desde Resources/Items")]
    public bool cargarDesdeResources = true;

    #endregion

    #region Variables Privadas

    // Diccionario para búsqueda rápida por ID
    // Key: itemID, Value: ItemData
    private Dictionary<int, ItemData> itemDictionary = new Dictionary<int, ItemData>();

    // Flag para saber si ya se inicializó
    private bool inicializado = false;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        // Configurar singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializar base de datos
        InicializarBaseDeDatos();
    }

    #endregion

    #region Inicialización

    /// <summary>
    /// Inicializa la base de datos cargando todos los items.
    /// </summary>
    void InicializarBaseDeDatos()
    {
        if (inicializado)
        {
            Debug.LogWarning("[ItemDatabase] Ya está inicializada.");
            return;
        }

        Debug.Log("[ItemDatabase] Inicializando base de datos de items...");

        // Limpiar diccionario
        itemDictionary.Clear();

        // Cargar items desde Resources si está habilitado
        if (cargarDesdeResources)
        {
            CargarItemsDesdeResources();
        }

        // Añadir items de la lista manual
        foreach (ItemData item in todosLosItems)
        {
            RegistrarItem(item);
        }

        inicializado = true;

        Debug.Log($"[ItemDatabase] Inicializada con {itemDictionary.Count} items.");
    }

    /// <summary>
    /// Carga todos los ItemData desde la carpeta Resources/Items.
    /// </summary>
    void CargarItemsDesdeResources()
    {
        ItemData[] items = Resources.LoadAll<ItemData>("Items");

        Debug.Log($"[ItemDatabase] Cargando {items.Length} items desde Resources/Items...");

        foreach (ItemData item in items)
        {
            RegistrarItem(item);
        }
    }

    /// <summary>
    /// Registra un item en el diccionario.
    /// </summary>
    void RegistrarItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemDatabase] Intentando registrar item null.");
            return;
        }

        // Verificar que el ID sea válido
        if (item.itemID <= 0)
        {
            Debug.LogError($"[ItemDatabase] Item '{item.itemName}' tiene itemID inválido: {item.itemID}");
            return;
        }

        // Verificar si ya existe un item con este ID
        if (itemDictionary.ContainsKey(item.itemID))
        {
            Debug.LogError($"[ItemDatabase] ID DUPLICADO: {item.itemID}. Items: '{itemDictionary[item.itemID].itemName}' y '{item.itemName}'");
            Debug.LogError("[ItemDatabase] Cada item DEBE tener un ID único!");
            return;
        }

        // Registrar item
        itemDictionary[item.itemID] = item;

        Debug.Log($"[ItemDatabase] Item registrado: [{item.itemID}] {item.itemName}");
    }

    #endregion

    #region Métodos Públicos de Búsqueda

    /// <summary>
    /// Obtiene un ItemData por su ID.
    /// </summary>
    /// <param name="itemID">ID del item a buscar</param>
    /// <returns>El ItemData, o null si no se encuentra</returns>
    public ItemData GetItem(int itemID)
    {
        // Verificar que esté inicializado
        if (!inicializado)
        {
            InicializarBaseDeDatos();
        }

        // Buscar en diccionario
        if (itemDictionary.TryGetValue(itemID, out ItemData item))
        {
            return item;
        }

        Debug.LogWarning($"[ItemDatabase] Item con ID {itemID} no encontrado en la base de datos.");
        return null;
    }

    /// <summary>
    /// Obtiene un ItemData por su nombre.
    /// NOTA: Búsqueda lenta (O(n)), usar solo para debugging/tools.
    /// Para uso en juego, siempre usar GetItem(int itemID).
    /// </summary>
    /// <param name="itemName">Nombre del item (exacto, case-sensitive)</param>
    /// <returns>El ItemData, o null si no se encuentra</returns>
    public ItemData GetItemByName(string itemName)
    {
        foreach (var kvp in itemDictionary)
        {
            if (kvp.Value.itemName == itemName)
            {
                return kvp.Value;
            }
        }

        Debug.LogWarning($"[ItemDatabase] Item con nombre '{itemName}' no encontrado.");
        return null;
    }

    /// <summary>
    /// Verifica si existe un item con el ID dado.
    /// </summary>
    public bool ExisteItem(int itemID)
    {
        return itemDictionary.ContainsKey(itemID);
    }

    /// <summary>
    /// Obtiene todos los items de un tipo específico.
    /// Útil para UI de vendors, crafting, etc.
    /// </summary>
    public List<ItemData> GetItemsPorTipo(ItemType tipo)
    {
        List<ItemData> resultado = new List<ItemData>();

        foreach (var kvp in itemDictionary)
        {
            if (kvp.Value.itemType == tipo)
            {
                resultado.Add(kvp.Value);
            }
        }

        return resultado;
    }

    /// <summary>
    /// Obtiene la cantidad total de items registrados.
    /// </summary>
    public int GetCantidadItems()
    {
        return itemDictionary.Count;
    }

    /// <summary>
    /// Obtiene todos los items (útil para tools/debugging).
    /// </summary>
    public List<ItemData> GetTodosLosItems()
    {
        return new List<ItemData>(itemDictionary.Values);
    }

    #endregion

    #region Debug y Validación

    /// <summary>
    /// Valida que todos los items tengan IDs únicos.
    /// Llama este método desde el Editor para verificar.
    /// </summary>
    [ContextMenu("Validar IDs de Items")]
    public void ValidarIDsUnicos()
    {
        HashSet<int> idsVistos = new HashSet<int>();
        bool hayDuplicados = false;

        foreach (ItemData item in todosLosItems)
        {
            if (item == null) continue;

            if (idsVistos.Contains(item.itemID))
            {
                Debug.LogError($"[ItemDatabase] ¡ID DUPLICADO! ID {item.itemID} usado por múltiples items.");
                hayDuplicados = true;
            }
            else
            {
                idsVistos.Add(item.itemID);
            }
        }

        if (!hayDuplicados)
        {
            Debug.Log("[ItemDatabase] ✓ Todos los IDs son únicos.");
        }
    }

    /// <summary>
    /// Muestra estadísticas de la base de datos en la consola.
    /// </summary>
    [ContextMenu("Mostrar Estadísticas")]
    public void MostrarEstadisticas()
    {
        Debug.Log("=== ESTADÍSTICAS DE ITEMDATABASE ===");
        Debug.Log($"Total de items: {itemDictionary.Count}");

        foreach (ItemType tipo in System.Enum.GetValues(typeof(ItemType)))
        {
            int cantidad = GetItemsPorTipo(tipo).Count;
            Debug.Log($"  {tipo}: {cantidad} items");
        }

        Debug.Log("====================================");
    }

    #endregion
}
