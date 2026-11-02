using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RepairTomatoSoupCounter
{
    private const string PrefabPath = "Assets/Prefab/Counters/TomatoSoupCounter.prefab";
    private const int CountersLayer = 6;

    [MenuItem("Tools/HuNao/Repair Tomato Soup Counter Components")]
    public static void Repair()
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Configure(prefabRoot);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefabRoot); }

        var scene = EditorSceneManager.GetActiveScene();
        foreach (var counter in Object.FindObjectsOfType<TomatoSoupCounter>(true).Where(c => c.gameObject.scene == scene))
        {
            Configure(counter.gameObject);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("TomatoSoupCounter repaired without changing visual transforms, meshes or materials.");
    }

    private static void Configure(GameObject root)
    {
        root.layer = CountersLayer;
        var stove = root.GetComponent<StoveCounter>();
        if (stove != null) Object.DestroyImmediate(stove, true);

        var counter = root.GetComponent<TomatoSoupCounter>();
        if (counter == null) counter = root.AddComponent<TomatoSoupCounter>();
        var collider = root.GetComponent<BoxCollider>() ?? root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, .7f, 0f);
        collider.size = new Vector3(1.5f, 1.4f, 1.5f);
        collider.isTrigger = false;

        var select = root.GetComponent<SelectCounterVisual>() ?? root.AddComponent<SelectCounterVisual>();
        var target = root.transform.Find("SoupPot") ?? root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.GetComponent<Renderer>() != null && t != root.transform);
        if (target == null) { Debug.LogWarning("TomatoSoupCounter has no renderer target for selection highlight."); return; }
        var serialized = new SerializedObject(select);
        serialized.FindProperty("baseCounter").objectReferenceValue = counter;
        var array = serialized.FindProperty("visualGameObjectArray");
        array.arraySize = 1;
        array.GetArrayElementAtIndex(0).objectReferenceValue = target.gameObject;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
