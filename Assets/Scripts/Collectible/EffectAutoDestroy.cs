// --- CREATE NEW FILE: EffectAutoDestroy.cs ---

using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EffectAutoDestroy : MonoBehaviour
{
    void Start()
    {
        // Get the duration of the particle system and destroy the GameObject after that time.
        ParticleSystem ps = GetComponent<ParticleSystem>();
        Destroy(gameObject, ps.main.duration);
    }
}