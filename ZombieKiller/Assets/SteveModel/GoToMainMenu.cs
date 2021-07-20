using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    AudioSource victoryMusic;
    AudioSource deathSound;
    private void Start()
    {
            victoryMusic = GetComponent<AudioSource>();
            victoryMusic.Play();
            Invoke("GoToMenu", victoryMusic.clip.length);
    }
    void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
