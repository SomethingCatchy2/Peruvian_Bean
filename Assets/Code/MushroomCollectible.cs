using UnityEngine;

public class MushroomCollectible : Collectible
{
    public CollectibleData mushroomData;
    public bool isHealingMushroom = false;
    public int healAmount = 0;
    
    protected override void Start()
    {
        // Inherit basic setup from parent
        base.Start();
        
        // Apply data from scriptable object if assigned
        if (mushroomData != null)
        {
            itemId = mushroomData.id;
            itemName = mushroomData.displayName;
            icon = mushroomData.icon;
        }
    }
    
    // Override the base collection behavior to add mushroom-specific functionality
    public override void Collect()
    {
        // If this is a healing mushroom, apply health effect
        if (isHealingMushroom && playerObject != null)
        {
            // Example only - you would implement the actual health system
            Debug.Log($"Healing player for {healAmount} points");
            // playerObject.GetComponent<PlayerHealth>()?.Heal(healAmount);
        }
        
        // Call the base collection logic
        base.Collect();
    }
    
    protected override void AddToInventory()
    {
        base.AddToInventory();
        
        // Additional mushroom-specific inventory logic could go here
        // For now just adding mushroom to player if that component exists
        if (playerObject != null)
        {
            Player_Move playerMove = playerObject.GetComponent<Player_Move>();
            if (playerMove != null)
            {
                playerMove.AddMushroom();
            }
        }
    }
}
