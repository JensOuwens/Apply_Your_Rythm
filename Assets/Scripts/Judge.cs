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
    public float ErrorMarginMs => errorMarginMs;

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
            screenPulse.StopHoldPulse();
            return;
        }

        var baseIndex = metronome.GetBeat(songPosMs); // floor

        BeatData bestBeat = null;
        int bestIndex = -1;
        float bestDiff = float.MaxValue; 
        
        // check current + neighbors
        for (var i = -1; i <= 1; i++)
        {
            var index = baseIndex + i;
            if (index < 0) continue;

            var candidate = composers[playerId].GetBeat(index);
            if (candidate == null || candidate.hit) continue;

            var beatTime = index * metronome.beatDurationInMS;
            var diff = songPosMs - beatTime;

            var abs = Mathf.Abs(diff);
            if (abs < bestDiff)
            {
                bestDiff = abs;
                bestBeat = candidate;
                bestIndex = index;
            }
        }
        if (bestBeat == null || bestDiff > errorMarginMs * 2) // no valid beat nearby → ignore input completely
            return;
        
        var beatTimeMs = bestIndex  * metronome.beatDurationInMS;
        var timingDiff = songPosMs - beatTimeMs;
            
        if (Mathf.Abs(timingDiff) > errorMarginMs || bestBeat.hit)
        {
            if (timingDiff < 0)
                failParticleManager.SpawnParticleWithIndexIDPos(playerId, 1); // Too early particle
            else
                failParticleManager.SpawnParticleWithIndexIDPos(playerId, 0); // Too late particle
            incorrectInputs++;
            return;
        }

        bestBeat.hit = true;
        if (bestBeat.attribute == BeatAttribute.Water) correctInputsWater++;
        else if (bestBeat.attribute == BeatAttribute.Co2) correctInputsCo2++;

        var timeToNextBeat = (bestIndex  + 1) * metronome.beatDurationInMS - songPosMs;

        var maxDelay = errorMarginMs / 1000f;
        var delay = Mathf.Clamp(-timingDiff / 1000f, 0f, maxDelay);
        if (bestBeat.type == BeatType.Tap)
        {
            organismManager.TriggerAnim(playerId, timeToNextBeat / 1000f);
            beatParticleManager.SpawnRandomParticleIDPos(playerId);
            soundEffectManager.PlayRandomSoundEffect(playerId);
            screenPulse.HitPulse(delay, timingDiff);
        }
        else if (bestBeat.type == BeatType.Hold)
        {
            // Only start if not already active
            if (!activeHolds.TryAdd(playerId, bestBeat)) return;
            var holdDurationSec = (bestBeat.beatEnd - bestBeat.beatStart) * (metronome.beatDurationInMS / 1000f) - timingDiff / 1000f;

            organismManager.TriggerAnimHold(playerId, bestBeat, holdDurationSec);
            beatParticleManager.SpawnRandomParticleIDPosHold(playerId, bestBeat, holdDurationSec);
            soundEffectManager.PlaySoundEffectWithIndexHold(0, bestBeat, playerId, holdDurationSec);
            screenPulse.HoldPulse(holdDurationSec, delay, timingDiff);
        }
    }
}