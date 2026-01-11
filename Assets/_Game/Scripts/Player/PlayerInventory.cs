using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el inventario del jugador.
///
/// NETWORKING: Usa SyncList para sincronizar el inventario del servidor a los clientes.
/// CRÍTICO: Solo el servidor puede modificar el inventario (Server Authority).
///
/// SYNCLIST:
/// - Mirror sincroniza automáticamente la lista cuando cambia
/// - Los clientes reciben actualizaciones solo cuando hay cambios
/// - Mucho más eficiente que sincronizar todo cada frame
///
/// FLUJO DE AÑADIR ITEM:
/// 1. Cliente: Jugador recoge item → CmdPickupItem(itemID)
/// 2. Servidor: Valida que el item existe y hay espacio
/// 3. Servidor: Añade item a inventorySlots (SyncList)
/// 4. Mirror: Sincroniza automáticamente a todos los clientes
/// 5. Clientes: Callback se ejecuta, UI se actualiza
/// </summary>
public class PlayerInventory : NetworkBehaviour
{
    #region Variables de Red (Sincronizadas)

    /// <summary>
    /// Lista de slots del inventario.
    ///
    /// NETWORKING: SyncList sincroniza automáticamente del servidor a clientes.
    /// Cuando el servidor modifica esta lista, Mirror envía los cambios a todos
    /// los clientes que pueden ver a este jugador.
    ///
    /// IMPORTANTE: Solo el servidor puede modificar esta lista.
    /// Los clientes solo pueden leerla.
    /// </summary>
    public readonly SyncList<InventorySlot> inventorySlots = new SyncList<InventorySlot>();

    #endregion

    #region Variables de Configuración

    [Header("Configuración del Inventario")]
    [Tooltip("Número de slots en el inventario")]
    [Range(10, 100)]
    public int numeroSlots = 20;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        // Registrar callback para cuando el inventario cambie
        inventorySlots.Callback += OnInventoryChanged;
    }

    /// <summary>
    /// Se llama cuando el objeto se spawnea en el servidor.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Inicializar slots vacíos
        InicializarInventario();

        Debug.Log($"[PlayerInventory] Inventario inicializado con {numeroSlots} slots.");
    }

    /// <summary>
    /// Se llama cuando el cliente se inicia.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Actualizar UI con el inventario actual
        if (isLocalPlayer)
        {
            ActualizarUIInventario();
        }
    }

    #endregion

    #region Inicialización

    /// <summary>
    /// Inicializa el inventario con slots vacíos.
    /// Solo se ejecuta en el servidor.
    /// </summary>
    [Server]
    void InicializarInventario()
    {
        // Limpiar lista
        inventorySlots.Clear();

        // Añadir slots vacíos
        for (int i = 0; i < numeroSlots; i++)
        {
            inventorySlots.Add(InventorySlot.SlotVacio());
        }

        Debug.Log($"[SERVER] Inventario inicializado con {numeroSlots} slots vacíos.");
    }

    #endregion

    #region Añadir Items

    /// <summary>
    /// Añade un item al inventario.
    /// Solo el servidor puede llamar este método.
    /// </summary>
    /// <param name="itemID">ID del item a añadir</param>
    /// <param name="cantidad">Cantidad a añadir</param>
    /// <returns>True si se añadió exitosamente, False si no hay espacio</returns>
    [Server]
    public bool AgregarItem(int itemID, int cantidad = 1)
    {
        if (itemID <= 0 || cantidad <= 0)
        {
            Debug.LogWarning($"[SERVER] Intentando añadir item inválido. ID: {itemID}, Cantidad: {cantidad}");
            return false;
        }

        // Obtener datos del item
        ItemData itemData = ItemDatabase.Instance.GetItem(itemID);

        if (itemData == null)
        {
            Debug.LogError($"[SERVER] Item con ID {itemID} no existe en ItemDatabase!");
            return false;
        }

        Debug.Log($"[SERVER] Añadiendo {cantidad}x {itemData.itemName} (ID: {itemID}) al inventario...");

        int cantidadRestante = cantidad;

        // Si el item es apilable, intentar añadir a stacks existentes primero
        if (itemData.IsStackable())
        {
            cantidadRestante = AñadirAStacksExistentes(itemID, cantidadRestante, itemData.maxStackSize);
        }

        // Si aún queda cantidad por añadir, buscar slots vacíos
        if (cantidadRestante > 0)
        {
            cantidadRestante = AñadirANuevosSlots(itemID, cantidadRestante, itemData.maxStackSize);
        }

        // Verificar si se añadió todo
        if (cantidadRestante > 0)
        {
            Debug.LogWarning($"[SERVER] No hay espacio suficiente en el inventario. {cantidadRestante} items no añadidos.");
            return false;
        }

        Debug.Log($"[SERVER] {cantidad}x {itemData.itemName} añadido exitosamente.");
        return true;
    }

    /// <summary>
    /// Intenta añadir items a stacks existentes del mismo tipo.
    /// </summary>
    /// <returns>Cantidad restante que no se pudo añadir</returns>
    [Server]
    int AñadirAStacksExistentes(int itemID, int cantidad, int maxStackSize)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];

            // Si el slot tiene el mismo item y no está lleno
            if (slot.itemID == itemID && slot.cantidad < maxStackSize)
            {
                int espacio = maxStackSize - slot.cantidad;
                int cantidadAAñadir = Mathf.Min(espacio, cantidad);

                // Modificar slot
                slot.cantidad += cantidadAAñadir;
                inventorySlots[i] = slot; // IMPORTANTE: Actualizar la SyncList

                cantidad -= cantidadAAñadir;

                if (cantidad <= 0)
                {
                    return 0; // Todo añadido
                }
            }
        }

        return cantidad; // Retornar lo que quedó sin añadir
    }

    /// <summary>
    /// Añade items a slots vacíos.
    /// </summary>
    /// <returns>Cantidad restante que no se pudo añadir</returns>
    [Server]
    int AñadirANuevosSlots(int itemID, int cantidad, int maxStackSize)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];

            // Si el slot está vacío
            if (slot.EstaVacio())
            {
                int cantidadAAñadir = Mathf.Min(maxStackSize, cantidad);

                // Crear nuevo slot con el item
                inventorySlots[i] = new InventorySlot(itemID, cantidadAAñadir);

                cantidad -= cantidadAAñadir;

                if (cantidad <= 0)
                {
                    return 0; // Todo añadido
                }
            }
        }

        return cantidad; // Retornar lo que quedó sin añadir
    }

    #endregion

    #region Remover Items

    /// <summary>
    /// Remueve un item del inventario.
    /// </summary>
    /// <param name="slotIndex">Índice del slot</param>
    /// <param name="cantidad">Cantidad a remover (0 = remover todo el stack)</param>
    /// <returns>True si se removió exitosamente</returns>
    [Server]
    public bool RemoverItem(int slotIndex, int cantidad = 0)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
        {
            Debug.LogError($"[SERVER] Índice de slot inválido: {slotIndex}");
            return false;
        }

        InventorySlot slot = inventorySlots[slotIndex];

        if (slot.EstaVacio())
        {
            Debug.LogWarning($"[SERVER] Intentando remover de slot vacío: {slotIndex}");
            return false;
        }

        // Si cantidad es 0, remover todo el stack
        if (cantidad <= 0)
        {
            cantidad = slot.cantidad;
        }

        // Verificar que hay suficiente cantidad
        if (slot.cantidad < cantidad)
        {
            Debug.LogWarning($"[SERVER] No hay suficiente cantidad en slot {slotIndex}. Tiene: {slot.cantidad}, Intenta remover: {cantidad}");
            return false;
        }

        // Remover cantidad
        slot.cantidad -= cantidad;

        // Si el slot quedó vacío, limpiarlo
        if (slot.cantidad <= 0)
        {
            slot.Limpiar();
        }

        // Actualizar SyncList
        inventorySlots[slotIndex] = slot;

        ItemData itemData = ItemDatabase.Instance.GetItem(slot.itemID);
        string itemName = itemData != null ? itemData.itemName : $"Item #{slot.itemID}";

        Debug.Log($"[SERVER] Removido {cantidad}x {itemName} del slot {slotIndex}.");
        return true;
    }

    /// <summary>
    /// Remueve items por ID (busca en todo el inventario).
    /// </summary>
    /// <param name="itemID">ID del item a remover</param>
    /// <param name="cantidad">Cantidad total a remover</param>
    /// <returns>True si se removió la cantidad solicitada</returns>
    [Server]
    public bool RemoverItemPorID(int itemID, int cantidad)
    {
        // Primero verificar si hay suficiente cantidad total
        int cantidadTotal = ContarItem(itemID);

        if (cantidadTotal < cantidad)
        {
            Debug.LogWarning($"[SERVER] No hay suficiente del item {itemID}. Tiene: {cantidadTotal}, Necesita: {cantidad}");
            return false;
        }

        // Remover de los slots
        int cantidadARemover = cantidad;

        for (int i = 0; i < inventorySlots.Count && cantidadARemover > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];

            if (slot.itemID == itemID)
            {
                int remover = Mathf.Min(slot.cantidad, cantidadARemover);
                RemoverItem(i, remover);
                cantidadARemover -= remover;
            }
        }

        return true;
    }

    #endregion

    #region Mover Items

    /// <summary>
    /// Mueve un item de un slot a otro.
    /// </summary>
    /// <param name="fromSlot">Slot origen</param>
    /// <param name="toSlot">Slot destino</param>
    /// <returns>True si se movió exitosamente</returns>
    [Server]
    public bool MoverItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= inventorySlots.Count ||
            toSlot < 0 || toSlot >= inventorySlots.Count)
        {
            Debug.LogError($"[SERVER] Índices de slot inválidos. From: {fromSlot}, To: {toSlot}");
            return false;
        }

        if (fromSlot == toSlot)
        {
            Debug.LogWarning("[SERVER] Intentando mover item al mismo slot.");
            return false;
        }

        InventorySlot slotOrigen = inventorySlots[fromSlot];
        InventorySlot slotDestino = inventorySlots[toSlot];

        // Si slot origen está vacío, no hacer nada
        if (slotOrigen.EstaVacio())
        {
            return false;
        }

        // CASO 1: Slot destino está vacío → mover todo
        if (slotDestino.EstaVacio())
        {
            inventorySlots[toSlot] = slotOrigen;
            inventorySlots[fromSlot] = InventorySlot.SlotVacio();
            Debug.Log($"[SERVER] Item movido de slot {fromSlot} a slot {toSlot}.");
            return true;
        }

        // CASO 2: Ambos slots tienen el mismo item → intentar stack
        if (slotOrigen.MismoItem(slotDestino))
        {
            ItemData itemData = ItemDatabase.Instance.GetItem(slotOrigen.itemID);

            if (itemData != null && itemData.IsStackable())
            {
                int espacio = itemData.maxStackSize - slotDestino.cantidad;

                if (espacio > 0)
                {
                    int cantidadAMover = Mathf.Min(espacio, slotOrigen.cantidad);

                    slotDestino.cantidad += cantidadAMover;
                    slotOrigen.cantidad -= cantidadAMover;

                    if (slotOrigen.cantidad <= 0)
                    {
                        slotOrigen.Limpiar();
                    }

                    inventorySlots[fromSlot] = slotOrigen;
                    inventorySlots[toSlot] = slotDestino;

                    Debug.Log($"[SERVER] {cantidadAMover} items apilados en slot {toSlot}.");
                    return true;
                }
            }
        }

        // CASO 3: Diferentes items → intercambiar
        inventorySlots[fromSlot] = slotDestino;
        inventorySlots[toSlot] = slotOrigen;
        Debug.Log($"[SERVER] Items intercambiados entre slots {fromSlot} y {toSlot}.");
        return true;
    }

    #endregion

    #region Commands (Cliente → Servidor)

    /// <summary>
    /// Command para que el cliente solicite mover un item.
    /// </summary>
    [Command]
    public void CmdMoverItem(int fromSlot, int toSlot)
    {
        MoverItem(fromSlot, toSlot);
    }

    /// <summary>
    /// Command para usar un item consumible.
    /// </summary>
    [Command]
    public void CmdUsarItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
        {
            return;
        }

        InventorySlot slot = inventorySlots[slotIndex];

        if (slot.EstaVacio())
        {
            return;
        }

        ItemData itemData = ItemDatabase.Instance.GetItem(slot.itemID);

        if (itemData == null || itemData.itemType != ItemType.Consumable)
        {
            Debug.LogWarning($"[SERVER] Item {slot.itemID} no es consumible.");
            return;
        }

        // Aplicar efectos del consumible
        PlayerStats stats = GetComponent<PlayerStats>();

        if (stats != null)
        {
            if (itemData.healAmount > 0)
            {
                stats.Curar(itemData.healAmount);
            }

            if (itemData.manaRestoreAmount > 0)
            {
                stats.RestaurarMana(itemData.manaRestoreAmount);
            }
        }

        // Consumir item si está configurado
        if (itemData.consumeOnUse)
        {
            RemoverItem(slotIndex, 1);
        }

        Debug.Log($"[SERVER] {GetComponent<PlayerController>().nombreJugador} usó {itemData.itemName}.");
    }

    /// <summary>
    /// Command para dropear un item al suelo.
    /// </summary>
    [Command]
    public void CmdDropearItem(int slotIndex, int cantidad)
    {
        // TODO (FASE 6): Implementar lógica de dropear items
        // Crear ItemDrop prefab en el mundo
        Debug.Log($"[SERVER] Dropeando {cantidad} items del slot {slotIndex}...");
    }

    #endregion

    #region Consultas

    /// <summary>
    /// Cuenta cuántos items de un ID específico hay en el inventario.
    /// </summary>
    public int ContarItem(int itemID)
    {
        int total = 0;

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemID == itemID)
            {
                total += slot.cantidad;
            }
        }

        return total;
    }

    /// <summary>
    /// Verifica si el inventario tiene espacio para añadir items.
    /// </summary>
    public bool TieneEspacio(int itemID, int cantidad)
    {
        ItemData itemData = ItemDatabase.Instance.GetItem(itemID);

        if (itemData == null)
        {
            return false;
        }

        // Contar espacio disponible
        int espacioDisponible = 0;

        // Espacio en stacks existentes
        if (itemData.IsStackable())
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.itemID == itemID)
                {
                    espacioDisponible += itemData.maxStackSize - slot.cantidad;
                }
            }
        }

        // Espacio en slots vacíos
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.EstaVacio())
            {
                espacioDisponible += itemData.maxStackSize;
            }
        }

        return espacioDisponible >= cantidad;
    }

    /// <summary>
    /// Cuenta cuántos slots vacíos hay.
    /// </summary>
    public int ContarSlotsVacios()
    {
        int count = 0;

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.EstaVacio())
            {
                count++;
            }
        }

        return count;
    }

    #endregion

    #region Callbacks de SyncList

    /// <summary>
    /// Se llama cuando inventorySlots cambia.
    ///
    /// NETWORKING: Este callback se ejecuta en TODOS los clientes
    /// cuando el servidor modifica la SyncList.
    /// </summary>
    void OnInventoryChanged(SyncList<InventorySlot>.Operation op, int index, InventorySlot oldSlot, InventorySlot newSlot)
    {
        // Solo actualizar UI para el jugador local
        if (!isLocalPlayer)
        {
            return;
        }

        Debug.Log($"[CLIENT] Inventario cambió. Op: {op}, Index: {index}");

        // Actualizar UI
        ActualizarUIInventario();
    }

    #endregion

    #region UI

    /// <summary>
    /// Actualiza la UI del inventario.
    /// </summary>
    void ActualizarUIInventario()
    {
        // TODO (FASE 12): Actualizar UI real
        // InventoryUI.Instance.ActualizarInventario(inventorySlots);

        // Por ahora, solo log
        Debug.Log($"[UI] Inventario actualizado. Slots usados: {numeroSlots - ContarSlotsVacios()}/{numeroSlots}");
    }

    #endregion
}
