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
    private SongListScriptableObject songList;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void PlaySong(int songId)
    {
        currentSong = songList.songs[songId].clip;
        GetSongLengthInMS();
        audioSource.clip = currentSong;
        audioSource.Play();
    }

    public void StopSong()
    {
        
    }

    private void GetSongLengthInMS()
    {
        songLengthInMS =  audioSource.clip.length / 1000;
    }

    public void GetSongPositionInMS()
    {
        currentSongPositionInMS = audioSource.time / 1000;
    }
    
    
}
