using UnityEngine;
using UnityEditor;
using System.IO;

public class SetupClasses
{
    [MenuItem("Tools/Fix Class Assets")]
    public static void CreateAndAssignClasses()
    {
        // 1. Ensure directory exists
        string path = "Assets/_Game/Data/Clases";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 2. Create Instances
        ClaseBase mago = CreateClassAsset(path, "Mago", 80, 100, 20, 5f, Color.magenta);
        ClaseBase paladin = CreateClassAsset(path, "Paladin", 150, 50, 15, 4.5f, Color.yellow);
        ClaseBase clerigo = CreateClassAsset(path, "Clerigo", 100, 120, 12, 5f, Color.white);
        ClaseBase cazador = CreateClassAsset(path, "Cazador", 90, 60, 25, 6f, Color.green);

        // 3. Load Player Prefab
        string prefabPath = "Assets/_Game/Prefabs/Player.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (playerPrefab == null)
        {
            // Try searching if exact path failed
            string[] guids = AssetDatabase.FindAssets("Player t:Prefab");
            if (guids.Length > 0)
            {
                prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
        }

        if (playerPrefab != null)
        {
            PlayerClassSelector selector = playerPrefab.GetComponent<PlayerClassSelector>();
            if (selector != null)
            {
                // Assign classes
                selector.claseMago = mago;
                selector.clasePaladin = paladin;
                selector.claseClerigo = clerigo;
                selector.claseCazador = cazador;

                // Save Prefab
                EditorUtility.SetDirty(playerPrefab);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SetupClasses] Success! Classes assigned to {prefabPath}");
            }
            else
            {
                Debug.LogError($"[SetupClasses] PlayerClassSelector component not found on {prefabPath}");
            }
        }
        else
        {
            Debug.LogError("[SetupClasses] Player.prefab not found!");
        }
    }

    private static ClaseBase CreateClassAsset(string folder, string className, int hp, int mana, int dmg, float speed, Color color)
    {
        string fullPath = $"{folder}/{className}.asset";
        ClaseBase asset = AssetDatabase.LoadAssetAtPath<ClaseBase>(fullPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ClaseBase>();
            AssetDatabase.CreateAsset(asset, fullPath);
        }

        asset.nombreClase = className;
        asset.hpBase = hp;
        asset.manaBase = mana;
        asset.damageBase = dmg;
        asset.speedBase = speed;
        asset.colorClase = color;
        // Defaults
        asset.descripcion = $"Clase {className} generada automáticamente.";
        
        EditorUtility.SetDirty(asset);
        return asset;
    }
}
