// SceneSwitcher.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Call this function to go back to the Import Scene
    public void GoToImportViewer()
    {
        Debug.Log("Import Scene button clicked!");
        SceneManager.LoadScene("ImportViewer");
    }
    
    // Call this function to go to the Print Simulation Scene
    public void GoToSimulation()
    {
        Debug.Log("Simulation Scene button clicked!");
        SceneManager.LoadScene("Simulation");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("ImportViewer"); // or "PrintScene"
        }
    }
}

