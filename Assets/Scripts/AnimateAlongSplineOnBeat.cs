using System;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Splines.Interpolators;

[RequireComponent(typeof(SplineAnimate))]
public class AnimateAlongSplineOnBeat : MonoBehaviour
{
    [Header("RequireComponents")]
    [SerializeField]
    private SplineAnimate splineAnimate;
    [SerializeField]
    private Metronome metronome;
    [SerializeField]
    private MusicPlayer musicPlayer;
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
        
        currentState = math.clamp(currentState, 0, amountOfStates);
    }

    private void Start()
    {
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

    private float PositionOnTimeLine(int s) => Mathf.Clamp01((float)s / amountOfStates);
}
