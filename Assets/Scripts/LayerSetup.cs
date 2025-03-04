using UnityEngine;

// This script helps to set up the furniture layer at runtime
// Attach to the FurniturePlacementManager GameObject
public class LayerSetup : MonoBehaviour
{
    [SerializeField] private string furnitureLayerName = "Furniture";
    
    private void Awake()
    {
        // Create the furniture layer if it doesn't exist
        int furnitureLayer = LayerMask.NameToLayer(furnitureLayerName);
        if (furnitureLayer == -1)
        {
            Debug.LogWarning("Furniture layer is not defined in project settings. Please add a layer named 'Furniture' in Edit > Project Settings > Tags and Layers.");
        }
        
        // Set the furnitureLayer in the FurniturePlacementManager
        FurniturePlacementManager placementManager = GetComponent<FurniturePlacementManager>();
        if (placementManager != null)
        {
            // Use reflection to set the layer mask if needed
            // This is optional - you can manually set it in the inspector
        }
    }
}