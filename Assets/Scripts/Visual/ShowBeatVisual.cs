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
    
    [Serializable]
    public class VisualPrefab
    {
        public BeatType type;
        public BeatAttribute attribute;
        public GameObject prefab;
    }
    
    private List<BeatInstance> liveInstances = new();
    private struct BeatInstance
    {
        public GameObject instance;
        public int positionIndex;
    }
    
    private void OnValidate() => enabled = metronome && prefabs.Length > 0 && positions.Length > 0;
    private void OnEnable() => metronome.OnBeat.AddListener(OnBeatMove);
    private void OnDisable() => metronome.OnBeat.RemoveListener(OnBeatMove);

    public int CompareTo(object obj)
    {
        if (obj is ShowBeatVisual beatVisual)
        {
            return -beatVisual.id.CompareTo(id);
        }
        return -1;
    }
    
    private void OnBeatMove()
    {
        for (var i = liveInstances.Count - 1; i >= 0; i--)
        {
            var beatInstance = liveInstances[i];
            beatInstance.positionIndex++;
            liveInstances[i] = beatInstance;
            
            if (positions.Length > beatInstance.positionIndex) continue;
            Destroy(beatInstance.instance);
            liveInstances.RemoveAt(i);
        }
    }

    private void Update()
    {
        foreach (var beatInstance in liveInstances)
        {
            if (positions.Length - 1 < beatInstance.positionIndex && beatInstance.positionIndex > 0)
                continue;
            beatInstance.instance.transform.position = Vector3.Lerp(beatInstance.instance.transform.position,
                positions[beatInstance.positionIndex].position, Time.deltaTime * speed);
        }
    }

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
        var holdInstance = Instantiate(foundVisual.prefab, positions[0].position, Quaternion.identity);
        liveInstances.Add(new BeatInstance{instance = holdInstance, positionIndex = 0});
    }
}