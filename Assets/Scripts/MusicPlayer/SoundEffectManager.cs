using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffectItem
{
    public uint id;
    public AudioClip AudioClip;
}

public class SoundEffectManager : MonoBehaviour
{
    [HideInInspector]
    public static SoundEffectManager instance;
    
    [SerializeField]
    private List<SoundEffectItem> SoundEffectsTap;
    [SerializeField]
    private List<SoundEffectItem> SoundEffectsHold;
    
    [SerializeField]
    private Metronome metronome;

    private Dictionary<int, AudioSource> activeHoldSounds = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlayRandomSoundEffect()
    {
        int listIndex = Random.Range(0, SoundEffectsTap.Count - 1);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundEffectsTap[listIndex].AudioClip;
        audioSource.Play();
        Destroy(audioSource, audioSource.clip.length);
    }

    public void PlaySoundEffectWithIndex(int index)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundEffectsTap[index].AudioClip;
        audioSource.Play();
        Destroy(audioSource, audioSource.clip.length);
    }

    public void PlayRandomSoundEffectHold(BeatData beatData, int playerId)
    {
        float beatLength = (beatData.beatEnd - beatData.beatStart + 1) * metronome.beatDurationInMS / 1000f;
        int listIndex = Random.Range(0, SoundEffectsHold.Count);
        PlaySoundEffectWithIndexHold(listIndex, beatData, playerId);
    }

    public void PlaySoundEffectWithIndexHold(int index, BeatData beatData, int playerId)
    {
        float beatLength = (beatData.beatEnd - beatData.beatStart + 1) * metronome.beatDurationInMS / 1000f;

        StopHoldSound(playerId);

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundEffectsHold[index].AudioClip;
        audioSource.loop = true;
        audioSource.Play();

        activeHoldSounds[playerId] = audioSource;

        StartCoroutine(PlaySoundEffectHold(playerId, beatLength));
    }

    private IEnumerator PlaySoundEffectHold(int playerId, float beatLength)
    {
        float endtime = Time.time + beatLength;

        while (Time.time < endtime)
            yield return null;

        StopHoldSound(playerId);
    }

    public void StopHoldSound(int playerId)
    {
        if (activeHoldSounds.TryGetValue(playerId, out var source))
        {
            source.loop = false;
            Destroy(source);
            activeHoldSounds.Remove(playerId);
        }
    }
}