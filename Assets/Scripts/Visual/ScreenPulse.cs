using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(Camera))]
public class ScreenPulse : MonoBehaviour
{
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
    }

    // Beat pulse (low priority)
    public void PulseOnBeat()
    {
        if (held) return;
        TriggerPulse(maxZoom.First(a => a.priority == PulsePriority.Beat).zoomIn, zoomSpeed, 
            PulsePriority.Beat, 0.05f);
    }

    // Player hit (medium priority)
    public void HitPulse()
    {
        TriggerPulse(maxZoom.First(a => a.priority == PulsePriority.Player).zoomIn, zoomSpeed, 
            PulsePriority.Player, 0.1f);
    }

    // Hold (highest priority)
    public void HoldPulse(float holdDuration)
    {
        held = true;
        TriggerPulse(maxZoom.First(a => a.priority == PulsePriority.Hold).zoomIn, holdDuration, 
            PulsePriority.Hold, holdDuration);
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