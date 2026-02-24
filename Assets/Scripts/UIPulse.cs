using System;
using UnityEngine;

/// <summary>
/// A component class for making a rect transform pulsate back and forth, changing the size.
/// Simple animation that adds polish to static ui elements
/// </summary>
public class UIPulse : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private Vector3 minScale = Vector3.one * 0.9f;
    [SerializeField]
    private Vector3 maxScale = Vector3.one * 1.1f;

    [SerializeField]
    private RectTransform rectTransform;
    private float t;

    private void OnValidate()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();
        if (!rectTransform)
            enabled = false;
    }

    private void Update()
    {
        t += Time.unscaledDeltaTime * speed;
        var s = (Mathf.Sin(t) + 1f) * 0.5f;
        rectTransform.localScale = Vector3.Lerp(minScale, maxScale, s);
    }
}
