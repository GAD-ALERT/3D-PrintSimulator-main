using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB; // Standalone File Browser plugin
using Debug = UnityEngine.Debug;

public class STLMaker : MonoBehaviour
{
    public Text logText;
    public PrintHeadController printHeadController; // reference this in the inspector

    private void FitObjectToView(GameObject obj)
    {
        // Calculate bounds
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;

        Bounds bounds = rend.bounds;
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        // Desired size
        float desiredSize = 1.0f; // You can change this
        float scaleFactor = desiredSize / maxSize;

        // Apply scale and reposition
        obj.transform.localScale = obj.transform.localScale * scaleFactor;
        obj.transform.position = -bounds.center * scaleFactor;
    }
    
    private List<Facet> facets;
    private Stopwatch stopwatch = new Stopwatch();

    private GameObject currentMeshObject;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Import STL File"))
        {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select STL", "", "stl", false);
            if (paths.Length > 0 && File.Exists(paths[0]))
            {
                string filePath = paths[0];
                byte[] stlBytes = File.ReadAllBytes(filePath);
                CreateMeshFromBinary(stlBytes, Path.GetFileNameWithoutExtension(filePath));
            }
        }
    }

    public void CreateMeshFromBinary(byte[] stlData, string objectName = "ImportedSTL")
    {
        StartStopwatch();
        facets = new List<Facet>();
        using (MemoryStream stream = new MemoryStream(stlData))
        using (BinaryReader br = new BinaryReader(stream))
        {
            br.ReadBytes(80); // skip header
            uint triCount = br.ReadUInt32();

            for (int i = 0; i < triCount; i++)
            {
                Vector3 normal = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Vector3 v1 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Vector3 v2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Vector3 v3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                br.ReadUInt16(); // skip attribute byte count

                facets.Add(new Facet { normal = normal, v1 = v1, v2 = v2, v3 = v3 });
            }
        }

        StopStopwatchWithMessage("Binary STL parsed");

        CreateMesh(objectName);
    }

    public void CreateMesh(string objectName)
    {
        StartStopwatch();
        
        GameObject obj = gameObject;
        FitObjectToView(obj);

        if (currentMeshObject != null)
            Destroy(currentMeshObject);

        currentMeshObject = new GameObject(objectName);
        MeshFilter meshFilter = currentMeshObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = currentMeshObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedSTLMesh";

        List<Vector3> normals = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < facets.Count; i++)
        {
            normals.Add(facets[i].normal);
            normals.Add(facets[i].normal);
            normals.Add(facets[i].normal);

            vertices.Add(facets[i].v1);
            vertices.Add(facets[i].v2);
            vertices.Add(facets[i].v3);

            triangles.Add(i * 3);
            triangles.Add(i * 3 + 1);
            triangles.Add(i * 3 + 2);
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();

        meshFilter.mesh = mesh;

        currentMeshObject.transform.position = Vector3.zero;

        // Assign to print head
        if (printHeadController != null)
        {
            printHeadController.targetObject = currentMeshObject;
            Debug.Log("Assigned imported mesh to print head controller.");
        }

        StopStopwatchWithMessage("Mesh created");
    }

    void StartStopwatch() => stopwatch.Start();

    private int count = 0;
    void StopStopwatchWithMessage(string task)
    {
        stopwatch.Stop();
        string log = $"{++count} - {task} finished in {stopwatch.ElapsedMilliseconds} ms";
        if (logText) logText.text = log + "\n" + logText.text;
        Debug.Log(log);
        stopwatch.Reset();
    }

    public class Facet
    {
        public Vector3 normal { get; set; }
        public Vector3 v1 { get; set; }
        public Vector3 v2 { get; set; }
        public Vector3 v3 { get; set; }
    }
}
