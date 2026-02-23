using UnityEngine;

/// <summary>
/// Using the composer and metronome judges a player's beat position,
/// and keeps track of these judgments
/// </summary>
public class Judge : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public Composer composer;
    [SerializeField] private Metronome metronome;

    [Header("Timing")]
    [SerializeField] private float errorMarginMs = 80f; // milliseconds window

    [Header("Stats")]
    [SerializeField] private int correctInputs = 0;
    [SerializeField] private int incorrectInputs = 0;

    public void CheckInput(float songPosMs, bool b)
    {
        if (composer == null || metronome == null)
            return;

        if (!metronome.initialized)
            return;

        var beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0)
            return;

        var beatTimeMs = beatIndex * metronome.beatDurationInMS;

        // Timing check
        if (Mathf.Abs(songPosMs - beatTimeMs) > errorMarginMs)
        {
            incorrectInputs++;
            return;
        }

        var beat = composer.GetBeat(beatIndex);

        // No beat at this index
        if (beat == null)
        {
            incorrectInputs++;
            return;
        }

        // Already hit
        if (beat.hit)
        {
            incorrectInputs++;
            return;
        }
        
        

        beat.hit = true;
        correctInputs++;
    }

    public int GetCorrectCount() => correctInputs;
    public int GetIncorrectCount() => incorrectInputs;
}