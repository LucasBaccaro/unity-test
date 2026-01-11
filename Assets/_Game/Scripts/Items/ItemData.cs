using UnityEngine;

/// <summary>
/// Tipos de items en el MMO.
/// </summary>
public enum ItemType
{
    Weapon,         // Arma (espada, báculo, arco)
    Armor,          // Armadura (casco, pechera, botas)
    Consumable,     // Consumible (poción, comida)
    QuestItem,      // Item de quest (no se puede vender/tirar)
    Material,       // Material para crafting
    Misc            // Misceláneo (otros)
}

/// <summary>
/// Rareza del item.
/// Afecta el color en la UI y potencialmente los stats.
/// </summary>
public enum ItemRarity
{
    Common,         // Común (gris/blanco)
    Uncommon,       // Poco común (verde)
    Rare,           // Raro (azul)
    Epic,           // Épico (púrpura)
    Legendary       // Legendario (naranja/dorado)
}

/// <summary>
/// ScriptableObject que define un item del juego.
///
/// SCRIPTABLEOBJECT: Asset de datos reutilizable para items.
/// Cada item (Espada de Hierro, Poción de HP, etc.) será un asset separado.
///
/// IMPORTANTE: Solo guardamos el ID del item en el inventario, no el ScriptableObject
/// completo. Esto reduce significativamente el uso de red.
///
/// Ejemplo: En lugar de sincronizar todo el objeto "Espada de Hierro" (muchos bytes),
/// solo sincronizamos su ID (4 bytes). El cliente busca el ID en su ItemDatabase local.
/// </summary>
[CreateAssetMenu(fileName = "Nuevo_Item", menuName = "MMO/Item", order = 3)]
public class ItemData : ScriptableObject
{
    #region Información Básica

    [Header("Información Básica")]
    [Tooltip("ID único del item (DEBE ser único en todo el juego)")]
    [Range(1, 9999)]
    public int itemID = 1;

    [Tooltip("Nombre del item")]
    public string itemName = "Item Sin Nombre";

    [Tooltip("Descripción del item")]
    [TextArea(3, 5)]
    public string description = "Un item misterioso...";

    [Tooltip("Icono del item (para UI)")]
    public Sprite icon;

    #endregion

    #region Clasificación

    [Header("Clasificación")]
    [Tooltip("Tipo de item")]
    public ItemType itemType = ItemType.Misc;

    [Tooltip("Rareza del item")]
    public ItemRarity rarity = ItemRarity.Common;

    #endregion

    #region Stacking

    [Header("Apilamiento (Stacking)")]
    [Tooltip("Máximo de items que se pueden apilar en un solo slot")]
    [Range(1, 999)]
    public int maxStackSize = 1;

    #endregion

    #region Valor Económico

    [Header("Economía")]
    [Tooltip("Precio de venta al vendor")]
    [Range(0, 10000)]
    public int sellPrice = 10;

    [Tooltip("Precio de compra del vendor (0 = no se puede comprar)")]
    [Range(0, 10000)]
    public int buyPrice = 0;

    #endregion

    #region Stats y Efectos

    [Header("Stats y Efectos")]
    [Tooltip("Bonus de HP que da este item (si es equipo)")]
    public int bonusHP = 0;

    [Tooltip("Bonus de Mana que da este item")]
    public int bonusMana = 0;

    [Tooltip("Bonus de Damage que da este item")]
    public int bonusDamage = 0;

    [Tooltip("Bonus de Defense que da este item")]
    public int bonusDefense = 0;

    [Tooltip("Bonus de Speed que da este item")]
    public float bonusSpeed = 0f;

    [Header("Efectos de Consumibles")]
    [Tooltip("Cantidad de HP que restaura (si es consumible)")]
    public int healAmount = 0;

    [Tooltip("Cantidad de Mana que restaura (si es consumible)")]
    public int manaRestoreAmount = 0;

    #endregion

    #region Flags y Configuración

    [Header("Configuración")]
    [Tooltip("Si el item se puede vender a vendors")]
    public bool sellable = true;

    [Tooltip("Si el item se puede dropear/tirar")]
    public bool droppable = true;

    [Tooltip("Si el item se puede tradear con otros jugadores")]
    public bool tradeable = true;

    [Tooltip("Si el item se destruye al usarlo (consumibles)")]
    public bool consumeOnUse = false;

    #endregion

    #region Prefab del Item

    [Header("Prefab (Para dropear en el mundo)")]
    [Tooltip("Prefab que se usa cuando el item se dropea en el suelo")]
    public GameObject dropPrefab;

    #endregion

    #region Métodos de Validación

    /// <summary>
    /// Valida que el item esté configurado correctamente.
    /// </summary>
    void OnValidate()
    {
        // Validar ID único
        if (itemID <= 0)
        {
            Debug.LogError($"[ItemData] '{name}' tiene itemID inválido: {itemID}. Debe ser > 0.");
        }

        // Validar nombre
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"[ItemData] '{name}' no tiene itemName configurado.");
        }

        // Validar max stack
        if (maxStackSize < 1)
        {
            Debug.LogWarning($"[ItemData] '{itemName}' tiene maxStackSize < 1. Se ajustará a 1.");
            maxStackSize = 1;
        }

        // Validar precios
        if (buyPrice > 0 && sellPrice > buyPrice)
        {
            Debug.LogWarning($"[ItemData] '{itemName}' tiene sellPrice ({sellPrice}) mayor que buyPrice ({buyPrice}). ¿Es intencional?");
        }

        // Validar consumibles
        if (itemType == ItemType.Consumable)
        {
            if (healAmount == 0 && manaRestoreAmount == 0)
            {
                Debug.LogWarning($"[ItemData] '{itemName}' es Consumible pero no hace nada (heal y mana restore son 0).");
            }

            if (!consumeOnUse)
            {
                Debug.LogWarning($"[ItemData] '{itemName}' es Consumible pero consumeOnUse = false. Usualmente los consumibles se consumen.");
            }
        }

        // Validar quest items
        if (itemType == ItemType.QuestItem)
        {
            if (droppable)
            {
                Debug.LogWarning($"[ItemData] '{itemName}' es QuestItem pero es droppable. Quest items usualmente NO se pueden dropear.");
            }

            if (tradeable)
            {
                Debug.LogWarning($"[ItemData] '{itemName}' es QuestItem pero es tradeable. Quest items usualmente NO se pueden tradear.");
            }
        }
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Obtiene el color según la rareza del item.
    /// Útil para colorear el nombre/icono en la UI.
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.6f, 0.6f, 0.6f); // Gris
            case ItemRarity.Uncommon:
                return new Color(0f, 1f, 0f); // Verde
            case ItemRarity.Rare:
                return new Color(0f, 0.5f, 1f); // Azul
            case ItemRarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f); // Púrpura
            case ItemRarity.Legendary:
                return new Color(1f, 0.65f, 0f); // Naranja/Dorado
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Obtiene tooltip del item para mostrar en UI.
    /// </summary>
    public string GetTooltip()
    {
        string tooltip = $"<b><color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{itemName}</color></b>\n";
        tooltip += $"<i>{rarity} {itemType}</i>\n\n";
        tooltip += $"{description}\n\n";

        // Stats
        if (bonusHP > 0) tooltip += $"<color=green>+{bonusHP} HP</color>\n";
        if (bonusMana > 0) tooltip += $"<color=blue>+{bonusMana} Mana</color>\n";
        if (bonusDamage > 0) tooltip += $"<color=red>+{bonusDamage} Damage</color>\n";
        if (bonusDefense > 0) tooltip += $"<color=yellow>+{bonusDefense} Defense</color>\n";
        if (bonusSpeed > 0) tooltip += $"<color=cyan>+{bonusSpeed} Speed</color>\n";

        // Efectos de consumibles
        if (healAmount > 0) tooltip += $"<color=green>Restaura {healAmount} HP</color>\n";
        if (manaRestoreAmount > 0) tooltip += $"<color=blue>Restaura {manaRestoreAmount} Mana</color>\n";

        // Precio
        if (sellPrice > 0) tooltip += $"\n<color=yellow>Venta: {sellPrice} oro</color>";
        if (buyPrice > 0) tooltip += $"\n<color=yellow>Compra: {buyPrice} oro</color>";

        return tooltip;
    }

    /// <summary>
    /// Verifica si el item es apilable (stack size > 1).
    /// </summary>
    public bool IsStackable()
    {
        return maxStackSize > 1;
    }

    #endregion
}
