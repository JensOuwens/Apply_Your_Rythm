using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds a sorted list of beats
/// </summary>
[CreateAssetMenu(fileName = "Composition", menuName = "ScriptableObjects/CompositionObject")]
public class CompositionObject : ScriptableObject
{
    [SerializeField] 
    private int beatAmount;
    public List<BeatData> data = new();
    [SerializeField]
    public bool showVisual;

    public List<BeatData?> BuildVisualList()
    {
        var result = new List<BeatData?>(beatAmount);
        for (var i = 0; i < beatAmount; i++)
        {
            if (i >= result.Count)
                result.Add(null);
            else
                result[i] = null;
        }
        
        if (beatAmount == 0)
            return result;

        foreach (var beat in data)
        {
            if (beat.type == BeatType.Tap && beat.beatStart < result.Count && beat.beatStart >= 0)
            {
                result[beat.beatStart] = beat;
                continue;
            }
            for (var i = beat.beatStart; i <= beat.beatEnd && i < result.Count; i++)
                if (i < result.Count && i >= 0)
                    result[i] = beat;
        }

        return result;
    }
}