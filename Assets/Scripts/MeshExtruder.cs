
using UnityEngine;

public static class MeshExtruder
{
    public static Mesh ExtrudeToCube(Mesh planeMesh, float extrusionAmount)
    {
        // Validate input mesh
        if (planeMesh.vertexCount != 4 || planeMesh.triangles.Length != 6)
        {
            Debug.LogError("Input mesh must have 4 vertices and 2 triangles.");
            return null;
        }

        Vector3[] originalVertices = planeMesh.vertices;
        int[] originalTriangles = planeMesh.triangles;

        // Calculate normal from the first triangle
        Vector3 a = originalVertices[originalTriangles[0]];
        Vector3 b = originalVertices[originalTriangles[1]];
        Vector3 c = originalVertices[originalTriangles[2]];
        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

        // Create new vertices (front face + back face)
        Vector3[] newVertices = new Vector3[8];
        for (int i = 0; i < 4; i++)
        {
            newVertices[i] = originalVertices[i]; // Front face
            newVertices[i + 4] = originalVertices[i] + normal * extrusionAmount; // Back face
        }

        // Initialize triangles (6 faces * 2 triangles * 3 vertices = 36 indices)
        int[] newTriangles = new int[36];
        int triangleIndex = 0;

        // Front face (original triangles)
        newTriangles[triangleIndex++] = originalTriangles[0];
        newTriangles[triangleIndex++] = originalTriangles[1];
        newTriangles[triangleIndex++] = originalTriangles[2];
        newTriangles[triangleIndex++] = originalTriangles[3];
        newTriangles[triangleIndex++] = originalTriangles[4];
        newTriangles[triangleIndex++] = originalTriangles[5];

        // Back face (reversed winding)
        newTriangles[triangleIndex++] = 4 + originalTriangles[2]; // 6
        newTriangles[triangleIndex++] = 4 + originalTriangles[1]; // 5
        newTriangles[triangleIndex++] = 4 + originalTriangles[0]; // 4
        newTriangles[triangleIndex++] = 4 + originalTriangles[5]; // 7
        newTriangles[triangleIndex++] = 4 + originalTriangles[4]; // 6
        newTriangles[triangleIndex++] = 4 + originalTriangles[3]; // 5

        // Side faces (quads connecting front and back edges)
        // Bottom edge
        AddSideQuad(new int[] { 0, 1, 5, 4 }, ref newTriangles, ref triangleIndex);
        // Right edge
        AddSideQuad(new int[] { 1, 3, 7, 5 }, ref newTriangles, ref triangleIndex);
        // Top edge
        AddSideQuad(new int[] { 3, 2, 6, 7 }, ref newTriangles, ref triangleIndex);
        // Left edge
        AddSideQuad(new int[] { 2, 0, 4, 6 }, ref newTriangles, ref triangleIndex);

        // Create and return new mesh
        Mesh cubeMesh = new Mesh();
        cubeMesh.vertices = newVertices;
        cubeMesh.triangles = newTriangles;
        cubeMesh.RecalculateNormals();
        cubeMesh.RecalculateBounds();
        
        return cubeMesh;
    }

    private static void AddSideQuad(int[] vertexIndices, ref int[] triangles, ref int triangleIndex)
    {
        // First triangle
        triangles[triangleIndex++] = vertexIndices[0];
        triangles[triangleIndex++] = vertexIndices[1];
        triangles[triangleIndex++] = vertexIndices[2];
        
        // Second triangle
        triangles[triangleIndex++] = vertexIndices[0];
        triangles[triangleIndex++] = vertexIndices[2];
        triangles[triangleIndex++] = vertexIndices[3];
    }
}
