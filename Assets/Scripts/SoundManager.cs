using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using DG.Tweening;

/// <summary>
/// manages sound effects playback and provides global access through singleton pattern
/// </summary>
public class SoundManager : MonoBehaviour
{
    // constants for sound configuration
    private const float DEFAULT_PITCH_VARIATION = 0.1f;
    private const float PITCH_MODIFIER_MULTIPLIER = 0.05f;
    private const float SOUND_CLEANUP_DELAY_MULTIPLIER = 1.5f;

    [SerializeField] private AudioClip regularMusic, menuMusic;
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private float fadeSpeed = 0.5f;

    private Coroutine fadeCoroutine;

    private bool gameOver;

    private string musicState = "Menu";

    // Dictionary to track active sounds by their name
    private Dictionary<string, List<GameObject>> activeSounds = new Dictionary<string, List<GameObject>>();

    public void SwitchToMenuMusic()
    {
        if (musicState == "Menu")
            return;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        musicState = "Menu";
        fadeCoroutine = StartCoroutine(FadeAndSwitchMusic(menuMusic, 1f));
    }

    public void SwitchToRegularMusic()
    {
        if (musicState == "Main")
            return;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        musicState = "Main";
        fadeCoroutine = StartCoroutine(FadeAndSwitchMusic(regularMusic, 1f));
    }

    private IEnumerator FadeAndSwitchMusic(AudioClip newClip, float endVolume)
    {
        float startVolume = musicPlayer.volume;
        float timeElapsed = 0;

        while (timeElapsed < fadeSpeed)
        {
            timeElapsed += Time.unscaledDeltaTime;
            musicPlayer.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeSpeed);
            yield return null;
        }

        float playback = (musicPlayer.time / musicPlayer.clip.length) * newClip.length;

        musicPlayer.clip = newClip;
        musicPlayer.time = playback;
        musicPlayer.Play();

        timeElapsed = 0;
        while (timeElapsed < fadeSpeed)
        {
            timeElapsed += Time.unscaledDeltaTime;
            musicPlayer.volume = Mathf.Lerp(0f, startVolume, timeElapsed / fadeSpeed);
            yield return null;
        }

        musicPlayer.volume = endVolume;
    }

    // serialized fields for configuration
    [SerializeField]
    [Range(0, 1)]
    private float volumeMultiplier = 1;

    [SerializeField]
    private List<SoundEffect> soundEffects;

    // singleton instance
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    /// <summary>
    /// initializes the singleton instance
    /// </summary>
    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// plays the default click sound effect
    /// </summary>
    public void PlayClickSoundEffect() => PlaySoundEffect("click");

    /// <summary>
    /// plays a sound effect by name with default modifier
    /// </summary>
    public void PlaySoundEffect(string soundName) => PlaySoundEffect(soundName, 0);

    /// <summary>
    /// plays a sound effect with specified name and pitch modifier
    /// </summary>
    /// <param name="soundName">name of the sound effect to play</param>
    /// <param name="modifier">pitch modifier value</param>
    /// 

    public void PlayGameOverSound()
    {
        PlaySoundEffect("GameOver");
        transform.GetChild(1).GetComponent<AudioSource>().DOFade(0, 0.5f);
        Invoke("SwitchToMenuMusic", 2f);
        gameOver = true;
    }

    public void PlaySoundEffect(string soundName, int modifier)
    {
        if (gameOver)
            return;


        SoundEffect? soundEffect = GetRandomMatchingSoundEffect(soundName);
        if (!soundEffect.HasValue || !HasClips(soundEffect.Value)) return;

        AudioClip clip = GetRandomClip(soundEffect.Value);
        GameObject soundObject = CreateSoundGameObject(soundName, clip);
        ConfigureAndPlaySound(soundObject, soundEffect.Value, clip, modifier);

        // Add the sound object to the tracking dictionary
        if (!activeSounds.ContainsKey(soundName))
        {
            activeSounds[soundName] = new List<GameObject>();
        }
        activeSounds[soundName].Add(soundObject);
    }

    /// <summary>
    /// stops all instances of a sound effect with the specified name
    /// </summary>
    /// <param name="soundName">name of the sound effect to stop</param>
    public void StopSound(string soundName)
    {
        if (!activeSounds.ContainsKey(soundName)) return;

        foreach (var soundObject in activeSounds[soundName])
        {
            if (soundObject != null)
            {
                var audioSource = soundObject.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                Destroy(soundObject);
            }
        }

        activeSounds[soundName].Clear();
    }

    /// <summary>
    /// cleanup method to remove destroyed sound objects from tracking
    /// </summary>
    private void CleanupDestroyedSounds()
    {
        var keysToUpdate = activeSounds.Keys.ToList();
        foreach (var key in keysToUpdate)
        {
            activeSounds[key].RemoveAll(obj => obj == null);
        }
    }

    private void OnDestroy()
    {
        // Clean up all active sounds when the manager is destroyed
        foreach (var soundList in activeSounds.Values)
        {
            foreach (var soundObject in soundList)
            {
                if (soundObject != null)
                {
                    Destroy(soundObject);
                }
            }
        }
        activeSounds.Clear();
    }

    /// <summary>
    /// finds a random sound effect matching the given name
    /// </summary>
    private SoundEffect? GetRandomMatchingSoundEffect(string soundName)
    {
        var matchingEffects = soundEffects
            .Where(s => s.SoundName.Equals(soundName))
            .ToList();

        if (!matchingEffects.Any()) return null;

        return matchingEffects[Random.Range(0, matchingEffects.Count)];
    }

    /// <summary>
    /// checks if the sound effect has any clips available
    /// </summary>
    private bool HasClips(SoundEffect soundEffect)
    {
        return soundEffect.Clips != null && soundEffect.Clips.Count > 0;
    }

    /// <summary>
    /// gets a random clip from the sound effect
    /// </summary>
    private AudioClip GetRandomClip(SoundEffect soundEffect)
    {
        return soundEffect.Clips[Random.Range(0, soundEffect.Clips.Count)];
    }

    /// <summary>
    /// creates a game object to play the sound effect
    /// </summary>
    private GameObject CreateSoundGameObject(string soundName, AudioClip clip)
    {
        var soundObject = new GameObject($"Sound: {soundName}, {clip.length}s");
        soundObject.transform.parent = transform;
        StartCoroutine(RemoveFromTrackingAfterPlay(soundName, soundObject, clip.length * SOUND_CLEANUP_DELAY_MULTIPLIER));
        return soundObject;
    }

    /// <summary>
    /// coroutine to remove sound object from tracking after it finishes playing
    /// </summary>
    private IEnumerator RemoveFromTrackingAfterPlay(string soundName, GameObject soundObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeSounds.ContainsKey(soundName))
        {
            activeSounds[soundName].Remove(soundObject);
        }
        Destroy(soundObject);
    }

    /// <summary>
    /// configures and plays the sound effect
    /// </summary>
    private void ConfigureAndPlaySound(GameObject soundObject, SoundEffect soundEffect, AudioClip clip, int modifier)
    {
        AudioSource source = AddAndConfigureAudioSource(soundObject, clip, soundEffect);
        ApplyPitchModification(source, soundEffect, modifier);
        source.Play();
    }

    /// <summary>
    /// adds and configures the audio source component
    /// </summary>
    private AudioSource AddAndConfigureAudioSource(GameObject soundObject, AudioClip clip, SoundEffect soundEffect)
    {
        var source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = soundEffect.Volume * volumeMultiplier;
        return source;
    }

    /// <summary>
    /// applies pitch modifications to the audio source
    /// </summary>
    private void ApplyPitchModification(AudioSource source, SoundEffect soundEffect, int modifier)
    {
        if (soundEffect.Vary)
        {
            source.pitch += Random.Range(-DEFAULT_PITCH_VARIATION, DEFAULT_PITCH_VARIATION);
        }
        source.pitch += PITCH_MODIFIER_MULTIPLIER * modifier;
    }
}

/// <summary>
/// data structure to define a sound effect with its properties
/// </summary>
[System.Serializable]
public struct SoundEffect
{
    private string name;

    public string SoundName;
    public List<AudioClip> Clips;

    [Range(0, 1)]
    public float Volume;

    public bool Vary;
}