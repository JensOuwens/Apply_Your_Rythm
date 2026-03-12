using UnityEngine;

public class DestroyOnDone : MonoBehaviour
{
    void OnParticleSystemStopped()
    {
        Destroy(gameObject);
    }
}