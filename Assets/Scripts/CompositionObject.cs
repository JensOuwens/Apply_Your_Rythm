using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Holds a sorted list of beats
/// </summary>
[CreateAssetMenu(fileName = "Composition", menuName = "ScriptableObjects/CompositionObject")]
public class CompositionObject : ScriptableObject
{
    /// <summary>
    /// Holds a beat instance within a composition, used for holding beat information such as:
    /// beat position,
    /// beat length,
    /// beat type
    /// </summary>
    [Serializable]
    public struct BeatData : IComparable
    {
        public float beatStart;
        public float beatLength;
        public BeatType type;

        public BeatData(float beatStart = 0, float beatLength = 1f, BeatType type = BeatType.Tap)
        {
            this.beatStart = beatStart;
            this.beatLength = beatLength;
            this.type = type;
        }

        public int CompareTo(object other)
        {
            if (other is BeatData otherBeatData)
                return beatStart.CompareTo(otherBeatData.beatStart);
            return -1;
        }
    }

    [Serializable]
    public enum BeatType
    {
        Tap,
        Hold,
    }
    
    public List<List<BeatData>> data = new();
    [SerializeField] 
    private int playerCount;

    private void OnValidate()
    {
        if (data.Count != playerCount)
            return;
        var temp = new List<List<BeatData>>(playerCount);
        temp.AddRange(data.GetRange(0, playerCount));
        data = temp;
    }
}