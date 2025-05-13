using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Singleton instance for easy access
    public static InventoryManager Instance { get; private set; }
    
    // Dictionary to store collected items by ID
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    
    // Event for UI updates
    public delegate void InventoryChangedHandler();
    public event InventoryChangedHandler OnInventoryChanged;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Add an item to inventory
    public void AddItem(string itemId, int count = 1)
    {
        if (inventory.ContainsKey(itemId))
        {
            inventory[itemId] += count;
        }
        else
        {
            inventory[itemId] = count;
        }
        
        // Notify listeners that inventory changed
        OnInventoryChanged?.Invoke();
        
        Debug.Log($"Added {count} x {itemId} to inventory. Total: {inventory[itemId]}");
    }
    
    // Check if player has an item
    public bool HasItem(string itemId, int count = 1)
    {
        return inventory.ContainsKey(itemId) && inventory[itemId] >= count;
    }
    
    // Get inventory count of an item
    public int GetItemCount(string itemId)
    {
        return inventory.ContainsKey(itemId) ? inventory[itemId] : 0;
    }
    
    // Remove items from inventory
    public bool RemoveItem(string itemId, int count = 1)
    {
        if (!HasItem(itemId, count))
            return false;
            
        inventory[itemId] -= count;
        
        if (inventory[itemId] <= 0)
            inventory.Remove(itemId);
            
        OnInventoryChanged?.Invoke();
        return true;
    }
}
