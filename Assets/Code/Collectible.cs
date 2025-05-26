using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [Header("Basic Settings")]
    public string itemId;              // Unique identifier for inventory system
    public string itemName;            // Display name
    public Sprite icon;                // Inventory icon
    
    [Header("Collection Settings")]
    public bool requireKeyPress = true;  // False collects on touch
    public KeyCode collectKey = KeyCode.X;
    protected bool playerInRange = false;
    protected GameObject playerObject;

    [Header("Effects")]
    public AudioClip collectSound;
    public ParticleSystem collectParticles;
    
    [Header("Events")]
    public UnityEvent onCollected;     // Custom actions when collected
    
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
        // Handle collection input (X key or Pro A button)
        if (playerInRange && requireKeyPress 
            && (Input.GetKeyDown(collectKey) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
            Collect();
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
        
        // Trigger any custom events
        onCollected?.Invoke();
        
        // Remove the collectible
        Destroy(gameObject);
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
