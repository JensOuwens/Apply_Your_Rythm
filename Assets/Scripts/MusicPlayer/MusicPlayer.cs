using UnityEngine;

/// <summary>
/// handles playing/ stopping audio files, holds how long the song is, and how far we are in the current song
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    [Header("SongProperties")]
    private AudioClip currentSong;

    private float songLengthInMS;
    private float currentSongPositionInMS;
    
    private AudioSource audioSource;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void PlaySong()
    {
        
    }

    public void StopSong()
    {
        
    }

    private void GetSongLengthInMS()
    {
        
    }
    
    
}
