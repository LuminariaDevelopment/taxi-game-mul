using UnityEngine;

public class RandomClothes : MonoBehaviour
{
    [Header("Clothing Materials")]
    [Tooltip("Assign the clothing materials to be used for randomization.")]
    public Material[] clothesMaterials;
    
    [Tooltip("Specify the material slot indices for clothing (e.g., 2 and 3).")]
    public int[] clothesTargetIndices;

    [Header("Skin Materials")]
    [Tooltip("Assign the skin color materials to be used for randomization.")]
    public Material[] skinMaterials;
    
    [Tooltip("Specify the material slot indices for skin (e.g., 0).")]
    public int[] skinTargetIndices;

    [Header("Renderer Reference")]
    [Tooltip("Reference to the Renderer component (e.g., MeshRenderer or SkinnedMeshRenderer) with multiple material slots.")]
    public Renderer characterRenderer;

    void Start()
    {
        // If the renderer isn't manually assigned, try to get it from this GameObject.
        if (characterRenderer == null)
        {
            characterRenderer = GetComponent<Renderer>();
            if (characterRenderer == null)
            {
                Debug.LogError("Renderer component not found on this GameObject. Please assign a Renderer manually.");
                return;
            }
        }

        // Retrieve the renderer's current materials.
        Material[] currentMaterials = characterRenderer.materials;

        // Randomize clothing materials for specified indices.
        if (clothesTargetIndices != null && clothesTargetIndices.Length > 0 && clothesMaterials != null && clothesMaterials.Length > 0)
        {
            foreach (int index in clothesTargetIndices)
            {
                if (index < 0 || index >= currentMaterials.Length)
                {
                    Debug.LogError($"Clothing target index {index} is out of bounds for the renderer's materials array.");
                    continue;
                }
                int randomIndex = Random.Range(0, clothesMaterials.Length);
                currentMaterials[index] = clothesMaterials[randomIndex];
            }
        }
        else
        {
            Debug.LogWarning("Either clothing target indices or clothing materials are not assigned.");
        }

        // Randomize skin materials for specified indices.
        if (skinTargetIndices != null && skinTargetIndices.Length > 0 && skinMaterials != null && skinMaterials.Length > 0)
        {
            foreach (int index in skinTargetIndices)
            {
                if (index < 0 || index >= currentMaterials.Length)
                {
                    Debug.LogError($"Skin target index {index} is out of bounds for the renderer's materials array.");
                    continue;
                }
                int randomIndex = Random.Range(0, skinMaterials.Length);
                currentMaterials[index] = skinMaterials[randomIndex];
            }
        }
        else
        {
            Debug.LogWarning("Either skin target indices or skin materials are not assigned.");
        }

        // Reassign the modified materials array back to the renderer.
        characterRenderer.materials = currentMaterials;
    }
}
