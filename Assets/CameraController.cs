using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 10f;
    public float rotateSpeed = 5f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 100f;

    private Vector3 lastMousePosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        // Panning with Middle or Left Mouse Button
        if (Input.GetMouseButton(2) || Input.GetMouseButton(0)) // Middle mouse button
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.Self);
        }

        // Rotation with Right Mouse Button
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = -Input.GetAxis("Mouse Y") * rotateSpeed;
            transform.eulerAngles += new Vector3(mouseY, mouseX, 0);
        }

        // Zoom with Scroll Wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoom = transform.forward * scroll * zoomSpeed;
        transform.position += zoom;

        // Clamp zoom distance
        float distance = Vector3.Distance(transform.position, Vector3.zero);
        if (distance < minZoom)
        {
            transform.position = Vector3.zero + (transform.position - Vector3.zero).normalized * minZoom;
        }
        else if (distance > maxZoom)
        {
            transform.position = Vector3.zero + (transform.position - Vector3.zero).normalized * maxZoom;
        }

        lastMousePosition = Input.mousePosition;
    }
}