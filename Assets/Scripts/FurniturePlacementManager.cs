using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FurniturePlacementManager : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Button[] furnitureButtons;
    [SerializeField] private GameObject[] furniturePrefabs;
    [SerializeField] private Button deleteButton;
    [SerializeField] private LayerMask furnitureLayer;

    private GameObject selectedFurniture;
    private GameObject currentFurniture;
    private List<GameObject> placedFurniture = new List<GameObject>();
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isPlacementMode = false;
    private bool isRepositioningMode = false;

    void Start()
    {
        // Initialize and setup UI buttons
        for (int i = 0; i < furnitureButtons.Length; i++)
        {
            int index = i; // Needed for closure
            furnitureButtons[i].onClick.AddListener(() => SelectFurniture(index));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelectedFurniture);
            deleteButton.gameObject.SetActive(false); // Hide initially
        }
    }

    void Update()
    {
        if (isPlacementMode && currentFurniture != null)
        {
            UpdateFurniturePlacement();
        }
        else if (!isPlacementMode && !isRepositioningMode)
        {
            // Check for furniture selection through touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                SelectExistingFurniture();
            }
        }
        else if (isRepositioningMode && selectedFurniture != null)
        {
            UpdateFurnitureRepositioning();
        }
    }

    public void SelectFurniture(int index)
    {
        if (index < 0 || index >= furniturePrefabs.Length) return;


        // Cancel any previous operations
        CancelCurrentOperation();

        // Create new furniture object
        selectedFurniture = furniturePrefabs[index];
        currentFurniture = Instantiate(selectedFurniture);

        // Setup furniture for placement
        FurnitureController controller = currentFurniture.GetComponent<FurnitureController>();
        if (controller != null)
        {
            controller.SetupForPlacement();
        }

        isPlacementMode = true;
    }

    void UpdateFurniturePlacement()
    {
        // Raycast to find a valid position on a plane
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon) && currentFurniture != null)
        {
            // Position the furniture at raycast hit
            Pose hitPose = hits[0].pose;
            currentFurniture.transform.position = hitPose.position;

            FurnitureController controller = currentFurniture.GetComponent<FurnitureController>();

            // Check for potential collisions
            if (!WillCollide(currentFurniture))
            {
                // Show visual feedback for no collision
                if (controller != null)
                {  
                    controller.SetNoCollision();
                }
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    PlaceFurniture();
                }
            }
            else
            {
                // Show visual feedback for collision
                if (controller != null)
                {
                    controller.SetCollision();
                }
            }
        }
    }

    void PlaceFurniture()
    {
        if (currentFurniture == null) return;

        // Add to placed furniture list
        placedFurniture.Add(currentFurniture);

        // Switch from placement mode
        FurnitureController controller = currentFurniture.GetComponent<FurnitureController>();
        if (controller != null)
        {
            controller.SetupAfterPlacement();
        }

        currentFurniture = null;
        isPlacementMode = false;
    }

    void SelectExistingFurniture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, furnitureLayer))
        {
            GameObject hitObject = hit.transform.gameObject;

            // Find the root furniture object
            FurnitureController controller = hitObject.GetComponentInParent<FurnitureController>();
            if (controller != null)
            {
                CancelCurrentOperation();

                selectedFurniture = controller.gameObject;
                controller.SetSelected(true);

                // Show delete button
                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(true);
                }

                StartRepositioning();
            }
        }
        else
        {
            // Deselect if tapping elsewhere
            CancelCurrentOperation();
        }
    }

    void StartRepositioning()
    {
        if (selectedFurniture == null) return;

        isRepositioningMode = true;

        // Visual feedback for repositioning
        FurnitureController controller = selectedFurniture.GetComponent<FurnitureController>();
        if (controller != null)
        {
            controller.SetupForPlacement();
        }
    }

    void UpdateFurnitureRepositioning()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Move the selected furniture
            Pose hitPose = hits[0].pose;
            Vector3 newPosition = hitPose.position;

            // Temporarily remove from placed list to avoid self-collision
            placedFurniture.Remove(selectedFurniture);

            // Update position for collision check
            Vector3 originalPosition = selectedFurniture.transform.position;
            selectedFurniture.transform.position = newPosition;

            FurnitureController controller = selectedFurniture.GetComponent<FurnitureController>();

            if (!WillCollide(selectedFurniture))
            {
                // Visual feedback
                if (controller != null)
                {
                    controller.SetNoCollision();
                }
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    ConfirmRepositioning();
                }
            }
            else
            {
                // Reset position and show collision feedback
                selectedFurniture.transform.position = originalPosition;
                if (controller != null)
                {
                    controller.SetCollision();
                }
            }

            // Re-add to placed list
            placedFurniture.Add(selectedFurniture);
        }
    }

    void ConfirmRepositioning()
    {
        if (selectedFurniture == null) return;

        FurnitureController controller = selectedFurniture.GetComponent<FurnitureController>();
        if (controller != null)
        {
            controller.SetupAfterPlacement();
        }

        isRepositioningMode = false;
    }

    public void DeleteSelectedFurniture()
    {
        if (selectedFurniture == null) return;

        placedFurniture.Remove(selectedFurniture);
        Destroy(selectedFurniture);
        selectedFurniture = null;

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }

    bool WillCollide(GameObject furniture)
    {
        if (furniture == null) return false;

        Collider furnitureCollider = furniture.GetComponent<Collider>();
        if (furnitureCollider == null) return false;

        // Check collision with each placed furniture
        foreach (GameObject placed in placedFurniture)
        {
            if (placed == furniture) continue; // Skip self

            Collider placedCollider = placed.GetComponent<Collider>();
            if (placedCollider == null) continue;

            // Use Physics.ComputePenetration for accurate collision detection
            Vector3 direction;
            float distance;

            if (Physics.ComputePenetration(
                furnitureCollider, furniture.transform.position, furniture.transform.rotation,
                placedCollider, placed.transform.position, placed.transform.rotation,
                out direction, out distance))
            {
                return true; // Collision detected
            }
        }

        return false;
    }

    void CancelCurrentOperation()
    {
        // Clean up placement mode
        if (isPlacementMode && currentFurniture != null)
        {
            Destroy(currentFurniture);
            currentFurniture = null;
        }

        // Clean up selection
        if (selectedFurniture != null)
        {
            FurnitureController controller = selectedFurniture.GetComponent<FurnitureController>();
            if (controller != null)
            {
                controller.SetSelected(false);
            }
            selectedFurniture = null;
        }

        // Reset modes
        isPlacementMode = false;
        isRepositioningMode = false;

        // Hide delete button
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }
}