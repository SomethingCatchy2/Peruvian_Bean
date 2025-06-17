using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

public class Player_Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8.7f;
    Rigidbody2D rb;
    public bool isGrounded = false;
    public Collider2D groundCheckCollider; // Assign in inspector
    public Animator animator; // Assign in inspector
    public ParticleSystem dustParticles; // Assign in inspector

    public bool wasWalking = false;
    
    // New variables for collectibles
    public bool nearCollectible = false;
    public GameObject collectPrompt; // Optional UI element showing "Press Space to collect"
    
    // Collectibles inventory
    private int mushroomsCollected = 0;
    
    // Health component reference
    private PlayerHealth playerHealth;
    
    // Coyote time implementation
    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;  // How long the player can jump after leaving a platform
    public float coyoteTimeCounter;   // Timer to track coyote time
    public bool hasJumped = false;    // Flag to prevent double jumps during coyote time

    [Header("Jump Modifiers")]
    public float fallMultiplier = 2f;       // Multiplier for faster fall
    public float lowJumpMultiplier = 2f;    // Multiplier for short hop

    [Header("Footstep Audio")]
    public AudioClip[] footstepClips;          // assign your step sounds here
    public AudioSource footstepAudioSource;    // optional, falls back to PlayClipAtPoint
    public float footstepBaseInterval = 0.5f;  // seconds per step at full speed
    private float footstepTimer = 0f;

    [Header("Debug - Light Testing")]
    public bool enableLightDebug = true;       // Enable debug controls
    
    [Header("Debug - Controls")]
    public bool debugJumpInput = true;       // Enable jump input debugging
    
    [Header("Input Settings")]
    [Tooltip("Button for debugging controller mappings")]
    public bool debugControllerInput = true;  // Enabled by default to help troubleshoot
    public int jumpButtonIndex = 7;           // Button index for jump (default: 7)
    public int collectButtonIndex = 8;        // Button index for collect (default: 8)
    
    // Store detected input axes for debugging
    private Dictionary<string, float> lastInputValues = new Dictionary<string, float>();
    private float lastInputCheckTime = 0f;

    [Header("Damage System")]
    private Hurt currentHurtObject = null;     // Reference to the hurt object we're touching

    [Header("Invincibility Frames")]
    public float invincibilityDuration = 1.0f; // Duration of i-frames after being hit
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    [Header("I-Frame Visuals")]
    public SpriteRenderer playerSprite; // Assign in inspector (player's main SpriteRenderer)
    public Color iFramePulseColor = Color.black; // Color to pulse to during I-frames
    public float iFramePulseSpeed = 10f; // How fast to pulse (higher = faster)
    public int iFramePulseCount = 4; // How many pulses during I-frames

    private Coroutine iFramePulseCoroutine;
    private Color originalSpriteColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Get health component
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("Player_Move: No PlayerHealth component found!");
        }

        // Make sure particle system is initially stopped
        if (dustParticles != null)
            dustParticles.Stop();
            
        // Make sure collection prompt is hidden initially
        if (collectPrompt != null)
            collectPrompt.SetActive(false);

        // Cache original color
        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null)
            originalSpriteColor = playerSprite.color;
    }

    void Update()
    {
        // Enhanced controller debugging
        if (debugControllerInput)
        {
            // Check button presses (digital inputs)
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown((KeyCode)(KeyCode.JoystickButton0 + i)))
                {
                    Debug.Log($"Controller button pressed: JoystickButton{i}");
                }
            }
            
            // Check axes (analog inputs) every 1 second to avoid log spam
            if (Time.time - lastInputCheckTime > 1f)
            {
                lastInputCheckTime = Time.time;
                
                // Check all possible joystick axes
                string[] axisNames = {"Horizontal", "Vertical", 
                                     "3rd axis", "4th axis", "5th axis", "6th axis", 
                                     "7th axis", "8th axis", "9th axis", "10th axis"};
                
                StringBuilder axisInfo = new StringBuilder("Controller axes values:\n");
                bool hasActiveAxis = false;
                
                foreach (string axis in axisNames)
                {
                    try {
                        float value = Input.GetAxis(axis);
                        
                        // Only log axes that have values or have changed
                        if (Mathf.Abs(value) > 0.1f || 
                            (lastInputValues.ContainsKey(axis) && Mathf.Abs(value - lastInputValues[axis]) > 0.1f))
                        {
                            axisInfo.AppendLine($"- {axis}: {value:F2}");
                            hasActiveAxis = true;
                        }
                        
                        lastInputValues[axis] = value;
                    }
                    catch {
                        // Ignore invalid axes
                    }
                }
                
                if (hasActiveAxis)
                {
                    Debug.Log(axisInfo.ToString());
                    
                    // Debug info about connected joysticks
                    string[] joysticks = Input.GetJoystickNames();
                    if (joysticks.Length > 0)
                    {
                        Debug.Log($"Connected controllers ({joysticks.Length}):");
                        for (int i = 0; i < joysticks.Length; i++)
                        {
                            Debug.Log($"- Controller {i}: \"{joysticks[i]}\"");
                        }
                    }
                    else
                    {
                        Debug.Log("No controllers detected by Unity!");
                    }
                }
            }
        }

        // Invincibility frame timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                // End pulse effect
                if (iFramePulseCoroutine != null)
                {
                    StopCoroutine(iFramePulseCoroutine);
                    iFramePulseCoroutine = null;
                }
                if (playerSprite != null)
                    playerSprite.color = originalSpriteColor;
            }
        }

        float horizontal = Input.GetAxis("Horizontal");
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontal * moveSpeed;
        rb.linearVelocity = velocity;

        // Animation logic
        bool isWalking = Mathf.Abs(horizontal) > 0.4f;

        // Handle dust particles - reworked implementation
        if (dustParticles != null)
        {
            // Get particle system main module
            var main = dustParticles.main;
            
            // Handle different states
            if (isGrounded)
            {
                // Dust when walking on ground
                if (isWalking)
                {
                    // Direction based particle emission
                    var shape = dustParticles.shape;
                    if (horizontal < 0)
                    {
                        shape.position = new Vector3(0.2f, 0, 0); // Offset slightly right when moving left
                    }
                    else
                    {
                        shape.position = new Vector3(-0.2f, 0, 0); // Offset slightly left when moving right
                    }
                    
                    // Enable emission
                    if (!dustParticles.isEmitting)
                    {
                        dustParticles.Play();
                    }
                }
                else if (dustParticles.isEmitting)
                {
                    dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else if (dustParticles.isEmitting)
            {
                // Stop emission but allow particles to fade out naturally
                dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (isWalking && !wasWalking)
        {
            // Start walking: play toWalk forward
            animator.SetFloat("toWalkDirection", 1f);
            animator.SetTrigger("toWalk");
        }
        else if (!isWalking && wasWalking)
        {
            // Stop walking: play toWalk in reverse
            animator.SetFloat("toWalkDirection", -1f);
            animator.SetTrigger("toWalk");
        }

        if (isWalking)
        {
            animator.ResetTrigger("Idle");
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
            animator.SetTrigger("Idle");
        }

        wasWalking = isWalking;

        // Debug jump inputs
        if (debugJumpInput && Input.anyKeyDown)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
                {
                    Debug.Log($"Joystick button {i} pressed - potential jump button");
                }
            }
        }

        // Coyote time logic
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            hasJumped = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Hard-code B button (JoystickButton1) for jump
        bool jumpButtonPressed = Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton0);

        if (jumpButtonPressed && coyoteTimeCounter > 0f && !hasJumped)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            hasJumped = true;
            coyoteTimeCounter = 0f;
            Debug.Log("Jump executed using B or A button (JoystickButton0) I don't know why but it works for both. : ()"); 
        }

        // Variable jump height: if falling, speed up gravity; if ascending & key released, apply low‐jump gravity
        {
            Vector2 vel = rb.linearVelocity;
            if (vel.y < 0f)
            {
                vel += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            }
            else if (vel.y > 0f && !(Input.GetKey(KeyCode.Z) || 
                                     Input.GetKey(KeyCode.JoystickButton0))) // JoystickButton0 is usually A button
            {
                vel += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
            }
            rb.linearVelocity = vel;
        }

        // Optional: flip sprite if moving right (since sprites face left)
        if (horizontal > 0.2f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontal < -0.2f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Footstep sounds when moving on ground
        if (isGrounded && Mathf.Abs(horizontal) > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && footstepClips.Length > 0)
            {
                AudioClip clip = footstepClips[UnityEngine.Random.Range(0, footstepClips.Length)];
                if (footstepAudioSource != null)
                    footstepAudioSource.PlayOneShot(clip);
                else
                    AudioSource.PlayClipAtPoint(clip, transform.position);

                // shorter interval when faster
                footstepTimer = footstepBaseInterval / Mathf.Abs(horizontal);
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == groundCheckCollider)
            return; // Ignore self
        if (other.CompareTag("ground"))
        {
            isGrounded = true;
        }
        else if (other.CompareTag("collectable"))
        {
            nearCollectible = true;
            if (collectPrompt != null)
                collectPrompt.SetActive(true);
        }
        
        // Check for Hurt component instead of tag
        Hurt hurtComponent = other.GetComponent<Hurt>();
        if (hurtComponent != null)
        {
            currentHurtObject = hurtComponent;
            if (!isInvincible && playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(hurtComponent.damagePerSecond);
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;

                // Start pulse effect
                if (iFramePulseCoroutine != null)
                    StopCoroutine(iFramePulseCoroutine);
                if (playerSprite != null)
                    iFramePulseCoroutine = StartCoroutine(IFramePulseRoutine());

                Debug.Log($"Player hit by {hurtComponent.damageSource}, now invincible for {invincibilityDuration} seconds.");

                // Destroy the hurt object if specified
                if (hurtComponent.destroyOnContact)
                {
                    GameObject objectToDestroy = hurtComponent.gameObject;
                    currentHurtObject = null;
                    Destroy(objectToDestroy);
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Optional: If you want to allow repeated damage after i-frames expire while still in contact
        Hurt hurtComponent = other.GetComponent<Hurt>();
        if (hurtComponent != null)
        {
            currentHurtObject = hurtComponent;
            if (!isInvincible && playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(hurtComponent.damagePerSecond);
                isInvincible = true;
                invincibilityTimer = invincibilityDuration;

                // Start pulse effect
                if (iFramePulseCoroutine != null)
                    StopCoroutine(iFramePulseCoroutine);
                if (playerSprite != null)
                    iFramePulseCoroutine = StartCoroutine(IFramePulseRoutine());

                Debug.Log($"Player hit by {hurtComponent.damageSource} (OnTriggerStay2D), now invincible for {invincibilityDuration} seconds.");

                if (hurtComponent.destroyOnContact)
                {
                    GameObject objectToDestroy = hurtComponent.gameObject;
                    currentHurtObject = null;
                    Destroy(objectToDestroy);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == groundCheckCollider)
            return; // Ignore self
        if (other.CompareTag("ground"))
        {
            isGrounded = false;
            // Coyote time starts immediately when leaving the ground
            coyoteTimeCounter = coyoteTime;
        }
        else if (other.CompareTag("collectable"))
        {
            nearCollectible = false;
            if (collectPrompt != null)
                collectPrompt.SetActive(false);
        }
        
        // Check for Hurt component instead of tag
        Hurt hurtComponent = other.GetComponent<Hurt>();
        if (hurtComponent != null && currentHurtObject == hurtComponent)
        {
            currentHurtObject = null;
            // damageTimer = 0f; // Remove timer reset
        }
    }
    
    // Add this method to track mushroom collection
    public void AddMushroom()
    {
        mushroomsCollected++;
        Debug.Log("Mushrooms collected: " + mushroomsCollected);
        // You can add UI updates or other game logic here
    }
    
    // Public method to access health component
    public PlayerHealth GetPlayerHealth()
    {
        return playerHealth;
    }
    
    // Method for taking damage (can be called by enemies, hazards, etc.)
    public void TakeDamage(float damage)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
    
    // Method for healing (can be called by healing items)
    public void Heal(float healAmount)
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
        }
    }

    private IEnumerator IFramePulseRoutine()
    {
        if (playerSprite == null)
            yield break;

        float totalTime = invincibilityDuration;
        float pulseTime = totalTime / Mathf.Max(1, iFramePulseCount * 2);
        float elapsed = 0f;
        int pulse = 0;
        while (elapsed < totalTime)
        {
            // Pulse to color and back
            float t = 0f;
            while (t < 1f && elapsed < totalTime)
            {
                t += Time.deltaTime / pulseTime;
                playerSprite.color = Color.Lerp(originalSpriteColor, iFramePulseColor, Mathf.SmoothStep(0f, 1f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            t = 0f;
            while (t < 1f && elapsed < totalTime)
            {
                t += Time.deltaTime / pulseTime;
                playerSprite.color = Color.Lerp(iFramePulseColor, originalSpriteColor, Mathf.SmoothStep(0f, 1f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            pulse++;
        }
        playerSprite.color = originalSpriteColor;
        iFramePulseCoroutine = null;
    }
}
