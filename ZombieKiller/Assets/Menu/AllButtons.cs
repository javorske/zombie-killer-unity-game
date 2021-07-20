﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AllButtons : MonoBehaviour
{
    public Slider musicSlider;
    GameObject gController;
    private void Awake()
    {
        gController = GameObject.Find("GameController");
        if (gController == null) return;
        AudioSource music = gController.GetComponent<AudioSource>();
        musicSlider.value = music.volume;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ExitScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene01");
    }
    public void ChangeVolume(float volume)
    {
        if (gController == null) return;
        AudioSource music = gController.GetComponent<AudioSource>();
        music.volume = volume;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
