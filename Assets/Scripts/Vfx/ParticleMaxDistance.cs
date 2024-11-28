using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Destroys particles that are farther than DistanceToDestroy from the particlesystem emitter
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleMaxDistance : MonoBehaviour
{
    [FormerlySerializedAs("DistanceToDestroy")] [SerializeField] float distanceToDestroy;
    [SerializeField] private Transform target;

    private ParticleSystem cachedSystem;

    void Start()
    {
        cachedSystem = this.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        ParticleSystem.Particle[] ps = new ParticleSystem.Particle[cachedSystem.particleCount];
        cachedSystem.GetParticles(ps);

        // keep only particles that are within DistanceToDestroy
        var distanceParticles = ps.Where(p => Vector2.Distance(target.position, p.position) < distanceToDestroy).ToArray();
        cachedSystem.SetParticles(distanceParticles, distanceParticles.Length);
    }
}