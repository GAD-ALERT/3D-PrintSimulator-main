#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class STLMaker : MonoBehaviour
{
    public Text logText;
    private List<Facet> facets;
    private Stopwatch stopwatch = new Stopwatch();
    void FitMeshToView(GameObject meshObject)
    {
        Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;

        // Calculate the bounds of the mesh
        Bounds bounds = mesh.bounds;

        // Determine the maximum dimension
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        // Set the desired size (adjust as needed)
        float desiredSize = 5f;

        // Compute a uniform scale factor
        float scaleFactor = desiredSize / maxDimension;

        // Apply scale
        meshObject.transform.localScale = Vector3.one * scaleFactor;

        // Center the mesh at origin
        meshObject.transform.position = -bounds.center * scaleFactor;

        // Optional: Adjust the camera to look at the object
        Camera.main.transform.position = new Vector3(0, 0, -desiredSize * 2);
        Camera.main.transform.LookAt(Vector3.zero);
    }

    private void OnGUI()
    {
#if UNITY_EDITOR
        if (GUI.Button(new Rect(10, 10, 200, 40), "Load STL File"))
        {
            string filePath = EditorUtility.OpenFilePanel("Select STL File", "", "stl");
            if (!string.IsNullOrEmpty(filePath))
            {
                StartStopwatch();

                if (filePath.EndsWith(".stl"))
                {
                    byte[] bytes = File.ReadAllBytes(filePath);

                    if (IsBinarySTL(bytes))
                    {
                        StopStopwatchWithMessage("Binary STL loaded");
                        CreateMeshFromBinary(bytes);
                    }
                    else
                    {
                        string ascii = File.ReadAllText(filePath);
                        StopStopwatchWithMessage("ASCII STL loaded");
                        CreateMeshFromAscii(ascii);
                    }
                }
            }
        }
#else
        GUI.Label(new Rect(10, 10, 300, 40), "STL loading works only inside Unity Editor.");
#endif
    }

    public void CreateMesh()
    {
        StartStopwatch();

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        var clonedMesh = new Mesh
        {
            name = "GeneratedMesh",
            indexFormat = IndexFormat.UInt32
        };

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
            triangles.Add(i * 3 + 0);
            triangles.Add(i * 3 + 1);
            triangles.Add(i * 3 + 2);
        }

        clonedMesh.vertices = vertices.ToArray();
        clonedMesh.normals = normals.ToArray();
        clonedMesh.triangles = triangles.ToArray();
        meshFilter.mesh = clonedMesh;

        meshRenderer.material = new Material(Shader.Find("Standard"));
        
        meshFilter.mesh = clonedMesh;

// Fit the mesh in view
        FitMeshToView(this.gameObject);

        StopStopwatchWithMessage("created mesh");
    }

    public void CreateMeshFromAscii(string stlData)
    {
        StartStopwatch();
        facets = new List<Facet>();

        var facetSplits = stlData.Split(new[] { "facet normal" }, StringSplitOptions.None);
        foreach (var split in facetSplits)
        {
            var facetValues = split.Replace("outer loop", "")
                                   .Replace("vertex", "")
                                   .Replace("endloop", "")
                                   .Replace("endfacet", "")
                                   .Replace("\n", "")
                                   .Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            if (facetValues.Length == 12)
            {
                facets.Add(new Facet
                {
                    normal = new Vector3(Convert.ToSingle(facetValues[0]), Convert.ToSingle(facetValues[1]), Convert.ToSingle(facetValues[2])),
                    v1 = new Vector3(Convert.ToSingle(facetValues[3]), Convert.ToSingle(facetValues[4]), Convert.ToSingle(facetValues[5])),
                    v2 = new Vector3(Convert.ToSingle(facetValues[6]), Convert.ToSingle(facetValues[7]), Convert.ToSingle(facetValues[8])),
                    v3 = new Vector3(Convert.ToSingle(facetValues[9]), Convert.ToSingle(facetValues[10]), Convert.ToSingle(facetValues[11]))
                });
            }
        }

        StopStopwatchWithMessage("Parsed ASCII STL");
        CreateMesh();
    }

    public void CreateMeshFromBinary(byte[] stlData)
    {
        StartStopwatch();
        facets = new List<Facet>();

        using (MemoryStream s = new MemoryStream(stlData))
        using (BinaryReader br = new BinaryReader(s))
        {
            var header = br.ReadBytes(80);
            uint triCount = br.ReadUInt32();

            for (int i = 0; i < triCount; i++)
            {
                var normal = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v1 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                br.ReadUInt16(); // Attribute byte count

                facets.Add(new Facet { normal = normal, v1 = v1, v2 = v2, v3 = v3 });
            }
        }

        StopStopwatchWithMessage("Parsed Binary STL");
        CreateMesh();
    }

    private bool IsBinarySTL(byte[] data)
    {
        string header = Encoding.ASCII.GetString(data, 0, Mathf.Min(data.Length, 80));
        return !header.Trim().StartsWith("solid", StringComparison.OrdinalIgnoreCase);
    }

    void StartStopwatch() => stopwatch.Start();

    private int count = 0;
    void StopStopwatchWithMessage(string task)
    {
        stopwatch.Stop();
        string log = $"{++count} - {task} finished in {stopwatch.ElapsedMilliseconds} ms";
        if (logText != null)
            logText.text = (count % 3 == 0 ? "\n" : "") + log + "\n" + logText.text;

        Debug.Log(log);
        stopwatch.Reset();
    }
}

public class Facet
{
    public Vector3 normal { get; set; }
    public Vector3 v1 { get; set; }
    public Vector3 v2 { get; set; }
    public Vector3 v3 { get; set; }
}
