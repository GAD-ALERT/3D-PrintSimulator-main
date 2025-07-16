using UnityEngine;

public class PrintHeadController : MonoBehaviour
{
    public GameObject targetObject;       // The STL object to print
    public GameObject layerPrefab;        // Cube or quad used to simulate material
    public float layerHeight = 0.1f;      // Z increment per layer
    public float stepSize = 0.2f;         // X/Y step size
    public float printSpeed = 1f;         // Speed of print head

    private Bounds objectBounds;
    private Vector3 startPos;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target Object (STL) not assigned.");
            return;
        }

        objectBounds = targetObject.GetComponent<Renderer>().bounds;
        startPos = new Vector3(objectBounds.min.x, objectBounds.min.y, objectBounds.min.z);
        transform.position = startPos;

        Debug.Log("Starting position: " + startPos);

        // For now, just move the print head to the first position
        // Next, we’ll simulate actual printing movement
    }
}