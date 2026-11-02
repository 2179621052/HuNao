using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    [SerializeField] private bool keepVisualsVisible;
    [SerializeField] private Color highlightColor = new Color(1f, 0.45f, 0.05f, 1f);
    [SerializeField, Range(0f, 1f)] private float highlightStrength = 0.55f;

    private readonly HashSet<Player> playersSelecting = new HashSet<Player>();
    private readonly List<Player> subscribedPlayers = new List<Player>();
    private readonly List<Renderer> selectedRenderers = new List<Renderer>();
    private readonly Dictionary<Renderer, MaterialPropertyBlock> originalPropertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        // TomatoSoupCounter uses its normal model as the highlight target. Keep that model
        // active and use only the MaterialPropertyBlock color override for selection.
        if (GetComponent<TomatoSoupCounter>() != null)
        {
            keepVisualsVisible = true;
        }
        CacheSelectedRenderers();
    }

    private void Start()
    {
        foreach (Player player in Player.AllPlayers)
        {
            player.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
            subscribedPlayers.Add(player);
        }
    }

    private void OnDestroy()
    {
        foreach (Player player in subscribedPlayers)
        {
            if (player != null)
            {
                player.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            }
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (sender is Player player)
        {
            if (e.selectedCounter == baseCounter)
            {
                playersSelecting.Add(player);
            }
            else
            {
                playersSelecting.Remove(player);
            }
        }

        if (playersSelecting.Count > 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if (!keepVisualsVisible)
        {
            foreach (GameObject visualGameObject in visualGameObjectArray)
            {
                if (visualGameObject != null) visualGameObject.SetActive(true);
            }
        }

        ApplyHighlight();
    }

    private void Hide()
    {
        RestoreOriginalMaterials();
        if (!keepVisualsVisible)
        {
            foreach (GameObject visualGameObject in visualGameObjectArray)
            {
                if (visualGameObject != null) visualGameObject.SetActive(false);
            }
        }
    }

    private void CacheSelectedRenderers()
    {
        if (GetComponent<TomatoSoupCounter>() != null)
        {
            HashSet<Renderer> soupRenderers = new HashSet<Renderer>();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                if (renderer.GetComponentInParent<ParticleSystem>() != null) continue;
                if (renderer.GetComponentInParent<ProgressBarUI>() != null) continue;
                soupRenderers.Add(renderer);
            }
            selectedRenderers.AddRange(soupRenderers);
            return;
        }

        if (visualGameObjectArray == null) return;

        HashSet<Renderer> uniqueRenderers = new HashSet<Renderer>();
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            if (visualGameObject == null) continue;
            foreach (Renderer renderer in visualGameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (uniqueRenderers.Add(renderer)) selectedRenderers.Add(renderer);
            }
        }
    }

    private void ApplyHighlight()
    {
        foreach (Renderer renderer in selectedRenderers)
        {
            if (renderer == null) continue;

            // Capture the block only once per selection cycle. If Show is called again
            // while another player is still selecting, the current block may already
            // contain the highlight and must not replace the original state.
            if (!originalPropertyBlocks.ContainsKey(renderer))
            {
                MaterialPropertyBlock original = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original);
                originalPropertyBlocks.Add(renderer, original);
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            Material[] materials = renderer.sharedMaterials;
            bool hasSupportedColor = false;
            bool isTomatoSoupSurface = renderer.name == "TomatoSoupSurface";
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null) continue;

                if (material.HasProperty(BaseColorId))
                {
                    Color original = material.GetColor(BaseColorId);
                    Color target = isTomatoSoupSurface ? new Color(1f, 0.16f, 0.04f, 1f) : highlightColor;
                    float strength = isTomatoSoupSurface ? highlightStrength * 0.3f : highlightStrength;
                    block.SetColor(BaseColorId, Color.Lerp(original, target, strength));
                    hasSupportedColor = true;
                }
                else if (material.HasProperty(ColorId))
                {
                    Color original = material.GetColor(ColorId);
                    Color target = isTomatoSoupSurface ? new Color(1f, 0.16f, 0.04f, 1f) : highlightColor;
                    float strength = isTomatoSoupSurface ? highlightStrength * 0.3f : highlightStrength;
                    block.SetColor(ColorId, Color.Lerp(original, target, strength));
                    hasSupportedColor = true;
                }

                if (!isTomatoSoupSurface && material.HasProperty(EmissionColorId))
                {
                    block.SetColor(EmissionColorId, highlightColor * 0.18f);
                }
            }

            if (hasSupportedColor)
            {
                renderer.SetPropertyBlock(block);
            }
            else
            {
                Debug.LogWarning($"{name}: selected renderer '{renderer.name}' has no supported color property; highlight skipped.", this);
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (KeyValuePair<Renderer, MaterialPropertyBlock> entry in originalPropertyBlocks)
        {
            if (entry.Key != null) entry.Key.SetPropertyBlock(entry.Value);
        }
        originalPropertyBlocks.Clear();
    }
}
