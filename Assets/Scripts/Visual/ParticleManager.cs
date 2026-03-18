using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ParticleSystemItem
{
    public uint id;
    public ParticleSystem particleSystem;
}

public class ParticleManager : MonoBehaviour
{
    [HideInInspector]
    public static ParticleManager instance;
    
    [SerializeField]
    private List<ParticleSystemItem> particleSystems;
    [SerializeField]
    private List<ParticleSystemItem> holdParticleSystems;
    
    [SerializeField]
    private List<Vector2> spawnPositions;

    [SerializeField] private Metronome metronome;

    private Dictionary<int, Coroutine> activeHoldParticles = new();

    private void Awake()
    {
        if (instance) return;
        instance = this;
    }

    public void SpawnRandomParticleIDPos(int position)
    {
        int listIndex = Random.Range(0, particleSystems.Count);
        Instantiate(particleSystems[listIndex].particleSystem, spawnPositions[position], Quaternion.identity);
    }

    public void SpawnParticleWithIndexIDPos(int position, int index)
    {
        Instantiate(particleSystems[index].particleSystem, spawnPositions[position], Quaternion.identity);
    }

    public void SpawnRandomParticleIDPosHold(int position, BeatData beatData, float beatLength)
    {
        int listIndex = Random.Range(0, holdParticleSystems.Count);

        if (activeHoldParticles.TryGetValue(position, out var existing))
            StopCoroutine(existing);

        activeHoldParticles[position] = StartCoroutine(SpawnParticleForTimeFrame(position, listIndex, beatLength));
    }

    public void SpawnParticleWithIndexIDPosHold(int position, int index, BeatData beatData)
    {
        float beatLength = (beatData.beatEnd - beatData.beatStart + 1) * metronome.beatDurationInMS / 1000f;

        if (activeHoldParticles.TryGetValue(position, out var existing))
            StopCoroutine(existing);

        activeHoldParticles[position] = StartCoroutine(SpawnParticleForTimeFrame(position, index, beatLength));
    }

    private IEnumerator SpawnParticleForTimeFrame(int position, int index, float beatLength)
    {
        float endtime = Time.time + beatLength;

        while (Time.time < endtime)
        {
            Instantiate(holdParticleSystems[index].particleSystem, spawnPositions[position], Quaternion.identity);   
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void StopHoldParticles(int position)
    {
        if (activeHoldParticles.TryGetValue(position, out var coroutine))
        {
            StopCoroutine(coroutine);
            activeHoldParticles.Remove(position);
        }
    }
}