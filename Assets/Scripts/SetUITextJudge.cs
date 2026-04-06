using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple component that takes the judge values to render them in the end scene
/// </summary>
public class SetUITextJudge : MonoBehaviour
{
    [SerializeField] private CustomCounter score;
    [SerializeField] private CustomCounter maxAmount;

    [SerializeField] private Sprite[] numbers;

    [SerializeField] private float maxAnimTime = 5f;
    private int currentCount = 0;
    
    [Serializable]
    private class CustomCounter
    {
        [SerializeField] public GameObject parent;
        [SerializeField] private Image[] images;

        public void OnValidate()
        {
            if (!parent)
                return;
            images = parent.GetComponentsInChildren<Image>();
        }
        
        public void SetValue(int value, Sprite[] numbers)
        {
            if (images == null || images.Length == 0 || numbers == null || numbers.Length < 10)
                return;

            var str = value.ToString();
            var imgIndex = images.Length - 1;

            // fill from right to left
            for (var i = str.Length - 1; i >= 0 && imgIndex >= 0; i--, imgIndex--)
            {
                var digit = str[i] - '0';
                var img = images[imgIndex];
                img.sprite = numbers[digit];
            }

            // remaining -> set to 0 and active
            for (; imgIndex >= 0; imgIndex--)
            {
                var img = images[imgIndex];
                img.sprite = numbers[0];
            }
        }
    }

    private void OnValidate()
    {
        score.OnValidate();
        maxAmount.OnValidate();
        if (numbers.Length != 10)
        {
            var temp = new Sprite[10];
            Array.Copy(numbers, 0, temp, 0, numbers.Length > 10 ? 10 : numbers.Length);
            numbers = temp;
        }

        var sprites = new List<Sprite>();
        foreach (var sprite in numbers)
        {
            if (sprite == null || sprites.Contains(sprite))
            {
                enabled = false;
                return;
            }

            sprites.Add(sprite);
        }
        enabled = score.parent && maxAmount.parent;

        if (!enabled) return;
        score.SetValue(99, numbers);
        maxAmount.SetValue(123, numbers);
    }

    private void Start()
    {
        var max = Judge.maxAmount;
        maxAmount.SetValue(max, numbers);
        score.SetValue(0, numbers);
    }

    public void TriggerCounting()
    {
        var scoreAmount = Judge.correctInputsWater + Judge.correctInputsCo2;
        StartCoroutine(CountUpAnim(scoreAmount, 1, maxAnimTime / scoreAmount));
    }

    private IEnumerator CountUpAnim(int max, float delay, float speed)
    {
        yield return new WaitForSeconds(delay);
        currentCount++;
        
        if (currentCount >= max)
        {
            currentCount = max;
            score.SetValue(currentCount, numbers);
            yield break;
        }
        score.SetValue(currentCount, numbers);
        
        yield return new WaitForSeconds(speed);
        StartCoroutine(CountUpAnim(max, 0, speed));
    }
}
