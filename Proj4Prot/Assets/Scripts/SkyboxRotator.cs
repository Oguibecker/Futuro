
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SkyboxRotator : MonoBehaviour
{
    public float rotationSpeedX = 0f; 
    public float rotationSpeedY = 1f; 
    public float rotationSpeedZ = 0f; 

    void Awake() // InvertNormals
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null && filter.mesh != null)
        {
            Vector3[] normals = filter.mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i]; // Flip each normal direction
            }
            filter.mesh.normals = normals;

            // Also reverse the winding order of triangles to make them visible from inside
            for (int i = 0; i < filter.mesh.subMeshCount; i++)
            {
                int[] triangles = filter.mesh.GetTriangles(i);
                System.Array.Reverse(triangles);
                filter.mesh.SetTriangles(triangles, i);
            }
        }
    }
    
    void Update()
    {
        // Rotate the GameObject (the sphere) around its local axes
        transform.Rotate(
            rotationSpeedX * Time.deltaTime,
            rotationSpeedY * Time.deltaTime,
            rotationSpeedZ * Time.deltaTime,
            Space.Self // Rotate around the sphere's own axes
        );
    }
}