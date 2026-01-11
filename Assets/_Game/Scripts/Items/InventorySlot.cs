using System;

/// <summary>
/// Representa un slot individual del inventario.
///
/// NETWORKING: Esta estructura se sincroniza por la red usando Mirror's SyncList.
/// Por eso es muy importante que sea ligera (pocos bytes).
///
/// Solo guardamos el ID del item y la cantidad, NO el ScriptableObject completo.
/// Esto reduce drásticamente el uso de ancho de banda.
///
/// Ejemplo de ahorro:
/// - Guardar ItemData completo: ~500+ bytes
/// - Guardar solo ID + cantidad: 8 bytes (int + int)
/// - Para un inventario de 20 slots: ~10KB vs ~160 bytes
/// </summary>
[Serializable]
public struct InventorySlot
{
    /// <summary>
    /// ID del item en este slot.
    /// 0 = slot vacío
    /// > 0 = ID del item (referencia a ItemDatabase)
    /// </summary>
    public int itemID;

    /// <summary>
    /// Cantidad de items en este slot.
    /// Para items no apilables, siempre será 1.
    /// Para items apilables, puede ser de 1 hasta maxStackSize.
    /// </summary>
    public int cantidad;

    /// <summary>
    /// Constructor para crear un slot con un item.
    /// </summary>
    /// <param name="id">ID del item</param>
    /// <param name="cant">Cantidad del item</param>
    public InventorySlot(int id, int cant)
    {
        itemID = id;
        cantidad = cant;
    }

    /// <summary>
    /// Verifica si el slot está vacío.
    /// </summary>
    public bool EstaVacio()
    {
        return itemID == 0 || cantidad <= 0;
    }

    /// <summary>
    /// Limpia el slot (lo deja vacío).
    /// </summary>
    public void Limpiar()
    {
        itemID = 0;
        cantidad = 0;
    }

    /// <summary>
    /// Verifica si dos slots contienen el mismo item.
    /// Útil para stacking (apilar items).
    /// </summary>
    public bool MismoItem(InventorySlot otro)
    {
        return itemID == otro.itemID && itemID != 0;
    }

    /// <summary>
    /// Crea un slot vacío.
    /// </summary>
    public static InventorySlot SlotVacio()
    {
        return new InventorySlot(0, 0);
    }

    /// <summary>
    /// Representación en string para debugging.
    /// </summary>
    public override string ToString()
    {
        if (EstaVacio())
        {
            return "[Slot Vacío]";
        }

        return $"[ItemID: {itemID}, Cantidad: {cantidad}]";
    }
}
