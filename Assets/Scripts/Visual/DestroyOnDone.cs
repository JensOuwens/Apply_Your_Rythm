using UnityEngine;

public class DestroyOnDone : MonoBehaviour
{
    private ParticleSystem[] systems;
    
    void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>();
    }
    
    void Update()
    {
        foreach (var ps in systems)
        {
            if (ps.IsAlive(true))
                return;
        }

        Destroy(gameObject);
    }
}