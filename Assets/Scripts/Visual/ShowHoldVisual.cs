using System.Collections;
using UnityEngine;

public class ShowHoldVisual : MonoBehaviour
{
    [SerializeField] private GameObject holdHeadPrefab; // The head prefab
    [SerializeField] private Transform[] positions;     // Top-to-bottom positions

    public void HandleHoldVisual(BeatData beat, float beatDurationInMS)
    {
        var beatDelay = beatDurationInMS / 1000f;
        StartCoroutine(SpawnAndMoveHead(beat, beatDelay));
    }

    private IEnumerator SpawnAndMoveHead(BeatData beat, float beatDelay)
    {
        // Wait spawnOffset beats before showing the head
        yield return new WaitForSeconds(beatDelay);

        // Spawn head at top
        var head = Instantiate(holdHeadPrefab, positions[0].position, Quaternion.identity);

        var totalPositions = positions.Length;
        var holdLength = beat.beatEnd - beat.beatStart + 1;

        // Determine how many beats per step so the head reaches bottom exactly at beatEnd
        var beatsPerStep = holdLength / (float)(totalPositions - 1);

        var elapsedBeats = 0f;

        for (var step = 1; step < totalPositions; step++)
        {
            var waitTime = beatsPerStep * beatDelay;
            yield return new WaitForSeconds(waitTime);

            // Move head down one position
            head.transform.position = positions[step].position;

            elapsedBeats += beatsPerStep;
        }

        // Keep at last position for one beat then destroy
        yield return new WaitForSeconds(beatDelay);
        Destroy(head);
    }
}