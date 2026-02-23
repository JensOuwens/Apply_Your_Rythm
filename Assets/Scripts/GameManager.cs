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

        int tapBeatIndex = currentBeat + 3;

        if (tapBeatIndex == lastVisualizedBeat)
            return;

        BeatData beat = judge.composer.GetBeat(tapBeatIndex);
        if (beat == null) return;
        if (beat.type != BeatType.Tap) return;

        lastVisualizedBeat = tapBeatIndex;
        
        tapVisual.HandleTapVisual(metronome.beatDurationInMS);
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

        float songPosMs = musicPlayer.GetSongPositionInMS();
        judge.CheckInput(songPosMs, true);
    }

    private void OnPlayerReleased(int playerId)
    {
        if (!gameRunning) return;

        float songPosMs = musicPlayer.GetSongPositionInMS();
        int beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        BeatData beat = judge.composer.GetBeat(beatIndex);
        if (beat == null) return;
        
        if (beat.type == BeatType.Hold)
        {
            judge.CheckInput(songPosMs, false);
        }
    }
}