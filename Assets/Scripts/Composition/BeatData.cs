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
    public BeatAttribute attribute;
    [NonSerialized]
    public bool hit = false;

    public BeatData(int beatStart = 0, int beatEnd = 1, BeatType type = BeatType.Tap, BeatAttribute attribute = BeatAttribute.Water)
    {
        this.beatStart = beatStart;
        this.beatEnd = beatEnd;
        this.type = type;
        this.attribute = attribute;
    }
    public BeatData(BeatData other) : this(other.beatStart, other.beatEnd, other.type, other.attribute) { }

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

[Serializable]
public enum BeatAttribute
{
    Water,
    Co2,
}