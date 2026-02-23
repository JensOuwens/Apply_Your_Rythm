using System;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Splines.Interpolators;

/// <summary>
/// Moves object transform along a spline based on amount of set states, or knots within the spline
/// </summary>
[RequireComponent(typeof(SplineAnimate))]
public class AnimateAlongSplineOnBeat : MonoBehaviour
{
    [Header("RequireComponents")]
    [SerializeField]
    private Metronome metronome;
    [SerializeField]
    private MusicPlayer musicPlayer;
    [SerializeField]
    private SplineAnimate splineAnimate;
    [SerializeField]
    private bool shouldUseKnots;
    private float[] knotsInDistance;
    [Header("config")]
    [SerializeField]
    private int currentState;
    [SerializeField]
    private int amountOfStates;
    [SerializeField]
    private AnimationCurve beatEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private LerpFloat lerpFloat;
    private float fromTime;
    private float toTime;
    private double transitionStartBeat;
    private bool isTransitioning;
    [Header("Beat counting")]
    [SerializeField]
    private int amountOfBeatsToCount = 1;
    private int beatCount = 0;

    private void OnValidate()
    {
        if (!splineAnimate)
            splineAnimate = GetComponent<SplineAnimate>();
        enabled = splineAnimate && metronome && musicPlayer;
        
        if (shouldUseKnots && splineAnimate && splineAnimate.Container)
            amountOfStates = splineAnimate.Container.Spline.Count;
        currentState = math.clamp(currentState, 0, amountOfStates);
    }

    private void Start()
    {
        CalculateKnotsInDistance();
        if (!metronome)
            return;
        metronome.OnBeat.AddListener(OnBeat);
    }

    private void OnDisable() => metronome.OnBeat.RemoveListener(OnBeat);
    private void OnDestroy() => metronome.OnBeat.RemoveListener(OnBeat);

    private void OnBeat()
    {
        beatCount++;
        if (beatCount < amountOfBeatsToCount) return;
        beatCount = 0;
        AdvanceState();
    }

    private void AdvanceState()
    {
        var newState = (int)currentState + 1;
        if (newState > amountOfStates)
            SetState(0);
        else
            SetState(newState);
    }

    private void SetState(int stateToSet)
    {
        var previousState = currentState;

        fromTime = PositionOnTimeLine(previousState);
        currentState = stateToSet;
        toTime = PositionOnTimeLine(currentState);

        // Detect wrap: EndHidden -> StartHidden
        var isWrap = previousState == amountOfStates && currentState == 0;

        // teleport
        if (isWrap)
        {
            splineAnimate.NormalizedTime = toTime;
            isTransitioning = false;
            return;
        }

        transitionStartBeat = musicPlayer.GetSongPositionInMS() / metronome.BeatDurationMs;
        isTransitioning = true;
    }

    private void Update()
    {
        if (!splineAnimate ||
            !metronome ||
            !musicPlayer ||
            !splineAnimate.Container ||
            splineAnimate.Container.Spline == null ||
            splineAnimate.Container.Spline.Count < 2 ||
            splineAnimate.Container.Spline.GetLength() <= 0f)
            return;

        LerpPositionToState();
    }

    private void LerpPositionToState()
    {
        if (!isTransitioning)
            return;

        double currentBeat = musicPlayer.GetSongPositionInMS() / metronome.BeatDurationMs;
        var phase = (float)(currentBeat - transitionStartBeat);

        if (phase >= 1f)
        {
            splineAnimate.NormalizedTime = toTime;
            isTransitioning = false;
            return;
        }

        var curved = beatEasing.Evaluate(phase);
        splineAnimate.NormalizedTime = lerpFloat.Interpolate(fromTime, toTime, curved);
    }

    private float PositionOnTimeLine(int s)
    {
        if (!shouldUseKnots)
            return Mathf.Clamp01((float)s / amountOfStates);
        // calculate knotPosition along the spline
        var spline = splineAnimate.Container.Spline;

        if (s <= 0 || s >= spline.Count)
            return math.clamp(s, 0, 1);
        
        return knotsInDistance[s];
    }
    
    private void CalculateKnotsInDistance()
    {
        knotsInDistance = new float[amountOfStates];
        if (!splineAnimate || !splineAnimate.Container || splineAnimate.Container.Spline == null)
            return;
        
        var spline = splineAnimate.Container.Spline;
        var totalLength = spline.GetLength();

        for (var s = 0; s < knotsInDistance.Length; s++)
        {
            var distanceToKnot = 0f;
            for (var i = 0; i < s; i++) 
                distanceToKnot += spline.GetCurveLength(i);

            knotsInDistance[s] =  Mathf.Clamp01(distanceToKnot / totalLength);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (shouldUseKnots && !splineAnimate && !splineAnimate.Container && splineAnimate.Container.Spline == null)
            return;
        
        Gizmos.color = Color.red;
        foreach (var knot in splineAnimate.Container.Spline)
        {
            Gizmos.DrawSphere(knot.Position, 0.1f);
        }
    }
}
