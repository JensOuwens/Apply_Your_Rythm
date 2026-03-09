using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ComposerCreator : MonoBehaviour
{
    public static Action<Composer> ComposerSubscribed;
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
            ComposerSubscribed.Invoke(composer);
        }
    }
}
