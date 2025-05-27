using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible", menuName = "Inventory/Collectible Data")]
public class CollectibleData : ScriptableObject
{
    public string id;                  // Unique identifier
    public string displayName;         // Display name
    public Sprite icon;                // Icon
    public string description;         // Description
     
    [Header("Properties")]
    public CollectibleType type;       // Category or collectible type
    public int maxStackSize = 1;       // How many can stack in inventory
    public bool isConsumable = false;  // Whether it can be used/consumed
    
    // Add any other common properties here
}

public enum CollectibleType
{
    Resource,   // Basic resources
    Mushroom,   // Mushrooms
    Key,        // Keys for locks
    QuestItem,  // Quest-related items
    Powerup     // Items that give powers
    // Add more types as needed
}
