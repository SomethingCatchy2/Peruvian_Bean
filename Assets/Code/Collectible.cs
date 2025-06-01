using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class Collectible : MonoBehaviour
{
    [Header("Basic Settings")]
    public string itemId;              // Unique identifier for inventory system
    public string itemName;            // Display name
    public Sprite icon;                // Inventory icon
    
    [Header("Collection Settings")]
    public bool requireKeyPress = true;  // False collects on touch
    public KeyCode collectKey = KeyCode.X;
    
    [Header("Controller Settings")]
    [Tooltip("Try different button configurations for collection")]
    public bool useMultipleControllerButtons = true;
    [Tooltip("Add specific controller buttons that should trigger collection")]
    public List<int> collectionButtonIndices = new List<int> { 0, 1, 2, 3, 15 }; // Try B, A, X, Y, and ZR buttons
    
    protected bool playerInRange = false;
    protected GameObject playerObject;

    [Header("Effects")]
    public AudioClip collectSound;
    public ParticleSystem collectParticles;
    
    [Header("Healing")]
    public float healAmount = 0f;  // amount to heal player on collect (default 0)
    
    [Header("Events")]
    public UnityEvent onCollected;     // Custom actions when collected

    [Header("Sylodastic Effect")]
    public bool isSylodastic = false; // If true, triggers Sylodastic global volume effect
    public float sylodasticDuration = 60f; // Duration in seconds for Sylodastic effect
     
    protected virtual void Start()
    {
        // Ensure collider as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"Collectible {itemName} needs a Collider2D component!");
        }
    }
    
    protected virtual void Update()
    {
        if (playerInRange && requireKeyPress)
        {
            // Collect on X key or Y button (JoystickButton3)
            bool buttonPressed = Input.GetKeyDown(collectKey) || Input.GetKeyDown(KeyCode.JoystickButton3);
            if (buttonPressed)
                Debug.Log("Collection triggered by Y button (JoystickButton3)");

            if (buttonPressed)
            {
                Collect();
            }
        }
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            
            // Auto-collect if we don't require key press
            if (!requireKeyPress)
            {
                Collect();
            }
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerObject = null;
        }
    }
    
    // Main collection method - can be overridden by child classes
    public virtual void Collect()
    {
        // Play collection effects
        PlayCollectionEffects();
        
        // Add to player's inventory (will be handled by inventory system in the future)
        AddToInventory();
        
        // Heal or damage the player if applicable
        if (playerObject != null && healAmount != 0f)
        {
            var ph = playerObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                if (healAmount > 0f)
                    ph.Heal(healAmount);
                else
                    ph.TakeDamage(-healAmount);
            }
        }

        // If Sylodastic, trigger global volume profile switch
        if (isSylodastic)
        {
            // Old line: StartCoroutine(SwitchToSylodasticProfile());
            GameObject hostObject = new GameObject("SylodasticEffectCoroutineHost");
            CoroutineHost hostComponent = hostObject.AddComponent<CoroutineHost>();
            hostComponent.Run(SwitchToSylodasticProfile()); // Pass the IEnumerator from our method
        }
        
        // Trigger any custom events
        onCollected?.Invoke();
        
        // Remove the collectible
        Destroy(gameObject);
    }

    // Coroutine to switch global volume profile to Sylodastic and revert after duration
    protected virtual System.Collections.IEnumerator SwitchToSylodasticProfile()
    {
        Debug.Log("Collectible: Switching to Sylodastic profile.");
        GlobalVolumeManager.SetProfile("Sylodastic");
        yield return new WaitForSeconds(sylodasticDuration);
        Debug.Log("Collectible: Reverting to Default profile.");
        GlobalVolumeManager.SetProfile("Default");
    }
    
    protected virtual void PlayCollectionEffects()
    {
        // Sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Particles
        if (collectParticles != null)
        {
            ParticleSystem particles = Instantiate(collectParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    }
    
    protected virtual void AddToInventory()
    {
        // This will be replaced by inventory system later
        // For now just log that we collected the item
        Debug.Log($"Collected: {itemName} (ID: {itemId})");
    }
}

// New helper class for running coroutines on a temporary, persistent host
public class CoroutineHost : MonoBehaviour
{
    public void Run(System.Collections.IEnumerator routineToExecute)
    {
        StartCoroutine(ExecuteThenDestroy(routineToExecute));
    }

    private System.Collections.IEnumerator ExecuteThenDestroy(System.Collections.IEnumerator routineToExecute)
    {
        yield return StartCoroutine(routineToExecute);
        Destroy(gameObject); // Destroy the host GameObject after the routine is done
    }
}
