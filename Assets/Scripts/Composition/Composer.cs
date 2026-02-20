using UnityEngine;

/// <summary>
/// Used for managing a composition, making you able to retrieve what type of beat is pressed when
/// </summary>
public class Composer : MonoBehaviour
{
    [SerializeField]
    private CompositionObject composition;
    private BeatData?[] beatData;

    private void Start() => beatData = composition.BuildVisualList().ToArray();

    public BeatData? GetBeat(int beatPos)
    {
        if (beatPos >= beatData.Length || beatPos < 0)
            return null;
        return beatData[beatPos];
    }
}
