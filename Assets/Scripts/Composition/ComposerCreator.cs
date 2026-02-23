using System;
using Unity.VisualScripting;
using UnityEngine;

public class ComposerCreator : MonoBehaviour
{
    [SerializeField]
    private CompositionList compositionList;
    [SerializeField]
    private Judge judge;

    private void OnValidate()
    {
        judge = GetComponent<Judge>();
        enabled = judge && compositionList;
    }

    private void Start()
    {
        foreach (var composition in compositionList.compositions)
        {
            var composer = transform.AddComponent<Composer>();
            composer.SetComposition(composition);
            judge.composers.Add(composer);
        }
    }
}
