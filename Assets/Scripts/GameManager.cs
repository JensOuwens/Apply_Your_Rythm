using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private List<ShowBeatVisual> beatVisualManagers;
    [SerializeField] private OrganismManager organismManager;
    
    private List<int> lastCheckedBeatPerLane = new();
    private List<int> lastVisualizedBeatPerLane = new();

    private bool gameRunning;

    private void OnValidate()
    {
        beatVisualManagers = FindObjectsByType<ShowBeatVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        beatVisualManagers.Sort();
    }

    private void Awake()
    {
        playerInputManager.onPlayerPressed.AddListener(OnPlayerPressed);
        playerInputManager.onPlayerReleased.AddListener(OnPlayerReleased);
    }

    private void Start() => StartGame();
    
    private void OnEnable() => ComposerCreator.ComposerSubscribed += SubscribeComposer;
    private void OnDisable() => ComposerCreator.ComposerSubscribed -= SubscribeComposer;
    private void SubscribeComposer(Composer obj)
    {
        lastCheckedBeatPerLane.Add(-1);
        lastVisualizedBeatPerLane.Add(-1);
    }

    private void Update()
    {
        if (!gameRunning) return;
        CheckIncomingBeats();
    }

    private void CheckIncomingBeats()
    {
        var songPosMs = musicPlayer.GetSongPositionInMS();
        var currentBeat = metronome.GetNearestBeat(songPosMs);
        for (var i = 0; i < judge.composers.Count; i++)
        {
            if (currentBeat == lastCheckedBeatPerLane[i]) continue;
            lastCheckedBeatPerLane[i] = currentBeat;

            var targetBeatIndex = currentBeat + 3;
            if (targetBeatIndex == lastVisualizedBeatPerLane[i]) continue;

            var composer = judge.composers[i];
            var beat = composer.GetBeat(targetBeatIndex);
            if (beat == null) continue;
            
            if (beat.type == BeatType.Hold)
            {
                if (targetBeatIndex < beat.beatStart)
                    continue;

                if (targetBeatIndex >= beat.beatEnd)
                {
                    lastVisualizedBeatPerLane[i] = beat.beatEnd;
                    continue;
                }
            }
            lastVisualizedBeatPerLane[i] = targetBeatIndex;
            beatVisualManagers[i].TriggerSpawnVisual(beat, metronome.beatDurationInMS);
        }
    }

    private void StartGame()
    {
        musicPlayer.PlaySong(musicId);
        metronome.InitMetronome();
        organismManager.InitOrganisms();
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