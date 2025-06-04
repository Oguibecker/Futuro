using UnityEngine;
using System.Collections.Generic; // For List

public class OutlineController : MonoBehaviour
{
    public Material outlineMaterial; // Assign your "OutlineMaterial" here
    public float outlineWidth = 0.02f; // Overrides shader graph's default if needed

    private Renderer[] renderers;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool isOutlined = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("OutlineController: No Renderer found on this GameObject or its children.");
            enabled = false; // Disable if nothing to render
            return;
        }

        // Store original materials
        foreach (Renderer r in renderers)
        {
            originalMaterials.Add(r, r.sharedMaterials);
        }

        // Set the outline width in the shader if the property exists
        if (outlineMaterial != null && outlineMaterial.HasProperty("_OutlineWidth"))
        {
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    // Call this to enable the outline
    public void EnableOutline(Color? outlineColor = null)
    {
        if (isOutlined) return; // Prevent re-enabling if already active

        if (outlineMaterial == null)
        {
            Debug.LogError("Outline Material is not assigned!");
            return;
        }

        // Set outline color if provided
        if (outlineColor.HasValue && outlineMaterial.HasProperty("_OutlineColor"))
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor.Value);
        }

        foreach (Renderer r in renderers)
        {
            // Create a new array to add the outline material
            Material[] newMaterials = new Material[r.sharedMaterials.Length + 1];
            // Copy original materials
            r.sharedMaterials.CopyTo(newMaterials, 0);
            // Add the outline material at the end
            newMaterials[r.sharedMaterials.Length] = outlineMaterial;
            r.sharedMaterials = newMaterials;
        }
        isOutlined = true;
    }

    // Call this to disable the outline
    public void DisableOutline()
    {
        if (!isOutlined) return;

        foreach (Renderer r in renderers)
        {
            // Revert to original materials
            if (originalMaterials.ContainsKey(r))
            {
                r.sharedMaterials = originalMaterials[r];
            }
        }
        isOutlined = false;
    }

    // Example Usage (for testing)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!isOutlined)
            {
                EnableOutline(Color.cyan * 2f); // Example: glowing cyan
            }
            else
            {
                DisableOutline();
            }
        }
    }
}