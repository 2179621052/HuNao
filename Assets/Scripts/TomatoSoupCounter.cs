using System;
using UnityEngine;

public class TomatoSoupCounter : BaseCounter, IHasProgress, IKeepCompletedProgressVisible
{
    private enum SoupState { Idle, Cooking, ReadyToCollect }

    [SerializeField] private KitchenObjectSO tomatoSlicesSO;
    [SerializeField] private KitchenObjectSO tomatoSoupSO;
    [SerializeField, Min(0.1f)] private float cookingTime = 5f;

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public bool KeepCompletedProgressVisible => state == SoupState.ReadyToCollect;

    private SoupState state;
    private float cookingTimer;
    private AudioSource soupAudioSource;

    private void Awake()
    {
        soupAudioSource = gameObject.AddComponent<AudioSource>();
        soupAudioSource.playOnAwake = false;
        soupAudioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (state != SoupState.Cooking) return;
        cookingTimer = Mathf.Min(cookingTimer + Time.deltaTime, cookingTime);
        if (cookingTimer >= cookingTime)
        {
            cookingTimer = cookingTime;
            state = SoupState.ReadyToCollect;
            AudioManager.StartSoupReady(soupAudioSource);
        }
        NotifyProgress(cookingTimer / cookingTime);
    }

    public override void Interact(Player player)
    {
        if (player == null) return;
        if (state == SoupState.Idle)
        {
            if (tomatoSlicesSO == null || tomatoSoupSO == null || tomatoSoupSO.prefab == null) return;
            KitchenObject held = player.GetKitchenObject();
            if (held == null || held.GetKitchenObjectSO() != tomatoSlicesSO) return;
            held.DestroySelf();
            if (player.HasKitchenObject()) return;
            cookingTimer = 0f;
            state = SoupState.Cooking;
            AudioManager.StartSoupBoil(soupAudioSource);
            NotifyProgress(0.001f);
            return;
        }

        if (state != SoupState.ReadyToCollect || player.HasKitchenObject()) return;
        if (tomatoSoupSO == null || tomatoSoupSO.prefab == null ||
            tomatoSoupSO.prefab.GetComponent<KitchenObject>() == null) return;

        KitchenObject soup = null;
        try { soup = KitchenObject.SpawnKitchenObject(tomatoSoupSO, player); }
        catch (Exception exception) { Debug.LogException(exception, this); }

        if (soup == null || player.GetKitchenObject() != soup ||
            soup.GetKchenObjectParent() != player)
        {
            if (soup != null) soup.DestroySelf();
            NotifyProgress(1f);
            return;
        }

        cookingTimer = 0f;
        state = SoupState.Idle;
        AudioManager.StopLoop(soupAudioSource);
        NotifyProgress(0f);
    }

    private void OnDisable() { AudioManager.StopLoop(soupAudioSource); }
    private void OnDestroy() { AudioManager.StopLoop(soupAudioSource); }

    private void NotifyProgress(float value)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(value)
        });
    }
}
