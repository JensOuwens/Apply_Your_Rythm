using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(Camera))]
public class ScreenPulse : MonoBehaviour
{
    [SerializeField] private Judge judge;
    [SerializeField] private Camera cam;
    [Space]
    [SerializeField] private AnimationCurve zoomCurve;
    [SerializeField] private float defaultZoom = 5f;
    [SerializeField] private List<PulseZoom> maxZoom;
    [SerializeField] private float zoomSpeed = 0.1f;

    private float currentZoom;

    private float currentTime;
    private float currentDuration;
    private bool animating;
    private bool zoomingIn;

    private float releaseTime;
    private bool waitingForRelease;
    private bool held;
    
    private Coroutine currentHold;

    [Serializable]
    private struct PulseZoom
    {
        public PulsePriority priority;
        public float zoomIn;
    }

    private enum PulsePriority
    {
        Beat = 0,
        Player = 1,
        Hold = 2
    }

    private PulsePriority currentPriority;
    private float lockUntilTime;

    private void OnValidate()
    {
        cam ??= GetComponent<Camera>();
        if (cam)
            defaultZoom = cam.orthographicSize;
        judge = FindFirstObjectByType<Judge>();
    }

    // Beat pulse (low priority)
    public void PulseOnBeat()
    {
        if (held) return;
        TriggerPulse(maxZoom.First(a => a.priority == PulsePriority.Beat).zoomIn, zoomSpeed, 
            PulsePriority.Beat, 0.05f);
    }

    // Player hit (medium priority)
    public void HitPulse(float delay, float timingDiff) => 
        StartCoroutine(HitPulseDelayed(delay, timingDiff));

    private IEnumerator HitPulseDelayed(float delay, float timingDiff)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        var speedMultiplier = 1f;
        var holdDuration = zoomSpeed * speedMultiplier;

        if (timingDiff > 0f) // late
        {
            // compress animation so it catches up
            var lateSeconds = timingDiff / 1000f;
            speedMultiplier = Mathf.Clamp01(zoomSpeed / (zoomSpeed + lateSeconds));
            holdDuration = Mathf.Lerp(zoomSpeed, zoomSpeed * 0.5f, Mathf.Clamp01(lateSeconds * 10f));
        }

        TriggerPulse(
            maxZoom.First(a => a.priority == PulsePriority.Player).zoomIn,
            holdDuration,
            PulsePriority.Player,
            0.1f
        );
    }

    // Hold (highest priority)
    public void HoldPulse(float holdDuration, float delay, float timingDiff) =>
        currentHold = StartCoroutine(HoldPulseDelayed(holdDuration, delay, timingDiff));

    public void StopHoldPulse()
    {
        if (currentHold != null)
        {
            var timeRemaining = releaseTime - Time.time;
            Debug.Log($"{timeRemaining} <= {judge.ErrorMarginMs / 1000f}");
            if (timeRemaining <= judge.ErrorMarginMs / 1000f)
                return;
            
            StopCoroutine(currentHold);
        }
        currentHold = null;
        
        if (!held) return;
        held = false;
        waitingForRelease = false;
        zoomingIn = false;
        animating = true;
        currentTime = 0f;
        currentDuration = zoomSpeed;
        currentPriority = PulsePriority.Hold;
        lockUntilTime = Time.time + 0.05f;
    }
    
    private IEnumerator HoldPulseDelayed(float holdDuration, float delay, float timingDiff)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        held = true;

        var speedMultiplier = 1f;

        if (timingDiff > 0f)
        {
            var lateSeconds = timingDiff / 1000f;
            speedMultiplier = Mathf.Clamp01(zoomSpeed / (zoomSpeed + lateSeconds));
        }

        TriggerPulse(
            maxZoom.First(a => a.priority == PulsePriority.Hold).zoomIn,
            holdDuration * speedMultiplier,
            PulsePriority.Hold,
            holdDuration
        );
    }

    private void TriggerPulse(float zoom, float holdDuration, PulsePriority priority, float lockTime)
    {
        // priority gate
        if (Time.time < lockUntilTime && priority < currentPriority)
            return;

        // prevent weaker pulses interrupting zoom-out
        if (animating && !zoomingIn && priority <= currentPriority)
            return;

        currentPriority = priority;
        lockUntilTime = Time.time + lockTime;

        currentZoom = zoom;

        animating = true;
        zoomingIn = true;
        currentTime = 0f;
        currentDuration = zoomSpeed;

        waitingForRelease = true;
        releaseTime = Time.time + holdDuration;
    }

    private void Update()
    {
        // handle release
        if (waitingForRelease && Time.time >= releaseTime)
        {
            waitingForRelease = false;

            zoomingIn = false;
            currentTime = 0f;
            currentDuration = zoomSpeed;
        }

        if (!animating) return;

        var t = Mathf.Clamp01(currentTime / currentDuration);
        var curve = zoomCurve.Evaluate(t);

        if (zoomingIn)
            cam.orthographicSize = math.lerp(defaultZoom, currentZoom, curve);
        else
            cam.orthographicSize = math.lerp(currentZoom, defaultZoom, curve);

        currentTime += Time.deltaTime;

        if (!(t >= 1f) || zoomingIn) return;
        animating = false;
        held = false;
    }
}