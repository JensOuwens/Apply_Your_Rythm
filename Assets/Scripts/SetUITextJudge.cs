using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple component that takes the judge values to render them in the end scene
/// </summary>
public class SetUITextJudge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textJudge;

    private void OnValidate()
    {
        textJudge = GetComponent<TextMeshProUGUI>();
        enabled = textJudge;
    }

    private void Start() => textJudge.text = $"Water placed: {Judge.correctInputsWater}\nCo2 placed: {Judge.correctInputsCo2}";
}
