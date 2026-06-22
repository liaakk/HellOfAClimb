using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class Pause_Menu : MonoBehaviour
{
    public GameObject pausePanel;

    public AudioSource audioSource;
    public AudioClip somresume; // Resume
    public AudioClip somsave; // Save

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isPaused)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        audioSource.PlayOneShot(somresume);

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void SaveAndExit()
    {
        StartCoroutine(PlaySoundAndExit());
    }

    private IEnumerator PlaySoundAndExit()
    {
        audioSource.PlayOneShot(somsave);

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}