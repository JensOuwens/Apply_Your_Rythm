using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private GameObject carbonTapParticle;
    [SerializeField] private GameObject waterTapParticle;
    [SerializeField] private GameObject carbonHoldParticle;
    [SerializeField] private GameObject waterHoldParticle;
    
    [SerializeField]
    private List<Vector2> spawnPositions;
    
    [SerializeField]
    private List<ParticleSystem> particleSystems;
    
    [SerializeField] 
    private Metronome metronome;

    private Dictionary<int, Coroutine> activeHoldParticles = new();

    public void SpawnTapParticle(int position, BeatAttribute attribute)
    {
        GameObject currentParticle = new GameObject();
        switch (attribute)
        {
            case BeatAttribute.Co2:
                currentParticle = carbonTapParticle;
                break;
            case BeatAttribute.Water:
                currentParticle = waterTapParticle;
                break;
        }
        Instantiate(currentParticle, spawnPositions[position], Quaternion.identity);
    }

    public void SpawnHoldParticle(int position, float beatLength, BeatAttribute attribute)
    {
        GameObject currentParticle = null;
        switch (attribute)
        {
            case BeatAttribute.Co2:
                currentParticle = carbonTapParticle;
                break;
            case BeatAttribute.Water:
                currentParticle = waterTapParticle;
                break;
        }

        if (activeHoldParticles.TryGetValue(position, out var existing))
            StopCoroutine(existing);

        activeHoldParticles[position] = StartCoroutine(SpawnParticleForTimeFrame(position, beatLength, attribute, currentParticle));
    }

    private IEnumerator SpawnParticleForTimeFrame(int position, float beatLength, BeatAttribute attribute, GameObject currentParticle)
    {
        float endtime = Time.time + beatLength;

        while (Time.time < endtime)
        {
            Instantiate(currentParticle, spawnPositions[position], Quaternion.identity);   
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

    public void SpawnParticleWithIndexIDPos(int position, int index)
    {
        Instantiate(particleSystems[index].GetComponent<ParticleSystem>(), spawnPositions[position], Quaternion.identity);
    }
}