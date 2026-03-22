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
    private ParticleManager beatParticleManager;
    [SerializeField]
    private ParticleManager failParticleManager;
    [SerializeField]
    private SoundEffectManager soundEffectManager;
    [SerializeField]
    private ScreenPulse screenPulse;

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
    }

    private void OnEnable() => ComposerCreator.ComposerSubscribed += SubscribeComposer;
    private void OnDisable() => ComposerCreator.ComposerSubscribed -= SubscribeComposer;

    public void SubscribeComposer(Composer composer) => composers.Add(composer);

    public void CheckInput(float songPosMs, int playerId, bool pressed)
    {
        if (composers == null || metronome == null || !metronome.initialized)
            return;

        if (!pressed)
        {
            // RELEASED: stop any active hold
            if (!activeHolds.Remove(playerId, out _)) return;
            organismManager.StopHold(playerId);
            beatParticleManager.StopHoldParticles(playerId);
            soundEffectManager.StopHoldSound(playerId);
            return;
        }

        var beatIndex = metronome.GetNearestBeat(songPosMs);
        if (beatIndex < 0) return;

        var beat = composers[playerId].GetBeat(beatIndex);
        if (beat == null || beat.hit)
        {
            if (beat == null)
                failParticleManager.SpawnParticleWithIndexIDPos(playerId, 0); // beat has passed, so considered too late
            incorrectInputs++;
            return;
        }
        
        var beatTimeMs = beatIndex * metronome.beatDurationInMS;
        var timingDiff = songPosMs - beatTimeMs;
            
        if (Mathf.Abs(timingDiff) > errorMarginMs)
        {
            if (timingDiff < 0)
                failParticleManager.SpawnParticleWithIndexIDPos(playerId, 0); // Too late particle
            else
                failParticleManager.SpawnParticleWithIndexIDPos(playerId, 1); // Too early particle
            incorrectInputs++;
            return;
        }

        beat.hit = true;
        if (beat.attribute == BeatAttribute.Water) correctInputsWater++;
        else if (beat.attribute == BeatAttribute.Co2) correctInputsCo2++;

        var timeToNextBeat = (beatIndex + 1) * metronome.beatDurationInMS - songPosMs;

        if (beat.type == BeatType.Tap)
        {
            organismManager.TriggerAnim(playerId, timeToNextBeat / 1000f);
            beatParticleManager.SpawnRandomParticleIDPos(playerId);
            soundEffectManager.PlayRandomSoundEffect();
            screenPulse.HitPulse();
        }
        else if (beat.type == BeatType.Hold)
        {
            // Only start if not already active
            if (!activeHolds.TryAdd(playerId, beat)) return;
            var holdDurationSec = (beat.beatEnd - beat.beatStart) * (metronome.beatDurationInMS / 1000f) - timingDiff / 1000f;

            organismManager.TriggerAnimHold(playerId, beat, holdDurationSec);
            beatParticleManager.SpawnRandomParticleIDPosHold(playerId, beat, holdDurationSec);
            soundEffectManager.PlaySoundEffectWithIndexHold(0, beat, playerId, holdDurationSec);
            screenPulse.HoldPulse(holdDurationSec);
        }
    }
}