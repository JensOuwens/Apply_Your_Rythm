using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// keeps track of beat visuals
/// </summary>
public class ShowBeatVisual : MonoBehaviour, IComparable
{
    public int id = 0;
    [SerializeField] private Metronome metronome;
    [SerializeField] private VisualPrefab[] prefabs;
    [SerializeField] private Transform[] positions;
    [SerializeField] private float speed = 50;
    private readonly List<BeatInstance> liveInstances = new();
    
    [Serializable]
    public class VisualPrefab
    {
        public BeatType type;
        public BeatAttribute attribute;
        public GameObject prefab;
        [Space, SerializeField] private Material holdLineMaterial;
        [SerializeField] private Gradient holdLineColor;

        public BeatInstance Spawn(Transform[] positions, Vector3 position, int index = 0, int beatLength = 1)
        {
            BeatInstance instance;
            if (type == BeatType.Hold)
                instance = new HoldBeatInstance(Instantiate(prefab, position, Quaternion.identity), index, positions, beatLength, holdLineMaterial, holdLineColor);
            else
                instance = new BeatInstance(Instantiate(prefab, position, Quaternion.identity), index, positions);
            return instance;
        }
    }
    
    public class BeatInstance
    {
        protected readonly GameObject visualInstance;
        protected readonly Transform[] positionsRef;
        protected int positionIndex;

        public BeatInstance(GameObject visualInstance, int positionIndex, Transform[] positions)
        {
            positionsRef = positions;
            this.visualInstance = visualInstance;
            this.positionIndex = positionIndex;
        }

        /// <summary>
        /// increments the visual's position (so update will move towards it)
        /// </summary>
        public void IncrementPosition() => positionIndex = Math.Clamp(positionIndex + 1, 0, positionsRef.Length);

        public virtual bool ShouldDestroyInstance()
        {
            if (positionsRef.Length > positionIndex) return false;
            Destroy(visualInstance);
            return true;
        }

        public virtual void Update(float speed)
        {
            if (positionsRef.Length - 1 < positionIndex && positionIndex > 0)
                return;
            visualInstance.transform.position = Vector3.Lerp(visualInstance.transform.position, 
                positionsRef[positionIndex].position, Time.deltaTime * speed);
        }
    }

    private class HoldBeatInstance : BeatInstance
    {
        private readonly GameObject holdLineInstance;
        private readonly LineRenderer lineRenderer;
        private int lineLength;

        public HoldBeatInstance(GameObject visualInstance, int positionIndex, Transform[] positions, int lineLength,
            Material holdLineMaterial, Gradient holdLineColor) : base(visualInstance, positionIndex, positions)
        {
            this.lineLength = lineLength + 1; // add extra beat for tail
            
            holdLineInstance = new GameObject($"{visualInstance.name}_HoldLine");
            lineRenderer = holdLineInstance.AddComponent<LineRenderer>();

            lineRenderer.material = holdLineMaterial;
            lineRenderer.widthCurve = AnimationCurve.EaseInOut(0, 0.6f, 1, 0.1f);
            lineRenderer.colorGradient = holdLineColor;
            lineRenderer.numCapVertices = 10;
            lineRenderer.sortingOrder = 0;
            
            lineRenderer.positionCount = Math.Max(lineLength, positions.Length);
            lineRenderer.useWorldSpace = true;
            for (var i = 0; i < lineRenderer.positionCount; i++) lineRenderer.SetPosition(i, visualInstance.transform.position);
        }

        public override bool ShouldDestroyInstance()
        {
            // ONLY shrink after head is finished
            if (!visualInstance) 
                lineLength--;

            if (!base.ShouldDestroyInstance() || lineLength > 0)
                return false;
            Destroy(holdLineInstance);
            return true;
        }

        public override void Update(float speed)
        {
            if (!lineRenderer) return;

            #region GetHeadPosition
            Vector3 headPos;
            if (!visualInstance)
                headPos = positionsRef.Last().position; // head is gone → lock to last position
            else
            {
                base.Update(speed);
                headPos = visualInstance.transform.position;
            }
            lineRenderer.SetPosition(0, headPos);
            #endregion

            var targetCount = Mathf.Max(1, lineLength);
            if (lineRenderer.positionCount <= targetCount) // when moving dont shrink
            {
                SetLinePointsAlongPositions(speed);
                return;
            }

            ShrinkLinePoints(speed);
        }

        private void ShrinkLinePoints(float speed)
        {
            var currentCount = lineRenderer.positionCount;
            
            var current = lineRenderer.GetPosition(currentCount - 1);
            var target = lineRenderer.GetPosition(currentCount - 2);
            var diff = target - current;
            var dist = diff.magnitude;
            
            var step = speed * Time.deltaTime;
            var newPos = current + diff.normalized * Mathf.Min(step, dist);
            lineRenderer.SetPosition(currentCount - 1, newPos);
            
            if (dist <= 0.001f)
                lineRenderer.positionCount--;
        }

        private void SetLinePointsAlongPositions(float speed)
        {
            for (var i = 1; i < lineRenderer.positionCount; i++)
            {
                var targetIndex = positionIndex - i;
                Vector3 targetPos;

                if (targetIndex >= 0 && targetIndex < positionsRef.Length)
                    targetPos = positionsRef[targetIndex].position;
                else
                    targetPos = positionsRef[0].position + Vector3.up * 5;

                var point = lineRenderer.GetPosition(i);
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(point, targetPos, Time.deltaTime * speed));
            }
        }
    }
    
    private void OnValidate() => enabled = metronome && prefabs.Length > 0 && positions.Length > 0;
    private void OnEnable() => metronome.OnBeat.AddListener(OnBeatMove);
    private void OnDisable() => metronome.OnBeat.RemoveListener(OnBeatMove);

    public int CompareTo(object obj)
    {
        if (obj is ShowBeatVisual beatVisual)
            return -beatVisual.id.CompareTo(id);
        return -1;
    }
    
    /// <summary>
    /// Moves all live instances, and removes them in case incrementation fails
    /// </summary>
    private void OnBeatMove()
    {
        for (var i = liveInstances.Count - 1; i >= 0; i--)
        {
            var beatInstance = liveInstances[i];
            beatInstance.IncrementPosition();
            if (!beatInstance.ShouldDestroyInstance())
                continue;
            liveInstances.RemoveAt(i);
        }
    }
    private void Update() => liveInstances.ForEach(instance => instance.Update(speed));
    
    public void TriggerSpawnVisual(BeatData beat, float beatDurationInMS)
    {
        var beatDelay = beatDurationInMS / 1000f;
        StartCoroutine(SpawnVisual(beat, beatDelay));
    }

    private IEnumerator SpawnVisual(BeatData beat, float beatDelay)
    {
        // Wait spawnOffset beats before instance
        yield return new WaitForSeconds(beatDelay);

        var foundVisual = prefabs.FirstOrDefault(instance => instance.type == beat.type && instance.attribute == beat.attribute);
        if (foundVisual == null)
            yield break;
        
        // Spawn instance at top
        liveInstances.Add(foundVisual.Spawn(positions, positions[0].position, beatLength: beat.beatEnd - beat.beatStart));
    }
}