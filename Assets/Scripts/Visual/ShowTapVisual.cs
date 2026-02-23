using System.Collections;
using UnityEngine;

public class ShowTapVisual : MonoBehaviour
{
    [SerializeField] private GameObject tapVisualPrefab;
    [SerializeField] private Transform[] tapLocations;

    public void HandleTapVisual(float beatDurationInMS)
    {
        StartCoroutine(TapCoroutine(beatDurationInMS));
    }

    IEnumerator TapCoroutine(float beatDurationInMS)
    {
        var delay = beatDurationInMS / 1000f;

        for (var i = 0; i < tapLocations.Length; i++)
        {
            var instance = Instantiate(
                tapVisualPrefab,
                tapLocations[i].position,
                Quaternion.identity
            );

            yield return new WaitForSeconds(delay);

            Destroy(instance);
        }
    }
}