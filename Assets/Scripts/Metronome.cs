using UnityEngine;

/// <summary>
/// Determines when a beat is in the song, how many beats the song has, and then transmits that data to other classes
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

    public float BeatDurationMs => beatDurationInMS;
    public bool Initialized => initialized;

    private void Update()
    {
        if (initialized) UpdateBeat();
    }

    public void InitMetronome()
    {
        bpm = musicPlayer.songList.songs[musicPlayer.songId].bpm;
        beatDurationInMS = (60f / bpm) * 1000f;
        totalBeatCount = musicPlayer.songLengthInMS / beatDurationInMS;
        nextBeatPosition = beatDurationInMS;
        lastBeat = 0;
        initialized = true;

        Debug.Log($"[Metronome] Initialized: BPM={bpm}, BeatDuration={beatDurationInMS:F2}ms, TotalBeats={totalBeatCount:F0}");
    }

    private void UpdateBeat()
    {
        float songPosMs = musicPlayer.GetSongPositionInMS();

        while (songPosMs >= nextBeatPosition) // use while to catch multiple beats if frame skips
        {
            lastBeat += 1;
            //Debug.Log($"[Metronome] Beat {lastBeat} at {songPosMs:F2}ms");

            nextBeatPosition += beatDurationInMS;
        }
    }

    public int GetNearestBeat(float songPosMs)
    {
        if (!initialized)
            return -1;

        return Mathf.RoundToInt(songPosMs / beatDurationInMS);
    }
}