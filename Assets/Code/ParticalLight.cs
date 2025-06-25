using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ParticleLight : MonoBehaviour
{
    [Header("Light Settings")]
    public GameObject lightPrefab; // Prefab with Light2D component
    public float lightIntensity = 1f;
    public float lightRange = 5f;
    public Color lightColor = Color.white;
    
    [Header("Performance")]
    public int maxLights = 100;
    
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private List<GameObject> activeLights = new List<GameObject>();
    
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        
        // Pre-populate light pool
        for (int i = 0; i < maxLights; i++)
        {
            GameObject light = CreateLight();
            light.SetActive(false);
            lightPool.Enqueue(light);
        }
    }
    
    void Update()
    {
        if (particleSystem == null) return;
        
        int particleCount = particleSystem.particleCount;
        
        // Resize particle array if needed
        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }
        
        // Get current particles
        int numParticlesAlive = particleSystem.GetParticles(particles);
        
        // Return excess lights to pool
        while (activeLights.Count > numParticlesAlive)
        {
            ReturnLightToPool(activeLights.Count - 1);
        }
        
        // Create or update lights for particles
        for (int i = 0; i < numParticlesAlive; i++)
        {
            if (i < activeLights.Count)
            {
                // Update existing light position
                activeLights[i].transform.position = particles[i].position;
            }
            else
            {
                // Create new light from pool
                GameObject light = GetLightFromPool();
                if (light != null)
                {
                    light.transform.position = particles[i].position;
                    light.SetActive(true);
                    activeLights.Add(light);
                }
            }
        }
    }
    
    GameObject CreateLight()
    {
        GameObject lightObj;
        
        if (lightPrefab != null)
        {
            lightObj = Instantiate(lightPrefab, transform);
        }
        else
        {
            // Create default light setup
            lightObj = new GameObject("ParticleLight");
            lightObj.transform.SetParent(transform);
            
            Light2D light2D = lightObj.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Point;
            light2D.intensity = lightIntensity;
            light2D.pointLightOuterRadius = lightRange;
            light2D.color = lightColor;
        }
        
        return lightObj;
    }
    
    GameObject GetLightFromPool()
    {
        if (lightPool.Count > 0)
        {
            return lightPool.Dequeue();
        }
        return null;
    }
    
    void ReturnLightToPool(int index)
    {
        if (index < activeLights.Count)
        {
            GameObject light = activeLights[index];
            light.SetActive(false);
            lightPool.Enqueue(light);
            activeLights.RemoveAt(index);
        }
    }
    
    void OnDestroy()
    {
        // Clean up all lights
        foreach (GameObject light in activeLights)
        {
            if (light != null) DestroyImmediate(light);
        }
        
        while (lightPool.Count > 0)
        {
            GameObject light = lightPool.Dequeue();
            if (light != null) DestroyImmediate(light);
        }
    }
}
