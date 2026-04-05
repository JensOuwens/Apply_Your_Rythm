using UnityEngine;

/// <summary>
/// Used for managing a composition, making you able to retrieve what type of beat is pressed when
/// </summary>
public class Composer : MonoBehaviour
{
    [SerializeField]
    private CompositionObject composition;
    private BeatData[] beatData;

    private void Start() => beatData = composition.BuildVisualList().ToArray();
    
    /// <param name="beatPos">position in song</param>
    /// <returns>null if no beat exists or if position is out of range</returns>
    public BeatData GetBeat(int beatPos)
    {
        if (beatPos >= beatData.Length || beatPos < 0)
            return null;
        return beatData[beatPos];
    }

    public void SetComposition(CompositionObject compositionObject) => composition = compositionObject;
    public int GetCount() => composition.BuildVisualList().ToArray().Length;
}
