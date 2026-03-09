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

}
