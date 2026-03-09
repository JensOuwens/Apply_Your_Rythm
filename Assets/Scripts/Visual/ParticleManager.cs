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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);   
        }
    }

    public void SpawnRandomParticle(Vector2 position)
    {
        int listIndex = Random.Range(0, particleSystems.Count - 1);
        
        Instantiate(particleSystems[listIndex].particleSystem, position, Quaternion.identity);
    }

    public void SpawnParticleWithIndex(Vector2 position, int index)
    {
        Instantiate(particleSystems[index].particleSystem, position, Quaternion.identity);
    }
    
    public void SpawnRandomParticleIDPos(int position)
    {
        int listIndex = Random.Range(0, particleSystems.Count - 1);
        
        Instantiate(particleSystems[listIndex].particleSystem,spawnPositions[position],  Quaternion.identity);
    }

    public void SpawnParticleWithIndexIDPos(int position, int index)
    {
        Instantiate(particleSystems[index].particleSystem, spawnPositions[position], Quaternion.identity);
    }
    
    
}
