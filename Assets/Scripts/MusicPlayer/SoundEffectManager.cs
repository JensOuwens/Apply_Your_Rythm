using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffectItem
{
    public uint id;
    public AudioClip AudioClip;
    public float Volume;
}

public class SoundEffectManager : MonoBehaviour
{
    [HideInInspector]
    public static SoundEffectManager instance;
    
    [SerializeField]
    private List<SoundEffectItem> SoundEffectsTapWater;
    
    [SerializeField]
    private List<SoundEffectItem> SoundEffectTapC02;
    [SerializeField]
    private List<SoundEffectItem> SoundEffectsHold;
    
    [SerializeField]
    private Metronome metronome;

    [Header("player pitches")]
    [SerializeField] private float player1Pitch;
    [SerializeField] private float player2Pitch;
    [SerializeField] private float player3Pitch;
    [SerializeField] private float player4Pitch;


    private Dictionary<int, AudioSource> activeHoldSounds = new();

    private void Awake()
    {
        if (instance) return;
        instance = this;
    }

    public void PlayRandomSoundEffect(int playerID, BeatAttribute attribute)
    {
        List<SoundEffectItem> currentSFXList = new List<SoundEffectItem>();

        switch (attribute)
        {
            case BeatAttribute.Water:
                currentSFXList = SoundEffectsTapWater;
                break;
            case BeatAttribute.Co2 :
                currentSFXList = SoundEffectTapC02;
                break;
        }
        
        int listIndex = Random.Range(0, currentSFXList.Count - 1);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = currentSFXList[listIndex].AudioClip;
        audioSource.volume = currentSFXList[listIndex].Volume;
        float pitch = 0;
        switch (playerID)
        {
            case 0:
                pitch = player1Pitch;
                break;
            case 1:
                pitch =  player2Pitch;
                break;
            case 2:
                pitch = player3Pitch;
                break;
            case 3:
                pitch = player4Pitch;
                break;
        }
        audioSource.pitch = pitch;
        audioSource.Play();
        Destroy(audioSource, audioSource.clip.length);
    }

    public void PlayRandomSoundEffectHold(BeatData beatData, int playerId)
    {
        float beatLength = (beatData.beatEnd - beatData.beatStart + 1) * metronome.beatDurationInMS / 1000f;
        int listIndex = Random.Range(0, SoundEffectsHold.Count);
        PlaySoundEffectWithIndexHold(listIndex, beatData, playerId, beatLength);
    }

    public void PlaySoundEffectWithIndexHold(int index, BeatData beatData, int playerId, float beatLength)
    {
        StopHoldSound(playerId);

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundEffectsHold[index].AudioClip;
        audioSource.loop = true;
        audioSource.volume = SoundEffectsHold[index].Volume;
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