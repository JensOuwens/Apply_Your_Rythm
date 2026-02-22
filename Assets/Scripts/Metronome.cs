using System;
using UnityEngine;


/// <summary>
/// determines when a beat is in the song, how many beats the song has, and then transmits that data to other classes
/// </summary>
public class Metronome : MonoBehaviour
{
    [SerializeField] private MusicPlayer musicPlayer;
    
    private int bpm;
    public float beatDurationInMS;
    public int lastBeat = 0;
    private float nextBeatPosition;
    private float totalBeatCount;

    public bool initialized = false;

    private void Update()
    {
        if (initialized) UpdateBeat();
    }
    
    public void InitMetronome()
    {
        bpm = musicPlayer.songList.songs[musicPlayer.songId].bpm;
        beatDurationInMS = (60f / bpm) * 1000;
        totalBeatCount = musicPlayer.songLengthInMS / beatDurationInMS;
        nextBeatPosition = beatDurationInMS;
        initialized = true;
    }

    private void UpdateBeat()
    {
        if (musicPlayer.GetSongPositionInMS() >= nextBeatPosition)
        {
            lastBeat += 1;
            nextBeatPosition += beatDurationInMS;
        }
    }

    public int GetNearestBeat(float beatPos)
    {
        if (!initialized)
            return -1;

        return Mathf.RoundToInt(beatPos / beatDurationInMS);
    }
}
