using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
  public void OnStartButtonClicked()
  {
    // Load the main game scene
    SceneManager.LoadScene("MainGame");
  }

  public void OnExitButtonClicked()
  {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
    #endif
        Application.Quit(); // Quit the application
  }
}
