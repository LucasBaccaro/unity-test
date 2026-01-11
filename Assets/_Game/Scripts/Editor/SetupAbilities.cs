using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Herramienta de Editor para crear automáticamente las habilidades del MVP.
/// </summary>
public class SetupAbilities
{
    [MenuItem("Tools/Fix Ability Assets")]
    public static void CreateAbilities()
    {
        string path = "Assets/_Game/Data/Habilidades";
        string classesPath = "Assets/_Game/Data/Clases";

        // Crear carpeta si no existe
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // --- 1. Crear Habilidades ---

        // MAGO
        HabilidadBase bolaFuego = CrearHabilidad(path, "BolaDeFuego", "Bola de Fuego", 
            TipoHabilidad.Instant, TipoObjetivo.Enemy, 20, 15, 2.0f, 20, 0); // Hack: Instant para MVP, Projectile luego
        
        HabilidadBase escudoArcano = CrearHabilidad(path, "EscudoArcano", "Escudo Arcano", 
            TipoHabilidad.Instant, TipoObjetivo.Self, 0, 10, 8.0f, 0, 20); // Heal 20 como "escudo" simple

        // PALADIN
        HabilidadBase golpeSagrado = CrearHabilidad(path, "GolpeSagrado", "Golpe Sagrado", 
            TipoHabilidad.Instant, TipoObjetivo.Enemy, 15, 10, 3.0f, 3, 0); // Melee range 3
        
        HabilidadBase bendicion = CrearHabilidad(path, "Bendicion", "Bendición", 
            TipoHabilidad.Instant, TipoObjetivo.Self, 0, 15, 12.0f, 0, 30); // Heal 30

        // CLERIGO
        HabilidadBase curacion = CrearHabilidad(path, "Curacion", "Curación Mayor", 
            TipoHabilidad.Instant, TipoObjetivo.Ally, 0, 25, 5.0f, 15, 40); // Heal 40
        
        HabilidadBase destello = CrearHabilidad(path, "DestelloSagrado", "Destello Sagrado", 
            TipoHabilidad.Instant, TipoObjetivo.Enemy, 12, 12, 6.0f, 10, 0);

        // CAZADOR
        HabilidadBase disparo = CrearHabilidad(path, "DisparoRapido", "Disparo Rápido", 
            TipoHabilidad.Instant, TipoObjetivo.Enemy, 18, 10, 1.5f, 20, 0); // Cooldown bajo
        
        HabilidadBase trampa = CrearHabilidad(path, "Trampa", "Trampa Explosiva", 
            TipoHabilidad.Instant, TipoObjetivo.Enemy, 25, 20, 15.0f, 10, 0);

        AssetDatabase.SaveAssets();

        // --- 2. Asignar a Clases ---

        AsignarAClase(classesPath, "Mago", bolaFuego, escudoArcano);
        AsignarAClase(classesPath, "Paladin", golpeSagrado, bendicion);
        AsignarAClase(classesPath, "Clerigo", curacion, destello);
        AsignarAClase(classesPath, "Cazador", disparo, trampa);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ [SetupAbilities] ¡Habilidades creadas y asignadas exitosamente!");
    }

    static HabilidadBase CrearHabilidad(string path, string fileName, string nombre, 
        TipoHabilidad tipo, TipoObjetivo obj, int dmg, int cost, float cd, float range, int heal)
    {
        string fullPath = $"{path}/{fileName}.asset";
        HabilidadBase habilidad = AssetDatabase.LoadAssetAtPath<HabilidadBase>(fullPath);

        if (habilidad == null)
        {
            habilidad = ScriptableObject.CreateInstance<HabilidadBase>();
            AssetDatabase.CreateAsset(habilidad, fullPath);
        }

        habilidad.nombreHabilidad = nombre;
        habilidad.tipoHabilidad = tipo;
        habilidad.tipoObjetivo = obj;
        habilidad.descripcion = $"Habilidad de {nombre}.";
        
        habilidad.damage = dmg;
        habilidad.manaCost = cost;
        habilidad.cooldown = cd;
        habilidad.range = range;
        habilidad.healing = heal;
        
        // Valores default seguros
        habilidad.damageMultiplier = 1f;
        habilidad.aoeRadius = 0f;
        habilidad.usableEnMovimiento = true;

        EditorUtility.SetDirty(habilidad);
        return habilidad;
    }

    static void AsignarAClase(string path, string className, HabilidadBase h1, HabilidadBase h2)
    {
        string fullPath = $"{path}/{className}.asset";
        ClaseBase clase = AssetDatabase.LoadAssetAtPath<ClaseBase>(fullPath);

        if (clase != null)
        {
            clase.habilidades = new HabilidadBase[] { h1, h2 };
            EditorUtility.SetDirty(clase);
            Debug.Log($"Asignadas habilidades a {className}");
        }
        else
        {
            Debug.LogError($"No se encontró la clase {className} en {fullPath}");
        }
    }
}
