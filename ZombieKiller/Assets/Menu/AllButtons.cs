using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class AllButtons : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RawImage fadeImage;

    [SerializeField] Slider musicSlider;
    GameObject gController;

    void Awake()
    {
        gController = GameObject.Find("GameController");
        if (gController == null) return;
        AudioSource music = gController.GetComponent<AudioSource>();
        musicSlider.value = music.volume;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        fadeImage.DOFade(0f, 1.5f).OnComplete(() => canvasGroup.blocksRaycasts = false).OnComplete(() => canvasGroup.interactable = true);
    }

    public void ExitScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayGame()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        fadeImage.DOFade(1f, 1.5f).OnComplete(() => SceneManager.LoadScene("GameScene01"));
    }

    public void ChangeVolume()
    {
        if (gController == null) return;
        AudioSource music = gController.GetComponent<AudioSource>();
        music.volume = musicSlider.value;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
