

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds a list of compositions
/// </summary>
[CreateAssetMenu(fileName = "CompositionList", menuName = "ScriptableObjects/CompositionList")]
public class CompositionList : ScriptableObject
{
    public List<CompositionObject> compositions;
    public bool showVisual;
}