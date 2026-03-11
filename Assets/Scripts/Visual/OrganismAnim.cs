using System;
using System.Collections;
using UnityEngine;

public class OrganismAnim : MonoBehaviour
{
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int JumpSpeed = Animator.StringToHash("JumpSpeed");
    private static readonly int IdleSpeed = Animator.StringToHash("IdleSpeed");
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite catchSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Coroutine currentAnim;
    [Header("movementAnim")]
    [SerializeReference] private AnimationClip idleAnim;
    [SerializeReference] private AnimationClip jumpAnim;
    [SerializeReference] private Animator animator;

    private void OnValidate()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        enabled = spriteRenderer != null;
    }

    private static float GetNeededSpeedMult(AnimationClip clip, float targetDuration)
    {
        var lengthAnim = clip.length;
        return lengthAnim / targetDuration; //lengthAnim / 100 * (lengthAnim - targetDuration) + 1;
    }

    public void SetIdleSpeed(float beatDurationInMS)
    {
        var speedMultiplier = GetNeededSpeedMult(idleAnim, beatDurationInMS);
        animator.SetFloat(IdleSpeed, speedMultiplier);
    }

    public void GooberAnim(float animLength)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        animator.SetTrigger(Jump);
        var speedMultiplier = GetNeededSpeedMult(jumpAnim, animLength);
        animator.SetFloat(JumpSpeed, speedMultiplier);
        
        currentAnim = StartCoroutine(GooberAnimCoroutine(animLength));
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
    
    public void StopHoldAnim()
    {
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
            currentAnim = null;
        }

        spriteRenderer.sprite = originalSprite;
    }
}