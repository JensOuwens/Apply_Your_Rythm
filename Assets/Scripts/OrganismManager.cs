using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class OrganismManager : MonoBehaviour
{
    [SerializeField]
    private SplineContainer spline;
    [SerializeField] 
    private OrganismAnim[] catchAnims;

    private void OnValidate()
    {
        if (spline == null)
            return;
        catchAnims = GetComponentsInChildren<OrganismAnim>();
    }

    public void TriggerAnim(int playerId, float animLength) => catchAnims[playerId].GooberAnim(animLength);
    public void TriggerAnimHold(int playerId, BeatData beat, Metronome metronome)
    {
        var holdDurationSec = (beat.beatEnd - beat.beatStart + 1) * (metronome.beatDurationInMS / 1000f);
        catchAnims[playerId].GooberHoldAnim(holdDurationSec);
    }

    public void InitOrganisms(Metronome metronome)
    {
        foreach (var organismAnim in catchAnims)
        {
            organismAnim.SetIdleSpeed(metronome.beatDurationInMS / 1000f);
        }
    }
}
