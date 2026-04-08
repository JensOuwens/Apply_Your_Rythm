using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrganismAnim : MonoBehaviour
{
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int JumpSpeed = Animator.StringToHash("JumpSpeed");
    private static readonly int IdleSpeed = Animator.StringToHash("IdleSpeed");
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite catchSprite;
    [SerializeField] private Sprite blinkgOriginalSprite;
    [SerializeField] private Sprite blinkCatchSprite;
    [SerializeField] private GameObject hands;
    private Sprite currentIdleSprite;
    private Sprite currentcatchSprite;
    [SerializeField] private int blinkChance;
    [SerializeField] private float blinkTimeInSeconds;
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

    private void Awake()
    {
        currentIdleSprite = originalSprite;
        currentcatchSprite = catchSprite;
        
        InvokeRepeating("blinkLogic", 1, 1);
        
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
        spriteRenderer.sprite = currentcatchSprite;
        hands.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        spriteRenderer.sprite = currentIdleSprite;
        hands.gameObject.SetActive(false);
        currentAnim = null;
    }
    
    public void StopHoldAnim()
    {
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
            currentAnim = null;
        }

        spriteRenderer.sprite = currentIdleSprite;
        hands.gameObject.SetActive(false);
    }
    
    private void blinkLogic()
    {
        int chanceRoll = Random.Range(0, 100);

        if (blinkChance < chanceRoll) return;
        
        StartCoroutine(blinkTime());
    }

    IEnumerator blinkTime()
    {
        Debug.Log("working");
        currentIdleSprite = blinkgOriginalSprite;
        currentcatchSprite = blinkCatchSprite;
        ApplyCurrentSprite();
        
        yield return new WaitForSeconds(blinkTimeInSeconds);
        
        currentIdleSprite = originalSprite;
        currentcatchSprite =  catchSprite;
        ApplyCurrentSprite();
    }
    
    private void ApplyCurrentSprite()
    {
        if (currentAnim == null)
        {
            spriteRenderer.sprite = currentIdleSprite;
            
        }
        else
        {
            spriteRenderer.sprite = currentcatchSprite;
            
        }
        
    }
}