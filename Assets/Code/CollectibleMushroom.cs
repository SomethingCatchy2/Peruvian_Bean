using UnityEngine;

public class CollectibleMushroom : MonoBehaviour
{
    [Header("Collection Settings")]
    private bool playerInRange = false;
    private Player_Move playerScript;
    
    [Header("Effects")]
    public AudioClip collectSound;
    public ParticleSystem collectParticles; // Add particle system for collection effect
    
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
        // Check if player is in range and pressing X or Pro A button
        if (playerInRange && (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton0)))
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
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerScript = null;
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
