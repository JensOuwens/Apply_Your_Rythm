using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Using the composer and metronome judges a player's beat position,
/// and keeps track of these judgments
/// </summary>
public class Judge : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public List<Composer> composers = new();
    [SerializeField] 
    private Metronome metronome;
    [SerializeField] 
    private OrganismManager organismManager;

    [Header("Timing")]
    [SerializeField] 
    private float errorMarginMs = 80f; // milliseconds window

    public static int correctInputsWater = 0;
    public static int correctInputsCo2 = 0;
    public static int incorrectInputs = 0;

    private void Start()
    {
        correctInputsWater = 0;
        correctInputsCo2 = 0;
        incorrectInputs = 0;
        DontDestroyOnLoad(gameObject);
    }

    public void CheckInput(float songPosMs, int playerId, bool pressed)
    {
        if (composers == null || metronome == null || !metronome.initialized)
            return;

        var beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        var beatTimeMs = beatIndex * metronome.beatDurationInMS;

        // Timing check
        if (Mathf.Abs(songPosMs - beatTimeMs) > errorMarginMs)
        {
            incorrectInputs++;
            return;
        }

        var beat = composers[playerId].GetBeat(beatIndex);
        if (beat == null || beat.hit)
        {
            incorrectInputs++;
            return;
        }

        beat.hit = true;
        if (beat.attribute == BeatAttribute.Water)
            correctInputsWater++;
        else if (beat.attribute == BeatAttribute.Co2)
            correctInputsCo2++;

        if (beat.type == BeatType.Tap)
            organismManager.TriggerAnim(playerId);
        else if (beat.type == BeatType.Hold)
            organismManager.TriggerAnimHold(playerId, beat, metronome);
    }
}