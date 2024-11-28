using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(ParticleSystem))]
public class GenerateParticleLight : MonoBehaviour
{
    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private float intensity;

    private ParticleSystem m_ParticleSystem;
    private List<GameObject> m_Instances = new List<GameObject>();
    private ParticleSystem.Particle[] m_Particles;

    void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        m_Particles = new ParticleSystem.Particle[m_ParticleSystem.main.maxParticles];
    }

    void LateUpdate()
    {
        int count = m_ParticleSystem.GetParticles(m_Particles);

        while (m_Instances.Count < count)
            m_Instances.Add(Instantiate(m_Prefab, m_ParticleSystem.transform));

        var worldSpace = m_ParticleSystem.main.simulationSpace;
        for (var i = 0; i < m_Instances.Count; i++)
        {
            if (i < count)
            {
                if (worldSpace == ParticleSystemSimulationSpace.World)
                    m_Instances[i].transform.position = m_Particles[i].position;
                else if(worldSpace == ParticleSystemSimulationSpace.Local)
                    m_Instances[i].transform.localPosition = m_Particles[i].position;
                else if(worldSpace == ParticleSystemSimulationSpace.Custom)
                {
                    m_Instances[i].transform.parent = m_ParticleSystem.main.customSimulationSpace;
                    m_Instances[i].transform.localPosition = m_Particles[i].position;
                }

                m_Instances[i].GetComponent<Light2D>().intensity = m_Particles[i].GetCurrentColor(m_ParticleSystem).a / 255f * intensity;
                m_Instances[i].SetActive(true);
            }
            else
            {
                m_Instances[i].SetActive(false);
            }
        }
    }
}
