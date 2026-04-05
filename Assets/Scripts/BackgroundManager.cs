using System;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer background;
    [SerializeField]
    private Bucket[] buckets;
    private Composer composer;
    private int maxAmount;
    private int currentAmount = 0;
    private float alpha = 1;

    private void OnValidate()
    {
        background ??= GetComponent<SpriteRenderer>();
        buckets = FindObjectsByType<Bucket>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        alpha = 1;
        ComposerCreator.ComposerSubscribed += SubscribeComposer;
        foreach (var bucket in buckets)
            bucket.OnEmptyBucket += AddToAmount;
    }

    private void OnDisable()
    {
        ComposerCreator.ComposerSubscribed -= SubscribeComposer;
        foreach (var bucket in buckets)
            bucket.OnEmptyBucket -= AddToAmount;
    }

    private void SubscribeComposer(Composer obj) => maxAmount = obj.GetCount();

    public void AddToAmount(Int32 amount)
    {
        if (maxAmount <= 0 || amount <= 0)
            return;
        currentAmount += amount;
        var percentage = (float)currentAmount / maxAmount;
        alpha = 1f - Mathf.Clamp01(percentage * 2);
    }

    private void Update()
    {
        var color = background.color;
        color.a = Mathf.Lerp(color.a, alpha, Time.deltaTime);
        background.color = color;
    }
}
