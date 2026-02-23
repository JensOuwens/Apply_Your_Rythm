using System;
using System.Collections;
using UnityEngine;

public class GooberCatchAnim : MonoBehaviour
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite catchSprite;
    [SerializeField] private float durationAnim = 0.8f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Coroutine currentAnim;

    private void OnValidate()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        enabled = spriteRenderer != null;
    }

    public void GooberAnim()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(GooberAnimCoroutine(durationAnim));
    }
    
    public void GooberHoldAnim(float duration)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(GooberAnimCoroutine(duration));
    }

    private IEnumerator GooberAnimCoroutine(float duration)
    {
        spriteRenderer.sprite = catchSprite;
        yield return new WaitForSeconds(duration);
        spriteRenderer.sprite = originalSprite;
        currentAnim = null;
    }
}