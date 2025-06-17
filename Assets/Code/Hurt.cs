using UnityEngine;

public class Hurt : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 2f;         // How much damage to deal per hit
    
    [Header("Optional Settings")]
    public bool destroyOnContact = false;      // Whether this object should be destroyed when touched
    public string damageSource = "Unknown";    // Description of what's causing damage (for debugging)
}

