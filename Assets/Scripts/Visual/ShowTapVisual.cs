using System.Collections;
using UnityEngine;

public class ShowTapVisual : MonoBehaviour
{
    [SerializeField] private GameObject visualPrefabWater;
    [SerializeField] private GameObject visualPrefabCo2;
    [SerializeField] private Transform[] tapLocations;

    public void HandleTapVisual(BeatData beat, float beatDurationInMS)
    {
        StartCoroutine(TapCoroutine(beat, beatDurationInMS));
    }

    IEnumerator TapCoroutine(BeatData beat, float beatDurationInMS)
    {
        var delay = beatDurationInMS / 1000f;

        for (var i = 0; i < tapLocations.Length; i++)
        {
            var instance = Instantiate(
                beat.attribute == BeatAttribute.Water ? visualPrefabWater : visualPrefabCo2,
                tapLocations[i].position,
                Quaternion.identity
            );

            yield return new WaitForSeconds(delay);

            Destroy(instance);
        }
    }
}