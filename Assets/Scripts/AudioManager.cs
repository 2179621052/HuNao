using System.Collections.Generic;
using UnityEngine;

/// <summary>Small shared audio service. Clip references are intentionally serialized so they can be bound in the Inspector.</summary>
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Short sounds")]
    [SerializeField] private AudioClip deliverySuccess;
    [SerializeField, Range(0f, 1f)] private float deliverySuccessVolume = 0.8f;
    [SerializeField] private AudioClip deliveryFail;
    [SerializeField, Range(0f, 1f)] private float deliveryFailVolume = 0.8f;
    [SerializeField] private AudioClip chop;
    [SerializeField, Range(0f, 1f)] private float chopVolume = 0.8f;
    [SerializeField] private AudioClip pickup;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.8f;
    [SerializeField] private AudioClip drop;
    [SerializeField, Range(0f, 1f)] private float dropVolume = 0.8f;
    [SerializeField] private AudioClip objectPickup;
    [SerializeField, Range(0f, 1f)] private float objectPickupVolume = 0.8f;
    [SerializeField] private AudioClip fryingLoop;
    [SerializeField, Range(0f, 1f)] private float fryingLoopVolume = 0.65f;
    [SerializeField] private AudioClip friedLoop;
    [SerializeField, Range(0f, 1f)] private float friedLoopVolume = 0.65f;
    [SerializeField] private AudioClip soupBoilLoop;
    [SerializeField, Range(0f, 1f)] private float soupBoilLoopVolume = 0.6f;
    [SerializeField] private AudioClip soupReadyLoop;
    [SerializeField, Range(0f, 1f)] private float soupReadyLoopVolume = 0.7f;

    [Header("Background music")]
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;

    private AudioSource musicSource;
    private readonly Dictionary<AudioSource, AudioClip> loops = new Dictionary<AudioSource, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource = gameObject.GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (musicSource == null)
        {
            Debug.LogError("AudioManager requires an AudioSource on the same GameObject.", this);
            enabled = false;
            return;
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        if (music != null && !musicSource.isPlaying) { musicSource.clip = music; musicSource.Play(); }
    }

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (Instance == null || clip == null) return;
        var source = new GameObject("AudioOneShot").AddComponent<AudioSource>();
        source.volume = Mathf.Clamp01(volume);
        source.PlayOneShot(clip);
        Object.Destroy(source.gameObject, clip.length + 0.05f);
    }

    public static void PlayDeliverySuccess() => Play(Instance?.deliverySuccess, Instance?.deliverySuccessVolume ?? 1f);
    public static void PlayDeliveryFail() => Play(Instance?.deliveryFail, Instance?.deliveryFailVolume ?? 1f);
    public static void PlayChop() => Play(Instance?.chop, Instance?.chopVolume ?? 1f);
    public static void PlayPickup() => Play(Instance?.pickup, Instance?.pickupVolume ?? 1f);
    public static void PlayDrop() => Play(Instance?.drop, Instance?.dropVolume ?? 1f);
    public static void PlayObjectPickup() => Play(Instance?.objectPickup, Instance?.objectPickupVolume ?? 1f);
    public static void StartFrying(AudioSource source) => StartLoop(source, Instance?.fryingLoop, Instance?.fryingLoopVolume ?? 1f);
    public static void StartFried(AudioSource source) => StartLoop(source, Instance?.friedLoop, Instance?.friedLoopVolume ?? 1f);
    public static void StartSoupBoil(AudioSource source) => StartLoop(source, Instance?.soupBoilLoop, Instance?.soupBoilLoopVolume ?? 1f);
    public static void StartSoupReady(AudioSource source) => StartLoop(source, Instance?.soupReadyLoop, Instance?.soupReadyLoopVolume ?? 1f);

    public static void StartLoop(AudioSource source, AudioClip clip, float volume = 1f)
    {
        if (source == null || clip == null) return;
        if (source.isPlaying && source.clip == clip) return;
        source.Stop(); source.clip = clip; source.loop = true; source.volume = Mathf.Clamp01(volume); source.Play();
        if (Instance != null) Instance.loops[source] = clip;
    }

    public static void StopLoop(AudioSource source)
    {
        if (source == null) return;
        source.Stop(); source.clip = null;
        if (Instance != null) Instance.loops.Remove(source);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        foreach (var source in loops.Keys) if (source != null) source.Stop();
        loops.Clear();
    }
}
