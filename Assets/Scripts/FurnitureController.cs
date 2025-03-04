using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material placementMaterial;
    [SerializeField] private Material repositioningMaterial;

    [SerializeField] private Material noCollisionMaterial;
    [SerializeField] private Material collisionMaterial;
    
    private Renderer[] renderers;
    private Collider furnitureCollider;
    
    void Awake()
    {
        // Cache components
        renderers = GetComponentsInChildren<Renderer>();
        furnitureCollider = GetComponent<Collider>();
        
        // If no collider exists, add a box collider
        if (furnitureCollider == null)
        {
            furnitureCollider = gameObject.AddComponent<BoxCollider>();
            // Adjust collider to fit the mesh
            AdjustColliderToMesh();
        }
    }

    
    public void SetupForPlacement()
    {
        // Semi-transparent during placement
        if (placementMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = placementMaterial;
            }
        }
        
        // Enable collider for collision detection
        if (furnitureCollider != null)
        {
            furnitureCollider.isTrigger = true;
        }
    }

    public void SetNoCollision() {
        if (noCollisionMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = noCollisionMaterial;
            }
        }
    }

    public void SetCollision() {
        if (collisionMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = collisionMaterial;
            }
        }
    }
    
    public void SetupAfterPlacement()
    {
        // Restore to default appearance
        if (defaultMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = defaultMaterial;
            }
        }
        
        // Make collider solid
        if (furnitureCollider != null)
        {
            furnitureCollider.isTrigger = false;
        }
    }
    
    public void SetSelected(bool isSelected)
    {
        if (isSelected && selectedMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = selectedMaterial;
            }
        }
    }
    
    public void SetupForRepositioning()
    {
        // Visual feedback for repositioning mode
        if (repositioningMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = repositioningMaterial;
            }
        }
        
        // Enable trigger for collision detection
        if (furnitureCollider != null)
        {
            furnitureCollider.isTrigger = true;
        }
    }
    
    private void AdjustColliderToMesh()
    {
        // Find all mesh renderers
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length == 0) return;
        
        // Calculate combined bounds
        Bounds bounds = meshRenderers[0].bounds;
        for (int i = 1; i < meshRenderers.Length; i++)
        {
            bounds.Encapsulate(meshRenderers[i].bounds);
        }
        
        // Apply to box collider
        BoxCollider boxCollider = furnitureCollider as BoxCollider;
        if (boxCollider != null)
        {
            // Convert bounds to local space
            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            boxCollider.center = localCenter;
            boxCollider.size = bounds.size;
        }
    }
}