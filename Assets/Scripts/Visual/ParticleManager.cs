using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// call this function to spawn particle effects at the desired location
/// </summary>

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
    private List<Vector2>  spawnPositions;

    [SerializeField] private Metronome metronome;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);   
        }
    }
    
    public void SpawnRandomParticleIDPos(int position)
    {
        int listIndex = Random.Range(0, particleSystems.Count);
        
        Instantiate(particleSystems[listIndex].particleSystem,spawnPositions[position],  Quaternion.identity);
    }

    public void SpawnParticleWithIndexIDPos(int position, int index)
    {
        Instantiate(particleSystems[index].particleSystem, spawnPositions[position], Quaternion.identity);
    }

    public void SpawnRandomParticleIDPosHold(int position, BeatData beatData)
    {
        float beatLength = beatData.beatEnd -  beatData.beatStart;
        beatLength = beatLength * metronome.beatDurationInMS / 1000f;
        
        int listIndex = Random.Range(0, particleSystems.Count);
        
        StartCoroutine(SpawnParticleForTimeFrame(position, listIndex, beatLength));
    }
    
    public void SpawnParticleWithIndexIDPosHold(int position, int index, BeatData beatData)
    {
        float beatLength = beatData.beatEnd -  beatData.beatStart;
        beatLength = beatLength * metronome.beatDurationInMS / 1000f;
        
        StartCoroutine(SpawnParticleForTimeFrame(position, index, beatLength));
    }

    private IEnumerator SpawnParticleForTimeFrame(int position, int index, float beatLength)
    {
        float endtime = Time.time + beatLength;
        
        while (Time.time < endtime)
        {
            Instantiate(particleSystems[index].particleSystem, spawnPositions[position], Quaternion.identity);   
            
            yield return new WaitForSeconds(0.2f);
        }
        
    }


}
