#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public static class ConfigureAudioManager
{
    [MenuItem("HuNao/Audio/Configure AudioManager")]
    public static void Run()
    {
        const string prefabPath = "Assets/Prefab/Audio/AudioManager.prefab";
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Prefab/Audio"));
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject root = prefab != null ? PrefabUtility.LoadPrefabContents(prefabPath) : new GameObject("AudioManager");
        if (root.GetComponent<AudioManager>() == null) root.AddComponent<AudioManager>();
        var requiredSource = root.GetComponent<AudioSource>();
        if (requiredSource == null) requiredSource = root.AddComponent<AudioSource>();
        requiredSource.playOnAwake = false;
        requiredSource.loop = true;
        var audio = root.GetComponent<AudioManager>();
        var serialized = new SerializedObject(audio);
        Bind(serialized, "music", "Assets/_Assets/Sounds/Music/Music.wav");
        Bind(serialized, "chop", "Assets/_Assets/Sounds/SFX/SFX_chop01.wav");
        Bind(serialized, "pickup", "Assets/_Assets/Sounds/SFX/SFX_footstep01_01.wav");
        Bind(serialized, "drop", "Assets/_Assets/Sounds/SFX/SFX_object_drop02.wav");
        Bind(serialized, "objectPickup", "Assets/_Assets/Sounds/SFX/SFX_object_pickup03.wav");
        Bind(serialized, "deliverySuccess", "Assets/_Assets/Sounds/SFX/SFX_delivery_success01.wav");
        Bind(serialized, "deliveryFail", "Assets/_Assets/Sounds/SFX/SFX_delivery_fail01.wav");
        Bind(serialized, "fryingLoop", "Assets/_Assets/Sounds/SFX/SFX_pan_sizzle_loop.wav");
        Bind(serialized, "friedLoop", "Assets/_Assets/Sounds/SFX/pan-sizzle-loop.wav");
        Bind(serialized, "soupBoilLoop", "Assets/_Assets/Sounds/SFX/water-boil-rolling.wav");
        Bind(serialized, "soupReadyLoop", "Assets/_Assets/Sounds/SFX/kettle-whistle.wav");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        if (prefab == null) PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        else { PrefabUtility.SaveAsPrefabAsset(root, prefabPath); PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("AudioManager prefab prepared. Bind clips in its Inspector, then place it once in each game scene.");
    }

    [MenuItem("HuNao/Audio/Configure AudioManager Scenes")]
    public static void ConfigureScenes()
    {
        var sceneSetups = EditorSceneManager.GetSceneManagerSetup();
        foreach (var scene in sceneSetups)
            if (scene.isLoaded && scene.path != "" && EditorSceneManager.GetSceneByPath(scene.path).isDirty)
                throw new System.InvalidOperationException("保存当前未保存场景后再配置 AudioManager：" + scene.path);

        const string prefabPath = "Assets/Prefab/Audio/AudioManager.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) throw new System.InvalidOperationException("AudioManager Prefab 不存在，请先运行 Configure AudioManager。");
        foreach (var path in new[] { "Assets/Scenes/GameScene.unity", "Assets/Scenes/GameScene_Double.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var existing = Object.FindObjectsOfType<AudioManager>(true);
            if (existing.Length == 0) PrefabUtility.InstantiatePrefab(prefab, scene);
            else if (existing.Length > 1) throw new System.InvalidOperationException("场景存在多个 AudioManager：" + path);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("AudioManager 已幂等配置到 GameScene 和 GameScene_Double。");
    }

    private static void Bind(SerializedObject so, string field, string path)
    {
        var property = so.FindProperty(field);
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (property == null || clip == null) throw new System.InvalidOperationException("Missing audio field or asset: " + field + " / " + path);
        property.objectReferenceValue = clip;
    }
}
#endif
