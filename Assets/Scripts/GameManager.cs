using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the game, intializes the music player and metronome, plus binds the judge and inputmanager together.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private Metronome metronome;
    [SerializeField] private Judge judge;

    [Header("Input")]
    [SerializeField] private PlayerInputManager playerInputManager;

    [Header("Config")]
    [SerializeField] private int musicId;
    
    [Header("Visuals")]
    [SerializeField] private List<ShowTapVisual> tapVisual;
    [SerializeField] private List<ShowHoldVisual> holdVisual;
    [SerializeField] private OrganismManager organismManager;
    
    private int lastCheckedBeat = -1;
    private int lastVisualizedBeat = -1;

    private bool gameRunning;

    private void Awake()
    {
        playerInputManager.onPlayerPressed.AddListener(OnPlayerPressed);
        playerInputManager.onPlayerReleased.AddListener(OnPlayerReleased);
    }

    private void Start() => StartGame();

    private void Update()
    {
        if (!gameRunning) return;
        CheckIncomingBeats();
    }

    private void CheckIncomingBeats()
    {
        var songPosMs = musicPlayer.GetSongPositionInMS();
        var currentBeat = metronome.GetNearestBeat(songPosMs);

        if (currentBeat == lastCheckedBeat) return;
        lastCheckedBeat = currentBeat;

        var targetBeatIndex = currentBeat + 3;

        if (targetBeatIndex == lastVisualizedBeat)
            return;

        for (var i = 0; i < judge.composers.Count; i++)
        {
            var composer = judge.composers[i];
            var beat = composer.GetBeat(targetBeatIndex);
            if (beat == null) return;

            lastVisualizedBeat = targetBeatIndex;

            if (beat.type == BeatType.Tap)
                tapVisual[i].HandleTapVisual(beat, metronome.beatDurationInMS);
            else if (beat.type == BeatType.Hold)
                holdVisual[i].HandleHoldVisual(beat, metronome.beatDurationInMS);
        }
    }

    private void StartGame()
    {
        musicPlayer.PlaySong(musicId);
        metronome.InitMetronome();
        organismManager.InitOrganisms(metronome);
        gameRunning = true;
    }

    private void OnPlayerPressed(int playerId)
    {
        if (!gameRunning) return;

        var songPosMs = musicPlayer.GetSongPositionInMS();
        judge.CheckInput(songPosMs, playerId, true);
    }

    private void OnPlayerReleased(int playerId)
    {
        if (!gameRunning) return;

        var songPosMs = musicPlayer.GetSongPositionInMS();
        var beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        var beat = judge.composers[playerId].GetBeat(beatIndex);
        if (beat == null) return;
        
        if (beat.type == BeatType.Hold) 
            judge.CheckInput(songPosMs, playerId, false);
    }
}