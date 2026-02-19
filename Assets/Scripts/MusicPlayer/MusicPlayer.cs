using System;
using UnityEngine;

/// <summary>
/// handles playing/ stopping audio files, holds how long the song is, and returns far we are in the current song
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    [Header("SongProperties")]
    private AudioClip currentSong;

    private float songLengthInMS;
    private float currentSongPositionInMS;
    
    private AudioSource audioSource;
    [SerializeField]private SongListScriptableObject songList;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySong(int songId)
    {
        currentSong = songList.songs[songId].clip;
        audioSource.clip = currentSong;
        GetSongLengthInMS();
        audioSource.Play();
    }

    public void StopSong()
    {
        audioSource.Stop();
    }

    private void GetSongLengthInMS()
    {
        songLengthInMS =  audioSource.clip.length * 1000;
    }

    public float GetSongPositionInMS()
    {
        currentSongPositionInMS = audioSource.time * 1000;
        return currentSongPositionInMS;
    }
}
