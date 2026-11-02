using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class InstallTomatoSoupGameplay
{
    private const string CounterPath = "Assets/Prefab/Counters/TomatoSoupCounter.prefab";
    private const string ItemPath = "Assets/Prefab/KitchenObjects/TomatoSoupItem.prefab";
    private const string SoupPath = "Assets/Scenes/ScriptableObjects/KitchenObjectSO/TomatoSoup.asset";
    private const string SlicesPath = "Assets/Scenes/ScriptableObjects/KitchenObjectSO/TomatoSlices.asset";
    private const string SlicePrefabPath = "Assets/Prefab/KitchenObjects/TomatoSlices.prefab";
    private const string VisualPath = "Assets/Prefab/KitchenObjects/TomatoSoup.prefab";
    private const string ProgressPath = "Assets/Prefab/ProgressBarUI.prefab";

    [MenuItem("Tools/HuNao/Install Tomato Soup Gameplay")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Stop Play mode before installing tomato soup gameplay."); return;
        }

        var counter = AssetDatabase.LoadAssetAtPath<GameObject>(CounterPath);
        var slices = AssetDatabase.LoadAssetAtPath<KitchenObjectSO>(SlicesPath);
        var slicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlicePrefabPath);
        var visual = AssetDatabase.LoadAssetAtPath<GameObject>(VisualPath);
        var progress = AssetDatabase.LoadAssetAtPath<GameObject>(ProgressPath);
        var stove = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Counters/StoveCounter.prefab");
        if (counter == null || slices == null || slicePrefab == null || visual == null || progress == null || stove == null)
        {
            Debug.LogError("Tomato soup install aborted: required counter, input item, visual, progress UI or stove asset is missing."); return;
        }
        if (slicePrefab.GetComponent<KitchenObject>() == null || slices.prefab == null ||
            slices.prefab.GetComponent<KitchenObject>() == null || slices.prefab.gameObject != slicePrefab)
        {
            Debug.LogError("Tomato soup install aborted: TomatoSlices SO/prefab relationship is invalid."); return;
        }
        if (visual.GetComponent<KitchenObject>() != null)
        {
            Debug.LogError("Tomato soup install aborted: visual prefab unexpectedly contains KitchenObject."); return;
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ItemPath) != null ||
            AssetDatabase.LoadAssetAtPath<KitchenObjectSO>(SoupPath) != null)
        {
            Debug.LogError("Tomato soup install aborted: output item or SO already exists; refusing to overwrite."); return;
        }
        if (counter.GetComponent<BaseCounter>() == null)
        {
            // The existing untracked asset is visual-only. Replace that asset with a StoveCounter-derived copy.
            var replacement = (GameObject)PrefabUtility.InstantiatePrefab(stove);
            replacement.name = "TomatoSoupCounter";
            PrefabUtility.SaveAsPrefabAsset(replacement, CounterPath);
            UnityEngine.Object.DestroyImmediate(replacement);
            counter = AssetDatabase.LoadAssetAtPath<GameObject>(CounterPath);
        }
        if (counter.GetComponent<BaseCounter>() == null)
        {
            Debug.LogError("Tomato soup install aborted: no BaseCounter exists on the target prefab."); return;
        }

        // First create the item and its SO. The counter prefab is saved only after both links validate.
        var item = (GameObject)PrefabUtility.InstantiatePrefab(slicePrefab);
        item.name = "TomatoSoupItem";
        foreach (Transform child in item.transform.Cast<Transform>().ToArray())
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        var soupVisual = (GameObject)PrefabUtility.InstantiatePrefab(visual, item.transform);
        soupVisual.name = "TomatoSoupVisual";
        soupVisual.transform.localPosition = Vector3.zero;
        soupVisual.transform.localRotation = Quaternion.identity;
        soupVisual.transform.localScale = Vector3.one * .45f;
        var soupSO = ScriptableObject.CreateInstance<KitchenObjectSO>();
        soupSO.objectName = "TomatoSoup";
        soupSO.sprite = slices.sprite;
        AssetDatabase.CreateAsset(soupSO, SoupPath);
        var itemKitchenObject = item.GetComponent<KitchenObject>();
        var itemSerialized = new SerializedObject(itemKitchenObject);
        itemSerialized.FindProperty("kitchenObjectSO").objectReferenceValue = soupSO;
        itemSerialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(item, ItemPath);
        UnityEngine.Object.DestroyImmediate(item);

        var savedItem = AssetDatabase.LoadAssetAtPath<GameObject>(ItemPath);
        soupSO.prefab = savedItem != null ? savedItem.transform : null;
        EditorUtility.SetDirty(soupSO);
        if (savedItem == null || savedItem.GetComponent<KitchenObject>() == null || soupSO.prefab == null)
        {
            Debug.LogError("Tomato soup item creation failed; counter prefab was not changed."); return;
        }

        var root = PrefabUtility.LoadPrefabContents(CounterPath);
        try
        {
            root.layer = stove.layer;
            var collider = root.GetComponent<BoxCollider>();
            if (collider == null) collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, .75f, 0f);
            collider.size = new Vector3(1f, 1.5f, 1f);
            var top = new GameObject("CounterTopPoint").transform;
            top.SetParent(root.transform, false);
            top.localPosition = new Vector3(0f, 1.5f, 0f);
            var soupCounter = root.AddComponent<TomatoSoupCounter>();
            var serialized = new SerializedObject(soupCounter);
            serialized.FindProperty("counterTopPoint").objectReferenceValue = top;
            serialized.FindProperty("tomatoSlicesSO").objectReferenceValue = slices;
            serialized.FindProperty("tomatoSoupSO").objectReferenceValue = soupSO;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var ui = (GameObject)PrefabUtility.InstantiatePrefab(progress, root.transform);
            ui.name = "ProgressBarUI";
            ui.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            var progressUI = ui.GetComponent<ProgressBarUI>();
            if (progressUI == null) throw new InvalidOperationException("ProgressBarUI component missing.");
            var uiSerialized = new SerializedObject(progressUI);
            uiSerialized.FindProperty("hasProgressGameObject").objectReferenceValue = root;
            uiSerialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, CounterPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("TomatoSoup gameplay installed. Existing scene prefab instances inherit the new logic. Review the scene, compile, then test in Play mode.");
    }
}
