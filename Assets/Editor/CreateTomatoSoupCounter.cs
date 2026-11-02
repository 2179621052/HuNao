using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateTomatoSoupCounter
{
    private const string StoveCounterPath = "Assets/Prefab/Counters/StoveCounter.prefab";
    private const string VisualPath = "Assets/_Assets/PrefabsVisuals/CountersVisuals/TomatoSoupCounter_Visual.prefab";
    private const string CounterPath = "Assets/Prefab/Counters/TomatoSoupCounter.prefab";
    private const string MaterialFolder = "Assets/_Assets/Materials/TomatoSoup";

    [MenuItem("Tools/HuNao/Create Tomato Soup Counter")]
    public static void Create()
    {
        if (!File.Exists(StoveCounterPath)) { Debug.LogError("TomatoSoupCounter aborted: StoveCounter.prefab was not found."); return; }
        if (!File.Exists("Assets/Scripts/StoveCounter.cs")) { Debug.LogError("TomatoSoupCounter aborted: StoveCounter.cs was not found."); return; }
        if (File.Exists(VisualPath) || File.Exists(CounterPath)) { Debug.LogError("TomatoSoupCounter aborted: target prefab already exists; refusing to overwrite."); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/GameScene.unity") { Debug.LogError("TomatoSoupCounter aborted: open Assets/Scenes/GameScene.unity first."); return; }

        var stove = AssetDatabase.LoadAssetAtPath<GameObject>(StoveCounterPath);
        if (stove == null || stove.GetComponentInChildren<StoveCounter>() == null) { Debug.LogError("TomatoSoupCounter aborted: StoveCounter prefab has no StoveCounter component."); return; }
        var stoveInScene = Object.FindObjectsOfType<StoveCounter>().FirstOrDefault();
        if (stoveInScene == null) { Debug.LogError("TomatoSoupCounter aborted: no StoveCounter instance exists in GameScene."); return; }

        Directory.CreateDirectory(Path.Combine(Application.dataPath, "_Assets/Materials/TomatoSoup"));
        AssetDatabase.Refresh();
        var potVisual = BuildVisual();
        var counter = BuildCounter(stove, potVisual);
        var targetPosition = stoveInScene.transform.position + stoveInScene.transform.right * 1.5f;
        if (Physics.OverlapBox(targetPosition + Vector3.up * .45f, new Vector3(.45f, .45f, .45f), stoveInScene.transform.rotation).Any(c => c.GetComponentInParent<BaseCounter>() != null))
        {
            Object.DestroyImmediate(counter);
            Debug.LogError("TomatoSoupCounter aborted: the proposed position overlaps an existing counter.");
            return;
        }
        counter.transform.SetPositionAndRotation(targetPosition, stoveInScene.transform.rotation);
        PrefabUtility.SaveAsPrefabAsset(counter, CounterPath);
        Object.DestroyImmediate(counter);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CounterPath), EditorSceneManager.GetActiveScene());
        instance.transform.SetPositionAndRotation(targetPosition, stoveInScene.transform.rotation);
        instance.name = "TomatoSoupCounter";
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("TomatoSoupCounter created. Original StoveCounter assets were not modified.");
    }

    private static GameObject BuildVisual()
    {
        var root = new GameObject("TomatoSoupCounter_Visual");
        var body = Primitive(PrimitiveType.Cylinder, "SoupPot", root.transform, new Vector3(0, .18f, 0), new Vector3(.72f, .28f, .72f), new Color(.12f, .12f, .14f));
        var rim = Primitive(PrimitiveType.Cylinder, "PotRim", root.transform, new Vector3(0, .47f, 0), new Vector3(.8f, .06f, .8f), new Color(.2f, .2f, .22f));
        Primitive(PrimitiveType.Cylinder, "TomatoSoupSurface", root.transform, new Vector3(0, .51f, 0), new Vector3(.68f, .025f, .68f), new Color(.9f, .12f, .035f));
        Primitive(PrimitiveType.Cube, "HandleLeft", root.transform, new Vector3(-.52f, .32f, 0), new Vector3(.28f, .12f, .18f), new Color(.16f, .16f, .18f));
        Primitive(PrimitiveType.Cube, "HandleRight", root.transform, new Vector3(.52f, .32f, 0), new Vector3(.28f, .12f, .18f), new Color(.16f, .16f, .18f));
        PrefabUtility.SaveAsPrefabAsset(root, VisualPath);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(VisualPath);
    }

    private static GameObject BuildCounter(GameObject stove, GameObject visual)
    {
        var counter = (GameObject)PrefabUtility.InstantiatePrefab(stove);
        counter.name = "TomatoSoupCounter";
        var oldVisual = counter.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "StoveCounter_Visual");
        if (oldVisual == null) { Object.DestroyImmediate(counter); throw new System.InvalidOperationException("StoveCounter visual child was not found; no counter was saved."); }
        oldVisual.gameObject.SetActive(false);
        var newVisual = (GameObject)PrefabUtility.InstantiatePrefab(visual, counter.transform);
        newVisual.name = "TomatoSoupCounter_Visual";
        newVisual.transform.localPosition = oldVisual.localPosition;
        newVisual.transform.localRotation = oldVisual.localRotation;
        newVisual.transform.localScale = oldVisual.localScale;
        return counter;
    }

    private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { color = color };
        var path = MaterialFolder + "/" + name + ".mat";
        AssetDatabase.CreateAsset(mat, path);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
}
