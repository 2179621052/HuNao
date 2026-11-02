using TMPro;
using UnityEditor;
using UnityEngine;

public static class CreateSubmitCounterAsset
{
    [MenuItem("Tools/HuNao/Create Submit Counter Prefab")]
    public static void Create()
    {
        const string sourcePath = "Assets/Prefab/Counters/ClearCounter.prefab";
        const string targetPath = "Assets/Prefab/Counters/SubmitCounter.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) != null)
        {
            Debug.LogError("SubmitCounter.prefab already exists; refusing to overwrite.");
            return;
        }

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (source == null) { Debug.LogError("ClearCounter prefab not found."); return; }
        var root = (GameObject)PrefabUtility.InstantiatePrefab(source);
        root.name = "SubmitCounter";
        var top = root.transform.Find("CounterTopPoint");
        if (top == null) { Object.DestroyImmediate(root); Debug.LogError("CounterTopPoint missing."); return; }

        var submit = root.AddComponent<SubmitCounter>();
        var icon = new GameObject("SubmitIcon");
        icon.transform.SetParent(root.transform, false);
        icon.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        icon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        icon.transform.localScale = Vector3.one * .12f;
        var text = icon.AddComponent<TextMeshPro>();
        text.text = "$";
        text.fontSize = 4f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, .65f, .05f, 1f);

        var select = root.AddComponent<SelectCounterVisual>();
        var serialized = new SerializedObject(select);
        serialized.FindProperty("baseCounter").objectReferenceValue = submit;
        serialized.FindProperty("keepVisualsVisible").boolValue = true;
        var array = serialized.FindProperty("visualGameObjectArray");
        array.arraySize = 1;
        array.GetArrayElementAtIndex(0).objectReferenceValue = icon;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, targetPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("SubmitCounter prefab created; no scene was modified.");
    }
}
