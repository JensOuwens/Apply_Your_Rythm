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
    [SerializeField] private GooberCatchAnim gooberCatchAnim;

    [Header("Timing")]
    [SerializeField] private float errorMarginMs = 80f; // milliseconds window

    [Header("Stats")]
    [SerializeField] private int correctInputs = 0;
    [SerializeField] private int incorrectInputs = 0;

    public void CheckInput(float songPosMs, bool pressed)
    {
        if (composer == null || metronome == null || !metronome.initialized)
            return;

        int beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        float beatTimeMs = beatIndex * metronome.beatDurationInMS;

        // Timing check
        if (Mathf.Abs(songPosMs - beatTimeMs) > errorMarginMs)
        {
            incorrectInputs++;
            return;
        }

        BeatData beat = composer.GetBeat(beatIndex);
        if (beat == null || beat.hit)
        {
            incorrectInputs++;
            return;
        }

        beat.hit = true;
        correctInputs++;
        
        if (beat.type == BeatType.Tap)
        {
            gooberCatchAnim.GooberAnim();
        }
        else if (beat.type == BeatType.Hold)
        {
            float holdDurationSec = (beat.beatEnd - beat.beatStart + 1) * (metronome.beatDurationInMS / 1000f);
            gooberCatchAnim.GooberHoldAnim(holdDurationSec);
        }
    }

    public int GetCorrectCount() => correctInputs;
    public int GetIncorrectCount() => incorrectInputs;
}