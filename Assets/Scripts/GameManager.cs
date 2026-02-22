using UnityEngine;

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
        judge.CheckInput(songPosMs);
    }

    private void OnPlayerReleased(int playerId)
    {
        if (!gameRunning) return;

        float songPosMs = musicPlayer.GetSongPositionInMS();
        judge.CheckInput(songPosMs);
    }
}