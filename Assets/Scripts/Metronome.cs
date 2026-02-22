using System;
using UnityEngine;


/// <summary>
/// determines when a beat is in the song, how many beats the song has, and then transmits that data to other classes
/// </summary>
public class Metronome : MonoBehaviour
{
    [SerializeField] private MusicPlayer musicPlayer;
    
    private int bpm;
    private float beatDurationInMS;
    private int lastBeat = 0;
    private float nextBeatPosition;
    private float totalBeatCount;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InitMetronome();
        }
    }
    
    private void InitMetronome()
    {
        bpm = musicPlayer.songList.songs[musicPlayer.songId].bpm;
        Debug.Log(bpm);
        beatDurationInMS = (60f / bpm) * 1000;
        Debug.Log(beatDurationInMS);
        totalBeatCount = musicPlayer.songLengthInMS / beatDurationInMS;
        Debug.Log(totalBeatCount);
    }

    public int ConvertToBpm(float beatPos)
    {
        return 0;
    }
}
