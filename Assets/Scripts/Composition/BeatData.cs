using System;

/// <summary>
/// Holds a beat instance within a composition, used for holding beat information such as:
/// beat position,
/// beat length,
/// beat type
/// </summary>
[Serializable]
public class BeatData : IComparable
{
    public int beatStart;
    public int beatEnd;
    public BeatType type;
    [NonSerialized]
    public bool hit = false;

    public BeatData(int beatStart = 0, int beatEnd = 1, BeatType type = BeatType.Tap)
    {
        this.beatStart = beatStart;
        this.beatEnd = beatEnd;
        this.type = type;
    }
    public BeatData(BeatData other) : this(other.beatStart, other.beatEnd, other.type) { }

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