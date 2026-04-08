using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.Splines.Interpolators;

[RequireComponent(typeof(SplineAnimate))]
public class AnimateAlongSplineOnBeat : MonoBehaviour
{
    [Header("RequireComponents")]
    private Metronome metronome;

    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private SplineAnimate splineAnimate;

    [Header("Config")]
    [SerializeField] private bool shouldUseKnots = true;
    [SerializeField] private int[] unUsedStates;

    [Tooltip("Adds extra spline space after last state so it can move off-screen")]
    [SerializeField] private float endPadding = 0.1f;

    private float[] knotsInDistance;
    private List<int> validStates;

    public int currentIndex;

    [SerializeField] private AnimationCurve beatEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private LerpFloat lerpFloat;

    private float fromTime;
    private float toTime;
    private double transitionStartBeat;
    private bool isTransitioning;

    [Header("Beat counting")]
    [SerializeField] private int amountOfBeatsToCount = 1;
    private int beatCount;

    [Header("Events")]
    [SerializeField] private UnityEvent onLastState;
    [SerializeField] private UnityEvent onStartLoop;

    private int AmountOfStates =>
        splineAnimate && splineAnimate.Container != null
            ? splineAnimate.Container.Spline.Count
            : 0;

    private void OnValidate()
    {
        if (!splineAnimate)
            splineAnimate = GetComponent<SplineAnimate>();

        if (splineAnimate && splineAnimate.Container != null)
        {
            CalculateKnotsInDistance();
            BuildValidStates();
        }

        if (validStates != null && validStates.Count > 0)
            currentIndex = Mathf.Clamp(currentIndex, 0, validStates.Count - 1);
    }

    public void Initialize(Metronome metronome1)
    {
        metronome = metronome1;

        CalculateKnotsInDistance();
        BuildValidStates();

        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, validStates.Count - 1));

        if (validStates.Count > 0)
            SetState(validStates[currentIndex], true);
    }

    private void BuildValidStates()
    {
        validStates = Enumerable.Range(0, AmountOfStates)
            .Where(i => unUsedStates == null || !unUsedStates.Contains(i))
            .ToList();
    }

    public void OnMove()
    {
        beatCount++;

        if (beatCount < amountOfBeatsToCount)
            return;

        beatCount = 0;
        AdvanceState();
    }

    private void AdvanceState()
    {
        if (validStates == null || validStates.Count == 0)
            return;

        int previousIndex = currentIndex;

        currentIndex++;

        if (currentIndex >= validStates.Count)
        {
            currentIndex = 0;
            onStartLoop.Invoke();
        }

        SetState(validStates[currentIndex], false, previousIndex);
    }

    private void SetState(int stateToSet, bool instant = false, int previousIndexOverride = -1)
    {
        int previousIndex = previousIndexOverride >= 0 ? previousIndexOverride : currentIndex;

        int previousState = validStates[Mathf.Clamp(previousIndex, 0, validStates.Count - 1)];
        int newState = stateToSet;

        bool isLoopWrap = previousIndex == validStates.Count - 1 && currentIndex == 0;

        fromTime = PositionOnTimeLine(previousState);
        toTime = PositionOnTimeLine(newState);

        if (instant || isLoopWrap)
        {
            splineAnimate.NormalizedTime = toTime;
            isTransitioning = false;

            if (isLoopWrap)
                onStartLoop.Invoke();

            return;
        }

        transitionStartBeat = musicPlayer.GetSongPositionInMS() / metronome.beatDurationInMS;
        isTransitioning = true;
    }

    private void Update()
    {
        if (!splineAnimate ||
            !metronome ||
            !musicPlayer ||
            splineAnimate.Container == null ||
            splineAnimate.Container.Spline == null ||
            splineAnimate.Container.Spline.Count < 2)
            return;

        LerpPositionToState();
    }

    private void LerpPositionToState()
    {
        if (!isTransitioning)
            return;

        double currentBeat = musicPlayer.GetSongPositionInMS() / metronome.beatDurationInMS;
        float phase = (float)(currentBeat - transitionStartBeat);

        if (phase >= 1f)
        {
            splineAnimate.NormalizedTime = toTime;
            isTransitioning = false;
            return;
        }

        float curved = beatEasing.Evaluate(phase);
        splineAnimate.NormalizedTime = lerpFloat.Interpolate(fromTime, toTime, curved);
    }

    private float PositionOnTimeLine(int state)
    {
        if (validStates == null || validStates.Count == 0)
            return 0f;

        bool isLastState = state == validStates[validStates.Count - 1];

        if (shouldUseKnots)
        {
            float t = knotsInDistance[state];

            if (isLastState)
                return 1f + endPadding;

            return t;
        }

        int index = validStates.IndexOf(state);

        if (validStates.Count <= 1)
            return 0f;

        float normalized = index / (validStates.Count - 1f);

        if (isLastState)
            return 1f + endPadding;

        return normalized;
    }

    private void CalculateKnotsInDistance()
    {
        if (!splineAnimate || splineAnimate.Container == null || splineAnimate.Container.Spline == null)
            return;

        var spline = splineAnimate.Container.Spline;

        knotsInDistance = new float[spline.Count];

        float totalLength = spline.GetLength();

        for (int s = 0; s < spline.Count; s++)
        {
            float distanceToKnot = 0f;

            for (int i = 0; i < s; i++)
                distanceToKnot += spline.GetCurveLength(i);

            knotsInDistance[s] = Mathf.Clamp01(distanceToKnot / totalLength);
        }
    }
}