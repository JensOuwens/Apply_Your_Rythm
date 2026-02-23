using System.Collections;
using UnityEngine;

public class HoldVisual : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;

    private float beatDurationSec;
    private Transform[] positions;

    public void Init(int beatStart, int beatEnd, int spawnOffset, float beatDurationInMS, Transform[] positions)
    {
        this.positions = positions;
        this.beatDurationSec = beatDurationInMS / 1000f;

        if (body != null) body.gameObject.SetActive(true);
        if (head != null) head.gameObject.SetActive(true);
        if (tail != null) tail.gameObject.SetActive(true);

        StartCoroutine(Run(beatStart, beatEnd, spawnOffset));
    }

    private IEnumerator Run(int beatStart, int beatEnd, int spawnOffset)
    {
        var holdLength = beatEnd - beatStart;
        var initialDelay = spawnOffset * beatDurationSec;

        // Wait before spawning to match spawnOffset
        yield return new WaitForSeconds(initialDelay);

        var startPos = positions[0].position;
        var endPos = positions[positions.Length - 1].position;

        var elapsed = 0f;
        var totalDuration = holdLength * beatDurationSec;

        while (elapsed <= totalDuration)
        {
            var t = Mathf.Clamp01(elapsed / totalDuration);

            // Move head smoothly from top to bottom
            if (head != null)
                head.position = Vector3.Lerp(startPos, endPos, t);

            // Stretch body from head → tail
            if (body != null && head != null && tail != null)
            {
                var bodyPos = (head.position + tail.position) / 2f;
                body.position = bodyPos;

                var scale = body.localScale;
                scale.y = Mathf.Abs(tail.position.y - head.position.y) / 2f; // scale.y is half-distance if pivot is center
                body.localScale = scale;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide body and tail at the end
        if (body != null) body.gameObject.SetActive(false);
        if (tail != null) tail.gameObject.SetActive(false);

        Destroy(gameObject);
    }
}