using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Bucket manages the different components that makes up a bucket, letting movement be seperate and reusable still
/// </summary>
[RequireComponent(typeof(AnimateAlongSplineOnBeat))]
public class Bucket : MonoBehaviour
{
    private static readonly int IsFull = Animator.StringToHash("IsFull");

    [SerializeField]
    private AnimateAlongSplineOnBeat animateAlongSpline;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Animator animator;
    private OrganismManager parentManager;
    private OrganismAnim organismToFollow;
    public int CurrentState => animateAlongSpline.CurrentState;

    private void OnValidate()
    {
        animateAlongSpline ??= GetComponent<AnimateAlongSplineOnBeat>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Initialize(Metronome metronome, OrganismManager organismManager)
    {
        parentManager = organismManager;
        parentManager.onMoveBucket += animateAlongSpline.OnMove;
        animateAlongSpline.Initialize(metronome);
    }
    
    private void OnDisable() => parentManager.onMoveBucket -= animateAlongSpline.OnMove;
    
    private void Update()
    {
        if (!organismToFollow) return;
        transform.position = organismToFollow.transform.position;
    }

    public void FollowOrganism(OrganismAnim catchAnim, float duration)
    {
        organismToFollow = catchAnim;
        StartCoroutine(FollowOrganism(duration));
    }

    private IEnumerator FollowOrganism(float duration)
    {
        yield return new WaitForSeconds(duration);
        organismToFollow = null;
    }

    public void Fill() => animator.SetBool(IsFull, true);

    public void OnLastBeat() => spriteRenderer.enabled = false;

    public void OnStartLoop()
    {
        animator.SetBool(IsFull, false);
        spriteRenderer.enabled = true;
    }
}