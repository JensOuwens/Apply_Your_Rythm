using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Splines.Interpolators;

[RequireComponent(typeof(SplineAnimate))]
public class OrganismPositionManager : MonoBehaviour
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
    private PositionState state;
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

    [Serializable]
    public enum PositionState
    {
        StartHidden = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        EndHidden = 5,
    }

    #region Validation
    private void OnValidate()
    {
        if (!splineAnimate)
            splineAnimate = GetComponent<SplineAnimate>();
        enabled = splineAnimate && metronome && musicPlayer;

        if (!splineAnimate) return;
#if UNITY_EDITOR
        EditorApplication.delayCall += ApplyStateToSpline;
#endif
    }
    
#if UNITY_EDITOR
    private void ApplyStateToSpline()
    {
        if (this == null || !splineAnimate)
            return;

        if (!splineAnimate.isActiveAndEnabled)
            return;

        var container = splineAnimate.Container;
        if (container == null || container.Spline == null || container.Spline.Count < 2)
            return;

        if (container.Spline.GetLength() <= 0f)
            return;

        splineAnimate.NormalizedTime = PositionOnTimeLine(state);
    }
#endif
    #endregion

    private void Start()
    {
        if (!metronome)
            return;
        metronome.OnBeat.AddListener(OnBeat);
    }
    private void OnDisable() => metronome.OnBeat.RemoveListener(OnBeat);
    private void OnDestroy() => metronome.OnBeat.RemoveListener(OnBeat);

    public void OnBeat()
    {
        beatCount++;
        if (beatCount < amountOfBeatsToCount) return;
        beatCount = 0;
        AdvanceState();
    }
    
    public void AdvanceState()
    {
        var newState = (int)state + 1;
        if (newState > (int)PositionState.EndHidden)
            SetState(PositionState.StartHidden);
        else
            SetState((PositionState)newState);
    }
    public void SetState(PositionState stateToSet)
    {
        var previousState = state;

        fromTime = PositionOnTimeLine(previousState);
        state = stateToSet;
        toTime = PositionOnTimeLine(state);

        // Detect wrap: EndHidden -> StartHidden
        var isWrap =
            previousState == PositionState.EndHidden &&
            state == PositionState.StartHidden;

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

    private static float PositionOnTimeLine(PositionState s)
    {
        const int max = (int)PositionState.EndHidden;
        return Mathf.Clamp01((float)s / max);
    }
}
