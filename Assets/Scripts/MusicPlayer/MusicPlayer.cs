using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// handles playing/ stopping audio files, holds how long the song is, and returns far we are in the current song
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    [Header("SongProperties")]
    private AudioClip currentSong;

    public float songLengthInMS;
    [NonSerialized]
    public float currentSongPositionInMS;
    public int songId;
    
    public SongListScriptableObject songList;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float songLengthUsedForEnd;
    public UnityEvent onSongEnd;

    private void OnValidate() => audioSource = GetComponent<AudioSource>();

    private void Start() => PlaySong(0);

    public void PlaySong(int songId)
    {
        this.songId = songId;
        currentSong = songList.songs[songId].clip;
        audioSource.clip = currentSong;
        GetSongLengthInMS();
        audioSource.Play();
    }

    public void StopSong() => audioSource.Stop();
    public void GetSongLengthInMS() => songLengthInMS = audioSource.clip.length * 1000;

    public float GetSongPositionInMS()
    {
        currentSongPositionInMS = audioSource.time * 1000;
        return currentSongPositionInMS;
    }

    private void Update()
    {
        if (audioSource.time >= songLengthUsedForEnd)
            onSongEnd.Invoke();
    }
}
