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
    [SerializeField] private ShowTapVisual tapVisual;
    [SerializeField] private ShowHoldVisual holdVisual;
    
    private int lastCheckedBeat = -1;
    private int lastVisualizedBeat = -1;

    private bool gameRunning;

    private void Awake()
    {
        playerInputManager.onPlayerPressed.AddListener(OnPlayerPressed);
        playerInputManager.onPlayerReleased.AddListener(OnPlayerReleased);
    }

    private void Start()
    {
        StartGame();
    }
    
    private void Update()
    {
        if (!gameRunning) return;
        CheckIncomingBeats();
    }

    private void CheckIncomingBeats()
    {
        float songPosMs = musicPlayer.GetSongPositionInMS();
        int currentBeat = metronome.GetNearestBeat(songPosMs);

        if (currentBeat == lastCheckedBeat) return;
        lastCheckedBeat = currentBeat;

        int targetBeatIndex = currentBeat + 3;

        if (targetBeatIndex == lastVisualizedBeat)
            return;

        BeatData beat = judge.composer.GetBeat(targetBeatIndex);
        if (beat == null) return;

        lastVisualizedBeat = targetBeatIndex;

        if (beat.type == BeatType.Tap)
        {
            tapVisual.HandleTapVisual(metronome.beatDurationInMS);
        }
        else if (beat.type == BeatType.Hold)
        {
            holdVisual.HandleHoldVisual(beat, metronome.beatDurationInMS);
        }
    }

    private void StartGame()
    {
        musicPlayer.PlaySong(musicId);
        metronome.InitMetronome();
        gameRunning = true;
    }

    private void OnPlayerPressed(int playerId)
    {
        if (!gameRunning) return;

        var songPosMs = musicPlayer.GetSongPositionInMS();
        judge.CheckInput(songPosMs, true);
    }

    private void OnPlayerReleased(int playerId)
    {
        if (!gameRunning) return;

        var songPosMs = musicPlayer.GetSongPositionInMS();
        var beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        var beat = judge.composer.GetBeat(beatIndex);
        if (beat == null) return;
        
        if (beat.type == BeatType.Hold)
        {
            judge.CheckInput(songPosMs, false);
        }
    }
}