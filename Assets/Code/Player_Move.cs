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
    public float jumpForce = 7f;
    Rigidbody2D rb;
    public bool isGrounded = false;
    public Collider2D groundCheckCollider; // Assign in inspector
    public Animator animator; // Assign in inspector
    public ParticleSystem dustParticles; // Assign in inspector

    public bool wasWalking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // rb.gravityScale = 0; // Remove this for platformer gravity

        // Make sure particle system is initially stopped
        if (dustParticles != null)
            dustParticles.Stop();
    }

    void Update()
    {
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

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Optional: flip sprite if moving right (since sprites face left)
        if (horizontal > 0.2f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontal < -0.2f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == groundCheckCollider)
            return; // Ignore self
        if (other.CompareTag("ground"))
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == groundCheckCollider)
            return; // Ignore self
        if (other.CompareTag("ground"))
        {
            isGrounded = false;
        }
    }
}
