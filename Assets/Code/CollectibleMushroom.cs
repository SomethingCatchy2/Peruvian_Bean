using UnityEngine;

public class CollectibleMushroom : MonoBehaviour
{
    [Header("Collection Settings")]
    private bool playerInRange = false;
    private Player_Move playerScript;
    public int collectButtonIndex = 8;  // Button index for collect (default: 8)
    
    [Header("Effects")]
    public AudioClip collectSound;
    public ParticleSystem collectParticles; // Add particle system for collection effect
    
    [Header("Debug")]
    public bool debugInput = true; // Enable input debugging
    
    void Start()
    { 
        // Make sure the collider is set as trigger and properly tagged
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            gameObject.tag = "collectable";
        }
        else
        {
            Debug.LogError("Collectible mushroom needs a Collider2D component!");
        }
    }
    
    void Update()
    {
        // Check if player is in range and pressing the collect button (button 8)
        bool collectButtonPressed = Input.GetKeyDown(KeyCode.X) || 
                                   Input.GetKeyDown((KeyCode)(KeyCode.JoystickButton0 + collectButtonIndex));
        
        if (debugInput && playerInRange && collectButtonPressed)
        {
            Debug.Log($"Collection button {collectButtonIndex} pressed");
        }
        
        if (playerInRange && collectButtonPressed)
        {
            Collect();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerScript = other.GetComponent<Player_Move>();
            if (debugInput)
                Debug.Log("Player entered collection range");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerScript = null;
            if (debugInput)
                Debug.Log("Player exited collection range");
        }
    }
    
    private void Collect()
    {
        // Play collection particles if available
        if (collectParticles != null)
        {
            // Detach particles from parent so they don't get destroyed with the mushroom
            collectParticles.transform.parent = null;
            collectParticles.Play();
            
            // Destroy the particle system after it finishes playing
            float duration = collectParticles.main.duration + collectParticles.main.startLifetime.constantMax;
            Destroy(collectParticles.gameObject, duration);
        }
        
        // Play sound effect if available
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Add mushroom to player's inventory
        if (playerScript != null)
        {
            playerScript.AddMushroom();
        }
        
        // Destroy the mushroom
        Destroy(gameObject);
    }
}
