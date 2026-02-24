using System;
using TMPro;
using UnityEngine;

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
