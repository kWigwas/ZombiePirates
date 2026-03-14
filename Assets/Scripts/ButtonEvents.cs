using UnityEngine;
using UnityEngine.SceneManagement; // Needed to switch scenes

public class ButtonEvents : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "Ocean"; // Set this to the scene you want to load

    // Called when the Start button is clicked
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Called when the Exit button is clicked
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in Editor
        #else
            Application.Quit(); // Quits the built game
        #endif
    }
}