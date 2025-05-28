using UnityEngine;

public class Hurt : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 2f;         // How much damage to deal per second
    public float damageInterval = 0.5f;        // How often to apply damage (in seconds)
    
    [Header("Optional Settings")]
    public bool destroyOnContact = false;      // Whether this object should be destroyed when touched
    public string damageSource = "Unknown";    // Description of what's causing damage (for debugging)
}

