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
    private MusicPlayer musicPlayer;
    [SerializeField] 
    private OrganismManager organismManager;
    [SerializeField]
    private ParticleManager particleManager;
    [SerializeField]
    private SoundEffectManager soundEffectManager;

    [Header("Timing")]
    [SerializeField] 
    private float errorMarginMs = 80f; // milliseconds window

    public static int correctInputsWater = 0;
    public static int correctInputsCo2 = 0;
    public static int incorrectInputs = 0;

    private Dictionary<int, BeatData> activeHolds = new();

    private void Start()
    {
        correctInputsWater = 0;
        correctInputsCo2 = 0;
        incorrectInputs = 0;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => ComposerCreator.ComposerSubscribed += SubscribeComposer;
    private void OnDisable() => ComposerCreator.ComposerSubscribed -= SubscribeComposer;

    public void SubscribeComposer(Composer composer) => composers.Add(composer);

    public void CheckInput(float songPosMs, int playerId, bool pressed)
    {
        if (composers == null || metronome == null || !metronome.initialized)
            return;

        var beatIndex = metronome.GetBeat(songPosMs);
        if (beatIndex < 0) return;

        var beatTimeMs = beatIndex * metronome.beatDurationInMS;

        var timingDiff = songPosMs - beatTimeMs;
        if (Mathf.Abs(timingDiff) > errorMarginMs)
        {
            incorrectInputs++;
            return;
        }

        var beat = composers[playerId].GetBeat(beatIndex);
        if (beat == null)
        {
            incorrectInputs++;
            return;
        }

        var timeToNextBeat = (beatIndex + 1) * metronome.beatDurationInMS - songPosMs;

        if (pressed)
        {
            if (beat.type == BeatType.Tap)
            {
                if (beat.hit)
                {
                    incorrectInputs++;
                    return;
                }

                beat.hit = true;

                if (beat.attribute == BeatAttribute.Water)
                    correctInputsWater++;
                else if (beat.attribute == BeatAttribute.Co2)
                    correctInputsCo2++;

                organismManager.TriggerAnim(playerId, timeToNextBeat / 1000f);
                particleManager.SpawnRandomParticleIDPos(playerId);
                soundEffectManager.PlaySoundEffectWithIndex(0);
            }
            else if (beat.type == BeatType.Hold)
            {
                if (beatIndex != beat.beatStart)
                {
                    incorrectInputs++;
                    return;
                }

                if (activeHolds.ContainsKey(playerId))
                    return;

                activeHolds[playerId] = beat;
                beat.hit = true;

                if (beat.attribute == BeatAttribute.Water)
                    correctInputsWater++;
                else if (beat.attribute == BeatAttribute.Co2)
                    correctInputsCo2++;

                organismManager.TriggerAnimHold(playerId, beat);
                particleManager.SpawnRandomParticleIDPosHold(playerId, beat);
                soundEffectManager.PlaySoundEffectWithIndexHold(1, beat, playerId);
            }
        }
        else
        {
            if (!activeHolds.TryGetValue(playerId, out var holdBeat))
                return;

            activeHolds.Remove(playerId);

            organismManager.StopHold(playerId);
            particleManager.StopHoldParticles(playerId);
            soundEffectManager.StopHoldSound(playerId);
        }
    }
}