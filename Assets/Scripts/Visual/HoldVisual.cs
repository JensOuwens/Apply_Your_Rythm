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
        int holdLength = beatEnd - beatStart;
        float initialDelay = spawnOffset * beatDurationSec;

        // Wait before spawning to match spawnOffset
        yield return new WaitForSeconds(initialDelay);

        Vector3 startPos = positions[0].position;
        Vector3 endPos = positions[positions.Length - 1].position;

        float elapsed = 0f;
        float totalDuration = holdLength * beatDurationSec;

        while (elapsed <= totalDuration)
        {
            float t = Mathf.Clamp01(elapsed / totalDuration);

            // Move head smoothly from top to bottom
            if (head != null)
                head.position = Vector3.Lerp(startPos, endPos, t);

            // Stretch body from head → tail
            if (body != null && head != null && tail != null)
            {
                Vector3 bodyPos = (head.position + tail.position) / 2f;
                body.position = bodyPos;

                Vector3 scale = body.localScale;
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