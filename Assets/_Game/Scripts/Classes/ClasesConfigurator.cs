#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Script helper para crear automáticamente las 4 clases del MMO.
///
/// USO:
/// 1. En Unity Editor, ve a: MMO > Crear Todas Las Clases
/// 2. Esto creará los 4 ScriptableObjects de clases en Assets/_Game/Data/Classes/
/// 3. Las clases se crearán con stats balanceados según el plan
///
/// IMPORTANTE: Solo funciona en el Editor (por eso tiene #if UNITY_EDITOR).
/// No se incluye en el build final del juego.
/// </summary>
public class ClasesConfigurator
{
    /// <summary>
    /// Menú que aparece en Unity Editor para crear las clases.
    /// </summary>
    [MenuItem("MMO/Crear Todas Las Clases")]
    static void CrearTodasLasClases()
    {
        Debug.Log("[ClasesConfigurator] Creando las 4 clases del MMO...");

        // Verificar que la carpeta existe
        string rutaClases = "Assets/_Game/Data/Classes";
        if (!AssetDatabase.IsValidFolder(rutaClases))
        {
            // Crear la carpeta si no existe
            AssetDatabase.CreateFolder("Assets/_Game/Data", "Classes");
            Debug.Log($"[ClasesConfigurator] Carpeta creada: {rutaClases}");
        }

        // Crear cada clase
        CrearMago();
        CrearPaladin();
        CrearClerigo();
        CrearCazador();

        // Guardar cambios
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ClasesConfigurator] ¡Todas las clases creadas exitosamente!");
        Debug.Log($"[ClasesConfigurator] Revisa la carpeta: {rutaClases}");
    }

    #region Crear Clases Individuales

    /// <summary>
    /// Crea la clase MAGO.
    /// Stats: HP bajo, Mana alto, Damage mágico alto, Defense baja, Speed medio
    /// </summary>
    static void CrearMago()
    {
        ClaseBase mago = ScriptableObject.CreateInstance<ClaseBase>();

        // Información básica
        mago.nombreClase = "Mago";
        mago.descripcion = "Maestro de las artes arcanas. Controla el poder del fuego y las energías mágicas.\n\n" +
                          "Fortalezas:\n" +
                          "• Alto daño mágico a distancia\n" +
                          "• Gran reserva de mana\n" +
                          "• Escudo temporal para defensa\n\n" +
                          "Debilidades:\n" +
                          "• Vida baja, vulnerable en combate cuerpo a cuerpo\n" +
                          "• Depende del mana para todo";

        // Stats base (nivel 1)
        mago.hpBase = 80;           // HP bajo
        mago.manaBase = 150;        // Mana ALTO
        mago.damageBase = 25;       // Damage alto
        mago.defenseBase = 5;       // Defense baja
        mago.speedBase = 4f;        // Speed medio

        // Color de clase
        mago.colorClase = new Color(0.5f, 0f, 1f); // Púrpura/Morado

        // Guardar asset
        string ruta = "Assets/_Game/Data/Classes/Mago.asset";
        AssetDatabase.CreateAsset(mago, ruta);
        Debug.Log($"[ClasesConfigurator] Mago creado en: {ruta}");
    }

    /// <summary>
    /// Crea la clase PALADIN.
    /// Stats: HP alto, Mana bajo, Damage medio, Defense alta, Speed bajo
    /// </summary>
    static void CrearPaladin()
    {
        ClaseBase paladin = ScriptableObject.CreateInstance<ClaseBase>();

        // Información básica
        paladin.nombreClase = "Paladin";
        paladin.descripcion = "Guerrero sagrado que combina fuerza física con magia divina.\n\n" +
                             "Fortalezas:\n" +
                             "• Alta vida y defensa\n" +
                             "• Fuerte en combate cuerpo a cuerpo\n" +
                             "• Puede curarse a sí mismo\n\n" +
                             "Debilidades:\n" +
                             "• Lento\n" +
                             "• Poco mana, no puede usar muchas habilidades seguidas";

        // Stats base (nivel 1)
        paladin.hpBase = 150;       // HP ALTO
        paladin.manaBase = 50;      // Mana bajo
        paladin.damageBase = 20;    // Damage medio
        paladin.defenseBase = 15;   // Defense ALTA
        paladin.speedBase = 3f;     // Speed bajo

        // Color de clase
        paladin.colorClase = new Color(1f, 0.84f, 0f); // Dorado

        // Guardar asset
        string ruta = "Assets/_Game/Data/Classes/Paladin.asset";
        AssetDatabase.CreateAsset(paladin, ruta);
        Debug.Log($"[ClasesConfigurator] Paladin creado en: {ruta}");
    }

    /// <summary>
    /// Crea la clase CLERIGO.
    /// Stats: HP medio, Mana alto, Damage bajo, Defense media, Speed medio
    /// </summary>
    static void CrearClerigo()
    {
        ClaseBase clerigo = ScriptableObject.CreateInstance<ClaseBase>();

        // Información básica
        clerigo.nombreClase = "Clerigo";
        clerigo.descripcion = "Sacerdote que canaliza el poder divino para curar y proteger.\n\n" +
                             "Fortalezas:\n" +
                             "• Poderosas habilidades de curación\n" +
                             "• Buena reserva de mana\n" +
                             "• Útil en grupo (soporte)\n\n" +
                             "Debilidades:\n" +
                             "• Bajo daño individual\n" +
                             "• Requiere aliados para ser efectivo";

        // Stats base (nivel 1)
        clerigo.hpBase = 100;       // HP medio
        clerigo.manaBase = 120;     // Mana alto
        clerigo.damageBase = 12;    // Damage BAJO
        clerigo.defenseBase = 8;    // Defense media
        clerigo.speedBase = 4f;     // Speed medio

        // Color de clase
        clerigo.colorClase = new Color(1f, 1f, 1f); // Blanco

        // Guardar asset
        string ruta = "Assets/_Game/Data/Classes/Clerigo.asset";
        AssetDatabase.CreateAsset(clerigo, ruta);
        Debug.Log($"[ClasesConfigurator] Clerigo creado en: {ruta}");
    }

    /// <summary>
    /// Crea la clase CAZADOR.
    /// Stats: HP medio, Mana medio-bajo, Damage medio-alto, Defense baja, Speed ALTO
    /// </summary>
    static void CrearCazador()
    {
        ClaseBase cazador = ScriptableObject.CreateInstance<ClaseBase>();

        // Información básica
        cazador.nombreClase = "Cazador";
        cazador.descripcion = "Experto en combate a distancia y trampas.\n\n" +
                             "Fortalezas:\n" +
                             "• Muy rápido, excelente movilidad\n" +
                             "• Buen daño físico a distancia\n" +
                             "• Trampas para control de área\n\n" +
                             "Debilidades:\n" +
                             "• Defensa baja\n" +
                             "• Vulnerable si el enemigo se acerca";

        // Stats base (nivel 1)
        cazador.hpBase = 110;       // HP medio
        cazador.manaBase = 70;      // Mana medio-bajo
        cazador.damageBase = 22;    // Damage medio-alto
        cazador.defenseBase = 6;    // Defense baja
        cazador.speedBase = 6f;     // Speed ALTO

        // Color de clase
        cazador.colorClase = new Color(0f, 0.8f, 0f); // Verde

        // Guardar asset
        string ruta = "Assets/_Game/Data/Classes/Cazador.asset";
        AssetDatabase.CreateAsset(cazador, ruta);
        Debug.Log($"[ClasesConfigurator] Cazador creado en: {ruta}");
    }

    #endregion

    #region Botones de Creación Individual

    [MenuItem("MMO/Crear Clases/Mago")]
    static void SoloMago() { CrearMago(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    [MenuItem("MMO/Crear Clases/Paladin")]
    static void SoloPaladin() { CrearPaladin(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    [MenuItem("MMO/Crear Clases/Clerigo")]
    static void SoloClerigo() { CrearClerigo(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    [MenuItem("MMO/Crear Clases/Cazador")]
    static void SoloCazador() { CrearCazador(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

    #endregion
}
#endif
