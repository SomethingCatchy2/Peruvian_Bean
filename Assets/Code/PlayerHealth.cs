using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 10f;
    public float currentHealth;
    
    [Header("Visual Effects")]
    public Light2D playerLight;                    // The light component attached to player
    public float maxLightIntensity = 1.5f;         // Light intensity at full health
    public float minLightIntensity = 0.1f;         // Light intensity at 0 health (before death)
    public float lightTransitionTime = 2f;        // Time for light to transition smoothly
    
    [Header("Shadow Effects")]
    public float maxShadowIntensity = 0.2f;        // Shadow intensity at full health (lighter shadows)
    public float minShadowIntensity = 1.0f;        // Shadow intensity at 0 health (darker shadows)
    
    [Header("Death Settings")]
    public bool isDead = false;
    public float deathLightFadeTime = 1f;          // Time for light to fade out on death
    
    [Header("Health Bar UI")]
    public Image healthBarImage;              // UI Image for the health bar
    public Sprite[] healthBarSprites;         // 102 sprites (frame 0 full → 101 empty)
    public float healthBarTransitionTime = 1f;// animation duration

    // Private variables for smooth transitions
    private float targetLightIntensity;
    private float targetShadowIntensity;
    private Coroutine lightTransitionCoroutine;
    private Coroutine shadowTransitionCoroutine;
    private Coroutine healthBarTransitionCoroutine;
    private int currentHealthBarFrame = 0;

    // Events
    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;
    
    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;
    
    void Start()
    {
        // Initialize health with one decimal
        currentHealth = Mathf.Round(maxHealth * 10f) / 10f;
        maxHealth     = Mathf.Round(maxHealth * 10f) / 10f;
        
        // Set initial visual states
        UpdateVisualEffects();
        
        // Find player light if not assigned
        if (playerLight == null)
            playerLight = GetComponentInChildren<Light2D>();
            
        if (playerLight == null)
            Debug.LogWarning("PlayerHealth: No Light2D component found. Light effects will not work.");
        
        // Validate UI setup
        if (healthBarImage == null)
            Debug.LogWarning("PlayerHealth: healthBarImage not assigned; UI will not update.");
        if (healthBarSprites == null || healthBarSprites.Length != 102)
            Debug.LogWarning("PlayerHealth: healthBarSprites should contain exactly 102 frames.");
    }
    
    // Main method to take damage
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        damage = Mathf.Round(damage * 10f) / 10f;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        currentHealth = Mathf.Round(currentHealth * 10f) / 10f;
        
        // Trigger visual updates
        UpdateVisualEffects();
        
        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death 
        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    // Method to heal player
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        healAmount = Mathf.Round(healAmount * 10f) / 10f;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        currentHealth = Mathf.Round(currentHealth * 10f) / 10f;
        
        // Trigger visual updates
        UpdateVisualEffects();
        
        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Player healed for {healAmount}. Health: {currentHealth}/{maxHealth}");
    }
    
    // Update visual effects based on current health
    private void UpdateVisualEffects()
    {
        float healthPercentage = currentHealth / maxHealth;
        
        // Calculate target light intensity
        targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, healthPercentage);
        
        // Calculate target shadow intensity (inverted - less health = more intense shadows)
        targetShadowIntensity = Mathf.Lerp(minShadowIntensity, maxShadowIntensity, healthPercentage);
        
        // Start smooth transitions
        if (lightTransitionCoroutine != null)
            StopCoroutine(lightTransitionCoroutine);
        lightTransitionCoroutine = StartCoroutine(SmoothLightTransition());
        
        if (shadowTransitionCoroutine != null)
            StopCoroutine(shadowTransitionCoroutine);
        shadowTransitionCoroutine = StartCoroutine(SmoothShadowTransition());

        // now update the health‐bar UI
        UpdateHealthBarUI();
    }
    
    // compute target sprite frame and launch smooth transition
    private void UpdateHealthBarUI()
    {
        if (healthBarImage == null || healthBarSprites == null || healthBarSprites.Length != 102)
            return;
        float pct = currentHealth / maxHealth;
        int targetFrame = Mathf.Clamp(Mathf.RoundToInt((1f - pct) * 101f), 0, 101);
        if (healthBarTransitionCoroutine != null)
            StopCoroutine(healthBarTransitionCoroutine);
        healthBarTransitionCoroutine = StartCoroutine(SmoothHealthBarTransition(targetFrame));
    }
    
    // Smooth light intensity transition
    private IEnumerator SmoothLightTransition()
    {
        if (playerLight == null) yield break;
        
        float startIntensity = playerLight.intensity;
        float elapsed = 0f;
        
        while (elapsed < lightTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightTransitionTime;
            
            // Smooth curve for natural feeling transition
            t = Mathf.SmoothStep(0f, 1f, t);
            
            playerLight.intensity = Mathf.Lerp(startIntensity, targetLightIntensity, t);
            yield return null;
        }
        
        playerLight.intensity = targetLightIntensity;
    }
    
    // Smooth shadow intensity transition
    private IEnumerator SmoothShadowTransition()
    {
        if (playerLight == null) yield break;
        
        float startShadowIntensity = playerLight.shadowIntensity;
        float elapsed = 0f;
        
        while (elapsed < lightTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightTransitionTime;
            
            // Smooth curve for natural feeling transition
            t = Mathf.SmoothStep(0f, 1f, t);
            
            playerLight.shadowIntensity = Mathf.Lerp(startShadowIntensity, targetShadowIntensity, t);
            yield return null;
        }
        
        playerLight.shadowIntensity = targetShadowIntensity;
    }
    
    // smoothly interpolate between sprite indices
    private IEnumerator SmoothHealthBarTransition(int targetFrame)
    {
        if (healthBarImage == null || healthBarSprites == null) yield break;
        int startFrame = currentHealthBarFrame;
        if (startFrame == targetFrame) yield break;
        float elapsed = 0f;
        while (elapsed < healthBarTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / healthBarTransitionTime);
            int frame = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(startFrame, targetFrame, t)), 0, 101);
            if (frame != currentHealthBarFrame)
            {
                currentHealthBarFrame = frame;
                var spr = healthBarSprites[frame];
                if (spr != null) healthBarImage.sprite = spr;
            }
            yield return null;
        }
        // ensure final frame
        currentHealthBarFrame = targetFrame;
        var finalSpr = healthBarSprites[targetFrame];
        if (finalSpr != null) healthBarImage.sprite = finalSpr;
    }
    
    // Handle player death
    private void Die()
    {
        isDead = true;
        
        // Start death light fade
        StartCoroutine(DeathLightFade());
        
        // Notify listeners
        OnPlayerDeath?.Invoke();
        
        Debug.Log("Player has died!");
    }
    
    // Fade light out on death
    private IEnumerator DeathLightFade()
    {
        if (playerLight == null) yield break;
        
        float startIntensity = playerLight.intensity;
        float elapsed = 0f;
        
        while (elapsed < deathLightFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathLightFadeTime;
            
            playerLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }
        
        playerLight.intensity = 0f;
    }
    
    // Public getter for health percentage
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    // Public method to set max health (useful for upgrades)
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = GetHealthPercentage();
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthPercentage;
        
        UpdateVisualEffects();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
