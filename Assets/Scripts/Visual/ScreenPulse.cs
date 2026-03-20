using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenPulse : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [Space]
    [SerializeField] private AnimationCurve zoomCurve;
    [SerializeField] private float defaultZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float zoomSpeed;
    private bool animating;
    private bool direction = true;
    private float currentTime = 0;
    private float currentDuration;

    private void OnValidate()
    {
        cam ??= GetComponent<Camera>();
        enabled = cam;
        if (cam)
            defaultZoom = cam.orthographicSize;
    }

    public void Pulse()
    {
        animating = true;
        direction = true;
        cam.orthographicSize = defaultZoom;
        currentTime = 0;
        currentDuration = zoomSpeed;
        StartCoroutine(InverseDirection(zoomSpeed));
    }

    public void HoldPulse(float holdDurationSec)
    {
        animating = true;
        direction = true;
        cam.orthographicSize = defaultZoom;
        currentTime = 0;
        currentDuration = zoomSpeed;
        StartCoroutine(InverseDirection(holdDurationSec));
    }

    private IEnumerator InverseDirection(float time)
    {
        yield return new WaitForSeconds(time);
        
        animating = true;
        direction = false;
        cam.orthographicSize = maxZoom;
        currentTime = 0;
        currentDuration = zoomSpeed;
    }
    
    private void Update()
    {
        if (!animating)
            return;

        var t = Mathf.Clamp01(currentTime / currentDuration);
        var pos = zoomCurve.Evaluate(t);

        if (direction)
            cam.orthographicSize = math.lerp(defaultZoom, maxZoom, pos);
        else
            cam.orthographicSize = math.lerp(maxZoom, defaultZoom, pos);

        if (t >= 1f)
            animating = false;

        currentTime += Time.deltaTime;
    }
}
