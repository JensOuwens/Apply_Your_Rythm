using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// use this class to spawn sound effects, and play them
/// </summary>

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
    private List<SoundEffectItem> SoundEffects;
    
    [SerializeField]
    private Metronome metronome;

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
        int listIndex = Random.Range(0, SoundEffects.Count - 1);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.clip = SoundEffects[listIndex].AudioClip;
        audioSource.Play();
        Destroy(audioSource, audioSource.clip.length);
        
    }

    public void PlaySoundEffectWithIndex(int index)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.clip = SoundEffects[index].AudioClip;
        audioSource.Play();
        Destroy(audioSource, audioSource.clip.length);
    }
    
    public void PlayRandomSoundEffectHold(BeatData beatData)
    {
        float beatLength = beatData.beatEnd -  beatData.beatStart;
        beatLength = beatLength * metronome.beatDurationInMS / 1000f;
        
        int listIndex = Random.Range(0, SoundEffects.Count);
        
        StartCoroutine(PlaySoundEffectHold(listIndex, beatLength));
        
    }

    public void PlaySoundEffectWithIndexHold(int index, BeatData beatData)
    {
        float beatLength = beatData.beatEnd -  beatData.beatStart;
        beatLength = beatLength * metronome.beatDurationInMS / 1000f;

        StartCoroutine(PlaySoundEffectHold(index, beatLength));
    }
    
    private IEnumerator PlaySoundEffectHold(int index, float beatLength)
    {
        float endtime = Time.time + beatLength;
        
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundEffects[index].AudioClip;
        audioSource.Play();
        audioSource.loop = true;
        
        while (Time.time < endtime)
        {
            yield return null;
        }
        
        audioSource.loop = false;
        Destroy(audioSource);
        yield return null;
    }
    

}
